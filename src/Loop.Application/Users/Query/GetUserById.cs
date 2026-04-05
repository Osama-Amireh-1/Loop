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

            UserResponse? user = await _userReadRepo.Find(new UserByIdSpecification(query.UserId))
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
