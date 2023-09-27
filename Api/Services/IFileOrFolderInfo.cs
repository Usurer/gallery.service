namespace Api.Services
{
    public interface IFileOrFolderInfo
    {
        public string Name
        {
            get; set;
        }

        public string Path
        {
            get; set;
        }

        public bool IsFolder
        {
            get;
        }
    }
}