namespace TSRawStreamMarker.TransportStream.Packets.Descriptors
{
    /// <summary>
    /// The Conditinal Access(CA) descriptor is used to pecify both 
    /// system-wide conditional access management information such as 
    /// EMMs and elementary stream-specific information such as ECMs.
    /// </summary>
    public class CADescriptor : IDescriptor
    {
        /// <summary>
        /// The descriptor_tag is an 8-bit field which identifies each descriptor. 
        /// <para>See(ISO/IEC13818-1) 2.6 table 2-39.</para>
        /// <para>In case of <see cref="CADescriptor"/> this value should be '9'.</para>
        /// </summary>
        public byte Tag { get; set; }
        /// <summary>
        /// The number of byts of the descriptors. 8-bit.
        /// </summary>
        public byte Length { get; set; }
        /// <summary>
        /// This 16-bit field indicating the type of CA system applicable for either
        /// the associated ECM and/or EMM systems. The coding of this is privately 
        /// defined and is not specified by ITU-T|ISO/IEC. 
        /// </summary>
        public int CASystemID
        {
            get
            {
                return this.Data.ReadInt(0, 16);
            }
            set
            {
                this.Data.WriteInt(value, 0,16);
            }
        }
        /// <summary>
        /// This is a 13-bit field indicating the PID of the Transport Stream packets 
        /// which shall contain either ECM or EMM information for the CA ststem as 
        /// specified with the associated CA_system_ID.
        /// </summary>
        public int CAPID {
            get
            {
                return this.Data.ReadInt(19, 13);
            }
            set
            {
                this.Data.WriteInt(value, 19, 13);
            }
        }

        public byte[] PrivateDataBytes {
            get
            {
                return this.Data.ReadBlock(32,(this.Length - 4) * 8);
            }
            set
            {
                this.Data.WriteBlock(value, 32, (this.Length - 4) * 8);
            }
        }

        public BitPacket Data { get; set; }

        public CADescriptor(BitPacket packet)
        {
            this.Tag = packet.ReadByte();
            this.Length = packet.ReadByte();
            //this.CASystemID = packet.ReadInt(16);
            //packet.SkipBit(3);
            //this.CAPID = packet.ReadInt(13);
            //this.PrivateDataBytes = packet.ReadBlock((this.Length - 4) * 8);
        }

        public CADescriptor(Descriptor descriptor)
        {
            this.Tag = descriptor.Tag;
            this.Length = descriptor.Tag;
            this.Data = descriptor.Data;
        }

        private CADescriptor() { }

        //public static explicit operator CADescriptor(Descriptor descriptor)
        //{
        //    BitPacket packet = new BitPacket(descriptor.Data);
        //    var result = new CADescriptor();
        //    result.Tag = descriptor.Tag;
        //    result.Length = descriptor.Length;
        //    result.CASystemID = packet.ReadInt(16);
        //    packet.SkipBit(3);
        //    result.CAPID = packet.ReadInt(13);
        //    result.PrivateDataBytes = packet.ReadBlock((result.Length - 4) * 8);
        //    return result;
        //}

        //public static explicit operator Descriptor(CADescriptor descriptor)
        //{
        //    var result = new Descriptor();
        //    result.Tag = descriptor.Tag;
        //    result.Length = descriptor.Length;
        //    var data = new BitPacket();
        //    data.WriteInt(descriptor.CASystemID, 16);
        //    data.SkipBit(3);
        //    data.WriteInt(descriptor.CAPID, 13);
        //    data.WriteBlock(descriptor.PrivateDataBytes, (descriptor.Length - 4) * 8);
        //    result.Data = data.ToByteArray();
        //    return result;
        //}
    }
}
