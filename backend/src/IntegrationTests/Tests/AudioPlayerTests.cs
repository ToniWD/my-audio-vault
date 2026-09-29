using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MusicStream.Application.DTOs;
using MusicStream.Core.Entities;

namespace MusicStream.IntegrationTests.Tests;

[Collection("IntegrationTestWithDBCollection")]
public class AudioPlayerTests : IClassFixture<AudioPlayerTestFixture>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private readonly ILogger<AudioPlayerTests> _logger;
    private static User? SharedUser;
    private readonly AudioPlayerTestFixture _fixture;

    public AudioPlayerTests(CustomWebApplicationFactory factory, AudioPlayerTestFixture fixture)
    {
        _factory = factory;
        _client = factory.CreateClient();
        _fixture = fixture;

        using var scope = _factory.Services.CreateScope();
        var loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
        _logger = loggerFactory.CreateLogger<AudioPlayerTests>();
    }

    [Theory]
    [InlineData(1, 25, null)]
    [InlineData(2, 7, null)]
    [InlineData(3, 2, null)]
    [InlineData(4, 1, null)]
    [InlineData(1, 30, null)]
    [InlineData(1, 7, "Title #2")]
    [InlineData(1, 5, "Title #1")]
    public async Task GetSongsDetails_WithValidQuery_ReturnOk(
        int pageNumber,
        int pageSize,
        string? search
        )
    {
        var filter = new SongQueryFilter(
            pageNumber,
            pageSize,
            null,
            search
        );

        var httpRequest = new HttpRequestMessage(HttpMethod.Get, "/api/songs")
        {
            Content = JsonContent.Create(filter)
        };

        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _fixture.AccessToken);

        // Act
        var response = await _client.SendAsync(httpRequest);

        // Assert StatusCode
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<PagedResponse<SongDetailsDto>?>();

        Assert.NotNull(result);
        Assert.Equal(pageNumber, result.PageNumber);
        Assert.Equal(pageSize, result.PageSize);
        Assert.InRange<int>(result.PageNumber, 0, result.TotalPages);
        Assert.InRange<int>(result.Data.Count, 0, pageSize);

        if (!string.IsNullOrEmpty(search))
        {
            foreach (var song in result.Data)
            {
                Assert.True(song.Title.Contains(search) || song.Genre.Contains(search));
            }
        }
    }

    [Theory]
    [InlineData(1, 25, 0, null)]
    [InlineData(1, 25, 1, null)]
    [InlineData(1, 25, 2, null)]
    [InlineData(1, 25, 3, null)]
    [InlineData(1, 25, 2, "Title #2")]
    [InlineData(1, 25, 3, "Title #1")]
    [InlineData(1, 25, 4, null)]
    [InlineData(1, 25, 4, "Title #2")]
    public async Task GetSongsDetailsSorted_WithValidQuery_ReturnOk(
        int pageNumber,
        int pageSize,
        int predefinedSortMode,
        string? search
        )
    {
        string? sortBy = null;
        Comparison<SongDetailsDto>? compare = null;

        switch (predefinedSortMode)
        {
            case 1:
                sortBy = "genre";
                compare = (s1, s2) =>
                {
                    return string.Compare(s1.Genre, s2.Genre);
                };
                break;

            case 2:
                sortBy = "ReleaseDate";
                compare = (s1, s2) =>
                {
                    return s1.ReleaseDate.CompareTo(s2.ReleaseDate);
                };
                break;

            case 3:
                sortBy = "ReleaseDate desc";
                compare = (s1, s2) =>
                {
                    return s2.ReleaseDate.CompareTo(s1.ReleaseDate);
                };
                break;

            case 4:
                sortBy = "title";
                compare = (s1, s2) =>
                {
                    return string.Compare(s1.Title, s2.Title);
                };
                break;

            default:
                sortBy = "title desc";
                compare = (s1, s2) =>
                {
                    return string.Compare(s2.Title, s1.Title);
                };
                break;
        }

        var filter = new SongQueryFilter(
            pageNumber,
            pageSize,
            sortBy,
            search
        );

        var httpRequest = new HttpRequestMessage(HttpMethod.Get, "/api/songs")
        {
            Content = JsonContent.Create(filter)
        };

        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _fixture.AccessToken);

        // Act
        var response = await _client.SendAsync(httpRequest);

        // Assert StatusCode
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<PagedResponse<SongDetailsDto>?>();

        Assert.NotNull(result);
        Assert.Equal(pageNumber, result.PageNumber);
        Assert.Equal(pageSize, result.PageSize);
        Assert.InRange<int>(result.PageNumber, 0, result.TotalPages);
        Assert.InRange<int>(result.Data.Count, 0, pageSize);

        for (int i = 1; i < result.Data.Count; i++)
        {
            Assert.True(compare(result.Data[i - 1], result.Data[i]) <= 0);
        }
    }
}