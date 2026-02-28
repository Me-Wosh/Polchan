using System.ComponentModel.DataAnnotations;
using Ardalis.Result;
using MediatR;
using Microsoft.Extensions.Logging;
using Polchan.Application.Files;
using Polchan.Application.Interfaces;
using Polchan.Core.Interfaces;
using Polchan.Core.Posts.Entities;
using Polchan.Core.Resources.Entities;
using Polchan.Shared.MediatR;

namespace Polchan.Application.Posts;

public record CreatePostCommand(
    [Required] Guid ThreadId,
    [Required] string Title,
    [Required] string Description,
    IEnumerable<FileUpload>? Images
) : ICommand<Unit>;

public class CreatePostHandler(
    IUserAccessor userAccessor,
    IPolchanDbContext dbContext,
    IStorageService storageService,
    ILogger<CreatePostHandler> logger
) : ICommandHandler<CreatePostCommand, Unit>
{
    public async Task<Result<Unit>> Handle(CreatePostCommand command, CancellationToken cancellationToken)
    {
        var userId = userAccessor.GetUserId();

        if (!userId.IsSuccess)
            return userId.Map();

        var user = await dbContext.Users.FindAsync([userId], cancellationToken);

        if (user is null)
            return Result.Unauthorized();

        var createPostResult = Post.Create(command.Title, command.Description, command.ThreadId);

        if (!createPostResult.IsSuccess)
            return createPostResult.Map();

        var post = createPostResult.Value;
        var images = command.Images ?? [];

        var filePathsResult = await storageService.CreateFilesAsync(
            images.Select(image => image.Stream),
            cancellationToken
        );

        if (!filePathsResult.IsSuccess)
            return filePathsResult.Map();

        var filePaths = filePathsResult.Value;
        var resources = new List<Resource>();

        foreach (var (filePath, image) in filePaths.Zip(images))
        {
            var resource = Resource.Create(filePath, image.FileName, image.ContentType);

            if (!resource.IsSuccess)
            {
                CleanupFiles(filePaths, "resource creation");
                return resource.Map();
            }

            resources.Add(resource.Value);
        }

        var addImagesResult = post.AddImages(resources);

        if (!addImagesResult.IsSuccess)
        {
            CleanupFiles(filePaths, "adding images to post");
            return addImagesResult.Map();
        }

        var addPostResult = user.AddPost(post);

        if (!addPostResult.IsSuccess)
        {
            CleanupFiles(filePaths, "adding post");
            return addPostResult.Map();
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
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
