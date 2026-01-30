#nullable enable
using _0xOptimizer.Core.Operations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace _0xOptimizer.UI.Tweaks
{
    internal sealed record AutoTweakEntry(
        string Group,
        string Title,
        string SubTitle,
        Type OperationType,
        bool RequiresAdmin
    );

    internal static class AutoTweakDiscovery
    {
        public static IReadOnlyList<AutoTweakEntry> Discover()
        {
            // Pega todas as assemblies já carregadas (inclui a do Core).
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            var opInterface = typeof(IOperation);

            var ops =
                assemblies
                    .SelectMany(SafeGetTypes)
                    .Where(t =>
                        t is not null &&
                        t.IsClass &&
                        !t.IsAbstract &&
                        opInterface.IsAssignableFrom(t) &&
                        HasPublicParameterlessCtor(t) &&
                        t.FullName != null &&
                        t.FullName.StartsWith("_0xOptimizer.Core.Operations.", StringComparison.Ordinal))
                    .Select(t => CreateEntry(t!))
                    .OrderBy(e => e.Group)
                    .ThenBy(e => e.Title)
                    .ToList();

            return ops;
        }

        private static IEnumerable<Type> SafeGetTypes(Assembly a)
        {
            try { return a.GetTypes(); }
            catch { return Array.Empty<Type>(); }
        }

        private static bool HasPublicParameterlessCtor(Type t)
            => t.GetConstructor(Type.EmptyTypes) != null;

        private static AutoTweakEntry CreateEntry(Type t)
        {
            // Grupo = sub-namespace após Core.Operations.
            // Ex: _0xOptimizer.Core.Operations.WindowsTweaks.DisableGameDvrOperation
            // Group => "WindowsTweaks"
            var ns = t.Namespace ?? "";
            var group = "Misc";
            const string prefix = "_0xOptimizer.Core.Operations.";
            if (ns.StartsWith(prefix, StringComparison.Ordinal))
            {
                var rest = ns.Substring(prefix.Length);
                group = rest.Split('.')[0];
                if (string.IsNullOrWhiteSpace(group)) group = "Misc";
            }

            // Título "bonito" a partir do nome da classe
            var raw = t.Name;
            raw = raw.EndsWith("Operation", StringComparison.Ordinal) ? raw[..^"Operation".Length] : raw;
            var title = SplitCamel(raw);

            // Sub = nome real da classe (útil pra debug)
            var sub = t.FullName ?? t.Name;

            // Admin: tenta ler propriedade "RequiresAdmin" se existir, senão false
            bool requiresAdmin = false;
            var prop = t.GetProperty("RequiresAdmin", BindingFlags.Public | BindingFlags.Instance);
            if (prop != null && prop.PropertyType == typeof(bool))
            {
                try
                {
                    var inst = (IOperation)Activator.CreateInstance(t)!;
                    requiresAdmin = (bool)(prop.GetValue(inst) ?? false);
                }
                catch { requiresAdmin = false; }
            }

            return new AutoTweakEntry(group, title, sub, t, requiresAdmin);
        }

        private static string SplitCamel(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return s;

            var chars = s.ToCharArray();
            var outChars = new List<char>(chars.Length + 8);

            for (int i = 0; i < chars.Length; i++)
            {
                var c = chars[i];
                if (i > 0 && char.IsUpper(c) && !char.IsUpper(chars[i - 1]))
                    outChars.Add(' ');
                outChars.Add(c);
            }
            return new string(outChars.ToArray());
        }
    }
}
