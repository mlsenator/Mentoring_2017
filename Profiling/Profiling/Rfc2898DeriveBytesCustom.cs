using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Profiling
{
    class Rfc2898DeriveBytesCustom : Rfc2898DeriveBytes
    {
        private byte[] m_salt;
        private uint m_iterations;
        private int m_startIndex;
        private int m_endIndex;
        private byte[] m_buffer;
        private HMACSHA1 m_hmacsha1;
        private byte[] m_password;

        public byte[] Salt
        {
            get
            {
                return (byte[])this.m_salt.Clone();
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                if (value.Length < 8)
                    throw new ArgumentException();
                this.m_salt = (byte[])value.Clone();
                this.Initialize();
            }
        }

        public Rfc2898DeriveBytesCustom(string password, byte[] salt, int iterations)
            : base(password, salt, iterations)
        {
            this.Salt = salt;
            this.m_password = new UTF8Encoding(false).GetBytes(password);
            this.m_hmacsha1 = new HMACSHA1(this.m_password);
            this.m_iterations = (uint)iterations;
            this.Initialize();
        }

        public override byte[] GetBytes(int cb)
        {

            if (cb <= 0)
                throw new ArgumentOutOfRangeException("cb");
            byte[] numArray1 = new byte[cb];
            int dstOffsetBytes = 0;
            int byteCount = this.m_endIndex - this.m_startIndex;
            if (byteCount > 0)
            {
                if (cb >= byteCount)
                {
                    Buffer.BlockCopy((Array)this.m_buffer, this.m_startIndex, (Array)numArray1, 0, byteCount);
                    this.m_startIndex = this.m_endIndex = 0;
                    dstOffsetBytes += byteCount;
                }
                else
                {
                    Buffer.BlockCopy((Array)this.m_buffer, this.m_startIndex, (Array)numArray1, 0, cb);
                    this.m_startIndex = this.m_startIndex + cb;
                    return numArray1;
                }
            }

            byte[] numArray2 = this.Func();
            while (dstOffsetBytes < cb)
            {
                int num1 = cb - dstOffsetBytes;
                if (num1 > 20)
                {
                    Buffer.BlockCopy((Array)numArray2, 0, (Array)numArray1, dstOffsetBytes, 20);
                    dstOffsetBytes += 20;
                }
                else
                {
                    Buffer.BlockCopy((Array)numArray2, 0, (Array)numArray1, dstOffsetBytes, num1);
                    int num2 = dstOffsetBytes + num1;
                    Buffer.BlockCopy((Array)numArray2, num1, (Array)this.m_buffer, this.m_startIndex, 20 - num1);
                    this.m_endIndex = this.m_endIndex + (20 - num1);
                    return numArray1;
                }
            }
            return numArray1;
        }

        private byte[] Func()
        {
            //byte[] array = new byte[] { (byte)(this.m_block >> 24), (byte)(this.m_block >> 16), (byte)(this.m_block >> 8), (byte)this.m_block };
            this.m_hmacsha1.TransformBlock(this.m_salt, 0, this.m_salt.Length, null, 0);
            //this.m_hmacsha1.TransformBlock(array, 0, array.Length, null, 0);
            this.m_hmacsha1.TransformFinalBlock(new byte[0], 0, 0);
            byte[] hashValue = this.m_hmacsha1.Hash;
            this.m_hmacsha1.Initialize();
            var array2 = new byte[20];
            Buffer.BlockCopy((Array)hashValue, 0, (Array)array2, 0, 20);

            int num = 2;
            var seed = BitConverter.ToInt32(hashValue, 0);
            var rnd = new Random(seed);
            while ((long)num <= (long)((ulong)this.m_iterations))
            {
                //this.m_hmacsha1.TransformBlock(hashValue, 0, hashValue.Length, null, 0);
                //this.m_hmacsha1.TransformFinalBlock(new byte[0], 0, 0);
                //hashValue = this.m_hmacsha1.Hash;
                rnd.NextBytes(hashValue);
                for (int i = 0; i < 20; i++)
                {
                    byte[] expr_AB_cp_0 = array2;
                    int expr_AB_cp_1 = i;
                    expr_AB_cp_0[expr_AB_cp_1] ^= hashValue[i];
                }
                this.m_hmacsha1.Initialize();
                num++;
            }
            //this.m_block += 1u;
            return array2;
        }

        private void Initialize()
        {
            if (this.m_buffer != null)
                Array.Clear((Array)this.m_buffer, 0, this.m_buffer.Length);
            this.m_buffer = new byte[20];
            this.m_startIndex = this.m_endIndex = 0;
            //this.m_block = 1u;
        }
    }
}
