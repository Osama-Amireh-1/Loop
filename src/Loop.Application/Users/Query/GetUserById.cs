using System;
using System.Collections.Generic;
using System.Text;
using Loop.Application.Abstractions.Authentication;
using Loop.Application.Abstractions.Messaging;
using Loop.Application.Interfaces;
using Loop.Application.Users.Contract;
using Loop.Domain.Users;
using Loop.Domain.Users.Specifications;
using Microsoft.EntityFrameworkCore;
using Loop.SharedKernel;

namespace Loop.Application.Users.Query;

public static class GetUserById
{
    public sealed record GetUserByIdQuery(Guid UserId) : IQuery<UserResponse>;


    public sealed class GetUserByIdQueryHandler(IReadOnlyRepository<User> _userReadRepo, IUserContext userContext)
    : IQueryHandler<GetUserByIdQuery, UserResponse>
    {
        public async Task<Result<UserResponse>> Handle(GetUserByIdQuery query, CancellationToken cancellationToken)
        {
            if (query.UserId != userContext.UserId)
            {
                return Result.Failure<UserResponse>(UserErrors.Unauthorized());
            }

            UserResponse? user = await _userReadRepo.Find(new UserByPKSpecification(query.UserId))
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
                return Result.Failure<UserResponse>(UserErrors.NotFound(query.UserId));
            }

            return user;
        }
    }

}


