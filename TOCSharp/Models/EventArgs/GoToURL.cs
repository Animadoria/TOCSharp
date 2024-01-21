namespace TOCSharp.Models.EventArgs
{
    public class GoToURL
    {
        public string URL;
        public string WindowName;

        public GoToURL(string url, string windowName)
        {
            this.URL = url;
            this.WindowName = windowName;
        }
    }
}