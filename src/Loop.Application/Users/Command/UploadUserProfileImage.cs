using Loop.Application.Abstractions.Authentication;
using Loop.Application.Abstractions.Messaging;
using Loop.Application.Abstractions.Storage;
using Loop.Application.Interfaces;
using Loop.Application.Users.Contract;
using Loop.Domain.Users;
using Loop.Domain.Users.Specifications;
using Microsoft.EntityFrameworkCore;
using Loop.SharedKernel;

namespace Loop.Application.Users.Command;

public static class UploadUserProfileImage
{
    public sealed record UploadUserProfileImageCommand(
        string FileName,
        byte[] ImageContent)
        : ICommand<bool>;

    public sealed class Handler(
        IRepository<User> userRepo,
        IUserContext userContext,
        IImageFileStore imageFileStore)
        : ICommandHandler<UploadUserProfileImageCommand, bool>
    {
        public async Task<Result<bool>> Handle(
            UploadUserProfileImageCommand command,
            CancellationToken cancellationToken)
        {
            User? user = await userRepo.Find(new UserByPKSpecification(userContext.UserId))
                .SingleOrDefaultAsync(cancellationToken);

            if (user is null)
            {
                return Result.Failure<bool>(UserErrors.NotFound(userContext.UserId));
            }

            // Validate image file
            if (command.ImageContent == null || command.ImageContent.Length == 0)
            {
                return Result.Failure<bool>(new Error(
                    "Users.EmptyImage",
                    "Image content cannot be empty.",
                    ErrorType.Validation));
            }

            if (string.IsNullOrWhiteSpace(command.FileName))
            {
                return Result.Failure<bool>(new Error(
                    "Users.EmptyFileName",
                    "File name cannot be empty.",
                    ErrorType.Validation));
            }

            string imageUrl = await imageFileStore.SaveAsync(
                user.UserId,
                command.FileName,
                command.ImageContent,
                cancellationToken);

            user.UpdateProfile(user.FirstName, user.LastName, user.Gender, imageUrl, user.Phone);

            return true;
        }
    }
}
