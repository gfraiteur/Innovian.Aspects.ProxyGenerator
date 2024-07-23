using Innovian.Aspects.ProxyGenerator;

namespace TestProject;

[ProxyGenerator("MyHttpClient")]
public interface ISampleService : IService
{
    public Task<List<Guid>> ListAllIdsAsync();
}