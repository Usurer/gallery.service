using Api.Services.DTO;

namespace Api.Services
{
    public interface IStorageService
    {
        public IItemInfo GetItem(long id);

        public IEnumerable<FileItemInfo> GetFileItems(long? rootId, int skip, int take, string[]? extensions);

        public IEnumerable<FolderItemInfo> GetFolderItems(long? rootId, int skip, int take);

        public CollectionMetadata GetCollectionMetadata(long? rootId);

        public FileItemData GetImage(long id);
    }
}