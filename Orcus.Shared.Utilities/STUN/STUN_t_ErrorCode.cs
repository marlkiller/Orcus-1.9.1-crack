namespace Orcus.Shared.Utilities.STUN
{
    /// <summary>
    ///     This class implements STUN ERROR-CODE. Defined in RFC 3489 11.2.9.
    /// </summary>
    internal class STUN_t_ErrorCode
    {
        private int m_Code;
        private string m_ReasonText = "";

        /// <summary>
        ///     Default constructor.
        /// </summary>
        /// <param name="code">Error code.</param>
        /// <param name="reasonText">Reason text.</param>
        public STUN_t_ErrorCode(int code, string reasonText)
        {
            m_Code = code;
            m_ReasonText = reasonText;
        }

        /// <summary>
        ///     Gets or sets error code.
        /// </summary>
        public int Code
        {
            get { return m_Code; }
            set { m_Code = value; }
        }

        /// <summary>
        ///     Gets reason text.
        /// </summary>
        public string ReasonText
        {
            get { return m_ReasonText; }
            set { m_ReasonText = value; }
        }
    }
}