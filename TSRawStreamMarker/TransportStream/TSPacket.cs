namespace TSRawStreamMarker.TransportStream
{
    /// <summary>
    /// Partial Transport Stream packet format
    /// </summary>
    public class TSPacket
    {
        public BitPacket BsePacket { get; }
        //Header is big-endian
        /// <summary>
        /// Bit pattern of 0x47 (ASCII char 'G')
        /// </summary>
        public const byte SYNC_BYTE = 0x47; // 8bits

        /// <summary>
        /// Transport Error Indicator (TEI)
        /// <para>Set when a demodulator can't correct errors from FEC data; indicating the packet is corrupt.</para>
        /// </summary>
        public bool IsError { get; set; } // 1bits

        /// <summary>
        /// Payload Unit Start Indicator (PUSI)
        /// <para>Set when a PES, PSI, or DVB-MIP packet begins immediately following the header.</para>
        /// </summary>
        public bool IsPayloadEntry { get; set; } //1bits

        /// <summary>
        /// Transport Priority
        /// <para>Set when the current packet has a higher priority than other packets with the same PID.</para>
        /// </summary>
        public bool Priority { get; set; } //1bits

        /// <summary>
        /// Packet Identifier, describing the payload data.
        /// </summary>
        public int PID { get; set; } //13 bits

        public string PacketType=> GetPIDDefinition(this.PID);

        /// <summary>
        /// Transport Scrambling Control (TSC)
        /// </summary>
        public Scrambling Scrambling { get; set; } //2 bits

        /// <summary>
        /// Adaptation field control
        /// </summary>
        public AdaptationField AdaptationFieldControl { get; set; } //2bits

        /// <summary>
        /// Sequence number of payload packets (0x00 to 0x0F) within each stream (except PID 8191)
        /// <para>Incremented per-PID, only when a payload flag is set.</para>
        /// </summary>
        public byte CountinuityCounter { get; set; }//4 bits


        //Optional fields
        /// <summary>
        /// *Optional. Present if adaptation field control is 10 or 11.
        /// </summary>
        public AdaptionFieldStuct AdaptionField { get; set; } //variable

        /// <summary>
        /// *Optional. Present if adaptation field control is 11. Payload may be PES packets, program specific information, or other data.
        /// </summary>
        public byte[] Payload { get; set; } //variable

        public TSPacket(byte[] data)
        {
            this.BsePacket = new BitPacket(data);
            //Check SyncBytes:
            long Pos = 0; //Packet start position(after sync byte)
            while (true)
            {
                var bte = BsePacket.ReadByte();
                if (bte == SYNC_BYTE)
                {
                    Pos = BsePacket.Position;
                    break;
                }
            }
            //Error Indicator
            this.IsError = BsePacket.ReadBool();
            //Payload Unit Start Indicator
            this.IsPayloadEntry = BsePacket.ReadBool();
            //Transport Priority
            this.Priority = BsePacket.ReadBool();
            //PID
            this.PID = BsePacket.ReadInt(13);
            //Transport Scrambling Control
            this.Scrambling = (Scrambling)BsePacket.ReadInt(2);
            //Adaptation field control
            this.AdaptationFieldControl = (AdaptationField)BsePacket.ReadInt(2);
            //Countinuity Counter
            this.CountinuityCounter = BsePacket.ReadByte(4);
            Pos += 24; //Moved to optional fields.
            if(this.AdaptationFieldControl.HasFlag(AdaptationField.AdaptationOnly)||
                this.AdaptationFieldControl.HasFlag(AdaptationField.AdaptationWithPayload))
            { //Adaption field.
                this.AdaptionField = new AdaptionFieldStuct(BsePacket);
            }
            if (this.AdaptationFieldControl.HasFlag(AdaptationField.None)||
                this.AdaptationFieldControl.HasFlag(AdaptationField.AdaptationWithPayload))
            { //Payload.
                this.Payload = BsePacket.ReadBlock(184*8 - (BsePacket.Position - Pos));
            }
        }

        private static string GetPIDDefinition(int pid)
        {
            switch (pid)
            {
                case 0x0000: return "Program Association Table";
                case 0x0001: return "Conditional Access Table";
                case 0x0002: return "Transport Stream Description Table";
                case 0x0003: return "IPMP Control Information Table";
                case int _pid when _pid >= 0x0004 && _pid<= 0x000F:
                    return "Reserved";
                case int _pid when _pid >= 0x0010 && _pid <= 0x001F:
                    return "DVB metadata";
                case int _pid when _pid >= 0x0020 && _pid <= 0x1FFA:
                    return "Program Map Table Elementary streams or Other";
                case 0x1ffb: return "DigiCipher 2/ATSC MGT metadata";
                case int _pid when _pid >= 0x1FFC && _pid <= 0x1FFE:
                    return "Program Map Table Elementary streams or Other";
                case 0x1fff: return "Null packet";
                default: return "Unknown packet";
            }
        }
    }
}
