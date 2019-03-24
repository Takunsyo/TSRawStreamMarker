using System.Collections.Generic;
namespace TSRawStreamMarker.TransportStream.Packets
{
    /// <summary>
    /// Packetized elementary stream packet structure
    /// </summary>
    public class PESPacket
    {
        #region StreamID PreSet
        /// <summary>
        /// PES packets of type program_stream_map have unique syntax.
        /// </summary>
        internal const int _Program_Stream_Map_ID = 0b1011_1100;
        /// <summary>
        /// PES packets of type private_stream_1 and ISO/IEC_13552_stream followthe 
        /// same PES packet as those for ITU-T Rec. H.262|ISO/IEC 13818-2 video and 
        /// ISO/IEC 12818-3 audio streams.
        /// </summary>
        internal const int _Private_Stream_1_ID = 0b1011_1101;
        internal const int _Padding_Stream_ID = 0b1011_1110;
        internal const int _Private_Stream_2_ID = 0b1011_1111;

        internal const int _ECM_Stream_ID = 0b1111_0000;
        internal const int _EMM_Stream_ID = 0b1111_0001;
        /// <summary>
        /// ITU-T Rec. H.222.0 | ISO/IEC 13818-1 Annex A or ISO/IEC 13818-6_DSMCC_Stream.
        /// </summary>
        internal const int _DSMCC_Stream_ID = 0b1111_0010;
        internal const int _ISO_IEC_13522_Stream_ID = 0b1111_0011;
        internal const int _ITU_T_H222_A_ID = 0b1111_0100;
        internal const int _ITU_T_H222_B_ID = 0b1111_0101;
        internal const int _ITU_T_H222_C_ID = 0b1111_0110;
        internal const int _ITU_T_H222_D_D = 0b1111_0111;
        internal const int _ITU_T_H222_E_ID = 0b1111_1000;
        internal const int _Ancillary_Stream_ID = 0b1111_1001;
        internal const int _ISO_IEC14496_SL_Packetized_Stream_ID = 0b1111_1010;
        internal const int _ISO_IEC14496_FlexMux_Stream_ID = 0b1111_1011;
        internal const int _Program_Stream_Directory_ID = 0b1111_1111;
        #endregion 

        /// <summary>
        /// Packet start code prefix
        /// <para>First part of start code.</para>
        /// </summary>
        public byte[] StartCodePrefix { get; set; } //3bytes 24 bits.

        /// <summary>
        /// Stream id.2
        /// <para>e.g. Audio streams (0xC0-0xDF), Video streams (0xE0-0xEF)</para>
        /// <para>Second and last part of Start code.</para>
        /// </summary>
        public byte StreamID { get; set; } // 1 byte 8 bits.

        /// <summary>
        /// PES Packet length
        /// <para>Specifies the number of bytes remaining in the packet after this field. Can be zero. 
        /// If the PES packet length is set to zero, the PES packet can be of any length. 
        /// A value of zero for the PES packet length can be used only when the PES packet payload is a 
        /// video elementary stream.</para>
        /// </summary>
        public int Length { get; set; } // 16 bits.


        #region Optional Felids
        public const byte MarkerBists = 0x2;

        /// <summary>
        /// Scrambling control
        /// </summary>
        public Scrambling ScramblingControl { get; set; } // 2 bits

        /// <summary>
        /// Priority
        /// </summary>
        public bool Priority { get; set; } // 1bits.

        /// <summary>
        /// Data alignment indicator
        /// <para>1 indicates that the PES packet header is immediately followed by the video start code or audio syncword</para>
        /// </summary>
        public bool AlignmentIndicator { get; set; } //1 bits

        /// <summary>
        /// Copyright
        /// </summary>
        public bool HasCopyright { get; set; } //1 bits

        /// <summary>
        /// If not true then this is a copied version.
        /// </summary>
        public bool IsOriginal { get; set; } //1 bits.

        public enum PTS_DTS_INDICATOR
        {
            Both = 0b11,
            Foribidden = 0b01,
            PTS_Only = 0b10,
            None = 0b00
        }

        /// <summary>
        /// PTS DTS indicator	
        /// </summary>
        public PTS_DTS_INDICATOR PTS_DTS_Indicator { get; set; } //2 bits.

