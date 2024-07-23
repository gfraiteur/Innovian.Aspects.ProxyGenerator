using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.SyntaxBuilders;

namespace Innovian.Aspects.ProxyGenerator;

public sealed class ProxyGeneratorAttribute : TypeAspect
{
    /// <summary>
    /// The name of the registered HTTP client to connect to the service API with.
    /// </summary>
    public string HttpClientName { get; set; }

    public ProxyGeneratorAttribute(string httpClientName)
    {
        HttpClientName = httpClientName;
    }

    /// <inheritdoc />
    public override void BuildAspect(IAspectBuilder<INamedType> builder)
    {
        base.BuildAspect(builder);

        var builderClass = builder
            .With(builder.Target.GetNamespace()!)
            .WithChildNamespace("Proxies")
            .IntroduceClass(builder.Target.Name + "Proxy");
        
        //Introduce the field to the type
        const string httpClientFieldName = "_httpClient";
        builderClass.IntroduceField(
            httpClientFieldName,
            typeof(HttpClient),
            IntroductionScope.Instance,
            buildField: b =>
            {
                b.Accessibility = Accessibility.Private;
                b.Writeability = Writeability.ConstructorOnly;
            });

        //Inject the template constructor
        builderClass.IntroduceConstructor(nameof(ConstructorTemplate),
            OverrideStrategy.Ignore,
            b =>
            {
                b.Accessibility = Accessibility.Public;
            });

        const string httpClientFactoryParameterName = "httpClientFactory";
        if (builderClass.Target.Constructors.Count > 0)
        {
            foreach (var ctr in builderClass.Target.Constructors)
            {
                if (ctr.Parameters.All(
                        p => p.Name != httpClientFactoryParameterName &&
                             p.Type.GetType() != typeof(IHttpClientFactory)))
                {
                    //Insert the IHttpClientFactory as a parameter on the constructor
                    builder.Advice.IntroduceParameter(
                        ctr,
                        httpClientFactoryParameterName,
                        typeof(IHttpClientFactory),
                        TypedConstant.Default(typeof(IHttpClientFactory)));

                    var exprBuilder = new ExpressionBuilder();
                    exprBuilder.AppendVerbatim(
                        $"{httpClientFieldName} = {httpClientFactoryParameterName}.CreateClient(\"{HttpClientName}\")");
                    builder.Advice.AddInitializer(ctr, StatementFactory.FromExpression(exprBuilder.ToExpression()));
                }
            }
        }

        //if (builder.Target.Methods.Count > 0)
        //{
        //    foreach (var method in builderClass.Target.Methods)
        //    {
        //        //If the method isn't decorated with a `ApiMethodAttribute`, skip it
        //        if (method.Attributes.Any(typeof(ApiMethodAttribute)) != true)
        //            continue;

        //        if (method.ReturnType == TypeFactory.GetType(SpecialType.Void))
        //        {
        //            builder.Advice.IntroduceMethod(method, nameof(VoidMethodTemplate), IntroductionScope.Default,
        //                OverrideStrategy.Override,
        //                b =>
        //                {
        //                    b.ReturnType = TypeFactory.GetType(SpecialType.Task);
        //                    b.IsReadOnly = false;
        //                    b.Accessibility = Accessibility.Public;

        //                });


        //        }
        //    }
        //}
    }
    
    [Template]
    public void ConstructorTemplate()
    {
    }

    [Template]
    public void VoidMethodTemplate()
    {

    }
}