using System;
using System.Collections;
using System.Collections.Generic;

namespace TSRawStreamMarker.TransportStream
{
    /// <summary>
    /// A helper class for raw data reading.
    /// </summary>
    public class BitPacket
    {
        public List<byte> Data { get; private set; }
        //public byte[] Data { get; private set; }
        //private List<bool> bits;

        /// <summary>
        /// Gets or Sets the last position of operations in secqueens of bits.
        /// </summary>
        public int Position { get; set; }
        public int Length
        {
            get
            {
                return Data.Count * 8;
            }
        }

        public bool this[int indexer]
        {
            get
            {
                int byteIndex = indexer / 8;
                int bitIndex = (indexer + 1) % 8;
                var shiftCount = 8 - ( bitIndex ==0 ? 8 : bitIndex);
                var result = ((this.Data[byteIndex] >> shiftCount) & 1) != 0;
                return result;
            }
            set
            {
                int byteIndex = indexer / 8;
                int bitIndex = (indexer + 1) % 8;
                var shiftCount = 8 - (bitIndex == 0 ? 8 : bitIndex);
                if (value)
                    this.Data[byteIndex] |= (byte)(1 << shiftCount);
                else
                    this.Data[byteIndex] &= (byte)~(1 << shiftCount);
            }
        }

        /// <summary>
        /// Add a new bit.
        /// <para>*Caution: Calling this method will add an extra byte to the end of the byte array.</para>
        /// </summary>
        private void Add(bool value)=>        
            this.Data.Add((byte)((value ? 1 : 0) << 7));
        

        public BitPacket()
        {
            //bits = new List<bool>();
        }
        public BitPacket(byte[] Data)
        {
            this.Data = new List<byte>(Data);
            //this.Data = Data;
            //this.bits = new List<bool>();
            //foreach (byte i in Data)
            //{
            //    for (int c = 7; c >= 0; c--)
            //    {
            //        var tmp = (i & (1 << c)) >> c == 1;
            //        bits.Add(tmp);
            //    }
            //}
            Position = 0;
        }
        //public void GenerateData()=>this.Data = this.ToByteArray();
        public long ReadLong(int bitlength, bool bigEndian = false)
        {
            if (bitlength > 64) throw new InvalidOperationException("A long type number cannot be longer then 64 bits");
            if (this.Length < (Position + bitlength))
                throw new InvalidOperationException("Requested bit length is out of range.");
            long result = 0;
            if (bigEndian)
            {
                for (int i = bitlength; i > 0; i--)
                {
                    result = (result << 1) + (this[Position + i] ? 1 : 0);
                }
            }
            else
            {
                for (int i = 0; i < bitlength; i++)
                {
                    result = (result << 1) + (this[Position + i] ? 1 : 0);
                }
            }
            Position += bitlength;
            return result;
        }

        public ulong ReadULong(int bitlength,bool bigEndian=false)
        {
            if (bitlength > 64) throw new InvalidOperationException("An ulong type number cannot be longer then 64 bits");
            if (this.Length < (Position + bitlength))
                throw new InvalidOperationException("Requested bit length is out of range.");
            long result = 0;
            if (bigEndian)
            {
                for (int i = bitlength; i >0 ; i--)
                {
                    result = (result << 1) + (this[Position + i] ? 1 : 0);
                }
            }
            else
            {
                for (int i = 0; i < bitlength; i++)
                {
                    result = (result << 1) + (this[Position + i] ? 1 : 0);
                }
            }
            Position += bitlength;
            return (ulong)result;
        }

        public int ReadInt(int bitlength, bool bigEndian = false)
        {
            if (bitlength > 32) throw new InvalidOperationException("An int type number cannot be longer then 32 bits");
            if (this.Length < (Position + bitlength))
                throw new InvalidOperationException("Requested bit length is out of range.");
            int result = 0;
            if (bigEndian)
            {
                for (int i = bitlength-1; i >= 0; i--)
                {
                    result = (result << 1) + (this[Position + i] ? 1 : 0);
                }
            }
            else
            {
                for (int i = 0; i < bitlength; i++)
                {
                    result = (result << 1) + (this[Position + i] ? 1 : 0);
                }
            }
            Position += bitlength;
            return result;
        }

        public byte ReadByte(int bitlength = 8)
        {
            if (bitlength > 8) throw new InvalidOperationException("An byte type number cannot be longer then 8 bits");
            if (this.Length < (Position + bitlength))
                throw new InvalidOperationException("Requested bit length is out of range.");
            long counter = 0;
            byte tmp = new byte();
            while (counter < bitlength)
            {
                tmp = (byte)((tmp << 1) + (this[Position] ? 1 : 0));
                Position += 1;
                counter += 1;
            }
            return tmp;
        }