        /// <summary>
        /// ESCR Flag
        /// </summary>
        public bool ESCR_Flag { get; set; } // 1bits.

        /// <summary>
        /// ES rate flag.
        /// </summary>
        public bool ES_Rate_Flag { get; set; }//1 bits

        /// <summary>
        /// DSM trick mode flag
        /// </summary>
        public bool DSM_Trick_Mode_Flag { get; set; } //1 bits.

        /// <summary>
        /// Additional copy info flag
        /// </summary>
        public bool HasAdditionalCopyInfo { get; set; } //1 bits.

        /// <summary>
        /// CRC flag
        /// </summary>
        public bool HasCRC { get; set; }// 1 bits.

        /// <summary>
        /// extension flag
        /// </summary>
        public bool HasExtension { get; set; }// 1bits

        /// <summary>
        /// PES header length. gives the length of the remainder of the PES header in bytes
        /// </summary>
        public byte PES_Header_Data_Length { get; set; }// 8 bits.
        #region PTS_DTS
        // Presentation and decode timestamp is like a structure like:
        // '0010' for PTS '0001' for DTS -> 4 bits
        // PTS[32..30] -> 3 bits.
        // marker -> 1 bit.
        // PTS[29..15] -> 15 bits.
        // marker -> 1 bit
        // PTS[14..0] -> 15 bis.
        // marker -> 1 bit.
        // -===============-
        // Total 40 bits. 5 bytes.

        private long ReadTimeStamp(BitPacket packet)
        {
            long value = 0;
            int counter = 0;
            int shiftCount = 32;
            while(counter <= 34)
            {
                if(counter == 3 || counter ==20)
                {
                    packet.ReadBool();
                }
                else
                {
                    value += (packet.ReadBool() ? 1 : 0) << shiftCount;
                    shiftCount--;
                }
                counter++;
            }
            return value;
        }

        /// <summary>
        /// *Optional. Presentation Time Stamp. only present when <see cref="PTS_DTS_Indicator"/> has value
        /// <see cref="PTS_DTS_INDICATOR.Both"/> or  <see cref="PTS_DTS_INDICATOR.PTS_Only"/>.
        /// <para>Total 40 bits.</para>
        /// </summary>
        public long PTS { get; set; }

        /// <summary>
        /// *Optional. Decode Time Stamp. only present when <see cref="PTS_DTS_Indicator"/> has value 
        /// <see cref="PTS_DTS_INDICATOR.Both"/>.
        /// <para>Total 40 bits.</para>
        /// </summary>
        public long DTS { get; set; }
        #endregion

        #region ESCR
        //Elementary stream clock reference.(ESCR) structure is like:
        //reserved -> 2 bits
        //ESCR[32..30] ->3 bits
        //marker -> 1 bit.
        //ESCR[29..15] ->15 bits
        //marker -> 1 bit.
        //ESCR[14..0] ->15 bits
        //ESCR_Extension ->9 bits.
        //marker -> 1 bit.
        /// <summary>
        /// *Optional. Elementary stream clock reference.
        /// <para>Total 48 bits.</para>
        /// </summary>
        public long ESCR { get; set; }

        public int ESCR_Extension { get; set; }
        #endregion

        /// <summary>
        /// *Optional. Elementary stream rate.
        /// <para>This 22 bits value is surrounded by two 1 bits marker on both side alike MARKER ES_RATE MARKER.</para>
        /// <para>total 24 bits.</para>
        /// </summary>
        public int ES_Rate { get; set; }

        #region DSM trick mode
        public enum TrickModes
        {
            FastForward = 0b000,
            SlowMotion = 0b001,
            FreezeFrame = 0b010,
            FastReverse = 0b011,
            SlowReverse = 0b100
        }
        /// <summary>
        /// A 3-bit field that indicates which trick mode is applied to the associated video stream. 
        /// In case of other types of elementary streams, the meanings of this field and those 
        /// defined by the following five bits are underfind.
        /// </summary>
        public TrickModes TrickModeControl { get; set; }

        public byte Trick_FieldID { get; set; }

        public bool Trick_IntraSliceRefresh { get; set; }
        
        public byte Trick_FrequencyTruncation { get; set; }

