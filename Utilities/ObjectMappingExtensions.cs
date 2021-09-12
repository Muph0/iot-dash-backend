using IotDash.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Reflection;
using System.Security.Claims;

namespace IotDash.Extensions.ObjectMapping {
    public static class ObjectMappingExtensions {

        public static IEnumerable<T> RecursiveEnumerate<T>(this T obj, Func<T, T?> next) {
            T? node = obj;
            while (node != null) {
                yield return node;
                node = next(node);
            }
        }

        public static int CopyTo<TSource, TDest>(this TSource source, TDest destination,
                IEnumerable<string>? except = null, bool throwIfNoSource = false, bool throwIfNoTarget = true, bool mapNulls = false) {

            HashSet<string> exceptProps = new(except ?? Enumerable.Empty<string>());

            var sourceProps = typeof(TSource).GetProperties(BindingFlags.Public | BindingFlags.Instance).ToDictionary(p => p.Name);
            var destProps = typeof(TDest).GetProperties(BindingFlags.Public | BindingFlags.Instance).ToDictionary(p => p.Name);

            if (throwIfNoTarget) {
                foreach (var p in sourceProps.Values) {
                    if (!destProps.ContainsKey(p.Name) && !exceptProps.Contains(p.Name)) {
                        throw new InvalidOperationException($"No target property for source property '{p.Name}'.");
                    }
                }
            }

            if (throwIfNoSource) {
                foreach (var p in destProps.Values) {
                    if (!sourceProps.ContainsKey(p.Name) && !exceptProps.Contains(p.Name)) {
                        throw new InvalidOperationException($"No source property for destination property '{p.Name}'.");
                    }
                }
            }

            int mapped = 0;

            foreach (var p in sourceProps.Values) {
                object? sourceValue = p.GetValue(source);
                if (destProps.ContainsKey(p.Name) && !exceptProps.Contains(p.Name) && (sourceValue != null || mapNulls)) {
                    mapped++;
                    destProps[p.Name].SetValue(destination, sourceValue);
                }
            }

            return mapped;
        }

    }
}