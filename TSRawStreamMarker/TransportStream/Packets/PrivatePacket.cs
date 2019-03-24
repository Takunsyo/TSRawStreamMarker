namespace TSRawStreamMarker.TransportStream.Packets
{

    /// <summary>
    /// Private data
    /// <para>See ISO/IEC13818-1 Section2.4.4.10 Table2-30</para>
    /// </summary>
    public class PrivatePacket: IPSISection
    {
        public bool HasPointer { get; private set; }
        /// <summary>
        /// Program specific information pointer.
        /// <para>This field will present when <see cref="TSPacket.IsPayloadEntry"/> is set to true.</para>
        /// </summary>
        public byte Pointer_Field
        {
            get => this.HasPointer ? this.Data.ReadByte(0, 8) : (byte)0;
            set
            {
                if (this.HasPointer)
                    this.Data.WriteByte(value, 0, 8);
            }
        }
        /// <summary>
        /// Table ID
        /// </summary>
        public byte TableID
        {
            get => this.Data.ReadByte(0 + (this.HasPointer ? 8 : 0), 8);
            set => this.Data.WriteByte(value, 0 + (this.HasPointer ? 8 : 0), 8);
        }
        /// <summary>
        /// This should be set to true.
        /// <para>In case of <see cref="PrivatePacket"/> if this value is set to <see cref="false"/>
        /// then this packet will not follow <see cref="IPSISection"/> with exception of 
        /// <see cref="Pointer_Field"/>, <see cref="TableID"/>, <see cref="SectionLength"/> 
        /// and <see cref="IsPrivate"/>.</para>
        /// </summary>
        public bool SyntaxIndicator
        {
            get => this.Data.ReadBool(8 + (this.HasPointer ? 8 : 0));
            set => this.Data.WriteBool(value, 8 + (this.HasPointer ? 8 : 0));
        }
        /// <summary>
        /// Is allways false in <see cref="PATPacket"/>.
        /// </summary>
        public bool IsPrivate
        {
            get => this.Data.ReadBool(9 + (this.HasPointer ? 8 : 0));
            set => this.Data.WriteBool(value, 9 + (this.HasPointer ? 8 : 0));
        }
        /// <summary>
        /// Reserved.
        /// </summary>
        public byte PSIReserved
        {
            get => this.Data.ReadByte(10 + (this.HasPointer ? 8 : 0), 2);
            set => this.Data.WriteByte(value, 10 + (this.HasPointer ? 8 : 0), 2);
        }
        /// <summary>
        /// Specify the number of bytes of the section, starting immediately following 
        /// the SectionLength field, and including the CRC. the Value should never be greater then 0x3FD.
        /// </summary>
        public int SectionLength
        {// The sectionLength's first 2 bits should allways be '00'
            get => this.Data.ReadInt(14 + (this.HasPointer ? 8 : 0), 10);
            set => this.Data.WriteInt(value, 14 + (this.HasPointer ? 8 : 0), 10);
        }

        public int TableID_Extension
        {
            get
            {
                if (!this.SyntaxIndicator) return 0;
                return this.Data.ReadInt(24 + (this.HasPointer ? 8 : 0), 16);
            }
            set
            {
                if (this.SyntaxIndicator)
                    this.Data.WriteInt(value, 24 + (this.HasPointer ? 8 : 0), 16);
            }
        }
        /// <summary>
        /// The value shall be incremented by 1 modulo 32 whenever the definition of the PAT changes.
        /// </summary>
        public byte Version
        {
            get
            {
                if (!this.SyntaxIndicator) return 0;
                return this.Data.ReadByte(42 + (this.HasPointer ? 8 : 0), 5);
            }
            set
            {
                if (this.SyntaxIndicator)
                    this.Data.WriteByte(value, 42 + (this.HasPointer ? 8 : 0), 5);
            }
        }
        /// <summary>
        /// Current Next Indicator. indicates that the PAT is currently applicable.
        /// </summary>
        public bool IsApplicable
        {
            get
            {
                if (!this.SyntaxIndicator) return false;
                return this.Data.ReadBool(47 + (this.HasPointer ? 8 : 0));
            }
            set
            {
                if (this.SyntaxIndicator)
                    this.Data.WriteBool(value, 47 + (this.HasPointer ? 8 : 0));
            }
        }
        /// <summary>
        /// The section number of the first section in the PAT shall be 0x00 it 
        /// shall be incremented by 1 with each additional section in the PAT.
        /// </summary>
        public byte SectionNumber
        {
            get
            {
                if (!this.SyntaxIndicator) return 0;
                return this.Data.ReadByte(48 + (this.HasPointer ? 8 : 0), 8);
            }
            set
            {
                if (this.SyntaxIndicator)
                    this.Data.WriteByte(value, 48 + (this.HasPointer ? 8 : 0), 8);
            }
        }
        /// <summary>
        /// The number of the laster section.
        /// </summary>
        public byte LastSectionNumber
        {
            get
            {
                if (!this.SyntaxIndicator) return 0;
                return this.Data.ReadByte(56 + (this.HasPointer ? 8 : 0), 8);
            }
            set
            {
                if (this.SyntaxIndicator)
                    this.Data.WriteByte(value, 56 + (this.HasPointer ? 8 : 0), 8);
            }
        }

        public byte[] PrivateData
        {
            get
            {
                var offset = 24 + (this.HasPointer ? 8 : 0)+ (this.SyntaxIndicator?40:0);
                var len = (this.SectionLength * 8) - (this.SyntaxIndicator ? 72 : 0);
                return this.Data.ReadBlock(offset, len);
            }
            set
            {
                var offset = 24 + (this.HasPointer ? 8 : 0) + (this.SyntaxIndicator ? 40 : 0);
                this.SectionLength = value.Length + (this.SyntaxIndicator ? 9 : 0);
                this.Data.WriteBlock(value, value.Length * 8);
                if (this.SyntaxIndicator)
                {   //** TODO **
                    //Rewrite CRC32.
                }
            }
        }


        public PrivatePacket(BitPacket packet, bool hasPointer)
        {
            this.HasPointer = hasPointer;
            this.Data = packet;
            #region Old method
            //if (hasPointer)
            //{
            //    this.Pointer_Field = packet.ReadByte();
            //}
            //this.TableID = packet.ReadByte();
            //this.SyntaxIndicator = packet.ReadBool();
            //this.IsPrivate = packet.ReadBool();
            //this.PSIReserved = packet.ReadByte(2);
            //packet.SkipBit(2); // The sectionLengths first 2 bits should allways be '00'
            //this.SectionLength = packet.ReadInt(10);
            //if (this.SyntaxIndicator)
            //{
            //    this.TableID_Extension = packet.ReadInt(16);
            //    packet.SkipBit(2);
            //    this.Version = packet.ReadByte(5);
            //    this.IsApplicable = packet.ReadBool();
            //    this.SectionNumber = packet.ReadByte();
            //    this.LastSectionNumber = packet.ReadByte();
            //    var tmp = new List<byte>();
            //    for(int i = 0; i< this.SectionLength -9; i++)
            //    {
            //        tmp.Add(packet.ReadByte());
            //    }
            //    this.PrivateData = tmp.ToArray();
            //    this.CRC32 = (uint)packet.ReadInt(32);
            //}
            //else
            //{
            //    var tmp = new List<byte>();
            //    for (int i = 0; i < this.SectionLength; i++)
            //    {
            //        tmp.Add(packet.ReadByte());
            //    }
            //    this.PrivateData = tmp.ToArray();
            //}
            #endregion
        }
        public uint CRC32
        {
            get
            {
                if (!this.SyntaxIndicator) return 0;
                return this.Data.ReadUInt(24 + (this.HasPointer ? 8 : 0) + (this.SectionLength * 8 - 32), 32);
            }
            set
            {
                if(this.SyntaxIndicator)
                    this.Data.WriteInt(value, 24 + (this.HasPointer ? 8 : 0) + (this.SectionLength * 8 - 32), 32);
            }
        }


        public BitPacket Data { get; set; }
    }
}
