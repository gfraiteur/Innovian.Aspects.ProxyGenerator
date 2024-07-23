using Innovian.Aspects.ProxyGenerator;
using Metalama.Framework.Aspects;

[assembly: AspectOrder(AspectOrderDirection.RunTime, typeof(ProxyGeneratorAttribute))]