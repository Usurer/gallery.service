namespace Api.Services
{
    public interface IStorageService
    {
        public IList<IFileOrFolderInfo> GetFileSystemItems(long? rootId, int take);
    }
}