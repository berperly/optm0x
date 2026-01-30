using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.ServiceProcess;

namespace _0xOptimizer.Core.Status
{
    public sealed class OptimizerConfig
    {
        public StatusCategory GeneralStatusCategory { get; } = new("Limpeza Geral");
        public StatusCategory HardwareStatusCategory { get; } = new("Hardware Tweaks");
        public StatusCategory WindowsCleanStatusCategory { get; } = new("Windows Tweaks");

        public OptimizerConfig()
        {
            GeneralStatusCategory.Items.Add(new StatusItem("Notificações (desativadas)", () =>
            {
                try
                {
                    var v = Registry.GetValue(
                        @"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\Notifications\Settings",
                        "NOC_GLOBAL_SETTING_TOASTS_ENABLED", 1);
                    return v != null && Convert.ToInt32(v) == 0;
                }
                catch { return false; }
            }));

            HardwareStatusCategory.Items.Add(new StatusItem("Power plan (Performance)", () =>
            {
                try
                {
                    var p = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = "powercfg",
                            Arguments = "/getactivescheme",
                            UseShellExecute = false,
                            RedirectStandardOutput = true,
                            CreateNoWindow = true
                        }
                    };
                    p.Start();
                    string output = p.StandardOutput.ReadToEnd();
                    p.WaitForExit();
                    return output.Contains("8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c") ||
                           output.Contains("e9a42b02-d5df-448d-aa00-03f14749eb61");
                }
                catch { return false; }
            }));

            HardwareStatusCategory.Items.Add(new StatusItem("HAGS (quando suportado)", () =>
            {
                try
                {
                    var v = Registry.GetValue(
                        @"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\GraphicsDrivers",
                        "HwSchMode", null);
                    return v != null && Convert.ToInt32(v) == 2;
                }
                catch { return false; }
            }));

            HardwareStatusCategory.Items.Add(new StatusItem("Game Mode (ON)", () =>
            {
                try
                {
                    var v = Registry.GetValue(
                        @"HKEY_CURRENT_USER\Software\Microsoft\GameBar",
                        "AutoGameModeEnabled", 0);
                    return v != null && Convert.ToInt32(v) == 1;
                }
                catch { return false; }
            }));

            WindowsCleanStatusCategory.Items.Add(new StatusItem("Telemetria (reduzida)", () =>
            {
                try
                {
                    var v = Registry.GetValue(
                        @"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\DataCollection",
                        "AllowTelemetry", 1);
                    return v != null && Convert.ToInt32(v) == 0;
                }
                catch { return false; }
            }));
        }

        public List<StatusItem> MainStatusList()
        {
            return new List<StatusItem>
            {
                new("Windows Update (desativado)", () =>
                {
                    try
                    {
                        using var sc = new ServiceController("wuauserv");
                        return sc.StartType == ServiceStartMode.Disabled;
                    }
                    catch { return false; }
                }),
                new("Telemetria (reduzida)", () =>
                {
                    try
                    {
                        var v = Registry.GetValue(
                            @"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\DataCollection",
                            "AllowTelemetry", 1);
                        return v != null && Convert.ToInt32(v) == 0;
                    }
                    catch { return false; }
                }),
                new("Game Mode (ON)", () =>
                {
                    try
                    {
                        var v = Registry.GetValue(
                            @"HKEY_CURRENT_USER\Software\Microsoft\GameBar",
                            "AutoGameModeEnabled", 0);
                        return v != null && Convert.ToInt32(v) == 1;
                    }
                    catch { return false; }
                }),
                new("Superfetch/SysMain (desativado)", () =>
                {
                    try
                    {
                        using var sc = new ServiceController("SysMain");
                        return sc.StartType == ServiceStartMode.Disabled;
                    }
                    catch { return false; }
                }),
                new("Notificações (desativadas)", () =>
                {
                    try
                    {
                        var v = Registry.GetValue(
                            @"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\Notifications\Settings",
                            "NOC_GLOBAL_SETTING_TOASTS_ENABLED", 1);
                        return v != null && Convert.ToInt32(v) == 0;
                    }
                    catch { return false; }
                }),
                new("Power plan (Performance)", () =>
                {
                    try
                    {
                        var p = new Process
                        {
                            StartInfo = new ProcessStartInfo
                            {
                                FileName = "powercfg",
                                Arguments = "/getactivescheme",
                                UseShellExecute = false,
                                RedirectStandardOutput = true,
                                CreateNoWindow = true
                            }
                        };
                        p.Start();
                        string output = p.StandardOutput.ReadToEnd();
                        p.WaitForExit();
                        return output.Contains("8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c") ||
                               output.Contains("e9a42b02-d5df-448d-aa00-03f14749eb61");
                    }
                    catch { return false; }
                }),
            };
        }
    }
}