using System;
using System.Collections.Generic;
using System.Text;
using Loop.Application.Abstractions.Authentication;
using Loop.Application.Abstractions.Messaging;
using Loop.Application.Interfaces;
using Loop.Application.Users.Contract;
using Loop.Domain.Common;
using Loop.Domain.Users;
using Loop.Domain.Users.Specifications;
using Microsoft.EntityFrameworkCore;
using Loop.SharedKernel;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Loop.Application.Users.Query;

public static class GetUserByEmail
{
    public sealed record Query(string Email) : IQuery<UserResponse>;


    public sealed class Handler(IReadOnlyRepository<User> _userReadRepo, IUserContext userContext)
    : IQueryHandler<Query, UserResponse>
    {
        public async Task<Result<UserResponse>> Handle(Query query, CancellationToken cancellationToken)
        {
            var email = Email.Create(query.Email);

            UserResponse? user = await _userReadRepo.Find(new UserByEmailSpecification(email))
                .Select(u => new UserResponse
                {
                    Id = u.UserId,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Email = u.Email.Value
                })
                .SingleOrDefaultAsync(cancellationToken);

            if (user is null)
            {
                return Result.Failure<UserResponse>(UserErrors.NotFoundByEmail);
            }

            if (user.Id != userContext.UserId)
            {
                return Result.Failure<UserResponse>(UserErrors.Unauthorized());
            }

            return user;
        }
    }
}