        public int Trick_RepCntrl { get; set; }
        #endregion

        public byte AdditionalCopyInfo { get; set; }
        
        public long PreviosPES_CRC { get; set; }

        #region PES Extension

        public struct PESExtension
        {
            public bool HasPrivateData { get; set; }
            public bool HasPackHeader { get; set; }
            public bool HasPacketSeqCounter { get; set; }
            public bool HasPSTDBuffer { get; set; }
            public bool HasExtensionField { get; set; }

            /// <summary>
            /// Private data. This data, combined with the fields before and after, shall not emulate the packet_start_code_prefix(0x000001).
            /// </summary>
            public byte[] PrivateData { get; set; }
            /// <summary>
            /// for the pack_header_field(); *dont know what that is.
            /// </summary>
            public byte PackHeaderLength { get; set; }
            //Pack_header()?????? wtf?
            /// <summary>
            /// A sequence counter similar to countinuity counter. for PES packets.
            /// </summary>
            public byte PacketSeqCounter { get; set; }
            /// <summary>
            /// When true, this PES packet carries information from an ISO/IEC 11172-1 stream.
            /// </summary>
            public bool IsMpeg1 { get; set; }
            /// <summary>
            /// Specifies the stuffing bytes used in the 13818-1 or 11172-1 packet header.
            /// </summary>
            public byte OriginalStuffLength { get; set; }

            public bool PSTD_BufferScale { get; set; }
            /// <summary>
            /// See document for more info.
            /// </summary>
            public uint PSTD_BufferSize { get; set; }

            public byte ExtensionLength { get; set; }

            public PESExtension(BitPacket packet)
            {
                this.HasPrivateData = packet.ReadBool();
                this.HasPackHeader = packet.ReadBool();
                this.HasPacketSeqCounter = packet.ReadBool();
                this.HasPSTDBuffer = packet.ReadBool();
                packet.SkipBit(3);
                this.HasExtensionField = packet.ReadBool();

                if (this.HasPrivateData)
                {
                    this.PrivateData = packet.ReadBlock(128);
                }
                else this.PrivateData = null;

                if (this.HasPackHeader)
                {
                    this.PackHeaderLength = packet.ReadByte();
                    // Here might have something to do with the pack_header();
                    //I don't know.
                }
                else this.PackHeaderLength = 0;

                if (this.HasPacketSeqCounter)
                {
                    packet.SkipBit();
                    this.PacketSeqCounter = packet.ReadByte(7);
                    packet.SkipBit();
                    this.IsMpeg1 = packet.ReadBool();
                    this.OriginalStuffLength = packet.ReadByte(6);
                }
                else
                {
                    this.PacketSeqCounter = 0;
                    this.IsMpeg1 = false;
                    this.OriginalStuffLength = 0;
                }

                if (this.HasPSTDBuffer)
                {
                    packet.ReadByte(2);
                    this.PSTD_BufferScale = packet.ReadBool();
                    this.PSTD_BufferSize = (uint)packet.ReadInt(13);
                }
                else
                {
                    this.PSTD_BufferScale = false;
                    this.PSTD_BufferSize = 0;
                }

                if (this.HasExtensionField)
                {
                    packet.SkipBit();
                    this.ExtensionLength = packet.ReadByte(7);
                    for (int i = 0; i < this.ExtensionLength; i++)
                    {// reading extension??
                        packet.ReadByte();
                    }
                }
                else
                    this.ExtensionLength = 0;
            }

        }

        public PESExtension Extension { get; set; }

        #endregion

        public byte[] PES_Data_bytes { get; set; }

        #endregion

