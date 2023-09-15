namespace Api.Services
{
    public interface IFileSystemItem
    {
        public string FullName
        {
            get; set;
        }

        public string Path
        {
            get; set;
        }
    }
}