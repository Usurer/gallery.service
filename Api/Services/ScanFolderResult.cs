namespace Api.Services
{
    public class ScanFolderResult
    {
        public ScanFolderResult(string path)
        {
            Path = path;
        }

        public string Path
        {
            get; init;
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