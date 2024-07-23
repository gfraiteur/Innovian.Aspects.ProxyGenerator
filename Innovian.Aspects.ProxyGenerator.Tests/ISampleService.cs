namespace Innovian.Aspects.ProxyGenerator.Tests;

public interface ISampleService : IService
{
    Task<List<Guid>> ListAllIdsAsync(string organizationId);
}