using Ardalis.Result;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polchan.Application.Interfaces;
using Polchan.Shared.Options;

namespace Polchan.Infrastructure.Storage.Services;

public class LocalStorageService(
    IOptions<StorageOptions> options,
    ILogger<LocalStorageService> logger
) : IStorageService
{
    private readonly string _basePath = options.Value.Local.BasePath;

    public Result<Stream> GetFile(string filePath)
    {
        try
        {
            var verifyPathResult = VerifyPath(filePath);
            
            if (!verifyPathResult.IsSuccess)
                return verifyPathResult.Map();

            if (!File.Exists(filePath))
            {
                logger.LogError("File not found in local storage: {FilePath}", filePath);
                return Result.NotFound("File not found");
            }

            return File.OpenRead(filePath);
        }

        catch (Exception exception)
        {
            logger.LogError(exception, "Error while retrieving file from local storage: {FilePath}", filePath);
            return Result.Error("Error while retrieving file from local storage");
        }
    }

    public async Task<Result<List<string>>> CreateFilesAsync(
        IEnumerable<Stream> streams,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var tasks = streams.Select(async stream =>
            {
                using var file = File.Create(Path.Combine(_basePath, Guid.NewGuid().ToString()));
                await stream.CopyToAsync(file, cancellationToken);
                return file.Name;
            });

            var filePaths = await Task.WhenAll(tasks);
            return filePaths.ToList();
        }

        catch (Exception exception)
        {
            logger.LogError(exception, "Error while creating file in local storage");
            return Result.Error("Error while creating file in local storage");
        }
    }

    public Result DeleteFiles(IEnumerable<string> filePaths)
    {
        try
        {
            foreach (var filePath in filePaths)
            {
                var verifyPathResult = VerifyPath(filePath);
            
                if (!verifyPathResult.IsSuccess)
                    return verifyPathResult.Map();

                File.Delete(filePath);
            }

            return Result.Success();
        }

        catch (Exception exception)
        {
            logger.LogError(exception, "Error while deleting files from local storage");
            return Result.Error("Error while deleting files from local storage");
        }
    }

    private Result VerifyPath(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            return Result.Error("File path is null or empty");

        if (!Path.GetFullPath(filePath).StartsWith(_basePath))
        {
            logger.LogError("Tried to access file outside of local storage directory: {FilePath}", filePath);
            return Result.Error("Tried to access file outside of local storage directory");
        }

        return Result.Success();
    }
}
