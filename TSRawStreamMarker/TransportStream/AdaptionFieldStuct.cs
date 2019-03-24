using System;

namespace TSRawStreamMarker.TransportStream
{
    /// <summary>
    /// Adaptation field format
    /// </summary>
    public class AdaptionFieldStuct
    {
        /// <summary>
        /// Adaptation Field Length	
        /// <para>Number of bytes in the adaptation field immediately following this byte</para>
        /// </summary>
        public int FieldLength { get; set; } // 8 bits

        /// <summary>
        /// Discontinuity indicator
        /// <para>Set if current TS packet is in a discontinuity state with respect to either the continuity counter or the program clock reference</para>
        /// </summary>
        public bool IsDiscontinue { get; set; } // 1 bits

        /// <summary>
        /// Random Access indicator
        /// <para>Set when the stream may be decoded without errors from this point</para>
        /// </summary>
        public bool IsRandomAccess { get; set; } //1 bits

        /// <summary>
        /// Elementary stream priority indicator
        /// <para>Set when this stream should be considered "high priority"</para>
        /// </summary>
        public bool IsHighPriority { get; set; } //1 bits

        /// <summary>
        /// Set when PCR field is present
        /// </summary>
        public bool HasPCR { get; set; } //1 bits

        /// <summary>
        /// Set when OPCR field is present
        /// </summary>
        public bool HasOPCR { get; set; }//1 bits

        /// <summary>
        /// Set when splice countdown field is present
        /// </summary>
        public bool HasSplicingPoint { get; set; } //1 bits

        /// <summary>
        /// Transport private data flag
        /// <para>Set when private data field is present</para>
        /// </summary>
        public bool HasPrivateData { get; set; } //1 bits

        /// <summary>
        /// Set when extension field is present
        /// </summary>
        public bool HasExtension { get; set; } //1 bits

        //Optional fields

        /// <summary>
        /// Program clock reference, stored as 33 bits base, 6 bits reserved, 9 bits extension.
        /// </summary>
        public PCR MainPCR { get; set; } //48 bits.

        /// <summary>
        /// Original Program clock reference. Helps when one TS is copied into another
        /// </summary>
        public PCR OriginalPCR { get; set; }// 48 bits.

        /// <summary>
        /// Indicates how many TS packets from this one a splicing point occurs (Two's complement signed; may be negative)
        /// </summary>
        public byte SpliceCountdown { get; set; } // 8 bits.

        /// <summary>
        /// The length of the following field
        /// </summary>
        public byte PrivateDataLength { get; set; } // 8 bits.

        /// <summary>
        /// Transport private data
        /// </summary>
        public object PrivateData { get; set; }//variable

        /// <summary>
        /// Adaptation extension	
        /// </summary>
        public AdaptationExtension AdaptationExtension { get; set; }//variable

        public const byte StuffingByte = 0xff;

        internal AdaptionFieldStuct() { } //Leave empty for futher use.

        public AdaptionFieldStuct(byte[] data)
        {
            var packet = new BitPacket(data);
            //Adaptation Field Length	
            this.FieldLength = packet.ReadInt(8);
            //Discontinuity indicator
            this.IsDiscontinue = packet.ReadBool();
            // Random Access indicator
            this.IsRandomAccess = packet.ReadBool();
            // Elementary stream priority indicator
            this.IsHighPriority = packet.ReadBool();
            // PCR flag
            this.HasPCR = packet.ReadBool();
            // OPCR flag 
            this.HasOPCR = packet.ReadBool();
            //Splicing point flag
            this.HasSplicingPoint = packet.ReadBool();
            //Transport private data flag
            this.HasPrivateData = packet.ReadBool();
            //Adaptation field extension flag
            this.HasExtension = packet.ReadBool();
            //Option field:
            var counter = 0;
            if (this.HasPCR)
            {
                this.MainPCR = new PCR(packet);
                counter += 6;
            }
            
            if (this.HasOPCR)
            {
                this.OriginalPCR = new PCR(packet);
                counter += 6;
            }

            if (this.HasSplicingPoint)
            {
                this.SpliceCountdown = packet.ReadByte();
                counter += 1;
            }

            if (this.HasPrivateData)
            {
                this.PrivateDataLength = packet.ReadByte();
                counter += 1;
                this.PrivateData = packet.ReadBlock(this.PrivateDataLength);
            }

            if (this.HasExtension)
                this.AdaptationExtension = new AdaptationExtension(packet);
            
            //the counter is in byte but data length is in what ?
            //the stuffing byes position must be in here.
        }

