using System.Text.RegularExpressions;
using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Code.SyntaxBuilders;

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

        // Try to tokenize the UrlTemplate property now so we can report errors.
        if (!UrlTemplateTokenizer.TryTokenize(this.UrlTemplate, out var urlTemplateTokens))
        {
            builder.Diagnostics.Report(DiagnosticDefinitions.InvalidUrlTemplate.WithArguments(this.UrlTemplate));
            builder.SkipAspect();
            return;
        }
        
        // Build the URI interpolated string so we can report errors if we can't bind parameters.
        var url = new InterpolatedStringBuilder();
        foreach (var token in urlTemplateTokens)
        {
            switch (token.Kind)
            {
                case UrlTemplateTokenizer.TokenKind.Verbatim:
                    url.AddText(token.Value);
                    break;
                
                case UrlTemplateTokenizer.TokenKind.Parameter:
                    var parameterName = token.Value.Trim();
                    var parameter = builder.Target.Parameters.OfName(parameterName);
                    if (parameter == null)
                    {
                        builder.Diagnostics.Report(DiagnosticDefinitions.InvalidParameterNameInUrlTemplate.WithArguments(parameterName));
                        builder.SkipAspect();
                        return;
                    }
                    url.AddExpression(parameter);
                    break;
            }
        }
        
        var cancellationTokenParameter = builder.Target.Parameters.FirstOrDefault(p =>
            p.Type is INamedType { Name: nameof(CancellationToken)});
        
        if (Method == HttpVerb.Get)
        {
            
            if (builder.Target.ReturnType.Is(SpecialType.Void))
            {
                builder.Override( nameof(GetMethodTemplate), args: new { url, cancellationTokenParameter });
            }
            else if ( builder.Target.GetAsyncInfo().IsAwaitable )
            {
                builder.Override( nameof(GetMethodTemplateWithResult), args: new { url, cancellationTokenParameter, T = builder.Target.GetAsyncInfo().ResultType });
            }
            else
            {
                builder.Diagnostics.Report(DiagnosticDefinitions.InvalidReturnType);
                builder.SkipAspect();
                return;
            }
        }
        else
        {
            builder.Diagnostics.Report(DiagnosticDefinitions.UnsupportedVerb);
            builder.SkipAspect();
            return;
        }
    }
    
    [Template]
    private async Task GetMethodTemplate(InterpolatedStringBuilder url, IParameter? cancellationTokenParameter)
    {
         var cancellationToken = cancellationTokenParameter is not null
            ? cancellationTokenParameter.Value
            : CancellationToken.None;
        
        
        using var response = await _httpClient.GetAsync(url.ToValue(), cancellationToken);
    }

    [Template]
    private async Task<T> GetMethodTemplateWithResult<[CompileTime] T>(InterpolatedStringBuilder url, IParameter? cancellationTokenParameter)
    {
        var cancellationTokenExpression = cancellationTokenParameter ?? ExpressionFactory.Capture( CancellationToken.None );
        
        
        return await HttpExtensions.GetAsAsync<T>(_httpClient, url.ToValue(), cancellationTokenExpression.Value);
    }
}