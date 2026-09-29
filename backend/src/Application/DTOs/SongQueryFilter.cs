using MediatR;
using MusicStream.Application.DTOs;

public class SongQueryFilter : IRequest<PagedResponse<SongDetailsDto>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SortBy { get; set; }
    public string? Search { get; set; }

    public SongQueryFilter() { }

    public SongQueryFilter(int pageNumber, int pageSize, string? sortBy, string? search)
    {
        PageNumber = pageNumber;
        PageSize = pageSize;
        SortBy = sortBy;
        Search = search;
    }
}