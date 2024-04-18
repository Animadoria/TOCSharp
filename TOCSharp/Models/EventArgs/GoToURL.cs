namespace TOCSharp.Models.EventArgs
{
    /// <summary>
    /// Go To URL event args
    /// </summary>
    public class GoToURL
    {
        /// <summary>
        /// URL to go to
        /// </summary>
        public string URL;
        /// <summary>
        /// Window name
        /// </summary>
        public string WindowName;

        internal GoToURL(string url, string windowName)
        {
            this.URL = url;
            this.WindowName = windowName;
        }
    }
}