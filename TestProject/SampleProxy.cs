using Innovian.Aspects.ProxyGenerator;

namespace TestProject;


internal partial class SampleProxy : ISampleService
{
    [ApiMethod(HttpVerb.Get, "organizations/ids")]
    public async Task<List<Guid>> ListAllIdsAsync()
    {
        throw new NotImplementedException();
    }
}