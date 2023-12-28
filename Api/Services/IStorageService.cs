using Api.Services.DTO;

namespace Api.Services
{
    public interface IStorageService
    {
        public IItemInfo GetItem(long id);

        public IList<IItemInfo> GetItems(long? rootId, int skip, int take, string[]? extensions);

        public CollectionMetadata GetCollectionMetadata(long? rootId);

        public FileItemData GetImage(long id);
    }
}