using System.ComponentModel.DataAnnotations;

namespace Api.Database
{
    public class ScanTarget
    {
        // Id is PK by default convention, but I'll keep the attr
        [Key]
        public long Id
        {
            get; set;
        }

        public string? Path
        {
            get; set;
        }
    }
}