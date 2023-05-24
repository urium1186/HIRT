/*
This code is public domain.
The MurmurHash3 algorithm was created by Austin Appleby and put into the public domain.  See http://code.google.com/p/smhasher/
This C# variant was authored by
Elliott B. Edwards and was placed into the public domain as a gist
Status...Working on verification (Test Suite)
Set up to run as a LinqPad (linqpad.net) script (thus the ".Dump()" call)
*/

namespace LibHIRT.Utils
{
    public static class MurMurHash3
    {
        //Change to suit your needs
        const uint seed = 0;

        public static int Hash(Stream stream)
        {
            const uint c1 = 0xcc9e2d51;
            const uint c2 = 0x1b873593;

            uint h1 = seed;
            uint k1 = 0;
            uint streamLength = 0;

            using (BinaryReader reader = new BinaryReader(stream))
            {
                byte[] chunk = reader.ReadBytes(4);
                while (chunk.Length > 0)
                {
                    streamLength += (uint)chunk.Length;
                    switch (chunk.Length)
                    {
                        case 4:
                            /* Get four bytes from the input into an uint */
                            k1 = (uint)
                               (chunk[0]
                              | chunk[1] << 8
                              | chunk[2] << 16
                              | chunk[3] << 24);

                            /* bitmagic hash */
                            k1 *= c1;
                            k1 = rotl32(k1, 15);
                            k1 *= c2;

                            h1 ^= k1;
                            h1 = rotl32(h1, 13);
                            h1 = h1 * 5 + 0xe6546b64;
                            break;
                        case 3:
                            k1 = (uint)
                               (chunk[0]
                              | chunk[1] << 8
                              | chunk[2] << 16);
                            k1 *= c1;
                            k1 = rotl32(k1, 15);
                            k1 *= c2;
                            h1 ^= k1;
                            break;
                        case 2:
                            k1 = (uint)
                               (chunk[0]
                              | chunk[1] << 8);
                            k1 *= c1;
                            k1 = rotl32(k1, 15);
                            k1 *= c2;
                            h1 ^= k1;
                            break;
                        case 1:
                            k1 = (uint)(chunk[0]);
                            k1 *= c1;
                            k1 = rotl32(k1, 15);
                            k1 *= c2;
                            h1 ^= k1;
                            break;

                    }
                    chunk = reader.ReadBytes(4);
                }
            }

            // finalization, magic chants to wrap it all up
            h1 ^= streamLength;
            h1 = fmix(h1);

            unchecked //ignore overflow
            {
                return (int)h1;
            }
        }

        private static uint rotl32(uint x, byte r)
        {
            return (x << r) | (x >> (32 - r));
        }

        private static uint fmix(uint h)
        {
            h ^= h >> 16;
            h *= 0x85ebca6b;
            h ^= h >> 13;
            h *= 0xc2b2ae35;
            h ^= h >> 16;
            return h;
        }

        public static uint invert_shift_xor(uint hs, int s)
        {
            if (!(s >= 8 && s <= 16))
                return 1;
            uint hs0 = hs >> 24;
            uint hs1 = (hs >> 16) & 0xff;
            uint hs2 = (hs >> 8) & 0xff;
            uint hs3 = hs & 0xff;

            uint h0 = hs0;
            uint h1 = hs1 ^ (h0 >> (s - 8));
            uint h2 = (hs2 ^ (h0 << (16 - s)) ^ (h1 >> (s - 8))) & 0xff;
            uint h3 = (hs3 ^ (h1 << (16 - s)) ^ (h2 >> (s - 8))) & 0xff;
            return (h0 << 24) + (h1 << 16) + (h2 << 8) + h3;
        }

        public static uint murmur_hash_inverse(uint h, uint seed)
        {
            const uint m = 0x5bd1e995;
            const uint minv = 0xe59b19bd;   // Multiplicative inverse of m under % 2^32
            const int r = 24;

            h = invert_shift_xor(h, 15);
            h *= minv;
            h = invert_shift_xor(h, 13);

            uint hforward = seed ^ 4;
            hforward *= m;
            uint k = hforward ^ h;
            k *= minv;
            k ^= k >> r;
            k *= minv;
            /*
                #ifdef PLATFORM_BIG_ENDIAN
                    char *data = (char *)&k;
                    k = (data[0]) + (data[1] << 8) + (data[2] << 16) + (data[3] << 24);
                #endif
            */
            return k;
        }

    }
}
