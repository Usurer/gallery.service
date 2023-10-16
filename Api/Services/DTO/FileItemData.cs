namespace Api.Services.DTO
{
    public class FileItemData : IDisposable
    {
        private bool disposedValue;

        public FileItemInfo Info
        {
            get;
            set;
        }

        public Stream Data
        {
            get;
            set;
        }

        public bool IsFolder => false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (Data != null)
                    {
                        Data.Dispose();
                    }
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}