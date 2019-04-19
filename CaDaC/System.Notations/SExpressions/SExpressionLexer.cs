using System;
using System.Collections.Generic;
using System.Linq;

namespace System.Notations.SExpressions
{
    public class SExpressionLexer : Lexer
    {
        protected override ITokenType Match(string input, ITokenType tokenType, out string match)
        {
            string found = null;
            if (tokenType != null)
            {
                tokenType = (found = tokenType.Match(input, Offset)).Length > 0 ? tokenType : null;
            }
            else
            {
                tokenType = TokenTypes.FirstOrDefault(item => (found = item.Match(input, Offset)).Length > 0);
            }
            match = found ?? string.Empty;
            return tokenType;
        }
        public static readonly ITokenType WhiteSpace = new TokenType("space", @"\s+");
        public static readonly ITokenType Unexpected = new TokenType("unexpected", @"[^\s]+", Token.Identity);
        public static readonly ITokenType Opening = new TokenType('(', Token.Identity);
        public static readonly ITokenType Closing = new TokenType(')', Token.Identity);
        public SExpressionLexer() : this(default(ILexicon)) { }
        public SExpressionLexer(ILexicon lexicon) : this(lexicon?.TokenTypes) { }
        public SExpressionLexer(IReadOnlyList<ITokenType> tokenTypes)
            : base
            (
                new List<ITokenType>
                (
                    new[] { WhiteSpace, Opening, Closing }
                    .Concat
                    (
                        !(tokenTypes = tokenTypes ?? new ITokenType[0]).Any(item => item == null) ?
                        tokenTypes
                        :
                        throw new ArgumentException("invalid", nameof(tokenTypes))
                    )
                    .Concat(new[] { Unexpected })
                )
                .AsReadOnly()
            ) { }
    }
}