        public uint ReadUInt(int bitlength, bool bigEndian = false)
        {
            if (bitlength > 32) throw new InvalidOperationException("An uint type number cannot be longer then 32 bits");
            if (this.Length < (Position + bitlength))
                throw new InvalidOperationException("Requested bit length is out of range.");
            int result = 0;
            if (bigEndian)
            {
                for (int i = bitlength; i > 0; i--)
                {
                    result = (result << 1) + (this[Position + i] ? 1 : 0);
                }
            }
            else
            {
                for (int i = 0; i < bitlength; i++)
                {
                    result = (result << 1) + (this[Position + i] ? 1 : 0);
                }
            }
            Position += bitlength;
            return (uint)result;
        }

        public bool ReadBool()
        {
            bool result = this[Position];
            Position += 1;
            return result;
        }
        /// <summary>
        /// This may cause some problem because the last 1-7 bits will fill in to a full byte.
        /// </summary>
        /// <param name="bitlength"></param>
        /// <returns></returns>
        public byte[] ReadBlock(long bitlength)
        {
            if (this.Length < (Position + bitlength - 1))
                throw new InvalidOperationException("Requested bit length is out of range.");
            List<byte> result = new List<byte>();
            long counter = 0;
            byte tmp = new byte();
            while (counter < bitlength)
            {
                tmp = (byte)((tmp << 1) + (this[Position] ? 1 : 0));
                Position += 1;
                counter += 1;
                if ((counter) % 8 == 0 || counter == bitlength)
                {
                    result.Add(tmp);
                    tmp = new byte();
                }
            }
            return result.ToArray();
        }

        public long ReadLong(int startPos,int bitlength, bool bigEndian = false)
        {
            if (bitlength > 64) throw new InvalidOperationException("A long type number cannot be longer then 64 bits");
            if (this.Length < (startPos + bitlength))
                throw new InvalidOperationException("Requested bit length is out of range.");
            long result = 0;
            if (bigEndian)
            {
                for (int i = bitlength; i > 0; i--)
                {
                    result = (result << 1) + (this[startPos + i] ? 1 : 0);
                }
            }
            else
            {
                for (int i = 0; i < bitlength; i++)
                {
                    result = (result << 1) + (this[startPos + i] ? 1 : 0);
                }
            }
            startPos += bitlength;
            return result;
        }

        public ulong ReadULong(int startPos,int bitlength, bool bigEndian = false)
        {
            if (bitlength > 64) throw new InvalidOperationException("An ulong type number cannot be longer then 64 bits");
            if (this.Length < (startPos + bitlength))
                throw new InvalidOperationException("Requested bit length is out of range.");
            long result = 0;
            if (bigEndian)
            {
                for (int i = bitlength; i > 0; i--)
                {
                    result = (result << 1) + (this[startPos + i] ? 1 : 0);
                }
            }
            else
            {
                for (int i = 0; i < bitlength; i++)
                {
                    result = (result << 1) + (this[startPos + i] ? 1 : 0);
                }
            }
            startPos += bitlength;
            return (ulong)result;
        }

        public int ReadInt(int startPos,int bitlength, bool bigEndian = false)
        {
            if (bitlength > 32) throw new InvalidOperationException("An int type number cannot be longer then 32 bits");
            if (this.Length < (startPos + bitlength))
                throw new InvalidOperationException("Requested bit length is out of range.");
            int result = 0;
            if (bigEndian)
            {
                for (int i = bitlength - 1; i >= 0; i--)
                {
                    result = (result << 1) + (this[startPos + i] ? 1 : 0);
                }
            }
            else
            {
                for (int i = 0; i < bitlength; i++)
                {
                    result = (result << 1) + (this[startPos + i] ? 1 : 0);
                }
            }
            startPos += bitlength;
            return result;
        }

        public byte ReadByte(int startPos,int bitlength = 8)
        {
            if (bitlength > 8) throw new InvalidOperationException("An byte type number cannot be longer then 8 bits");
            if (this.Length < (startPos + bitlength))
                throw new InvalidOperationException("Requested bit length is out of range.");
            long counter = 0;
            byte tmp = new byte();
            while (counter < bitlength)
            {
                tmp = (byte)((tmp << 1) + (this[startPos] ? 1 : 0));
                Position += 1;
                counter += 1;
            }
            return tmp;
        }

