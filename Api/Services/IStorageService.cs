using Api.Services.DTO;

namespace Api.Services
{
    public interface IStorageService
    {
        public ItemInfo? GetItem(long id);

        public IEnumerable<FileItemInfo> GetFileItems(long? folderId, int skip, int take, string[]? extensions);

        public IEnumerable<FolderItemInfo> GetFolderItems(long? folderId, int skip, int take);

        public IEnumerable<FolderItemInfo>? GetFolderAncestors(long folderId);

        public CollectionMetadata GetCollectionMetadata(long? rootId);

        public FileItemData? GetImage(long id);
    }
}