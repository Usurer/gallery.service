namespace Api.DTO;

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

    public string? Extension
    {
        get; set;
    }
}