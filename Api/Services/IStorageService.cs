using Api.Services.DTO;

namespace Api.Services
{
    public interface IStorageService
    {
        public IList<IItemInfo> GetFileSystemItems(long? rootId, int take);
    }
}