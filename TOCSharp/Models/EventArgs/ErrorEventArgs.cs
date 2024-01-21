namespace TOCSharp.Models.EventArgs
{
    public class ErrorEventArgs
    {
        public string ErrorCode { get; }
        public string[] ErrorArgs { get; }

        public ErrorEventArgs(string errorCode, string[] errorArgs)
        {
            this.ErrorCode = errorCode;
            this.ErrorArgs = errorArgs;
        }
    }
}