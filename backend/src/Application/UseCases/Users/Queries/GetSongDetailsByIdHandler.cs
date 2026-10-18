using MediatR;
using MusicStream.Application.DTOs;
using MusicStream.Application.Interfaces;

public record GetSongDetailsByIdQuery(Guid SongId) : IRequest<SongDetailsDto?>;

public class GetSongDetailsByIdHandler : IRequestHandler<GetSongDetailsByIdQuery, SongDetailsDto?>
{
    private readonly ISongRepository _repo;

    public GetSongDetailsByIdHandler(ISongRepository repository)
    {
        _repo = repository;
    }

    public async Task<SongDetailsDto?> Handle(GetSongDetailsByIdQuery query, CancellationToken cancellationToken)
    {
        return await _repo.GetSongDetailsById(query.SongId, cancellationToken);
    }
}