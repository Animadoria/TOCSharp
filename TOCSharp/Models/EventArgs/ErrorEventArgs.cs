namespace TOCSharp.Models.EventArgs
{
    /// <summary>
    /// Error event args
    /// </summary>
    public class ErrorEventArgs
    {
        /// <summary>
        /// Error code
        /// </summary>
        public string ErrorCode { get; }

        /// <summary>
        /// Error arguments
        /// </summary>
        public string[] ErrorArgs { get; }

        internal ErrorEventArgs(string errorCode, string[] errorArgs)
        {
            this.ErrorCode = errorCode;
            this.ErrorArgs = errorArgs;
        }
    }
}