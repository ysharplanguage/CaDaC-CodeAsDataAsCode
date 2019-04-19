using System;
using System.Linq;

namespace System.Notations.SExpressions.Extensions
{
    public static class SExpressionExtensions
    {
        public static SExpressionBuilder SExpressionBuilder(this object anchor) =>
            values =>
                values ?? new object[] { null };
        public static SExpressionBuilder<TExpression> SExpressionBuilder<TExpression>(this TExpression anchor)
            where TExpression : class =>
            values =>
                SExpressionFactory
                .Create<TExpression>
                (
                    (values ?? new object[] { null })
                    .Select
                    (
                        value => !(value is TExpression)
                        ?
                        SExpressionFactory.Create<TExpression>(value)
                        :
                        (TExpression)value
                    )
                    .ToArray()
                );
        public static SExpressionBuilder<TExpression> SExpressionBuilder<TExpression>(this TExpression anchor, Reducer<object, TExpression> reducer)
            where TExpression : class =>
            SExpressionBuilder(anchor, reducer, null);
        public static SExpressionBuilder<TExpression> SExpressionBuilder<TContext, TExpression>(this TExpression anchor, Reducer<TContext, TExpression> reducer, TContext context)
            where TExpression : class =>
            values =>
                reducer
                (
                    context, null, null,
                    (values ?? new object[] { null })
                    .Select
                    (
                        value => !(value is TExpression)
                        ?
                        reducer(context, null, null, value)
                        :
                        (TExpression)value
                    )
                    .ToArray()
                );
        public static Reducer<TContext, TExpression> SExpressionReducer<TContext, TExpression>(this TExpression anchor, TContext prototype, Reducer<TContext, TExpression> reducer)
            where TExpression : class =>
            reducer;
    }
}