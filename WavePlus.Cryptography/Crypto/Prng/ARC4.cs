using DotNetty.Buffers;

namespace Plus.Communication.Encryption.Crypto.Prng
{
    public class ARC4
    {
        private int i;
        private int j;
        private readonly byte[] bytes;

        public const int PoolSize = 256;

        public ARC4()
        {
            bytes = new byte[PoolSize];
        }

        public ARC4(byte[] key)
        {
            bytes = new byte[PoolSize];
            Initialize(key);
        }

        public void Initialize(byte[] key)
        {
            i = 0;
            j = 0;

            for (i = 0; i < PoolSize; ++i) {
                bytes[i] = (byte)i;
            }

            for (i = 0; i < PoolSize; ++i) {
                j = (j + bytes[i] + key[i % key.Length]) & (PoolSize - 1);
                Swap(i, j);
            }

            i = 0;
            j = 0;
        }

        private void Swap(int a, int b)
        {
            (bytes[a], bytes[b]) = (bytes[b], bytes[a]);
        }

        public byte Next()
        {
            i = ++i & (PoolSize - 1);
            j = (j + bytes[i]) & (PoolSize - 1);
            Swap(i, j);
            return bytes[(bytes[i] + bytes[j]) & 255];
        }

        public void Encrypt(ref byte[] src)
        {
            for (int k = 0; k < src.Length; k++) {
                src[k] ^= Next();
            }
        }

        public void Decrypt(ref byte[] src)
        {
            Encrypt(ref src);
        }

        public IByteBuffer Decipher(IByteBuffer buffer)
        {
            IByteBuffer result = Unpooled.Buffer();

            while (buffer.IsReadable())
                result.WriteByte((byte)(buffer.ReadByte() ^ Next()));

            return result;
        }
    }
}