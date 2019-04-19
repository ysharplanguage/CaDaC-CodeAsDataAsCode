using System;
using System.Collections.Generic;

namespace System.Notations.SExpressions
{
    public class SExpressionParser : SExpressionParser<SExpression>
    {
        public SExpressionParser() : this(default(ILexicon)) { }
        public SExpressionParser(ILexicon lexicon) : base(lexicon) { }
    }

    public class SExpressionParser<TExpression> : Parser<TExpression>
        where TExpression : ISExpression<TExpression>
    {
        private readonly ILexicon _lexicon;
        private TExpression Parse<TContext>(ILexer lexer, string input, IToken lookAhead, Reducer<TContext, TExpression> reduce, TContext context, TExpression outer)
        {
            var opening = SExpressionLexer.Opening;
            var closing = SExpressionLexer.Closing;
            var unexpected = SExpressionLexer.Unexpected;
            TExpression expression;
            if ((lookAhead != null) && (lookAhead.TokenType == opening))
            {
                lexer.Consume(lookAhead);
                var sofar = new List<TExpression>();
                expression = SExpressionFactory.Create<TExpression>(sofar);
                while
                (
                    ((lookAhead = lexer.NextOf(input, true)) != null) &&
                    (lookAhead.TokenType != closing) &&
                    (lookAhead.TokenType != unexpected)
                )
                {
                    sofar.Add(Parse(lexer, input, lookAhead, reduce, context, expression));
                }
                if ((lookAhead != null) && (lookAhead.TokenType != unexpected))
                {
                    lexer.Consume(lookAhead);
                }
                else
                {
                    throw SyntaxError(nameof(input), lexer, lookAhead, closing);
                }
            }
            else if (lookAhead.TokenType != unexpected)
            {
                expression = SExpressionFactory.Create<TExpression>(lookAhead.TokenType.ValueOf(lexer.Consume(lookAhead)));
            }
            else
            {
                throw SyntaxError(nameof(input), lexer, lookAhead, opening);
            }
            return reduce(context, outer, expression);
        }
        protected override ILexer GetLexer<TContext>(TContext context) =>
            new SExpressionLexer(_lexicon);
        protected override Reducer<TContext, TExpression> GetDefaultReducer<TContext>(TContext context) =>
            (ignored, outer, expression) =>
                expression;
        protected override TExpression Parse<TContext>(ILexer lexer, string input, Reducer<TContext, TExpression> reducer, TContext context) =>
            Parse(lexer, input, lexer.NextOf(input, true), reducer, context, default(TExpression));
        //public static string Stringify(object expression) =>
        //        expression is IReadOnlyCollection<object> ?
        //        ((IReadOnlyCollection<object>)expression).Count > 0 ? ((IEnumerable<object>)expression).Aggregate(new StringBuilder("("), (result, obj) => result.AppendFormat(" {0}", Stringify(obj))).Append(" )").ToString() : "( )"
        //        :
        //        (expression != null ? (expression is string ? $"\"{((string)expression).Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\r", "\\r").Replace("\n", "\\n")}\"" : expression.ToString()) : "null");
        public SExpressionParser() : this(default(ILexicon)) { }
        public SExpressionParser(ILexicon lexicon) =>
            _lexicon = lexicon;
    }
}