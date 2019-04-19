using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace System.Notations
{
    public delegate object TokenMapper(IToken token);

    public interface ISourceInfo
    {
        int Offset { get; }
        int Line { get; }
        int Column { get; }
    }

    public interface ITokenType
    {
        string Match(string input, int offset);
        object ValueOf(IToken token);
        char? Char { get; }
        string Literal { get; }
        StringComparison ComparisonType { get; }
        int MaxLength { get; }
        string Pattern { get; }
        Regex Regex { get; }
        bool IsOneChar { get; }
        bool IsNonSignificant { get; }
    }

    public interface IToken
    {
        ITokenType TokenType { get; }
        string Match { get; }
        ISourceInfo SourceInfo { get; }
    }

    public class SourceInfo : ISourceInfo
    {
        public SourceInfo() : this(0, 1, 1) { }
        public SourceInfo(ISourceInfo other) : this((other ?? throw new ArgumentNullException(nameof(other))).Offset, other.Line, other.Column) { }
        public SourceInfo(int offset, int line, int column)
        {
            Offset = offset;
            Line = line;
            Column = column;
        }
        public override string ToString() =>
            $"line {Line}, column {Column} (offset {Offset})";
        public virtual int Offset { get; protected set; }
        public virtual int Line { get; protected set; }
        public virtual int Column { get; protected set; }
    }

    public class TokenType : ITokenType
    {
        protected TokenType(string literal, StringComparison comparisonType, int maxLength, TokenMapper tokenMapper)
        {
            Literal = ((TokenMapper = tokenMapper) != null) ? (!string.IsNullOrEmpty(literal) ? literal : throw new ArgumentException("cannot be null or empty", nameof(literal))) : (literal ?? string.Empty);
            ComparisonType = comparisonType;
            MaxLength = !IsOneChar ? (maxLength >= -1 ? maxLength : throw new ArgumentOutOfRangeException(nameof(maxLength))) : 1;
        }
        protected TokenMapper TokenMapper { get; private set; }
        public static readonly ITokenType EOF = new TokenType(null);
        public TokenType(char literal) : this(literal, null) { }
        public TokenType(char literal, TokenMapper tokenMapper) : this(new string(literal, 1), tokenMapper) =>
            Char = literal;
        public TokenType(string literal) : this(literal, StringComparison.Ordinal) { }
        public TokenType(string literal, TokenMapper tokenMapper) : this(literal, StringComparison.Ordinal, tokenMapper) { }
        public TokenType(string literal, StringComparison comparisonType) : this(literal, comparisonType, null) { }
        public TokenType(string literal, StringComparison comparisonType, TokenMapper tokenMapper) : this(literal, comparisonType, -1, tokenMapper) { }
        public TokenType(string literal, string pattern) : this(literal, pattern, null) { }
        public TokenType(string literal, string pattern, TokenMapper tokenMapper) : this(literal, pattern, RegexOptions.None, tokenMapper) { }
        public TokenType(string literal, string pattern, RegexOptions regexOptions) : this(literal, pattern, regexOptions, null) { }
        public TokenType(string literal, string pattern, RegexOptions regexOptions, TokenMapper tokenMapper) : this(literal, pattern, -1, regexOptions, tokenMapper) { }
        public TokenType(string literal, string pattern, int maxLength) : this(literal, pattern, maxLength, null) { }
        public TokenType(string literal, string pattern, int maxLength, TokenMapper tokenMapper) : this(literal, pattern, maxLength, RegexOptions.None, tokenMapper) { }
        public TokenType(string literal, string pattern, int maxLength, RegexOptions regexOptions) : this(literal, pattern, maxLength, regexOptions, null) { }
        public TokenType(string literal, string pattern, int maxLength, RegexOptions regexOptions, TokenMapper tokenMapper) : this(literal, StringComparison.Ordinal, maxLength, tokenMapper)
        {
            Regex = new Regex($"{(Pattern = !string.IsNullOrEmpty(pattern) ? pattern : throw new ArgumentException("cannot be null or empty", nameof(pattern)))}", regexOptions);
        }
        public override int GetHashCode() =>
            Literal.GetHashCode();
        public override string ToString() =>
            Regex == null ? $"{(Literal.Length > 0 ? Literal : "end of input")}" : Literal;
        public virtual string Match(string input, int offset)
        {
            string capture;
            bool success;
            input = input ?? throw new ArgumentNullException(nameof(input));
            if (Regex != null)
            { // regular expression-match
                var match = MaxLength > 0 ? Regex.Match(input, offset, Math.Min(MaxLength, input.Length - offset)) : Regex.Match(input, offset);
                success = match.Success && (match.Index == offset);
                capture = success ? match.Value : string.Empty;
            }
            else
            { // literal match or single-character match
                if (Literal.Length <= input.Length - offset)
                { // before EOF
                    if (!IsOneChar)
                    { // literal match
                        success = string.Compare(capture = input.Substring(offset, Literal.Length), Literal, ComparisonType) == 0;
                        capture = success ? capture : string.Empty;
                    }
                    else
                    {
                        success = input[offset] == Char.Value;
                        capture = success ? Literal : string.Empty;
                    }
                }
                else
                { // at or after EOF
                    capture = string.Empty;
                }
            }
            return capture;
        }
        public virtual object ValueOf(IToken token) =>
            !IsNonSignificant ? TokenMapper(token) : throw new InvalidOperationException();
        public virtual char? Char { get; private set; }
        public virtual string Literal { get; private set; }
        public virtual StringComparison ComparisonType { get; private set; }
        public virtual int MaxLength { get; private set; }
        public virtual string Pattern { get; private set; }
        public virtual Regex Regex { get; private set; }
        public virtual bool IsOneChar => Char.HasValue;
        public virtual bool IsNonSignificant => TokenMapper == null;
    }

    public class Token : IToken
    {
        public static readonly TokenMapper Identity = token => token;
        public Token(ITokenType tokenType, string match) : this(tokenType, match, null) { }
        public Token(ITokenType tokenType, string match, ISourceInfo sourceInfo)
        {
            TokenType = tokenType ?? throw new ArgumentNullException(nameof(tokenType));
            Match = match ?? throw new ArgumentNullException(nameof(match));
            SourceInfo = sourceInfo;
        }
        public override string ToString() =>
            TokenType.Regex == null ?
            (!TokenType.IsOneChar ? $"{(Match.Length > 0 ? $"{TokenType} {Match}" : TokenType.ToString())}" : Match)
            :
            $"{TokenType} {Match}";
        public virtual ITokenType TokenType { get; private set; }
        public virtual string Match { get; private set; }
        public virtual ISourceInfo SourceInfo { get; private set; }
    }

    public interface ILexicon
    {
        IReadOnlyList<ITokenType> TokenTypes { get; }
    }

    public interface ILexer : ISourceInfo
    {
        IToken NewToken(ITokenType tokenType, string match);
        IToken NextOf(string input);
        IToken NextOf(string input, bool skipWhiteSpace);
        IToken NextOf(string input, ITokenType tokenType);
        IToken NextOf(string input, ITokenType tokenType, bool skipWhiteSpace);
        IToken NextOf(string input, ITokenType[] tokenTypes);
        IToken NextOf(string input, ITokenType[] tokenTypes, bool skipWhiteSpace);
        IToken Consume(IToken token);
        IReadOnlyList<ITokenType> TokenTypes { get; }
    }

    public class Lexicon : ILexicon
    {
        public Lexicon() : this(null) { }
        public Lexicon(IReadOnlyList<ITokenType> tokenTypes) =>
            TokenTypes =
                !(tokenTypes = tokenTypes ?? new ITokenType[0]).Any(item => item == null) ?
                tokenTypes
                :
                throw new ArgumentException("invalid", nameof(tokenTypes));
        public IReadOnlyList<ITokenType> TokenTypes { get; private set; }
    }

    public abstract class Lexer : SourceInfo, ILexer
    {
        private bool TryNextOf(string input, ITokenType tokenType, out IToken token)
        {
            string match;
            tokenType = Match(input, tokenType, out match);
            if (tokenType != null)
            {
                token = NewToken(tokenType, match);
                return true;
            }
            token = null;
            return false;
        }
        private void SetPosition(int offset, int line, int column)
        {
            Offset = offset;
            Line = line;
            Column = column;
        }
        protected Lexer() : this(null) { }
        protected Lexer(IReadOnlyList<ITokenType> tokenTypes) =>
            TokenTypes =
                !(tokenTypes = tokenTypes ?? new ITokenType[0]).Any(item => item == null) ?
                tokenTypes
                :
                throw new ArgumentException("invalid", nameof(tokenTypes));
        protected abstract ITokenType Match(string input, ITokenType tokenType, out string match);
        protected virtual ISourceInfo NewSourceInfo() =>
            new SourceInfo(this);
        public virtual IToken NewToken(ITokenType tokenType, string match) =>
            new Token(tokenType, match, NewSourceInfo());
        public IToken NextOf(string input) =>
            NextOf(input, false);
        public IToken NextOf(string input, bool skipWhiteSpace) =>
            NextOf(input, default(ITokenType), skipWhiteSpace);
        public IToken NextOf(string input, ITokenType tokenType) =>
            NextOf(input, tokenType, false);
        public IToken NextOf(string input, ITokenType tokenType, bool skipWhiteSpace)
        {
            input = input ?? throw new ArgumentNullException(nameof(input));
            IToken token = null;
            if (skipWhiteSpace)
            {
                bool more;
                while
                (
                    (more = Offset < input.Length) &&
                    TryNextOf(input, tokenType, out token) &&
                    token.TokenType.IsNonSignificant
                )
                {
                    Consume(token);
                    token = null;
                }
            }
            else
            {
                TryNextOf(input, tokenType, out token);
            }
            return token;
        }
        public IToken NextOf(string input, ITokenType[] tokenTypes) =>
            NextOf(input, tokenTypes, false);
        public IToken NextOf(string input, ITokenType[] tokenTypes, bool skipWhiteSpace)
        {
            IToken token = null;
            return
                tokenTypes != null ?
                tokenTypes
                .Where(tokenType => tokenType != null)
                .Select
                (
                    tokenType =>
                        token = NextOf(input, tokenType, skipWhiteSpace)
                )
                .FirstOrDefault(tokenType => token != null)
                :
                NextOf(input, skipWhiteSpace);
        }
        public IToken Consume(IToken token)
        {
            var match = (token ?? throw new ArgumentNullException(nameof(token))).Match;
            var offset = Offset;
            var line = Line;
            var column = Column;
            var at = 0;
            offset += match.Length;
            while (at < match.Length)
            {
                if (match[at++] == '\n')
                {
                    line++;
                    column = 1;
                }
                else
                {
                    column++;
                }
            }
            SetPosition(offset, line, column);
            return token;
        }
        public IReadOnlyList<ITokenType> TokenTypes { get; private set; }
    }
}