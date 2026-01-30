#nullable enable
using Microsoft.Win32;
using System;

namespace _0xOptimizer.Core.Operations.Common
{
    public static class RegistryOps
    {
        public static void SetDword(RegistryKey root, string path, string name, int value)
        {
            using var k = root.CreateSubKey(path, writable: true) ?? throw new InvalidOperationException("Falha ao abrir chave: " + path);
            k.SetValue(name, value, RegistryValueKind.DWord);
        }

        public static int? GetDword(RegistryKey root, string path, string name)
        {
            using var k = root.OpenSubKey(path, writable: false);
            if (k == null) return null;
            var v = k.GetValue(name);
            if (v is int i) return i;
            return null;
        }
    }
}