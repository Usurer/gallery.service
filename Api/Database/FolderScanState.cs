using System.ComponentModel.DataAnnotations;

namespace Api.Database
{
    public class FolderScanState
    {
        // Id is PK by default convention, but I'll keep the attr
        [Key]
        public long Id
        {
            get; set;
        }

        // FK by convention
        public long? FileSystemItemId
        {
            get; set;
        }

        public string? Path
        {
            get; set;
        }

        public long? LastScanTimestamp
        {
            get; set;
        }

        public FileSystemItem? FileSystemItem
        {
            get; set;
        }
    }
}