        public uint ReadUInt(int startPos,int bitlength, bool bigEndian = false)
        {
            if (bitlength > 32) throw new InvalidOperationException("An uint type number cannot be longer then 32 bits");
            if (this.Length < (startPos + bitlength))
                throw new InvalidOperationException("Requested bit length is out of range.");
            int result = 0;
            if (bigEndian)
            {
                for (int i = bitlength; i > 0; i--)
                {
                    result = (result << 1) + (this[startPos + i] ? 1 : 0);
                }
            }
            else
            {
                for (int i = 0; i < bitlength; i++)
                {
                    result = (result << 1) + (this[startPos + i] ? 1 : 0);
                }
            }
            startPos += bitlength;
            return (uint)result;
        }

        public bool ReadBool(int startPos)
        {
            bool result = this[startPos];
            Position += 1;
            return result;
        }
        /// <summary>
        /// This may cause some problem because the last 1-7 bits will fill in to a full byte.
        /// </summary>
        /// <param name="bitlength"></param>
        /// <returns></returns>
        public byte[] ReadBlock(int startPos,long bitlength)
        {
            if (this.Length < (startPos + bitlength - 1))
                throw new InvalidOperationException("Requested bit length is out of range.");
            List<byte> result = new List<byte>();
            long counter = 0;
            byte tmp = new byte();
            while (counter < bitlength)
            {
                tmp = (byte)((tmp << 1) + (this[startPos] ? 1 : 0));
                startPos += 1;
                counter += 1;
                if ((counter) % 8 == 0 || counter == bitlength)
                {
                    result.Add(tmp);
                    tmp = new byte();
                }
            }
            return result.ToArray();
        }
        public string ReadString(int startPos, int bitlength, TextEncoding enc = TextEncoding.Unicode)
        {
            if (this.Length < (startPos + bitlength))
                throw new InvalidOperationException("Requested bit length is out of range.");
            string result = "";
            BitArray myBits = new BitArray(bitlength);
            for (int i = 0; i < bitlength; i++)
            {
                myBits.Set(i, this[startPos + i]);
            }
            byte[] buffer = BitArrayToByteArray(myBits);
            if (enc.HasFlag(TextEncoding.Unicode) && enc.HasFlag(TextEncoding.BigEndian))
            {
                result = System.Text.Encoding.BigEndianUnicode.GetString(buffer);
            }
            else if (enc.HasFlag(TextEncoding.Unicode))
            {
                result = System.Text.Encoding.Unicode.GetString(buffer);
            }
            else if (enc.HasFlag(TextEncoding.UTF32))
            {
                result = System.Text.Encoding.UTF32.GetString(buffer);
            }
            else if (enc.HasFlag(TextEncoding.ASCII))
            {
                result = System.Text.Encoding.ASCII.GetString(buffer);
            }
            else if (enc.HasFlag(TextEncoding.UTF7))
            {
                result = System.Text.Encoding.UTF7.GetString(buffer);
            }
            else if (enc.HasFlag(TextEncoding.UTF8))
            {
                result = System.Text.Encoding.UTF8.GetString(buffer);
            }
            startPos += bitlength;
            return result;
        }


        public string ReadString(int bitlength, TextEncoding enc = TextEncoding.Unicode)
        {
            if (this.Length < (Position + bitlength))
                throw new InvalidOperationException("Requested bit length is out of range.");
            string result = "";
            BitArray myBits = new BitArray(bitlength);
            for (int i = 0; i < bitlength; i++)
            {
                myBits.Set(i, this[Position + i]);
            }
            byte[] buffer = BitArrayToByteArray(myBits);
            if (enc.HasFlag(TextEncoding.Unicode) && enc.HasFlag(TextEncoding.BigEndian))
            {
                result = System.Text.Encoding.BigEndianUnicode.GetString(buffer);
            }
            else if (enc.HasFlag(TextEncoding.Unicode))
            {
                result = System.Text.Encoding.Unicode.GetString(buffer);
            }
            else if (enc.HasFlag(TextEncoding.UTF32))
            {
                result = System.Text.Encoding.UTF32.GetString(buffer);
            }
            else if (enc.HasFlag(TextEncoding.ASCII))
            {
                result = System.Text.Encoding.ASCII.GetString(buffer);
            }
            else if (enc.HasFlag(TextEncoding.UTF7))
            {
                result = System.Text.Encoding.UTF7.GetString(buffer);
            }
            else if (enc.HasFlag(TextEncoding.UTF8))
            {
                result = System.Text.Encoding.UTF8.GetString(buffer);
            }
            Position += bitlength;
            return result;
        }

