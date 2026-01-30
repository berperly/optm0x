using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using _0xOptimizer.Core.Utils;

namespace _0xOptimizer.Core.System
{
    public sealed class SystemInfoReader
    {
        private readonly CommandRunner _cmd = new();

        public SystemInfo Read()
        {
            var info = new SystemInfo
            {
                OS = "Windows",
                Build = Environment.OSVersion.Version.Build.ToString(),
                CPU = GetCpuFriendly(),
                GPU = GetGpuFriendly(),
                RAM = GetRamFriendly()
            };
            return info;
        }

        private static string GetCpuFriendly()
        {
            string cpu = Environment.GetEnvironmentVariable("PROCESSOR_IDENTIFIER") ?? "CPU";
            int threads = Environment.ProcessorCount;

            try
            {
                using var k = Registry.LocalMachine.OpenSubKey(@"HARDWARE\DESCRIPTION\System\CentralProcessor\0");
                var name = k?.GetValue("ProcessorNameString")?.ToString();
                if (!string.IsNullOrWhiteSpace(name)) cpu = name;
            }
            catch { }

            return $"{cpu} ({threads} threads)";
        }

        private string GetGpuFriendly()
        {
            try
            {
                using var baseKey = Registry.LocalMachine.OpenSubKey(
                    @"SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}");
                if (baseKey != null)
                {
                    var names = new List<string>();
                    foreach (var subName in baseKey.GetSubKeyNames())
                    {
                        if (!subName.All(char.IsDigit)) continue;
                        using var sub = baseKey.OpenSubKey(subName);
                        var desc = sub?.GetValue("DriverDesc")?.ToString();
                        if (!string.IsNullOrWhiteSpace(desc)) names.Add(desc.Trim());
                    }

                    var filtered = names.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
                    var nonBasic = filtered.Where(n => !n.Contains("Microsoft Basic Display", StringComparison.OrdinalIgnoreCase)).ToList();

                    if (nonBasic.Count > 0) return string.Join(" | ", nonBasic);
                    if (filtered.Count > 0) return string.Join(" | ", filtered);
                }
            }
            catch { }

            try
            {
                var (code, outp, _) = _cmd.Run("cmd.exe", "/c wmic path win32_VideoController get Name /value");
                if (code == 0 && !string.IsNullOrWhiteSpace(outp))
                {
                    var names = outp.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                        .Where(l => l.StartsWith("Name=", StringComparison.OrdinalIgnoreCase))
                        .Select(l => l.Substring(5).Trim())
                        .Where(s => !string.IsNullOrWhiteSpace(s))
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .ToList();

                    if (names.Count > 0) return string.Join(" | ", names);
                }
            }
            catch { }

            return "Placa de vídeo";
        }

        private string GetRamFriendly()
        {
            string inUseMb = $"{(Environment.WorkingSet / 1024 / 1024):N0} MB (app)";

            try
            {
                var (code, outp, _) = _cmd.Run("cmd.exe", "/c wmic computersystem get TotalPhysicalMemory /value");
                if (code == 0 && outp.Contains("TotalPhysicalMemory=", StringComparison.OrdinalIgnoreCase))
                {
                    var line = outp.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                                   .FirstOrDefault(l => l.StartsWith("TotalPhysicalMemory=", StringComparison.OrdinalIgnoreCase));
                    if (line != null)
                    {
                        var raw = line.Split('=').LastOrDefault()?.Trim();
                        if (ulong.TryParse(raw, out var bytes))
                        {
                            double gb = bytes / 1024d / 1024d / 1024d;
                            return $"{gb:0.#} GB ({inUseMb})";
                        }
                    }
                }
            }
            catch { }

            return $"(Total n/d) • {inUseMb}";
        }
    }
}