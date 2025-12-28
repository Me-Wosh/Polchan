using Ardalis.Result;
using Polchan.Core.Posts.Entities;

namespace Polchan.Core.Resources.Entities;

public class Resource : BaseEntity
{
    private Resource() { }

    public string FilePath { get; private set; } = string.Empty;
    public string OriginalFileName { get; private set; } = string.Empty;
    public string FileType { get; private set; } = string.Empty;

    public Guid? PostId { get; private set; }
    public Post? Post { get; private set; }

    public static Result<Resource> Create(string filePath, string originalFileName, string fileType)
    {
        return new Resource
        {
            FilePath = filePath,
            OriginalFileName = originalFileName,
            FileType = fileType
        };
    }
}