        [Obsolete]
        private byte[] BitArrayToByteArray(BitArray array)
        {
            var Bytes = new List<byte> { };
            for (int i = 0; i < array.Count; i += 8)
            {
                int OneByte = 0;
                for (int x = 0; x < 8; x++)
                {
                    try
                    { OneByte = OneByte << 1 + (array.Get(i * 8 + x) ? 1 : 0); }
                    catch (IndexOutOfRangeException e)
                    { break; }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                        return null;
                    }
                }
                Bytes.Add((byte)OneByte);
            }
            return Bytes.ToArray();
        }

        public byte[] ToByteArray()
        {
            //List<byte> result = new List<byte>();
            //int counter = 0;
            //byte tmp = new byte();
            //while (counter < this.Length)
            //{
            //    tmp = (byte)((tmp << 1) + (this[counter] ? 1 : 0));
            //    counter += 1;
            //    if ((counter) % 8 == 0 || counter == this.Length)
            //    {
            //        result.Add(tmp);
            //        tmp = new byte();
            //    }
            //}
            //return result.ToArray();
            return this.Data.ToArray();
        }
        
        /// <summary>
        /// Move <see cref="Position"/> to skip a number of bits, it will skip 1 bit as default.
        /// <para>*Notice: If current position is at the end of the bit stream, This action
        /// will add extra bit '0' to the end of the stream.</para>
        /// </summary>
        public void SkipBit(int bitlength = 1)
        {
            this.Position += bitlength;
            while(this.Position >= this.Length)
            {
                this.Add(false);
            }
        }
        /// <summary>
        /// Move <see cref="Position"/> to rewind a number of bits, it will skip 1 bit as default.
        /// </summary>
        public void RewindBit(int bitlenth = 1)
        {
            if (this.Position - bitlenth < 0) throw new InvalidOperationException("Reached start position!"); 
            this.Position -= bitlenth;
        }
        /// <summary>
        /// A bitwise reverse taable
        /// <para>https://stackoverflow.com/a/3590938/10732667</para>
        /// </summary>
        private static byte[] BitReverseTable =
        {
            0x00, 0x80, 0x40, 0xc0, 0x20, 0xa0, 0x60, 0xe0,
            0x10, 0x90, 0x50, 0xd0, 0x30, 0xb0, 0x70, 0xf0,
            0x08, 0x88, 0x48, 0xc8, 0x28, 0xa8, 0x68, 0xe8,
            0x18, 0x98, 0x58, 0xd8, 0x38, 0xb8, 0x78, 0xf8,
            0x04, 0x84, 0x44, 0xc4, 0x24, 0xa4, 0x64, 0xe4,
            0x14, 0x94, 0x54, 0xd4, 0x34, 0xb4, 0x74, 0xf4,
            0x0c, 0x8c, 0x4c, 0xcc, 0x2c, 0xac, 0x6c, 0xec,
            0x1c, 0x9c, 0x5c, 0xdc, 0x3c, 0xbc, 0x7c, 0xfc,
            0x02, 0x82, 0x42, 0xc2, 0x22, 0xa2, 0x62, 0xe2,
            0x12, 0x92, 0x52, 0xd2, 0x32, 0xb2, 0x72, 0xf2,
            0x0a, 0x8a, 0x4a, 0xca, 0x2a, 0xaa, 0x6a, 0xea,
            0x1a, 0x9a, 0x5a, 0xda, 0x3a, 0xba, 0x7a, 0xfa,
            0x06, 0x86, 0x46, 0xc6, 0x26, 0xa6, 0x66, 0xe6,
            0x16, 0x96, 0x56, 0xd6, 0x36, 0xb6, 0x76, 0xf6,
            0x0e, 0x8e, 0x4e, 0xce, 0x2e, 0xae, 0x6e, 0xee,
            0x1e, 0x9e, 0x5e, 0xde, 0x3e, 0xbe, 0x7e, 0xfe,
            0x01, 0x81, 0x41, 0xc1, 0x21, 0xa1, 0x61, 0xe1,
            0x11, 0x91, 0x51, 0xd1, 0x31, 0xb1, 0x71, 0xf1,
            0x09, 0x89, 0x49, 0xc9, 0x29, 0xa9, 0x69, 0xe9,
            0x19, 0x99, 0x59, 0xd9, 0x39, 0xb9, 0x79, 0xf9,
            0x05, 0x85, 0x45, 0xc5, 0x25, 0xa5, 0x65, 0xe5,
            0x15, 0x95, 0x55, 0xd5, 0x35, 0xb5, 0x75, 0xf5,
            0x0d, 0x8d, 0x4d, 0xcd, 0x2d, 0xad, 0x6d, 0xed,
            0x1d, 0x9d, 0x5d, 0xdd, 0x3d, 0xbd, 0x7d, 0xfd,
            0x03, 0x83, 0x43, 0xc3, 0x23, 0xa3, 0x63, 0xe3,
            0x13, 0x93, 0x53, 0xd3, 0x33, 0xb3, 0x73, 0xf3,
            0x0b, 0x8b, 0x4b, 0xcb, 0x2b, 0xab, 0x6b, 0xeb,
            0x1b, 0x9b, 0x5b, 0xdb, 0x3b, 0xbb, 0x7b, 0xfb,
            0x07, 0x87, 0x47, 0xc7, 0x27, 0xa7, 0x67, 0xe7,
            0x17, 0x97, 0x57, 0xd7, 0x37, 0xb7, 0x77, 0xf7,
            0x0f, 0x8f, 0x4f, 0xcf, 0x2f, 0xaf, 0x6f, 0xef,
            0x1f, 0x9f, 0x5f, 0xdf, 0x3f, 0xbf, 0x7f, 0xff
        };

