using System;
using System.IO;
using System.IO.Compression;

namespace Plus.HabboHotel.Camera
{
    public static class PngSanitizer
    {
        private static readonly byte[] Signature = { 137, 80, 78, 71, 13, 10, 26, 10 };

        private static readonly uint[] CrcTable = BuildCrcTable();

        private static uint[] BuildCrcTable()
        {
            uint[] table = new uint[256];
            for (uint n = 0; n < 256; n++) {
                uint c = n;
                for (int k = 0; k < 8; k++)
                    c = (c & 1) != 0 ? 0xEDB88320 ^ (c >> 1) : c >> 1;
                table[n] = c;
            }
            return table;
        }

        private static uint Crc32(byte[] buffer, int offset, int count)
        {
            uint crc = 0xFFFFFFFF;
            for (int i = offset; i < offset + count; i++)
                crc = CrcTable[(crc ^ buffer[i]) & 0xFF] ^ (crc >> 8);
            return crc ^ 0xFFFFFFFF;
        }

        private static uint ReadUInt32(byte[] buffer, int offset)
        {
            return ((uint)buffer[offset] << 24) | ((uint)buffer[offset + 1] << 16) | ((uint)buffer[offset + 2] << 8) | buffer[offset + 3];
        }

        public static byte[] Sanitize(byte[] input, int maxDimension)
        {
            return Sanitize(input, maxDimension, out _, out _);
        }

        public static byte[] Sanitize(byte[] input, int maxDimension, out int imageWidth, out int imageHeight)
        {
            imageWidth = 0;
            imageHeight = 0;

            if (input == null || input.Length < 8 + 25 + 12 + 12) // signature + IHDR + one IDAT + IEND
                return null;

            for (int i = 0; i < 8; i++)
                if (input[i] != Signature[i])
                    return null;

            long width = 0, height = 0;
            bool seenIhdr = false, seenIend = false, seenPlte = false;
            int idatCount = 0;
            bool lastWasIdat = false;

            using MemoryStream output = new();
            output.Write(Signature, 0, 8);

            using MemoryStream idatStream = new();

            int pos = 8;
            while (pos < input.Length) {
                if (seenIend)
                    return null; // trailing data after IEND -> polyglot attempt

                if (input.Length - pos < 12)
                    return null; // truncated chunk header/footer

                uint length = ReadUInt32(input, pos);
                if (length > int.MaxValue - 12 || length > input.Length - pos - 12)
                    return null;

                int dataOffset = pos + 8;
                string type = string.Concat((char)input[pos + 4], (char)input[pos + 5], (char)input[pos + 6], (char)input[pos + 7]);

                uint declaredCrc = ReadUInt32(input, dataOffset + (int)length);
                uint actualCrc = Crc32(input, pos + 4, (int)length + 4);
                if (declaredCrc != actualCrc)
                    return null;

                switch (type) {
                    case "IHDR":
                        if (seenIhdr || length != 13)
                            return null;

                        seenIhdr = true;
                        width = ReadUInt32(input, dataOffset);
                        height = ReadUInt32(input, dataOffset + 4);

                        byte bitDepth = input[dataOffset + 8];
                        byte colorType = input[dataOffset + 9];
                        byte compression = input[dataOffset + 10];
                        byte filter = input[dataOffset + 11];
                        byte interlace = input[dataOffset + 12];

                        if (width < 1 || height < 1 || width > maxDimension || height > maxDimension)
                            return null;
                        if (compression != 0 || filter != 0 || interlace > 1)
                            return null;
                        if (colorType != 0 && colorType != 2 && colorType != 3 && colorType != 4 && colorType != 6)
                            return null;
                        if (bitDepth != 1 && bitDepth != 2 && bitDepth != 4 && bitDepth != 8 && bitDepth != 16)
                            return null;
                        break;

                    case "PLTE":
                        if (!seenIhdr || seenPlte || idatCount > 0 || length == 0 || length % 3 != 0 || length > 768)
                            return null;
                        seenPlte = true;
                        break;

                    case "IDAT":
                        if (!seenIhdr)
                            return null;
                        if (idatCount > 0 && !lastWasIdat)
                            return null; // IDAT chunks must be consecutive per spec
                        idatCount++;
                        idatStream.Write(input, dataOffset, (int)length);
                        break;

                    case "tRNS":
                        if (!seenIhdr || idatCount > 0 || length > 768)
                            return null;
                        break;

                    case "IEND":
                        if (!seenIhdr || idatCount == 0 || length != 0)
                            return null;
                        seenIend = true;
                        break;

                    default:
                        // Any other chunk (ancillary or unknown critical) is silently dropped.
                        pos = dataOffset + (int)length + 4;
                        lastWasIdat = false;
                        continue;
                }

                if (type != "IEND") {
                    // Copy the whole verified chunk (length + type + data + crc) verbatim.
                    output.Write(input, pos, (int)length + 12);
                }

                lastWasIdat = type == "IDAT";
                pos = dataOffset + (int)length + 4;
            }

            if (!seenIhdr || !seenIend || idatCount == 0)
                return null;

            long maxDecompressed = (width * 8 + 1) * height + 1024;
            if (!ValidateZlibStream(idatStream.ToArray(), maxDecompressed))
                return null;

            imageWidth = (int)width;
            imageHeight = (int)height;

            // Append a canonical IEND chunk.
            byte[] iend = { 0, 0, 0, 0, (byte)'I', (byte)'E', (byte)'N', (byte)'D', 0xAE, 0x42, 0x60, 0x82 };
            output.Write(iend, 0, iend.Length);

            return output.ToArray();
        }

        private static bool ValidateZlibStream(byte[] compressed, long maxDecompressed)
        {
            if (compressed.Length < 2)
                return false;

            try {
                using MemoryStream source = new(compressed);
                using ZLibStream inflater = new(source, CompressionMode.Decompress);

                byte[] scratch = new byte[8192];
                long total = 0;
                int read;
                while ((read = inflater.Read(scratch, 0, scratch.Length)) > 0) {
                    total += read;
                    if (total > maxDecompressed)
                        return false;
                }

                return total > 0;
            } catch (Exception e) {
                Console.Write(e.ToString());
                return false;
            }
        }
    }
}