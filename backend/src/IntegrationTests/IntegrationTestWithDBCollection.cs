namespace MusicStream.IntegrationTests;

[CollectionDefinition("IntegrationTestWithDBCollection")]
public class IntegrationTestWithDBCollection : ICollectionFixture<CustomWebApplicationFactory>
{
}