using System.ComponentModel.DataAnnotations;
using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Polchan.Application.Files;
using Polchan.Application.Interfaces;
using Polchan.Core.Interfaces;
using Polchan.Core.Resources.Entities;
using Polchan.Shared.MediatR;

namespace Polchan.Application.Posts;

public record UpdatePostCommand(
    [Required] Guid Id,
    string? Title,
    string? Description,
    IEnumerable<Guid>? ImageIdsToRemove,
    IEnumerable<FileUpload>? ImagesToAdd
) : ICommand<Unit>;

public class UpdatePostHandler(
    IUserAccessor userAccessor,
    IPolchanDbContext dbContext,
    IStorageService storageService,
    ILogger<UpdatePostHandler> logger
) : ICommandHandler<UpdatePostCommand, Unit>
{
    public async Task<Result<Unit>> Handle(UpdatePostCommand command, CancellationToken cancellationToken)
    {
        var userId = userAccessor.GetUserId();

        if (!userId.IsSuccess)
            return userId.Map();

        var post = await dbContext
            .Posts
            .Include(p => p.Images)
            .SingleOrDefaultAsync(p => p.Id == command.Id, cancellationToken);

        if (post is null)
            return Result.NotFound("Post not found");

        if (post.OwnerId != userId)
            return Result.Forbidden("User is not the owner of the post");

        if (!string.IsNullOrEmpty(command.Title))
        {
            var updateTitleResult = post.UpdateTitle(command.Title);

            if (!updateTitleResult.IsSuccess)
                return updateTitleResult.Map();
        }

        if (!string.IsNullOrEmpty(command.Description))
        {
            var updateDescriptionResult = post.UpdateDescription(command.Description);

            if (!updateDescriptionResult.IsSuccess)
                return updateDescriptionResult.Map();
        }

        var imageIdsToRemove = command.ImageIdsToRemove ?? [];
        var removeImagesResult = post.RemoveImages(imageIdsToRemove);

        if (!removeImagesResult.IsSuccess)
            return removeImagesResult.Map();

        var imagesToAdd = command.ImagesToAdd ?? [];

        var filePathsToAddResult = await storageService.CreateFilesAsync(
            imagesToAdd.Select(i => i.Stream),
            cancellationToken
        );

        if (!filePathsToAddResult.IsSuccess)
            return filePathsToAddResult.Map();

        var filePathsToAdd = filePathsToAddResult.Value;
        var resources = new List<Resource>();

        foreach (var (filePath, image) in filePathsToAdd.Zip(imagesToAdd))
        {
            var resource = Resource.Create(filePath, image.FileName, image.ContentType);

            if (!resource.IsSuccess)
            {
                CleanupFiles(filePathsToAdd, "resource creation");
                return resource.Map();
            }

            resources.Add(resource);
        }

        var addImagesResult = post.AddImages(resources);

        if (!addImagesResult.IsSuccess)
        {
            CleanupFiles(filePathsToAdd, "adding images to post");
            return addImagesResult.Map();
        }

        var filePathsToRemove = post
            .Images
            .Where(i => imageIdsToRemove.Contains(i.Id))
            .Select(i => i.FilePath);

        var deleteFilesResult = storageService.DeleteFiles(filePathsToRemove);

        if (!deleteFilesResult.IsSuccess)
        {
            CleanupFiles(filePathsToAdd, "deleting old images from post");
            return deleteFilesResult.Map();
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }

    private void CleanupFiles(List<string> filePaths, string failScenario)
    {
        var deleteFilesResult = storageService.DeleteFiles(filePaths);

        if (!deleteFilesResult.IsSuccess)
        {
            logger.LogError(
                "Failed to cleanup files {FilePaths} after {FailScenario} failure. Result: {Result}",
                string.Join(", ", filePaths),
                failScenario,
                deleteFilesResult
            );
        }
    }
}
