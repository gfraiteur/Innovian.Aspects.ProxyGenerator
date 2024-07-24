using Metalama.Framework.Aspects;

namespace Innovian.Aspects.ProxyGenerator;

[CompileTime]
internal static class UrlTemplateTokenizer
{

    public static bool TryTokenize( string urlTemplate, out IReadOnlyList<Token> tokens )
    {
        // A simplistic implementation.
        var tokenList = new List<Token>();
        tokens = tokenList;
        var currentTokenStart = 0;
        var currentTokenKind = TokenKind.Verbatim;

        for (var i = 0; i < urlTemplate.Length; i++)
        {
            var c = urlTemplate[i];

            switch (c)
            {
                case '{':
                    if (!TryAddToken(TokenKind.Verbatim, i))
                    {
                        return false;
                    }
                    currentTokenKind = TokenKind.Parameter;
                    currentTokenStart = i + 1;
                    break;
                
                case '}':
                    if (!TryAddToken(TokenKind.Parameter, i))
                    {
                        return false;
                    }
                    currentTokenKind = TokenKind.Verbatim;
                    currentTokenStart = i + 1;
                    break;
            }
        }

        if (currentTokenKind == TokenKind.Parameter)
        {
            return false;
        }

        if (!TryAddToken(TokenKind.Verbatim, urlTemplate.Length))
        {
            return false;
        }

        return true;

        bool TryAddToken( TokenKind kind, int tokenEnd)
        {
            if (currentTokenKind != kind)
            {
                return false;
            }
            
            if (tokenEnd > currentTokenStart)
            {
                var value = urlTemplate.Substring(currentTokenStart, tokenEnd - currentTokenStart);
                tokenList.Add(new Token(kind, value));
            }
            else
            {
                // Ignore empty tokens.
            }
            
            return true;
        }

    }

    internal enum TokenKind
    {
        Verbatim,
        Parameter
    }

    internal record Token(TokenKind Kind, string Value);


}