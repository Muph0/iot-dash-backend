using System;
using System.Collections.Generic;

namespace IotDash.Utils.Collections {
    
    /// <summary>
    /// Extensions for IEnumerable.
    /// </summary>
    public static class CollectionExtensions {

        /// <summary>
        /// Execute action for each element of the <see cref="IEnumerable{T}"/>
        /// </summary>
        /// <typeparam name="T">Item of the enumerable.</typeparam>
        /// <param name="enumerable">The enumerable to iterate.</param>
        /// <param name="action">The action to execute.</param>
        public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action) {
            foreach(var t in enumerable) {
                action.Invoke(t);
            }
        }

    }
}