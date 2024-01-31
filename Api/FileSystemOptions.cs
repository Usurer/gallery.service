namespace Api
{
    public class FileSystemOptions
    {
        public const string FileSystem = "FileSystem";

        // TODO: we don't really need it anymore
        // Remove it or make available only for Internal API calls
        // Scans with nullable root param shouldn't be allowed
        [Obsolete]
        public string DefaultFolder
        {
            get; set;
        }
    }
}