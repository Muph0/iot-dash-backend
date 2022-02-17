using System;
using System.Collections.Generic;

namespace IotDash.Utils.Collections {
    public static class CollectionExtensions {

        public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action) {
            foreach(var t in enumerable) {
                action.Invoke(t);
            }
        }

    }
}