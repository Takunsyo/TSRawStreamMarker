namespace TSRawStreamMarker.TransportStream.Packets
{
    public interface IPSISection
    {
        bool HasPointer { get; }
        /// <summary>
        /// Program specific information pointer.
        /// <para>This field will present when <see cref="TSPacket.IsPayloadEntry"/> is set to true.</para>
        /// </summary>
        byte Pointer_Field { get; set; }

        byte TableID { get; set; }

        bool SyntaxIndicator { get; set; }

        bool IsPrivate { get; set; }

        byte PSIReserved { get; set; }

        int SectionLength { get; set; }
        /// <summary>
        /// The value shall be incremented by 1 modulo 32 whenever the definition of the PAT changes.
        /// </summary>
        byte Version { get; set; }
        /// <summary>
        /// Current Next Indicator. indicates that the PAT is currently applicable.
        /// </summary>
        bool IsApplicable { get; set; }
        /// <summary>
        /// The section number of the first section in the PAT shall be 0x00 it 
        /// shall be incremented by 1 with each additional section in the PAT.
        /// </summary>
        byte SectionNumber { get; set; }
        /// <summary>
        /// The number of the laster section.
        /// </summary>
        byte LastSectionNumber { get; set; }

        uint CRC32 { get; set; }

        BitPacket Data { get; set; }
    }
}
