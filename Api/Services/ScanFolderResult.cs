namespace Api.Services
{
    public class ScanFolderResult
    {
        public string Path
        {
            get; set;
        }

        public int Saved
        {
            get; set;
        }

        public int Total
        {
            get; set;
        }
    }
}