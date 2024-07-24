using Metalama.Framework.Aspects;
using Metalama.Framework.Diagnostics;

namespace Innovian.Aspects.ProxyGenerator;

[CompileTime]
internal static class DiagnosticDefinitions
{
    public static DiagnosticDefinition<string> InvalidUrlTemplate =
        new DiagnosticDefinition<string>("INOVIAN01", Severity.Error, "Invalid UrlTemplate: '{0}'.");
    
    public static DiagnosticDefinition<string> InvalidParameterNameInUrlTemplate =
        new DiagnosticDefinition<string>("INOVIAN02", Severity.Error, "Invalid parameter '{0}' in UrlTemplate.");
    
    public static DiagnosticDefinition InvalidReturnType =
        new DiagnosticDefinition("INOVIAN03", Severity.Error, "Invalid return type.");
    
    public static DiagnosticDefinition UnsupportedVerb =
        new DiagnosticDefinition("INOVIAN04", Severity.Error, "Unsupported HTTP Verb.");

}