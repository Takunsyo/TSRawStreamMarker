namespace TSRawStreamMarker.TransportStream.Packets
{
    /// <summary>
    /// Master strature for PSI sections.
    /// </summary>
    public class PSIPacket:IPSISection
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
        /// <summary>
        /// The value shall be incremented by 1 modulo 32 whenever the definition of the PAT changes.
        /// </summary>
        public byte Version
        {
            get => this.Data.ReadByte(42 + (this.HasPointer ? 8 : 0), 5);
            set => this.Data.WriteByte(value, 42 + (this.HasPointer ? 8 : 0), 5);
        }
        /// <summary>
        /// Current Next Indicator. indicates that the PAT is currently applicable.
        /// </summary>
        public bool IsApplicable
        {
            get => this.Data.ReadBool(47 + (this.HasPointer ? 8 : 0));
            set => this.Data.WriteBool(value, 47 + (this.HasPointer ? 8 : 0));
        }
        /// <summary>
        /// The section number of the first section in the PAT shall be 0x00 it 
        /// shall be incremented by 1 with each additional section in the PAT.
        /// </summary>
        public byte SectionNumber
        {
            get => this.Data.ReadByte(48 + (this.HasPointer ? 8 : 0), 8);
            set => this.Data.WriteByte(value, 48 + (this.HasPointer ? 8 : 0), 8);
        }
        /// <summary>
        /// The number of the laster section.
        /// </summary>
        public byte LastSectionNumber
        {
            get => this.Data.ReadByte(56 + (this.HasPointer ? 8 : 0), 8);
            set => this.Data.WriteByte(value, 56 + (this.HasPointer ? 8 : 0), 8);
        }
        public uint CRC32
        {
            get => this.Data.ReadUInt(24 + (this.HasPointer ? 8 : 0) + (this.SectionLength * 8 - 32), 32);
            set => this.Data.WriteInt(value, 24 + (this.HasPointer ? 8 : 0) + (this.SectionLength * 8 - 32), 32);
        }

        public BitPacket Data { get; set; }

        public PSIPacket(BitPacket packet, bool hasPointer)
        {
            this.HasPointer = hasPointer;
            this.Data = packet;
        }

        public static explicit operator PSIPacket(PATPacket packet)
        {
            return new PSIPacket(packet.Data, packet.HasPointer);
        }
        public static explicit operator PSIPacket(PMTPacket packet)
        {
            return new PSIPacket(packet.Data, packet.HasPointer);
        }
        public static explicit operator PSIPacket(PrivatePacket packet)
        {
            return new PSIPacket(packet.Data, packet.HasPointer);
        }
        public static explicit operator PSIPacket(DescriptionPacket packet)
        {
            return new PSIPacket(packet.Data, packet.HasPointer);
        }
        public static explicit operator PSIPacket(CATPacket packet)
        {
            return new PSIPacket(packet.Data, packet.HasPointer);
        }
        public static explicit operator PATPacket(PSIPacket packet)
        {
            return new PATPacket(packet.Data, packet.HasPointer);
        }
        public static explicit operator PMTPacket(PSIPacket packet)
        {
            return new PMTPacket(packet.Data, packet.HasPointer);
        }
        public static explicit operator PrivatePacket(PSIPacket packet)
        {
            return new PrivatePacket(packet.Data, packet.HasPointer);
        }
        public static explicit operator DescriptionPacket(PSIPacket packet)
        {
            return new DescriptionPacket(packet.Data, packet.HasPointer);
        }
        public static explicit operator CATPacket(PSIPacket packet)
        {
            return new CATPacket(packet.Data, packet.HasPointer);
        }
    }
}
