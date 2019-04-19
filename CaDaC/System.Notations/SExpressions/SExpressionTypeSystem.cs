using System;
using System.Collections;
using System.Collections.Generic;

namespace System.Notations.SExpressions
{
    public delegate object SExpressionBuilder(params object[] values);

    public delegate TExpression SExpressionBuilder<TExpression>(params object[] values)
        where TExpression : class;

    public interface ISExpression<TExpression> : IReadOnlyList<TExpression>
        where TExpression : ISExpression<TExpression>
    {
        bool IsAtom { get; }
    }

    public interface ISExpression<TExpression, TValue> : ISExpression<TExpression>
        where TExpression : ISExpression<TExpression, TValue>
    {
        TValue Value { get; }
    }

    public class SExpression : SExpression<SExpression, object>
    {
        public SExpression(object value) : base(value) { }
    }

    public class SExpression<TExpression> : ISExpression<TExpression>
        where TExpression : ISExpression<TExpression>
    {
        protected object Content { get; private set; }
        protected virtual Type ContentType => Content?.GetType() ?? typeof(object);
        protected virtual string DefaultLiteral => "null";
        public SExpression(object content) =>
            Content = content;
        public override string ToString() =>
            IsAtom ? $"{ContentType}{{{Content ?? DefaultLiteral}}}" : $"( {string.Join(" ", this)} )";
        IEnumerator IEnumerable.GetEnumerator() =>
            GetEnumerator();
        public IEnumerator<TExpression> GetEnumerator() =>
            (!IsAtom ? (IEnumerable<TExpression>)Content : throw new InvalidOperationException()).GetEnumerator();
        public TExpression this[int index] => (!IsAtom ? (IReadOnlyList<TExpression>)Content : throw new InvalidOperationException())[index];
        public int Count => (!IsAtom ? (IReadOnlyList<TExpression>)Content : throw new InvalidOperationException()).Count;
        public bool IsAtom => !(Content is IEnumerable<TExpression>);
    }

    public class SExpression<TExpression, TValue> : SExpression<TExpression>, ISExpression<TExpression, TValue>
        where TExpression : ISExpression<TExpression, TValue>
    {
        protected override Type ContentType => Content?.GetType() ?? typeof(TValue);
        protected override string DefaultLiteral => typeof(TValue).IsValueType ? default(TValue).ToString() : "null";
        public SExpression(object content) :
            base
            (
                content != null ?
                (
                    content is IEnumerable<TExpression> ?
                    (
                        (
                            (content as List<TExpression>)
                            ??
                            new List<TExpression>(!(content is TExpression) ? (IEnumerable<TExpression>)content : new[] { (TExpression)content })
                        )
                        .AsReadOnly()
                    )
                    :
                    (
                        !(content is TValue) ?
                        (
                            typeof(TValue).IsValueType ?
                            Convert.ChangeType(content, typeof(TValue))
                            :
                            (typeof(TValue).IsAssignableFrom(content.GetType()) ? content : throw new InvalidOperationException())
                        )
                        :
                        content
                    )
                )
                :
                content
            ) { }
        public TValue Value => IsAtom ? (Content != null ? (TValue)Content : default(TValue)) : throw new InvalidOperationException();
    }

    public static class SExpressionFactory
    {
        public static TExpression Create<TExpression>(object content)
            where TExpression : class =>
            (TExpression)Activator.CreateInstance(typeof(TExpression), new[] { content });
    }
}