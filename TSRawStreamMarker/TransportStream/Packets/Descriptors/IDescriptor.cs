namespace TSRawStreamMarker.TransportStream.Packets.Descriptors
{
    public interface IDescriptor
    {
        /// <summary>
        /// The descriptor_tag is an 8-bit field which identifies each descriptor. 
        /// <para>See(ISO/IEC13818-1) 2.5 table 2-39.</para>
        /// </summary>
        byte Tag { get; set; }
        /// <summary>
        /// The number of byts of the descriptors.
        /// </summary>
        byte Length { get; set; }
        /// <summary>
        /// Raw Data.
        /// </summary>
        BitPacket Data { get; set; }
    }
}
