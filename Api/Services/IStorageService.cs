using Api.Services.DTO;

namespace Api.Services
{
    public interface IStorageService
    {
        public IList<IItemInfo> GetItems(long? rootId, int skip, int take);

        public CollectionMetadata GetCollectionMetadata(long? rootId);

        public FileItemData GetImage(long id);
    }
}