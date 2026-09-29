
using MediatR;
using MusicStream.Application.DTOs;
using MusicStream.Application.Interfaces;

public class GetSongsDetailsHandler : IRequestHandler<SongQueryFilter, PagedResponse<SongDetailsDto>>
{
    private readonly ISongRepository _repo;

    public GetSongsDetailsHandler(ISongRepository repository)
    {
        _repo = repository;
    }

    public async Task<PagedResponse<SongDetailsDto>> Handle(SongQueryFilter filter, CancellationToken cancellationToken)
    {
        return await _repo.GetSongsDetails(filter, cancellationToken);
    }
}