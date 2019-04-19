using System;
using System.Collections.Generic;

namespace System.Notations.Extensions
{
    public static class AnonymousType
    {
        public static T As<T>(T prototype, object instance) where T : class =>
            (T)instance;
        public static Action<T> Action<T>(Action<T> action, T prototype) =>
            action;
        public static Action<T1, T2> Action<T1, T2>(Action<T1, T2> action, T1 prototype1, T2 prototype2) =>
            action;
        public static Action<T1, T2, T3> Action<T1, T2, T3>(Action<T1, T2, T3> action, T1 prototype1, T2 prototype2, T3 prototype3) =>
            action;
        public static Action<T1, T2, T3, T4> Action<T1, T2, T3, T4>(Action<T1, T2, T3, T4> action, T1 prototype1, T2 prototype2, T3 prototype3, T4 prototype4) =>
            action;
        public static Action<T1, T2, T3, T4, T5> Action<T1, T2, T3, T4, T5>(Action<T1, T2, T3, T4, T5> action, T1 prototype1, T2 prototype2, T3 prototype3, T4 prototype4, T5 prototype5) =>
            action;
        public static Action<T1, T2, T3, T4, T5, T6> Action<T1, T2, T3, T4, T5, T6>(Action<T1, T2, T3, T4, T5, T6> action, T1 prototype1, T2 prototype2, T3 prototype3, T4 prototype4, T5 prototype5, T6 prototype6) =>
            action;
        public static Action<T1, T2, T3, T4, T5, T6, T7> Action<T1, T2, T3, T4, T5, T6, T7>(Action<T1, T2, T3, T4, T5, T6, T7> action, T1 prototype1, T2 prototype2, T3 prototype3, T4 prototype4, T5 prototype5, T6 prototype6, T7 prototype7) =>
            action;
        public static Action<T1, T2, T3, T4, T5, T6, T7, T8> Action<T1, T2, T3, T4, T5, T6, T7, T8>(Action<T1, T2, T3, T4, T5, T6, T7, T8> action, T1 prototype1, T2 prototype2, T3 prototype3, T4 prototype4, T5 prototype5, T6 prototype6, T7 prototype7, T8 prototype8) =>
            action;
        public static Action<T1, T2, T3, T4, T5, T6, T7, T8, T9> Action<T1, T2, T3, T4, T5, T6, T7, T8, T9>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9> action, T1 prototype1, T2 prototype2, T3 prototype3, T4 prototype4, T5 prototype5, T6 prototype6, T7 prototype7, T8 prototype8, T9 prototype9) =>
            action;
        public static Func<TResult> Func<TResult>(Func<TResult> function, TResult resultPrototype) =>
            function;
        public static Func<T, TResult> Func<T, TResult>(Func<T, TResult> function, T prototype, TResult resultPrototype) =>
            function;
        public static Func<T1, T2, TResult> Func<T1, T2, TResult>(Func<T1, T2, TResult> function, T1 prototype1, T2 prototype2, TResult resultPrototype) =>
            function;
        public static Func<T1, T2, T3, TResult> Func<T1, T2, T3, TResult>(Func<T1, T2, T3, TResult> function, T1 prototype1, T2 prototype2, T3 prototype3, TResult resultPrototype) =>
            function;
        public static Func<T1, T2, T3, T4, TResult> Func<T1, T2, T3, T4, TResult>(Func<T1, T2, T3, T4, TResult> function, T1 prototype1, T2 prototype2, T3 prototype3, T4 prototype4, TResult resultPrototype) =>
            function;
        public static Func<T1, T2, T3, T4, T5, TResult> Func<T1, T2, T3, T4, T5, TResult>(Func<T1, T2, T3, T4, T5, TResult> function, T1 prototype1, T2 prototype2, T3 prototype3, T4 prototype4, T5 prototype5, TResult resultPrototype) =>
            function;
        public static Func<T1, T2, T3, T4, T5, T6, TResult> Func<T1, T2, T3, T4, T5, T6, TResult>(Func<T1, T2, T3, T4, T5, T6, TResult> function, T1 prototype1, T2 prototype2, T3 prototype3, T4 prototype4, T5 prototype5, T6 prototype6, TResult resultPrototype) =>
            function;
        public static Func<T1, T2, T3, T4, T5, T6, T7, TResult> Func<T1, T2, T3, T4, T5, T6, T7, TResult>(Func<T1, T2, T3, T4, T5, T6, T7, TResult> function, T1 prototype1, T2 prototype2, T3 prototype3, T4 prototype4, T5 prototype5, T6 prototype6, T7 prototype7, TResult resultPrototype) =>
            function;
        public static Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult> Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult>(Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult> function, T1 prototype1, T2 prototype2, T3 prototype3, T4 prototype4, T5 prototype5, T6 prototype6, T7 prototype7, T8 prototype8, TResult resultPrototype) =>
            function;
        public static Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult> Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult> function, T1 prototype1, T2 prototype2, T3 prototype3, T4 prototype4, T5 prototype5, T6 prototype6, T7 prototype7, T8 prototype8, T9 prototype9, TResult resultPrototype) =>
            function;
        public static T Class<T>(T prototype) where T : class =>
            null;
        public static T New<T>(T prototype, params object[] arguments) where T : class =>
            (T)Activator.CreateInstance(typeof(T), arguments ?? new object[] { null });
        public static T[] ArrayOf<T>(T prototype, params T[] values) where T : class =>
            values ?? new T[] { null };
        public static IDictionary<TKey, TValue> DictionaryOf<TKey, TValue>(TKey keyPrototype, TValue valuePrototype) where TKey : class where TValue : class =>
            new Dictionary<TKey, TValue>();
        public static IList<T> ListOf<T>(T prototype, params T[] values) where T : class =>
            new List<T>(values ?? new T[] { null });
        public static IReadOnlyList<T> ReadOnlyListOf<T>(T prototype, params T[] values) where T : class =>
            new List<T>(values ?? new T[] { null }).AsReadOnly();
        public static ISet<T> SetOf<T>(T prototype, params T[] values) where T : class =>
            new HashSet<T>(values ?? new T[] { null });
    }
}