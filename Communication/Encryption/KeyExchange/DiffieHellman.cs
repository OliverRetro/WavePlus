using System;
using Plus.Utilities;

namespace Plus.Communication.Encryption.KeyExchange
{
    public class DiffieHellman
    {
        public readonly int BitLength = 32;
        public BigInteger Prime { get; private set; }
        public BigInteger Generator { get; private set; }

        private BigInteger PrivateKey;
        public BigInteger PublicKey { get; private set; }

        public DiffieHellman()
        {
            Initialize();
        }

        public DiffieHellman(int b)
        {
            BitLength = b;

            Initialize();
        }

        public DiffieHellman(BigInteger prime, BigInteger generator)
        {
            Prime = prime;
            Generator = generator;

            Initialize(true);
        }

        private void Initialize(bool ignoreBaseKeys = false)
        {
            PublicKey = 0;

            Random rand = new();
            while (PublicKey == 0)
            {
                if (!ignoreBaseKeys)
                {
                    Prime = BigInteger.genPseudoPrime(BitLength, 10, rand);
                    Generator = BigInteger.genPseudoPrime(BitLength, 10, rand);
                }

                byte[] bytes = new byte[BitLength / 8];
                Randomizer.NextBytes(bytes);
                PrivateKey = new BigInteger(bytes);

                if (Generator > Prime)
                {
                    (Prime, Generator) = (Generator, Prime);
                }

                PublicKey = Generator.modPow(PrivateKey, Prime);

                if (!ignoreBaseKeys)
                {
                    break;
                }
            }
        }

        public BigInteger CalculateSharedKey(BigInteger m)
        {
            return m.modPow(PrivateKey, Prime);
        }
    }
}