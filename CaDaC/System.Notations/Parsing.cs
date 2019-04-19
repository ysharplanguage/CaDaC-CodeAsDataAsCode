using System;

namespace System.Notations
{
    public delegate TModel Reducer<TContext, TModel>(TContext context, TModel outer, TModel model, object value);

    public interface IParser<TModel>
    {
        TModel Parse(string input);
        TModel Parse(string input, ILexer lexer);
        TModel Parse<TContext>(string input, Reducer<TContext, TModel> reducer);
        TModel Parse<TContext>(string input, Reducer<TContext, TModel> reducer, TContext context);
        TModel Parse<TContext>(string input, ILexer lexer, Reducer<TContext, TModel> reducer);
        TModel Parse<TContext>(string input, ILexer lexer, Reducer<TContext, TModel> reducer, TContext context);
    }

    public abstract class Parser<TModel> : IParser<TModel>
    {
        protected static T Require<T>(T instance)
            where T : class =>
            instance ?? throw new InvalidOperationException();
        protected static Exception SyntaxError(string parameterName, ILexer lexer, IToken token) =>
            SyntaxError(parameterName, lexer, token, null);
        protected static Exception SyntaxError(string parameterName, ILexer lexer, ITokenType expectedTokenType) =>
            SyntaxError(parameterName, lexer, null, expectedTokenType);
        protected static Exception SyntaxError(string parameterName, ILexer lexer, IToken token, ITokenType expectedTokenType)
        {
            parameterName = parameterName ?? throw new ArgumentNullException(nameof(parameterName));
            lexer = lexer ?? throw new ArgumentNullException(nameof(lexer));
            token = token ?? lexer.NewToken(TokenType.EOF, string.Empty);
            var message = $"syntax error: {expectedTokenType ?? TokenType.EOF} expected but found {token} at {token.SourceInfo}";
            return new ArgumentException(message, parameterName);
        }
        protected abstract ILexer GetLexer<TContext>(TContext context);
        protected abstract Reducer<TContext, TModel> GetDefaultReducer<TContext>(TContext context);
        protected abstract TModel Parse<TContext>(ILexer lexer, string input, Reducer<TContext, TModel> reducer, TContext context);
        public TModel Parse(string input) =>
            Parse(input, default(ILexer));
        public TModel Parse(string input, ILexer lexer) =>
            Parse(input, lexer, default(Reducer<object, TModel>));
        public TModel Parse<TContext>(string input, Reducer<TContext, TModel> reducer) =>
            Parse(input, reducer, default(TContext));
        public TModel Parse<TContext>(string input, Reducer<TContext, TModel> reducer, TContext context) =>
            Parse(input, null, reducer, context);
        public TModel Parse<TContext>(string input, ILexer lexer, Reducer<TContext, TModel> reducer) =>
            Parse(input, lexer, reducer, default(TContext));
        public TModel Parse<TContext>(string input, ILexer lexer, Reducer<TContext, TModel> reducer, TContext context)
        {
            input = input ?? throw new ArgumentNullException(nameof(input));
            var parse = Parse(Require(lexer = lexer ?? GetLexer(context)), input, Require(reducer ?? GetDefaultReducer(context)), context);
            var token = lexer.NextOf(input, true);
            if (token != null)
            {
                // Reject any garbage present at the end of the input
                throw SyntaxError(nameof(input), lexer, token);
            }
            return parse;
        }
    }
}