        public AdaptionFieldStuct(BitPacket packet)
        {
            //Adaptation Field Length	
            this.FieldLength = packet.ReadInt(8);
            if (this.FieldLength <= 0) return;
            //Discontinuity indicator
            this.IsDiscontinue = packet.ReadBool();
            // Random Access indicator
            this.IsRandomAccess = packet.ReadBool();
            // Elementary stream priority indicator
            this.IsHighPriority = packet.ReadBool();
            // PCR flag
            this.HasPCR = packet.ReadBool();
            // OPCR flag 
            this.HasOPCR = packet.ReadBool();
            //Splicing point flag
            this.HasSplicingPoint = packet.ReadBool();
            //Transport private data flag
            this.HasPrivateData = packet.ReadBool();
            //Adaptation field extension flag
            this.HasExtension = packet.ReadBool();
            //Option field:
            var counter = 0;
            if (this.HasPCR)
            {
                this.MainPCR = new PCR(packet);
                counter += 6;
            }

            if (this.HasOPCR)
            {
                this.OriginalPCR = new PCR(packet);
                counter += 6;
            }

            if (this.HasSplicingPoint)
            {
                this.SpliceCountdown = packet.ReadByte();
                counter += 1;
            }

            if (this.HasPrivateData)
            {
                this.PrivateDataLength = packet.ReadByte();
                counter += 1;
                if (PrivateDataLength > 184) Console.WriteLine("Private Data Length is too long.");
                else this.PrivateData = packet.ReadBlock(this.PrivateDataLength*8);
            }

            if (this.HasExtension)
                this.AdaptationExtension = new AdaptationExtension(packet);

            //the counter is in byte but data length is in what ?
            //the stuffing byes position must be in here.
        }
    }

    /// <summary>
    /// Program clock reference (PCR) structure. In total 48 bits (6 byte).
    /// <para>The first 33 bits are based on a 90 kHz clock. The last 9 are based on a 27 MHz clock. The maximum jitter permitted for the PCR is +/- 500 ns.</para>
    /// </summary>
    public struct PCR
    {
        /// <summary>
        /// Base clk ,33 bits based on a 90 kHz clock
        /// </summary>
        public long CLK_Base { get; set; }
        public byte Reserved { get; set; }
        /// <summary>
        /// Extension clk, 9bits based on a 27 MHz clock
        /// </summary>
        public int Extension { get; set; }
        public long GetValue()
        {
            return this.CLK_Base * 300 + Extension;
        }
        public PCR(BitPacket packet)
        {
            this.CLK_Base = packet.ReadLong(33);
            this.Reserved = (byte)packet.ReadInt(6);
            this.Extension = packet.ReadInt(9);
        }
    }

    public struct AdaptationExtension
    {
        /// <summary>
        /// The length of the header
        /// </summary>
        public byte Length { get; set; } //8 bits.

        /// <summary>
        /// Legal time window (LTW) flag
        /// </summary>
        public bool LTWFlag { get; set; } // 1 bits.

        /// <summary>
        /// Piecewise rate flag	
        /// </summary>
        public bool PicewiseRateFlag { get; set; } //1 bits.

        /// <summary>
        /// Seamless splice flag
        /// </summary>
        public bool SeamlessSpliceFlag { get; set; }

        public object Reserved { get; set; } // 5 bits

        //Optional 
        //LTW flag set:
        /// <summary>
        /// Legal time window valid flag
        /// </summary>
        public bool LTWValidFlag { get; set; }

        /// <summary>
        /// Legal time window offset
        /// <para>Extra information for rebroadcasters to determine the state of buffers when packets may be missing.</para>
        /// </summary>
        public int LTWOffset { get; set; } // 15 bits.

        //Piecewise flag set
        public object _Reserved { get; set; } //2 bits.
        /// <summary>
        /// The rate of the stream, measured in 188-byte packets, to define the end-time of the LTW.
        /// </summary>
        public int PicewiseRate { get; set; }

        //Seamless splice flag set
        /// <summary>
        /// Splice type
        /// <para>	Indicates the parameters of the H.262 splice.</para>
        /// </summary>
        public byte SpliceType { get; set; }

        /// <summary>
        /// DTS next access unit
        /// <para>The PES DTS of the splice point.</para>
        /// <para>Split up as 3 bits, 1 marker bit(0x1), 15 bits, 1 marker bit, 15 bits, and 1 marker bit, for 33 data bits total.</para>
        /// </summary>
        public object DTSNextAccessUnit { get; set; }

        public AdaptationExtension(BitPacket packet)
        {
            this.Length = packet.ReadByte();

            this.LTWFlag = packet.ReadBool();

            this.PicewiseRateFlag = packet.ReadBool();

            this.SeamlessSpliceFlag = packet.ReadBool();

            this.Reserved = packet.ReadByte(5);

            if (this.LTWFlag)
            {
                this.LTWValidFlag = packet.ReadBool();
                this.LTWOffset = packet.ReadInt(15);
            }
            else
            {
                this.LTWValidFlag = false;
                this.LTWOffset = 0;
            }

            if (this.PicewiseRateFlag)
            {
                this._Reserved = packet.ReadByte(2);
                this.PicewiseRate = packet.ReadInt(22);
            }
            else
            {
                this._Reserved = 0;
                this.PicewiseRate = 0;
            }

            if (this.SeamlessSpliceFlag)
            {
                this.SpliceType = packet.ReadByte(4);
                this.DTSNextAccessUnit = packet.ReadBlock(36);
            }
            else
            {
                this.SpliceType = 0;
                this.DTSNextAccessUnit = null;
            }
        }

        /// <summary>
        /// Re calculate the length of this header.
        /// </summary>
        public void SetLength()
        {
            byte len = 16;
            if (this.LTWFlag) len += 16;
            if (this.PicewiseRateFlag) len += 24;
            if (this.SeamlessSpliceFlag) len += 40;
            this.Length = len;
        }
    }
}
