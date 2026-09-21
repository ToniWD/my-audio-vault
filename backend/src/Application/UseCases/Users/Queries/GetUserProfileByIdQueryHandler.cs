
using MediatR;
using MusicStream.Application.DTOs;
using MusicStream.Application.Interfaces;

public record GetUserProfileByIdQuery(Guid UserId) : IRequest<UserDTO?>;

public class GetUserProfileByIdQueryHandler(IUserRepository repo)
    : IRequestHandler<GetUserProfileByIdQuery, UserDTO?>
{
    public Task<UserDTO?> Handle(GetUserProfileByIdQuery request, CancellationToken cancellationToken)
    {
        return repo.GetProfileDtoByIdAsync(request.UserId, cancellationToken);
    }
}