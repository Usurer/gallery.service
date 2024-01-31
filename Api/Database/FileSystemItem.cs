using Microsoft.EntityFrameworkCore;

namespace Api.Database;

// Just alternative to [Key] attribute
[PrimaryKey(nameof(Id))]
public class FileSystemItem
{
    public long Id
    {
        get; set;
    }

    public long? ParentId
    {
        get; set;
    }

    public bool IsFolder
    {
        get; set;
    }

    public string Path
    {
        get; set;
    }

    public string Name
    {
        get; set;
    }

    public long CreationDate
    {
        get; set;
    }

    public string? Extension
    {
        get; set;
    }

    // TODO: With these fields nullable for Directory, out model becomes too de-normalized
    // Maybe we need separate table for image metadata
    public int? Width
    {
        get; set;
    }

    public int? Height
    {
        get; set;
    }

    public long UpdatedAtDate
    {
        get; set;
    }
}