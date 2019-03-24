namespace TSRawStreamMarker.TransportStream
{
    /// <summary>
    /// Transport Scrambling Control (TSC)
    /// </summary>
    public enum Scrambling
    {
        /// <summary>
        /// Not scrambled
        /// </summary>
        None = 0b00,
        /// <summary>
        /// Reserved for future use. (For DVB-CSA and ATSC DES only)
        /// </summary>
        Reserved = 0b01,
        /// <summary>
        /// Scrambled with even key. (For DVB-CSA and ATSC DES only)
        /// </summary>
        EvenKey = 0b10,
        /// <summary>
        /// Scrambled with odd key. (For DVB-CSA and ATSC DES only)
        /// </summary>
        OddKey = 0b11
    }
}
