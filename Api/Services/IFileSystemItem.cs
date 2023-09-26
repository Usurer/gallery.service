namespace Api.Services
{
    // TODO: REfactor! This has the same name as a DTO entity!
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