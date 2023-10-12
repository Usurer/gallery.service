using Api.Services.DTO;

namespace Api.Services
{
    public interface IStorageService
    {
        public IList<IItemInfo> GetItems(long? rootId, int take);

        public FileItemData GetImage(long id);
    }
}