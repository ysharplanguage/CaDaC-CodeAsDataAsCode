using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace System.Notations.Runtime
{
    internal class Binder
    {
        private static readonly IDictionary<Type, Type> _binderTypes =
            new Dictionary<Type, Type>
            {
                [typeof(Action<object>)] = typeof(BindAction<>),
                [typeof(Func<object, object>)] = typeof(BindFunc<,>)
            };

        internal static Binder<TBound> Create<TBound>(object target, MethodInfo method, Type[] parameterTypes, Type returnType) =>
            (Binder<TBound>)
            Activator.CreateInstance
            (
                _binderTypes[typeof(TBound)].MakeGenericType(parameterTypes.Concat(returnType != null ? new[] { returnType } : new Type[0]).ToArray()),
                target,
                method
            );
    }

    internal abstract class Binder<TBound>
    {
        internal abstract TBound Bound { get; }
    }

    internal abstract class BindAction1 : Binder<Action<object>>
    {
    }

    internal class BindAction<T> : BindAction1
    {
        private readonly Action<object> _bound;
        private readonly Action<T> _action;

        public BindAction(object target, MethodInfo method)
        {
            _action = (Action<T>)method.CreateDelegate(typeof(Action<T>), target);
            _bound = arg => _action((T)arg);
        }

        internal override Action<object> Bound => _bound;
    }

    internal abstract class BindFunc1 : Binder<Func<object, object>>
    {
    }

    internal class BindFunc<T, TResult> : BindFunc1
    {
        private readonly Func<object, object> _bound;
        private readonly Func<T, TResult> _func;

        public BindFunc(object target, MethodInfo method)
        {
            _func = (Func<T, TResult>)method.CreateDelegate(typeof(Func<T, TResult>), target);
            _bound = arg => _func((T)arg);
        }

        internal override Func<object, object> Bound => _bound;
    }

    public class DoubleDispatchObject
    {
        private readonly IDictionary<Type, Action<object>> _action1 = new Dictionary<Type, Action<object>>();
        private readonly IDictionary<Type, Func<object, object>> _function1 = new Dictionary<Type, Func<object, object>>();
        private readonly object _target;

        private void PrepareBinder<TBound>(IDictionary<Type, TBound> dispatch, int parameterCount, bool isFunc)
        {
            _target
            .GetType()
            .GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Where
            (
                m =>
                    (isFunc ? m.ReturnType != typeof(void) : m.ReturnType == typeof(void)) &&
                    m.GetParameters().Length == parameterCount &&
                    !m.GetParameters().Any(p => p.ParameterType.ContainsGenericParameters)
            )
            .Aggregate
            (
                dispatch,
                (map, method) =>
                {
                    var parameterTypes = method.GetParameters().Select(p => p.ParameterType).ToArray();
                    var returnType = method.ReturnType != typeof(void) ? method.ReturnType : null;
                    var binder = Binder.Create<TBound>(_target, method, parameterTypes, returnType);
                    map.Add(parameterTypes[0], binder.Bound);
                    return dispatch;
                }
            );
        }

        public DoubleDispatchObject() : this(null) { }

        public DoubleDispatchObject(object target)
        {
            _target = target ?? this;
            PrepareBinder(_action1, 1, false);
            PrepareBinder(_function1, 1, true);
        }

        public void Via<T>(Action<T> action, T arg) =>
            Via(action, arg, null);

        public void Via<T>(Action<T> action, T arg, Action orElse)
        {
            var type = arg?.GetType();
            if ((type != null) && _action1.TryGetValue(type, out var bound))
            {
                bound(arg);
                return;
            }
            orElse?.Invoke();
        }

        public TResult Via<T, TResult>(Func<T, TResult> function, T arg) =>
            Via(function, arg, null, default(TResult));

        public TResult Via<T, TResult>(Func<T, TResult> function, T arg, Func<TResult> orElse) =>
            Via(function, arg, orElse, default(TResult));

        public TResult Via<T, TResult>(Func<T, TResult> function, T arg, TResult defaultResult) =>
            Via(function, arg, null, defaultResult);

        public TResult Via<T, TResult>(Func<T, TResult> function, T arg, Func<TResult> orElse, TResult defaultResult)
        {
            var type = arg?.GetType();
            if ((type != null) && _function1.TryGetValue(type, out var bound))
            {
                return (TResult)bound(arg);
            }
            return orElse != null ? orElse() : defaultResult;
        }
    }
}