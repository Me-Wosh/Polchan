using Ardalis.Result;

namespace Polchan.Application.Interfaces;

public interface IStorageService
{
    Result<Stream> GetFile(string filePath);
    Task<Result<List<string>>> CreateFilesAsync(IEnumerable<Stream> streams, CancellationToken cancellationToken);
    Result DeleteFiles(IEnumerable<string> filePaths);
}
