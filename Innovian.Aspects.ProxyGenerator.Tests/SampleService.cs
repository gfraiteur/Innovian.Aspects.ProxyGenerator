namespace Innovian.Aspects.ProxyGenerator.Tests;

public class SampleService : ISampleService
{
    [ApiMethod(HttpVerb.Get, "organizations/{organizationId}")]
    public async Task<List<Guid>> ListAllIdsAsync(string organizationId)
    {
        throw new NotImplementedException();
    }
}
