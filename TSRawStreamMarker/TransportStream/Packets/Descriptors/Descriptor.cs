namespace TSRawStreamMarker.TransportStream.Packets.Descriptors
{
    /// <summary>
    /// This is the basic <see cref="IDescriptor"/> type structure.
    /// </summary>
    public class Descriptor: IDescriptor
    {
        /// <summary>
        /// The descriptor_tag is an 8-bit field which identifies each descriptor. 
        /// <para>See(ISO/IEC13818-1) 2.5 table 2-39.</para>
        /// </summary>
        public byte Tag { get; set; }
        /// <summary>
        /// The number of byts of the descriptors.
        /// </summary>
        public byte Length { get; set; }

        public BitPacket Data { get; set; }
        public Descriptor(BitPacket packet)
        {
            this.Tag = packet.ReadByte();
            this.Length = packet.ReadByte();
            this.Data = new BitPacket(packet.ReadBlock(this.Length * 8));
        }
        public Descriptor() { }

        public byte[] GetBytes()
        {
            var result = new BitPacket();
            result.WriteByte(this.Tag);
            result.WriteByte(Length);
            result.WriteBlock(this.Data.ToByteArray(),this.Length * 8);
            return result.ToByteArray();
        }
    }
}
