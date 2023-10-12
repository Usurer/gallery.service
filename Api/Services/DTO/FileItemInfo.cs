namespace Api.Services.DTO
{
    public class FileItemInfo : IItemInfo, IDisposable
    {
        private bool disposedValue;

        public long Id
        {
            get;
            set;
        }

        public string Name
        {
            get;
            set;
        }

        public string Extension
        {
            get;
            set;
        }

        // TODO: Don't make Stream nullable - better create another class with non-nullable Data
        public Stream? Data
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