        /// <summary>
        /// A bitwise reverse function
        /// <para>https://stackoverflow.com/a/3590938/10732667</para>
        /// </summary>
        private static byte ReverseWithLookupTable(byte toReverse)
        {
            return BitReverseTable[toReverse];
        }

        public void WriteInt(int input,int bitlength)
        {
            if (input <= (Math.Pow(2, bitlength) - 1))
            {
                for (int i = bitlength - 1; i >= 0; i--)
                {
                    var tmp = ((input >> i) & 1) != 0;
                    WriteBool(tmp);
                }
            }
            else
                throw new InvalidOperationException("Input integer is larger then input bit length can contain.");
        }
        public void WriteInt(uint input, int bitlength)
        {
            if (input <= (Math.Pow(2, bitlength) - 1))
            {
                for (int i = bitlength - 1; i >= 0; i--)
                {
                    var tmp = ((input >> i) & 1) != 0;
                    WriteBool(tmp);
                }
            }
            else
                throw new InvalidOperationException("Input integer is larger then input bit length can contain.");
        }
        public void WriteBool(bool value)
        {
            if (this.Position < this.Length)
            {
                this[Position] = value;
            }
            else
            {
                this.Add(value);
            }
            this.Position += 1;
        }

        public void WriteBlock(byte[] input,int bitlength)
        {
            if(Math.Ceiling((double)(bitlength/8)) < input.Length)
                throw new InvalidOperationException("Input integer is larger then input bit length can contain.");
            var counter = bitlength;
            foreach (var i in input)
            {
                WriteInt(i, (counter > 8 ? 8 : counter));
                counter -= 8;
            }
        }
        public void WriteByte(byte input, int bitlength = 8)=>
            WriteInt(input,bitlength);

        public void WriteInt(int input,int startPos, int bitlength)
        {
            if (input <= (Math.Pow(2, bitlength) - 1))
            {
                for (int i = bitlength - 1; i >= 0; i--)
                {
                    var tmp = ((input >> i) & 1) != 0;
                    WriteBool(tmp, startPos + (bitlength - i + 1));
                }
            }
            else
                throw new InvalidOperationException("Input integer is larger then input bit length can contain.");
        }
        public void WriteInt(uint input, int startPos, int bitlength)
        {
            if (input <= (Math.Pow(2, bitlength) - 1))
            {
                for (int i = bitlength - 1; i >= 0; i--)
                {
                    var tmp = ((input >> i) & 1) != 0;
                    WriteBool(tmp, startPos + (bitlength - i + 1));
                }
            }
            else
                throw new InvalidOperationException("Input integer is larger then input bit length can contain.");
        }
        public void WriteBool(bool value, int startPos)
        {
            if (startPos < this.Length)
            {
                this[startPos] = value;
            }
            else
            {
                this.Add(value);
            }
        }

        public void WriteBlock(byte[] input, int startPos, int bitlength)
        {
            if (Math.Ceiling((double)(bitlength / 8)) < input.Length)
                throw new InvalidOperationException("Input integer is larger then input bit length can contain.");
            var counter = bitlength;
            foreach (var i in input)
            {
                WriteInt(i, startPos + (bitlength - counter),(counter > 8 ? 8 : counter));
                counter -= 8;
            }
        }
        public void WriteByte(byte input, int startPos,int bitlength = 8) =>
            WriteInt(input,startPos, bitlength);

        public static explicit operator byte[](BitPacket packet)=>packet.ToByteArray();

        public static explicit operator BitPacket(byte[] packet)=> new BitPacket(packet);
    }
}
