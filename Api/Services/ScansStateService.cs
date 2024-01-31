using Api.Database;
using Api.Utils;
using Microsoft.EntityFrameworkCore;

namespace Api.Services
{
    public interface IScansStateService
    {
        public Task<long> AddFolderToScansAsync(string path);

        public Task RemoveFolderFromScansAsync(long id);

        public Task<ScanTarget?> GetNext();
    }

    public class ScansStateService : IScansStateService
    {
        private GalleryContext DbContext
        {
            get; set;
        }

        public ScansStateService(GalleryContext dbContext)
        {
            DbContext = dbContext;
        }

        public async Task<long> AddFolderToScansAsync(string path)
        {
            var existingItem = DbContext.FileSystemItems.Where(x => x.Path == path).SingleOrDefault();
            if (existingItem != null)
            {
                throw new ApplicationException($"{path} is already in DB");
            }

            var entity = new ScanTarget
            {
                Path = path
            };
            DbContext.ScanTargets.Add(entity);

            await DbContext.SaveChangesAsync();
            return entity.Id;
        }

        public async Task RemoveFolderFromScansAsync(long id)
        {
            var item = await DbContext.ScanTargets.Where(x => x.Id == id).SingleAsync();
            DbContext.ScanTargets.Remove(item);
            await DbContext.SaveChangesAsync();
        }

        public async Task<ScanTarget?> GetNext()
        {
            var item = await DbContext.ScanTargets.OrderBy(x => x.Id).FirstOrDefaultAsync();
            if (item != null)
            {
                return item;
            }
            return null;
        }
    }
}