using System;
using System.Collections.Generic;
using System.Text;
using Application.Abstractions.Authentication;
using Application.Abstractions.Messaging;
using Application.Interfaces;
using Application.Users.Contract;
using Domain.Users;
using Domain.Users.Specifications;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Users.Query;

public static class GetUserByEmail
{
    public sealed record Query(string Email) : IQuery<UserResponse>;


    public sealed class Handler(IReadOnlyRepository<User> _userReadRepo, IUserContext userContext)
    : IQueryHandler<Query, UserResponse>
    {
        public async Task<Result<UserResponse>> Handle(Query query, CancellationToken cancellationToken)
        {

            UserResponse? user = await _userReadRepo.Find(new UserByEmailSpecification(query.Email))
                .Select(u => new UserResponse
                {
                    Id = u.Id,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Email = u.Email
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