        public PESPacket(BitPacket packet)
        {
            this.StartCodePrefix = packet.ReadBlock(24);

            this.StreamID = packet.ReadByte();

            this.Length = packet.ReadByte(16);
            //Optional:
            if(this.StreamID == _Program_Stream_Map_ID ||
                this.StreamID == _Private_Stream_2_ID ||
                this.StreamID == _ECM_Stream_ID ||
                this.StreamID == _EMM_Stream_ID ||
                this.StreamID == _Program_Stream_Directory_ID ||
                this.StreamID == _DSMCC_Stream_ID ||
                this.StreamID == _ITU_T_H222_E_ID)
            {
                var mtmp = new List<byte>();
                for (int i = 0; i< this.Length; i++)
                {
                    mtmp.Add(packet.ReadByte());                   
                }
            }
            else if(this.StreamID != _Padding_Stream_ID)
            {
                packet.SkipBit(2);

                this.ScramblingControl = (Scrambling)packet.ReadInt(2);

                this.Priority = packet.ReadBool();

                this.AlignmentIndicator = packet.ReadBool();

                this.HasCopyright = packet.ReadBool();

                this.IsOriginal = packet.ReadBool();

                this.PTS_DTS_Indicator = (PTS_DTS_INDICATOR)packet.ReadInt(2);

                this.ESCR_Flag = packet.ReadBool();

                this.ES_Rate_Flag = packet.ReadBool();

                this.DSM_Trick_Mode_Flag = packet.ReadBool();

                this.HasAdditionalCopyInfo = packet.ReadBool();

                this.HasCRC = packet.ReadBool();

                this.HasExtension = packet.ReadBool();

                this.PES_Header_Data_Length = packet.ReadByte();

                if(PTS_DTS_Indicator == PTS_DTS_INDICATOR.PTS_Only)
                {
                    if (packet.ReadInt(4) == 0b0010)
                    {
                        this.PTS = ReadTimeStamp(packet);
                    }
                }
                else if(PTS_DTS_Indicator == PTS_DTS_INDICATOR.Both)
                {
                    var ptsordts = packet.ReadInt(4);
                    if (ptsordts == 0b0010)
                        this.PTS = ReadTimeStamp(packet);
                    else if (ptsordts == 0b0011)
                        this.DTS = ReadTimeStamp(packet);
                    else
                        packet.ReadInt(36);
                    ptsordts = packet.ReadInt(4);
                    if (ptsordts == 0b0010)
                        this.PTS = ReadTimeStamp(packet);
                    else if (ptsordts == 0b0011)
                        this.DTS = ReadTimeStamp(packet);
                    else
                        packet.ReadInt(36);
                }

                if (this.ESCR_Flag)
                {
                    packet.SkipBit(2);
                    this.ESCR = ReadTimeStamp(packet);
                    packet.SkipBit();
                    this.ESCR_Extension = packet.ReadInt(9);
                    packet.SkipBit();
                }

                if (ES_Rate_Flag)
                {
                    packet.SkipBit();
                    this.ES_Rate = packet.ReadInt(22);
                    packet.SkipBit();
                }

                if (this.DSM_Trick_Mode_Flag)
                {
                    this.TrickModeControl = (TrickModes)packet.ReadInt(3);
                    switch (this.TrickModeControl)
                    {
                        case TrickModes.FastForward:
                        case TrickModes.FastReverse:
                            this.Trick_FieldID = packet.ReadByte(2);
                            this.Trick_IntraSliceRefresh = packet.ReadBool();
                            this.Trick_FrequencyTruncation = packet.ReadByte(2);
                            break;
                        case TrickModes.FreezeFrame:
                            this.Trick_FieldID = packet.ReadByte(2);
                            packet.SkipBit(3);
                            break;
                        case TrickModes.SlowMotion:
                        case TrickModes.SlowReverse:
                            this.Trick_RepCntrl = packet.ReadInt(5);
                            break;
                        default:
                            packet.SkipBit(5);
                            break;
                    }
                }

                if (this.HasAdditionalCopyInfo)
                {
                    packet.SkipBit();
                    this.AdditionalCopyInfo = packet.ReadByte(7);
                }

                if (this.HasCRC)
                {
                    this.PreviosPES_CRC = packet.ReadInt(16);
                }

                if (this.HasExtension)
                {
                    this.Extension = new PESExtension(packet);
                }

                //Fill the stuffing bytes
                for(int i = 0; i <= 3; i++)
                {
                    if (packet.ReadByte() != 0b1111_1111)
                    {
                        packet.RewindBit(8);
                        break;
                    }
                }
                // the length might not be what I think it to be.
                var mtmp = new List<byte>();
                for (int i = 0; i < this.Length; i++)
                {
                    mtmp.Add(packet.ReadByte());
                }
            }
        }
    }
}
