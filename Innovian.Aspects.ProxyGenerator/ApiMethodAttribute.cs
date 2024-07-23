using System.Text.RegularExpressions;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;

namespace Innovian.Aspects.ProxyGenerator;

public sealed class ApiMethodAttribute : MethodAspect
{
    /// <summary>
    /// The HTTP Method to use for the request.
    /// </summary>
    private HttpVerb Method { get; set; }

    /// <summary>
    /// The template for the URL that the request should be directed to.
    /// </summary>
    private string UrlTemplate { get; set; }

    /// <summary>
    /// Initializes a new instance of <see cref="ApiMethodAttribute"/>.
    /// </summary>
    /// <param name="method"></param>
    /// <param name="urlTemplate"></param>
    public ApiMethodAttribute(HttpVerb method, string urlTemplate)
    {
        Method = method;
        UrlTemplate = urlTemplate;
    }

    [Introduce(WhenExists = OverrideStrategy.Ignore)]
    private HttpClient _httpClient;

    /// <inheritdoc />
    public override void BuildAspect(IAspectBuilder<IMethod> builder)
    {
        base.BuildAspect(builder);
        
        if (Method == HttpVerb.Get)
        {
            if (builder.Target.ReturnType != TypeFactory.GetType(SpecialType.Void))
            {
                builder.Advice.IntroduceMethod(builder.Target.DeclaringType, nameof(GetMethodTemplate),
                    IntroductionScope.Default, OverrideStrategy.Override, args: new
                    {
                        uri = UrlTemplate,
                        parameters = builder.Target.Parameters
                    });
            }
            else
            {
                builder.Advice.IntroduceMethod(builder.Target.DeclaringType, nameof(GetMethodTemplateWithResult),
                    IntroductionScope.Default, OverrideStrategy.Override, args: new
                    {
                        uri = UrlTemplate,
                        parameters = builder.Target.Parameters
                    });
            }
        }
    }
    
    [Template]
    public async Task GetMethodTemplate(string uri, IParameterList parameters)
    {
        //Resolve the cancellation token value
        var cancellationTokenParameter = meta.Target.Parameters.FirstOrDefault(p =>
            p.Type.ToType() == typeof(CancellationToken));
        var cancellationToken = cancellationTokenParameter is not null
            ? cancellationTokenParameter.Value
            : CancellationToken.None;

        var parameterValues = new Dictionary<string, string>();
        foreach (var p in parameters)
        {
            try
            {
                var paramValue = p.Value;
                if (paramValue is not null)
                {
                    parameterValues[p.Name.ToLowerInvariant()] = paramValue.ToString();
                }
            }
            catch
            {}
        }

        //Build the request URI
        var keyValue = new Dictionary<string, string>();

        var regexMatches = Regex.Matches(uri, $@"\{{[a-z0-9\-]\}}", RegexOptions.IgnoreCase);
        foreach (Match match in regexMatches)
        {
            var key = match.Value;
            var matchingParameter = parameterValues[key];
            if (matchingParameter is null)
                throw new Exception(
                    $"Unable to build out request URI as there isn't a matching parameter in the method for the template key {key}");

            keyValue.Add(key, matchingParameter);
        }

        foreach (var kvp in keyValue)
        {
            uri = uri.Replace("{" + kvp.Key + "}", kvp.Value, true, null);
        }

        using var response = await _httpClient.GetAsync(uri, cancellationToken);
    }

    [Template]
    public async Task<dynamic> GetMethodTemplateWithResult<T>(string uri, IParameterList parameters)
    {
        //Resolve the cancellation token value
        var cancellationTokenParameter = meta.Target.Parameters.FirstOrDefault(p =>
            p.Type.ToType() == typeof(CancellationToken));
        var cancellationToken = cancellationTokenParameter is not null
            ? cancellationTokenParameter.Value
            : CancellationToken.None;

        var parameterValues = new Dictionary<string, string>();
        foreach (var p in parameters)
        {
            try
            {
                var paramValue = p.Value;
                if (paramValue is not null)
                {
                    parameterValues[p.Name.ToLowerInvariant()] = paramValue.ToString();
                }
            }
            catch
            { }
        }

        //Build the request URI
        var keyValue = new Dictionary<string, string>();

        var regexMatches = Regex.Matches(uri, $@"\{{[a-z0-9\-]\}}", RegexOptions.IgnoreCase);
        foreach (Match match in regexMatches)
        {
            var key = match.Value;
            var matchingParameter = parameterValues[key];
            if (matchingParameter is null)
                throw new Exception(
                    $"Unable to build out request URI as there isn't a matching parameter in the method for the template key {key}");

            keyValue.Add(key, matchingParameter);
        }

        foreach (var kvp in keyValue)
        {
            uri = uri.Replace("{" + kvp.Key + "}", kvp.Value, true, null);
        }

        return await HttpExtensions.GetAsAsync<T>(_httpClient, uri, cancellationToken);
    }
}