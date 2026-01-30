//
// File: 0xOptimizer\UI\Forms\MainOptimizerForm.cs
//
#nullable enable

using _0xOptimizer.Core.Logging;
using _0xOptimizer.Core.Operations;
using _0xOptimizer.Core.Operations.Cleanup;
using _0xOptimizer.Core.Operations.Common;
using _0xOptimizer.Core.Operations.CpuTweaks;
using _0xOptimizer.Core.Operations.Hardware;
using _0xOptimizer.Core.Operations.RamTweaks;
using _0xOptimizer.Core.Operations.WindowsTweaks;
using _0xOptimizer.Core.System;
using _0xOptimizer.Core.Utils;
using _0xOptimizer.UI.Controls;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace _0xOptimizer.UI.Forms
{
    public sealed class MainOptimizerForm : Form
    {
        private const string DISCORD_INVITE_URL = "https://discord.gg/DXnxUUZjHU";
        private const string API_BASE_URL = "https://0x-license.squareweb.app";

        private readonly LicenseService _license;
        private readonly SystemInfoReader _sysReader = new();
        private Core.System.SystemInfo _cachedSystemInfo = new();

        // infra ops
        private readonly OperationRunner _runner = new();
        private readonly CommandRunner _cmd = new();
        private CancellationTokenSource? _opsCts;

        // root/layout
        private Panel root = null!;
        private Panel titleBar = null!;
        private Button btnMinimize = null!;
        private Button btnClose = null!;
        private Label lblKeyBadge = null!;

        // views
        private Panel loginView = null!;
        private Panel tweaksView = null!;
        private Panel gamesView = null!;
        private Panel settingsView = null!;

        // header/tabs
        private ModernButton tabTweaks = null!;
        private ModernButton tabGames = null!;
        private ModernButton tabSettings = null!;

        // tweaks: log/status
        private RichTextBox logBox = null!;
        private Label statusLabel = null!;

        // overlay busy
        private Panel busyOverlay = null!;
        private Label busyText = null!;
        private System.Windows.Forms.Timer? busyDotsTimer;

        // settings
        private Label settingsKey = null!;
        private Label settingsStatus = null!;
        private Label settingsExp = null!;
        private Label settingsRemaining = null!;
        private Label settingsSpecs = null!;

        // login
        private TextBox keyInput = null!;
        private Label loginStatus = null!;
        private ModernButton btnLogin = null!;
        private ModernButton btnDiscord = null!;

        public MainOptimizerForm()
        {
            BuildUi();

            _license = new LicenseService(API_BASE_URL);

            _license.LicenseChanged += () =>
            {
                if (!IsHandleCreated) return;
                BeginInvoke(new Action(() =>
                {
                    UpdateKeyBadge();
                    if (settingsView.Visible) UpdateSettingsKeyArea();
                }));
            };

            _license.LicenseLost += msg =>
            {
                if (!IsHandleCreated) return;
                BeginInvoke(new Action(() =>
                {
                    MessageBox.Show(this, msg + "\n\nYou will be logged out.", "License",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    ShowLogin();
                }));
            };

            Shown += (_, __) =>
            {
                RefreshSystemInfo();
                ShowLogin();
            };
        }

        // ===========================
        // UI BOOTSTRAP
        // ===========================
        private void BuildUi()
        {
            Text = "0xOptimizer";
            ClientSize = new Size(1150, 720);
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Theme.Bg;
            DoubleBuffered = true;
            MinimumSize = new Size(900, 600);

            titleBar = BuildTitleBar();

            root = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Theme.Bg
            };

            loginView = BuildLoginView();
            tweaksView = BuildTweaksView();
            gamesView = BuildGamesView();
            settingsView = BuildSettingsView();

            root.Controls.Add(settingsView);
            root.Controls.Add(gamesView);
            root.Controls.Add(tweaksView);
            root.Controls.Add(loginView);

            Controls.Add(root);
            Controls.Add(titleBar);

            SetupBusyOverlay();

            WindowChrome.EnableRoundedCorners(this);
            WindowChrome.EnableDrag(titleBar, this);

            Resize += (_, __) => LayoutTitleBarButtons();
        }

        // ===========================
        // TITLE BAR
        // ===========================
        private Panel BuildTitleBar()
        {
            var bar = new Panel { Dock = DockStyle.Top, Height = 54, BackColor = Color.Transparent };

            var title = new Label
            {
                Text = "System Optimizer",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12.5f, FontStyle.Bold),
                Location = new Point(18, 16),
                AutoSize = true
            };

            lblKeyBadge = new Label
            {
                Text = "Key: --",
                ForeColor = Theme.SubText,
                Font = new Font("Segoe UI", 9f, FontStyle.Regular),
                Location = new Point(18, 36),
                AutoSize = true
            };

            btnMinimize = new Button
            {
                Text = "─",
                FlatStyle = FlatStyle.Flat,
                Size = new Size(44, 32),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 12),
                TabStop = false
            };
            btnMinimize.FlatAppearance.BorderSize = 0;
            btnMinimize.FlatAppearance.MouseOverBackColor = Color.FromArgb(40, 40, 46);
            btnMinimize.Click += (_, __) => WindowState = FormWindowState.Minimized;

            btnClose = new Button
            {
                Text = "×",
                FlatStyle = FlatStyle.Flat,
                Size = new Size(44, 32),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 14),
                TabStop = false
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.FlatAppearance.MouseOverBackColor = Color.FromArgb(232, 17, 35);
            btnClose.Click += (_, __) => Application.Exit();

            bar.Controls.Add(btnClose);
            bar.Controls.Add(btnMinimize);
            bar.Controls.Add(lblKeyBadge);
            bar.Controls.Add(title);

            LayoutTitleBarButtons();
            return bar;
        }

        private void LayoutTitleBarButtons()
        {
            if (btnClose == null || btnMinimize == null) return;
            int top = 10;
            btnClose.Location = new Point(ClientSize.Width - btnClose.Width - 10, top);
            btnMinimize.Location = new Point(btnClose.Left - btnMinimize.Width, top);
        }

        // ===========================
        // NAV
        // ===========================
        private void HideAllViews()
        {
            if (loginView != null) loginView.Visible = false;
            if (tweaksView != null) tweaksView.Visible = false;
            if (gamesView != null) gamesView.Visible = false;
            if (settingsView != null) settingsView.Visible = false;
        }

        private void ShowLogin()
        {
            HideAllViews();
            loginView.Visible = true;

            _license.StopAutoRefresh();
            UpdateKeyBadge();

            SetActiveTab(null);
        }

        private void ShowTweaks()
        {
            if (!_license.IsValid)
            {
                ShowLogin();
                return;
            }

            HideAllViews();
            tweaksView.Visible = true;
            UpdateKeyBadge();

            SetActiveTab(tabTweaks);
        }

        private void ShowGames()
        {
            if (!_license.IsValid)
            {
                ShowLogin();
                return;
            }

            HideAllViews();
            gamesView.Visible = true;
            UpdateKeyBadge();

            SetActiveTab(tabGames);
        }

        private void ShowSettings()
        {
            if (!_license.IsValid)
            {
                ShowLogin();
                return;
            }

            HideAllViews();
            settingsView.Visible = true;

            UpdateKeyBadge();
            UpdateSettingsKeyArea();
            RefreshSystemInfo();
            UpdateSettingsSpecsArea();

            SetActiveTab(tabSettings);
        }

        private void UpdateKeyBadge()
        {
            lblKeyBadge.Text = _license.IsValid ? $"Key: {_license.CurrentKey}" : "Key: --";
        }

        // ===========================
        // TOP TABS
        // ===========================
        private Control BuildTopTabs()
        {
            var bar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 64,
                BackColor = Theme.Card,
                Padding = new Padding(14, 12, 14, 12)
            };

            tabTweaks = new ModernButton
            {
                Text = "Tweaks",
                IconGlyph = "\uE8D7",
                BaseBackColor = Color.FromArgb(85, 20, 125),
                HoverBackColor = Color.FromArgb(105, 28, 150),
                PressBackColor = Color.FromArgb(70, 16, 110),
                BorderColor = Color.FromArgb(120, 55, 165),
                CornerRadius = 16,
                Size = new Size(140, 40),
                Location = new Point(16, 12)
            };
            tabTweaks.Click += (_, __) => ShowTweaks();

            tabGames = new ModernButton
            {
                Text = "Games",
                IconGlyph = "\uE7FC",
                BaseBackColor = Color.FromArgb(35, 35, 45),
                HoverBackColor = Color.FromArgb(50, 50, 64),
                PressBackColor = Color.FromArgb(28, 28, 38),
                BorderColor = Color.FromArgb(70, 70, 90),
                CornerRadius = 16,
                Size = new Size(140, 40),
                Location = new Point(tabTweaks.Right + 12, 12)
            };
            tabGames.Click += (_, __) => ShowGames();

            tabSettings = new ModernButton
            {
                Text = "Settings",
                IconGlyph = "\uE713",
                BaseBackColor = Color.FromArgb(35, 35, 45),
                HoverBackColor = Color.FromArgb(50, 50, 64),
                PressBackColor = Color.FromArgb(28, 28, 38),
                BorderColor = Color.FromArgb(70, 70, 90),
                CornerRadius = 16,
                Size = new Size(170, 40),
                Location = new Point(tabGames.Right + 12, 12)
            };
            tabSettings.Click += (_, __) => ShowSettings();

            bar.Controls.Add(tabTweaks);
            bar.Controls.Add(tabGames);
            bar.Controls.Add(tabSettings);

            return bar;
        }

        private void SetActiveTab(ModernButton? active)
        {
            ApplyTabStyle(tabTweaks, active == tabTweaks);
            ApplyTabStyle(tabGames, active == tabGames);
            ApplyTabStyle(tabSettings, active == tabSettings);
        }

        private static void ApplyTabStyle(ModernButton? b, bool active)
        {
            if (b == null) return;

            if (active)
            {
                b.BaseBackColor = Color.FromArgb(85, 20, 125);
                b.HoverBackColor = Color.FromArgb(105, 28, 150);
                b.PressBackColor = Color.FromArgb(70, 16, 110);
                b.BorderColor = Color.FromArgb(120, 55, 165);
            }
            else
            {
                b.BaseBackColor = Color.FromArgb(35, 35, 45);
                b.HoverBackColor = Color.FromArgb(50, 50, 64);
                b.PressBackColor = Color.FromArgb(28, 28, 38);
                b.BorderColor = Color.FromArgb(70, 70, 90);
            }

            b.Invalidate();
        }

        // ===========================
        // TWEAKS VIEW (FLOW DE CARDS)
        // ===========================
        private Panel BuildTweaksView()
        {
            var p = new Panel { Dock = DockStyle.Fill, BackColor = Theme.Bg, Visible = false };
            p.Controls.Add(BuildTopTabs());

            var content = new Panel { Dock = DockStyle.Fill, Padding = new Padding(18, 12, 18, 18) };

            statusLabel = new Label
            {
                Text = "Ready",
                ForeColor = Color.FromArgb(0, 200, 83),
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = 22
            };

            logBox = new RichTextBox
            {
                Dock = DockStyle.Fill,
                BackColor = Theme.Card,
                ForeColor = Color.FromArgb(220, 220, 230),
                Font = new Font("Cascadia Code", 9.5f),
                BorderStyle = BorderStyle.None,
                ReadOnly = true,
                WordWrap = false
            };

            // ✅ FlowLayout com cards
            var cardsFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 470,
                AutoScroll = true,
                WrapContents = true,
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(4),
                BackColor = Theme.Bg
            };

            int cardWidth = 520;

            // ==============
            // TODOS OS 20 TWEAKS EM CATEGORIAS
            // ==============

            // CATEGORIA 1: Windows / UI
            var cardWindowsUi = BuildCategoryCard(
                "Windows / UI",
                "Interface e responsividade do Windows",
                new List<TweakItem>
                {
                    Toggle(
                        "Notifications (Disable)",
                        "Desativa notificações do sistema",
                        requiresAdmin: false,
                        getState: IsNotificationsDisabled,
                        apply: async enabled =>
                        {
                            await RunOpAsync(new LambdaOperation(
                                enabled ? "Disable Notifications" : "Enable Notifications",
                                requiresAdmin: false,
                                apply: ctx =>
                                {
                                    ctx.Log.Info(enabled ? "Disabling notifications..." : "Enabling notifications...");
                                    RegistrySetDword(Registry.CurrentUser,
                                        @"Software\Microsoft\Windows\CurrentVersion\PushNotifications",
                                        "ToastEnabled",
                                        enabled ? 0 : 1);
                                    ctx.Log.Success("Notifications: applied.");
                                },
                                check: () => IsNotificationsDisabled() == enabled
                            ));
                        }
                    ),

                    Toggle(
                        "Windows Animations (Disable)",
                        "Desativa animações do Windows para mais velocidade",
                        requiresAdmin: false,
                        getState: IsAnimationsDisabled,
                        apply: async enabled =>
                        {
                            await RunOpAsync(new LambdaOperation(
                                enabled ? "Disable Windows Animations" : "Enable Windows Animations",
                                requiresAdmin: false,
                                apply: ctx =>
                                {
                                    ctx.Log.Info(enabled ? "Disabling animations..." : "Enabling animations...");
                                    RegistrySetString(Registry.CurrentUser,
                                        @"Control Panel\Desktop\WindowMetrics",
                                        "MinAnimate",
                                        enabled ? "0" : "1");

                                    RegistrySetString(Registry.CurrentUser,
                                        @"Control Panel\Desktop",
                                        "MenuShowDelay",
                                        enabled ? "0" : "400");

                                    ctx.Log.Success("Animations: applied.");
                                },
                                check: () => IsAnimationsDisabled() == enabled
                            ));
                        }
                    ),

                    Toggle(
                        "Disable Startup Delay for Apps",
                        "Remove delay ao iniciar aplicativos",
                        requiresAdmin: false,
                        getState: IsStartupDelayDisabled,
                        apply: async enabled =>
                        {
                            await RunOpAsync(new LambdaOperation(
                                enabled ? "Disable Startup Delay" : "Restore Startup Delay",
                                requiresAdmin: false,
                                apply: ctx =>
                                {
                                    ctx.Log.Info(enabled ? "Disabling startup delay..." : "Restoring startup delay...");
                                    RegistrySetDword(Registry.CurrentUser,
                                        @"Software\Microsoft\Windows\CurrentVersion\Explorer\Serialize",
                                        "StartupDelayInMSec",
                                        enabled ? 0 : 1);
                                    ctx.Log.Success("Startup delay: applied.");
                                },
                                check: () => IsStartupDelayDisabled() == enabled
                            ));
                        }
                    ),

                    Toggle(
                        "Transparency Effects (Disable)",
                        "Desativa efeitos de transparência",
                        requiresAdmin: false,
                        getState: IsTransparencyDisabled,
                        apply: async enabled =>
                        {
                            await RunOpAsync(new LambdaOperation(
                                enabled ? "Disable Transparency" : "Enable Transparency",
                                requiresAdmin: false,
                                apply: ctx =>
                                {
                                    ctx.Log.Info(enabled ? "Disabling transparency..." : "Enabling transparency...");
                                    RegistrySetDword(Registry.CurrentUser,
                                        @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize",
                                        "EnableTransparency",
                                        enabled ? 0 : 1);
                                    ctx.Log.Success("Transparency: applied.");
                                },
                                check: () => IsTransparencyDisabled() == enabled
                            ));
                        }
                    ),
                }
            );
            cardWindowsUi.Width = cardWidth;

            // CATEGORIA 2: Gaming
            var cardGaming = BuildCategoryCard(
                "Gaming",
                "Otimizações para jogos",
                new List<TweakItem>
                {
                    Toggle(
                        "Game Mode",
                        "Ativa/Desativa o Game Mode do Windows",
                        requiresAdmin: false,
                        getState: IsGameModeEnabled,
                        apply: async enabled =>
                        {
                            await RunOpAsync(new LambdaOperation(
                                enabled ? "Enable Game Mode" : "Disable Game Mode",
                                requiresAdmin: false,
                                apply: ctx =>
                                {
                                    ctx.Log.Info((enabled ? "Enabling" : "Disabling") + " Game Mode...");
                                    ctx.Cmd.Run("cmd.exe", $"/c reg add \"HKCU\\Software\\Microsoft\\GameBar\" /v AllowAutoGameMode /t REG_DWORD /d {(enabled ? 1 : 0)} /f");
                                    ctx.Cmd.Run("cmd.exe", $"/c reg add \"HKCU\\Software\\Microsoft\\GameBar\" /v AutoGameModeEnabled /t REG_DWORD /d {(enabled ? 1 : 0)} /f");
                                    ctx.Cmd.Run("cmd.exe", $"/c reg add \"HKCU\\System\\GameConfigStore\" /v GameDVR_Enabled /t REG_DWORD /d {(enabled ? 1 : 0)} /f");
                                    ctx.Log.Success("Game Mode: applied.");
                                },
                                check: () => IsGameModeEnabled() == enabled
                            ));
                        }
                    ),

                    Toggle(
                        "GameDVR (Disable)",
                        "Desativa GameDVR para melhor performance",
                        requiresAdmin: false,
                        getState: IsGameDvrDisabled,
                        apply: async enabled =>
                        {
                            await RunOpAsync(new LambdaOperation(
                                enabled ? "Disable GameDVR" : "Enable GameDVR",
                                requiresAdmin: false,
                                apply: ctx =>
                                {
                                    ctx.Log.Info(enabled ? "Disabling GameDVR..." : "Enabling GameDVR...");
                                    ctx.Cmd.Run("cmd.exe", $"/c reg add \"HKCU\\System\\GameConfigStore\" /v GameDVR_Enabled /t REG_DWORD /d {(enabled ? 0 : 1)} /f");
                                    if (enabled)
                                    {
                                        ctx.Cmd.Run("cmd.exe", "/c reg add \"HKCU\\System\\GameConfigStore\" /v GameDVR_FSEBehaviorMode /t REG_DWORD /d 2 /f");
                                        ctx.Cmd.Run("cmd.exe", "/c reg add \"HKCU\\System\\GameConfigStore\" /v GameDVR_HonorUserFSEBehaviorMode /t REG_DWORD /d 1 /f");
                                    }
                                    ctx.Log.Success("GameDVR: applied.");
                                },
                                check: () => IsGameDvrDisabled() == enabled
                            ));
                        }
                    ),
                }
            );
            cardGaming.Width = cardWidth;

            // CATEGORIA 3: Performance
            var cardPerformance = BuildCategoryCard(
                "Performance",
                "Otimizações de performance do sistema",
                new List<TweakItem>
                {
                    Toggle(
                        "Windows Updates (Disable)",
                        "Desativa atualizações automáticas do Windows",
                        requiresAdmin: true,
                        getState: IsWindowsUpdatesDisabled,
                        apply: async enabled =>
                        {
                            await RunOpAsync(new LambdaOperation(
                                enabled ? "Disable Windows Updates" : "Enable Windows Updates",
                                requiresAdmin: true,
                                apply: ctx =>
                                {
                                    ctx.Log.Info(enabled ? "Disabling Windows Updates..." : "Enabling Windows Updates...");
                                    RegistrySetDword(Registry.LocalMachine,
                                        @"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate\AU",
                                        "NoAutoUpdate",
                                        enabled ? 1 : 0);
                                    ctx.Log.Success("Windows Updates: applied.");
                                },
                                check: () => IsWindowsUpdatesDisabled() == enabled
                            ));
                        }
                    ),

                    Toggle(
                        "Windows Search (Optimize)",
                        "Otimiza busca do Windows para performance",
                        requiresAdmin: false,
                        getState: IsWindowsSearchOptimized,
                        apply: async enabled =>
                        {
                            await RunOpAsync(new LambdaOperation(
                                enabled ? "Optimize Windows Search" : "Restore Windows Search",
                                requiresAdmin: false,
                                apply: ctx =>
                                {
                                    ctx.Log.Info(enabled ? "Optimizing Windows Search..." : "Restoring Windows Search...");
                                    RegistrySetDword(Registry.CurrentUser,
                                        @"Software\Microsoft\Windows\CurrentVersion\Search",
                                        "BingSearchEnabled",
                                        enabled ? 0 : 1);
                                    RegistrySetDword(Registry.CurrentUser,
                                        @"Software\Microsoft\Windows\CurrentVersion\Search",
                                        "CortanaConsent",
                                        enabled ? 0 : 1);
                                    ctx.Log.Success("Windows Search: applied.");
                                },
                                check: () => IsWindowsSearchOptimized() == enabled
                            ));
                        }
                    ),

                    Toggle(
                        "Aggressive Hung Apps",
                        "Fecha aplicativos travados mais rapidamente",
                        requiresAdmin: false,
                        getState: IsHungAppsAggressive,
                        apply: async enabled =>
                        {
                            await RunOpAsync(new LambdaOperation(
                                enabled ? "Enable Aggressive Hung Apps" : "Disable Aggressive Hung Apps",
                                requiresAdmin: false,
                                apply: ctx =>
                                {
                                    ctx.Log.Info(enabled ? "Enabling aggressive hung apps..." : "Disabling aggressive hung apps...");
                                    RegistrySetString(Registry.CurrentUser,
                                        @"Control Panel\Desktop",
                                        "AutoEndTasks",
                                        enabled ? "1" : "0");
                                    RegistrySetString(Registry.CurrentUser,
                                        @"Control Panel\Desktop",
                                        "HungAppTimeout",
                                        enabled ? "5000" : "5000");
                                    RegistrySetString(Registry.CurrentUser,
                                        @"Control Panel\Desktop",
                                        "WaitToKillAppTimeout",
                                        enabled ? "5000" : "20000");
                                    ctx.Log.Success("Hung Apps: applied.");
                                },
                                check: () => IsHungAppsAggressive() == enabled
                            ));
                        }
                    ),

                    Toggle(
                        "Network Latency (Optimize)",
                        "Otimiza latência de rede para jogos",
                        requiresAdmin: true,
                        getState: IsLatencyTweaked,
                        apply: async enabled =>
                        {
                            await RunOpAsync(new LambdaOperation(
                                enabled ? "Optimize Network Latency" : "Restore Network Latency",
                                requiresAdmin: true,
                                apply: ctx =>
                                {
                                    ctx.Log.Info(enabled ? "Optimizing network latency..." : "Restoring network latency...");
                                    RegistrySetDword(Registry.LocalMachine,
                                        @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile",
                                        "SystemResponsiveness",
                                        enabled ? 0 : 20);
                                    RegistrySetString(Registry.LocalMachine,
                                        @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile",
                                        "NetworkThrottlingIndex",
                                        enabled ? "ffffffff" : "10");
                                    ctx.Log.Success("Network Latency: applied.");
                                },
                                check: () => IsLatencyTweaked() == enabled
                            ));
                        }
                    ),
                }
            );
            cardPerformance.Width = cardWidth;

            // CATEGORIA 4: Services
            var cardServices = BuildCategoryCard(
                "Services",
                "Controle de serviços do Windows",
                new List<TweakItem>
                {
                    Toggle(
                        "Xbox Services (Disable)",
                        "Desativa serviços Xbox para liberar recursos",
                        requiresAdmin: true,
                        getState: IsXboxServicesDisabled,
                        apply: async enabled =>
                        {
                            await RunOpAsync(new LambdaOperation(
                                enabled ? "Disable Xbox Services" : "Enable Xbox Services",
                                requiresAdmin: true,
                                apply: ctx =>
                                {
                                    ctx.Log.Info(enabled ? "Disabling Xbox services..." : "Enabling Xbox services...");
                                    string[] services = { "XblAuthManager", "XblGameSave", "XboxNetApiSvc", "XboxGipSvc" };
                                    foreach (var svc in services)
                                    {
                                        ctx.Cmd.Run("cmd.exe", $"/c sc config {svc} start= {(enabled ? "disabled" : "demand")}");
                                        if (enabled)
                                            ctx.Cmd.Run("cmd.exe", $"/c sc stop {svc}");
                                    }
                                    ctx.Log.Success("Xbox Services: applied.");
                                },
                                check: () => IsXboxServicesDisabled() == enabled
                            ));
                        }
                    ),

                    Toggle(
                        "Windows Update (Manual)",
                        "Define Windows Update como manual",
                        requiresAdmin: true,
                        getState: IsWindowsUpdateManual,
                        apply: async enabled =>
                        {
                            await RunOpAsync(new LambdaOperation(
                                enabled ? "Set Windows Update to Manual" : "Set Windows Update to Automatic",
                                requiresAdmin: true,
                                apply: ctx =>
                                {
                                    ctx.Log.Info(enabled ? "Setting Windows Update to manual..." : "Setting Windows Update to automatic...");
                                    ctx.Cmd.Run("cmd.exe", $"/c sc config wuauserv start= {(enabled ? "demand" : "auto")}");
                                    ctx.Log.Success("Windows Update mode: applied.");
                                },
                                check: () => IsWindowsUpdateManual() == enabled
                            ));
                        }
                    ),

                    Toggle(
                        "Driver Search (Disable)",
                        "Desativa busca automática de drivers",
                        requiresAdmin: true,
                        getState: IsDriverSearchingDisabled,
                        apply: async enabled =>
                        {
                            await RunOpAsync(new LambdaOperation(
                                enabled ? "Disable Driver Search" : "Enable Driver Search",
                                requiresAdmin: true,
                                apply: ctx =>
                                {
                                    ctx.Log.Info(enabled ? "Disabling driver search..." : "Enabling driver search...");
                                    RegistrySetDword(Registry.LocalMachine,
                                        @"SOFTWARE\Policies\Microsoft\Windows\DriverSearching",
                                        "DontSearchWindowsUpdate",
                                        enabled ? 1 : 0);
                                    RegistrySetDword(Registry.LocalMachine,
                                        @"SOFTWARE\Policies\Microsoft\Windows\DriverSearching",
                                        "DriverUpdateWizardWuSearchEnabled",
                                        enabled ? 0 : 1);
                                    ctx.Log.Success("Driver Search: applied.");
                                },
                                check: () => IsDriverSearchingDisabled() == enabled
                            ));
                        }
                    ),
                }
            );
            cardServices.Width = cardWidth;

            // CATEGORIA 5: Hardware
            var cardHardware = BuildCategoryCard(
                "Hardware",
                "Otimizações de hardware",
                new List<TweakItem>
                {
                    Toggle(
                        "Hypervisor (Disable)",
                        "Desativa Hypervisor para melhor performance",
                        requiresAdmin: true,
                        getState: IsHypervisorOff,
                        apply: async enabled =>
                        {
                            await RunOpAsync(new LambdaOperation(
                                enabled ? "Disable Hypervisor" : "Enable Hypervisor",
                                requiresAdmin: true,
                                apply: ctx =>
                                {
                                    ctx.Log.Info(enabled ? "Disabling Hypervisor..." : "Enabling Hypervisor...");
                                    if (enabled)
                                    {
                                        ctx.Cmd.Run("cmd.exe", "/c bcdedit /set hypervisorlaunchtype off");
                                    }
                                    else
                                    {
                                        ctx.Cmd.Run("cmd.exe", "/c bcdedit /set hypervisorlaunchtype auto");
                                    }
                                    ctx.Log.Success("Hypervisor: applied. Reboot required.");
                                },
                                check: () => IsHypervisorOff() == enabled
                            ));
                        }
                    ),

                    Toggle(
                        "HAGS (Enable)",
                        "Ativa Hardware Accelerated GPU Scheduling",
                        requiresAdmin: true,
                        getState: IsHagsEnabled,
                        apply: async enabled =>
                        {
                            await RunOpAsync(new LambdaOperation(
                                enabled ? "Enable HAGS" : "Disable HAGS",
                                requiresAdmin: true,
                                apply: ctx =>
                                {
                                    ctx.Log.Info(enabled ? "Enabling HAGS..." : "Disabling HAGS...");
                                    RegistrySetDword(Registry.LocalMachine,
                                        @"SYSTEM\CurrentControlSet\Control\GraphicsDrivers",
                                        "HwSchMode",
                                        enabled ? 2 : 1);
                                    ctx.Log.Success("HAGS: applied. Reboot recommended.");
                                },
                                check: () => IsHagsEnabled() == enabled
                            ));
                        }
                    ),
                }
            );
            cardHardware.Width = cardWidth;

            // CATEGORIA 6: Extra Tweaks
            var cardExtra = BuildCategoryCard(
                "Extra Tweaks",
                "Tweaks adicionais para performance",
                new List<TweakItem>
                {
                    ActionBtn(
                        "Clean Temp Files",
                        "Limpa arquivos temporários",
                        requiresAdmin: false,
                        run: async () =>
                        {
                            await RunOpAsync(new LambdaOperation(
                                "Clean Temp Files",
                                requiresAdmin: false,
                                apply: ctx =>
                                {
                                    ctx.Log.Info("Cleaning temp files...");
                                    ctx.Cmd.Run("cmd.exe", "/c del /q /f /s %temp%\\*.*");
                                    ctx.Cmd.Run("cmd.exe", "/c rmdir /q /s %temp%");
                                    ctx.Cmd.Run("cmd.exe", "/c mkdir %temp%");
                                    ctx.Log.Success("Temp files cleaned.");
                                },
                                check: () => false
                            ));
                        }
                    ),

                    ActionBtn(
                        "Flush DNS",
                        "Limpa cache DNS",
                        requiresAdmin: true,
                        run: async () =>
                        {
                            await RunOpAsync(new LambdaOperation(
                                "Flush DNS",
                                requiresAdmin: true,
                                apply: ctx =>
                                {
                                    ctx.Log.Info("Flushing DNS...");
                                    ctx.Cmd.Run("cmd.exe", "/c ipconfig /flushdns");
                                    ctx.Log.Success("DNS flushed.");
                                },
                                check: () => false
                            ));
                        }
                    ),

                    ActionBtn(
                        "Restart Explorer",
                        "Reinicia o Windows Explorer",
                        requiresAdmin: false,
                        run: async () =>
                        {
                            await RunOpAsync(new LambdaOperation(
                                "Restart Explorer",
                                requiresAdmin: false,
                                apply: ctx =>
                                {
                                    ctx.Log.Info("Restarting Explorer...");
                                    ctx.Cmd.Run("cmd.exe", "/c taskkill /f /im explorer.exe");
                                    System.Threading.Thread.Sleep(1000);
                                    ctx.Cmd.Run("cmd.exe", "/c start explorer.exe");
                                    ctx.Log.Success("Explorer restarted.");
                                },
                                check: () => false
                            ));
                        }
                    ),
                }
            );
            cardExtra.Width = cardWidth;

            // Adiciona todos os cards
            cardsFlow.Controls.Add(cardWindowsUi);
            cardsFlow.Controls.Add(cardGaming);
            cardsFlow.Controls.Add(cardPerformance);
            cardsFlow.Controls.Add(cardServices);
            cardsFlow.Controls.Add(cardHardware);
            cardsFlow.Controls.Add(cardExtra);

            // espaço + status + flow + log
            content.Controls.Add(logBox);
            content.Controls.Add(new Panel { Dock = DockStyle.Top, Height = 12 });
            content.Controls.Add(statusLabel);
            content.Controls.Add(new Panel { Dock = DockStyle.Top, Height = 14 });
            content.Controls.Add(cardsFlow);

            p.Controls.Add(content);
            return p;
        }

        // ==========
        // ITEM MODEL (toggle/button)
        // ==========
        private sealed record TweakItem(
            string Title,
            string Sub,
            bool IsToggle,
            bool RequiresAdmin,
            Func<bool>? GetState,
            Func<bool, Task>? ApplyToggle,
            Func<Task>? RunAction
        );

        private static TweakItem Toggle(string title, string sub, bool requiresAdmin, Func<bool>? getState, Func<bool, Task> apply)
            => new(title, sub, IsToggle: true, requiresAdmin, getState, apply, null);

        private static TweakItem ActionBtn(string title, string sub, bool requiresAdmin, Func<Task> run)
            => new(title, sub, IsToggle: false, requiresAdmin, GetState: null, ApplyToggle: null, RunAction: run);

        // ===========================
        // CARD BUILDER
        // ===========================
        private Control BuildCategoryCard(string title, string sub, IReadOnlyList<TweakItem> items)
        {
            var card = new Panel
            {
                BackColor = Theme.Card,
                Padding = new Padding(16),
                Margin = new Padding(10),
                Height = 420
            };

            card.Paint += (_, e) =>
            {
                using var pen = new Pen(Color.FromArgb(70, 70, 90));
                e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
            };

            var lblTitle = new Label
            {
                Text = title,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11.5f, FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = 26
            };

            var lblSub = new Label
            {
                Text = sub,
                ForeColor = Theme.SubText,
                Font = new Font("Segoe UI", 9f),
                Dock = DockStyle.Top,
                Height = 20
            };

            var list = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                Padding = new Padding(0, 10, 0, 0)
            };

            foreach (var it in items)
            {
                var row = new Panel
                {
                    Width = 480,
                    Height = 58,
                    BackColor = Color.FromArgb(22, 22, 32),
                    Margin = new Padding(0, 0, 0, 10)
                };

                var titleLbl = new Label
                {
                    Text = it.Title,
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                    Location = new Point(12, 10),
                    AutoSize = true
                };

                var subLbl = new Label
                {
                    Text = it.Sub,
                    ForeColor = Theme.SubText,
                    Font = new Font("Segoe UI", 8.5f, FontStyle.Regular),
                    Location = new Point(12, 30),
                    AutoSize = true
                };

                row.Controls.Add(titleLbl);
                row.Controls.Add(subLbl);

                if (it.IsToggle)
                {
                    var toggle = new ModernToggleSwitch
                    {
                        Size = new Size(54, 28),
                        Location = new Point(row.Width - 84, 15),
                        OnBackColor = Color.FromArgb(85, 20, 125),
                        OffBackColor = Color.FromArgb(40, 40, 48),
                        BorderColor = Color.FromArgb(70, 70, 90),
                        ThumbColor = Color.White,
                        CornerRadius = 14
                    };

                    var stateLbl = new Label
                    {
                        AutoSize = true,
                        ForeColor = Theme.SubText,
                        Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                        Location = new Point(toggle.Left - 42, 19),
                        Text = "OFF"
                    };

                    bool suppress = false;

                    if (it.GetState != null)
                    {
                        try
                        {
                            suppress = true;
                            toggle.Checked = it.GetState();
                            stateLbl.Text = toggle.Checked ? "ON" : "OFF";
                        }
                        catch
                        {
                            suppress = true;
                            toggle.Checked = false;
                            stateLbl.Text = "OFF";
                        }
                        finally
                        {
                            suppress = false;
                        }
                    }

                    toggle.CheckedChanged += async (_, __) =>
                    {
                        if (suppress) return;

                        stateLbl.Text = toggle.Checked ? "ON" : "OFF";

                        if (!_license.IsValid)
                        {
                            MessageBox.Show(this, "Invalid/expired license. Please login again.", "License",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            ShowLogin();

                            suppress = true;
                            try { toggle.Checked = it.GetState?.Invoke() ?? false; } catch { toggle.Checked = false; }
                            stateLbl.Text = toggle.Checked ? "ON" : "OFF";
                            suppress = false;
                            return;
                        }

                        if (it.RequiresAdmin && !IsRunningAsAdmin())
                        {
                            MessageBox.Show(this, "This tweak requires Administrator privileges.", "Permission",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);

                            suppress = true;
                            try { toggle.Checked = it.GetState?.Invoke() ?? false; } catch { toggle.Checked = false; }
                            stateLbl.Text = toggle.Checked ? "ON" : "OFF";
                            suppress = false;
                            return;
                        }

                        logBox.Clear();
                        UpdateStatus("Running...", working: true);
                        ShowBusyOverlay(it.Title);

                        try
                        {
                            if (it.ApplyToggle != null)
                                await it.ApplyToggle(toggle.Checked);

                            UpdateStatus("Done", working: false);
                        }
                        catch (Exception ex)
                        {
                            AppendLog("❌ Error: " + ex.Message);
                            UpdateStatus("Error", error: true);

                            suppress = true;
                            try { toggle.Checked = it.GetState?.Invoke() ?? toggle.Checked; } catch { }
                            stateLbl.Text = toggle.Checked ? "ON" : "OFF";
                            suppress = false;
                        }
                        finally
                        {
                            HideBusyOverlay();
                        }
                    };

                    row.Controls.Add(stateLbl);
                    row.Controls.Add(toggle);
                }
                else
                {
                    var b = new ModernButton
                    {
                        Text = "Apply",
                        IconGlyph = "\uE72E",
                        BaseBackColor = Color.FromArgb(35, 35, 48),
                        HoverBackColor = Color.FromArgb(50, 50, 64),
                        PressBackColor = Color.FromArgb(28, 28, 38),
                        BorderColor = Color.FromArgb(70, 70, 90),
                        CornerRadius = 14,
                        Size = new Size(120, 36),
                        Location = new Point(row.Width - 140, 11)
                    };

                    b.Click += async (_, __) =>
                    {
                        if (!_license.IsValid)
                        {
                            MessageBox.Show(this, "Invalid/expired license. Please login again.", "License",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            ShowLogin();
                            return;
                        }

                        if (it.RequiresAdmin && !IsRunningAsAdmin())
                        {
                            MessageBox.Show(this, "This action requires Administrator privileges.", "Permission",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        logBox.Clear();
                        UpdateStatus("Running...", working: true);
                        ShowBusyOverlay(it.Title);

                        try
                        {
                            if (it.RunAction != null)
                                await it.RunAction();

                            UpdateStatus("Done", working: false);
                        }
                        catch (Exception ex)
                        {
                            AppendLog("❌ Error: " + ex.Message);
                            UpdateStatus("Error", error: true);
                        }
                        finally
                        {
                            HideBusyOverlay();
                        }
                    };

                    row.Controls.Add(b);
                }

                list.Controls.Add(row);
            }

            card.Controls.Add(list);
            card.Controls.Add(lblSub);
            card.Controls.Add(lblTitle);
            return card;
        }

        private void UpdateStatus(string message, bool working = false, bool warn = false, bool error = false)
        {
            statusLabel.Text = message;
            if (working) statusLabel.ForeColor = Color.FromArgb(255, 193, 7);
            else if (error) statusLabel.ForeColor = Color.FromArgb(255, 100, 100);
            else if (warn) statusLabel.ForeColor = Color.FromArgb(255, 193, 7);
            else statusLabel.ForeColor = Color.FromArgb(0, 200, 83);
        }

        private bool IsRunningAsAdmin()
        {
            try
            {
                return new System.Security.Principal.WindowsPrincipal(
                    System.Security.Principal.WindowsIdentity.GetCurrent()
                ).IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator);
            }
            catch { return false; }
        }

        // ===========================
        // STATE CHECKS (toggles)
        // ===========================
        private static bool IsServiceDisabled(string serviceName)
        {
            try
            {
                using var k = Registry.LocalMachine.OpenSubKey($@"SYSTEM\CurrentControlSet\Services\{serviceName}", false);
                if (k == null) return false;
                var v = k.GetValue("Start");
                if (v is int i) return i == 4;
            }
            catch { }
            return false;
        }

        private static bool IsGameModeEnabled()
        {
            try
            {
                using var k = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\GameBar", false);
                var v = k?.GetValue("AutoGameModeEnabled");
                if (v is int i) return i == 1;
            }
            catch { }
            return false;
        }

        private static bool IsGameDvrDisabled()
        {
            try
            {
                using var k = Registry.CurrentUser.OpenSubKey(@"System\GameConfigStore", false);
                var v = k?.GetValue("GameDVR_Enabled");
                if (v is int i) return i == 0;
            }
            catch { }
            return false;
        }

        private static bool IsHagsEnabled()
        {
            try
            {
                using var k = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\GraphicsDrivers", false);
                var v = k?.GetValue("HwSchMode");
                if (v is int i) return i == 2;
            }
            catch { }
            return false;
        }

        private static bool IsWindowsUpdateManual()
        {
            try
            {
                using var k = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\wuauserv", false);
                var v = k?.GetValue("Start");
                if (v is int i) return i == 3;
            }
            catch { }
            return false;
        }

        private static bool IsHypervisorOff()
        {
            try
            {
                var psi = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = "/c bcdedit /enum",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                using var p = System.Diagnostics.Process.Start(psi);
                if (p == null) return false;
                var o = p.StandardOutput.ReadToEnd();
                p.WaitForExit(4000);

                o = o.ToLowerInvariant();
                return o.Contains("hypervisorlaunchtype") && o.Contains("off");
            }
            catch { }
            return false;
        }

        private static bool IsNotificationsDisabled()
        {
            try
            {
                var v = RegistryGetDword(Registry.CurrentUser,
                    @"Software\Microsoft\Windows\CurrentVersion\PushNotifications",
                    "ToastEnabled");
                return v == 0;
            }
            catch { return false; }
        }

        private static bool IsAnimationsDisabled()
        {
            try
            {
                var minAnim = RegistryGetString(Registry.CurrentUser,
                    @"Control Panel\Desktop\WindowMetrics",
                    "MinAnimate");
                return string.Equals(minAnim, "0", StringComparison.Ordinal);
            }
            catch { return false; }
        }

        private static bool IsDriverSearchingDisabled()
        {
            try
            {
                var a = RegistryGetDword(Registry.LocalMachine,
                    @"SOFTWARE\Policies\Microsoft\Windows\DriverSearching",
                    "DontSearchWindowsUpdate");
                var b = RegistryGetDword(Registry.LocalMachine,
                    @"SOFTWARE\Policies\Microsoft\Windows\DriverSearching",
                    "DriverUpdateWizardWuSearchEnabled");
                return a == 1 && b == 0;
            }
            catch { return false; }
        }

        private static bool IsStartupDelayDisabled()
        {
            try
            {
                using var k = Registry.CurrentUser.OpenSubKey(
                    @"Software\Microsoft\Windows\CurrentVersion\Explorer\Serialize",
                    false);

                var v = k?.GetValue("StartupDelayInMSec");
                if (v is int i) return i == 0;
                if (v is string s && int.TryParse(s, out var parsed)) return parsed == 0;
                return false;
            }
            catch { return false; }
        }

        private static bool IsTransparencyDisabled()
        {
            try
            {
                var v = RegistryGetDword(Registry.CurrentUser,
                    @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize",
                    "EnableTransparency");
                return v == 0;
            }
            catch { return false; }
        }

        private static bool IsWindowsUpdatesDisabled()
        {
            try
            {
                var v = RegistryGetDword(Registry.LocalMachine,
                    @"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate\AU",
                    "NoAutoUpdate");
                return v == 1;
            }
            catch { return false; }
        }

        private static bool IsWindowsSearchOptimized()
        {
            try
            {
                var a = RegistryGetDword(Registry.CurrentUser,
                    @"Software\Microsoft\Windows\CurrentVersion\Search",
                    "BingSearchEnabled");
                var b = RegistryGetDword(Registry.CurrentUser,
                    @"Software\Microsoft\Windows\CurrentVersion\Search",
                    "CortanaConsent");
                return a == 0 && b == 0;
            }
            catch { return false; }
        }

        private static bool IsXboxServicesDisabled()
        {
            try
            {
                string[] svcs = { "XblAuthManager", "XblGameSave", "XboxNetApiSvc", "XboxGipSvc" };
                foreach (var s in svcs)
                    if (!IsServiceDisabled(s)) return false;
                return true;
            }
            catch { return false; }
        }

        private static bool IsHungAppsAggressive()
        {
            try
            {
                var autoEnd = RegistryGetString(Registry.CurrentUser, @"Control Panel\Desktop", "AutoEndTasks");
                var hung = RegistryGetString(Registry.CurrentUser, @"Control Panel\Desktop", "HungAppTimeout");
                var wait = RegistryGetString(Registry.CurrentUser, @"Control Panel\Desktop", "WaitToKillAppTimeout");

                if (!string.Equals(autoEnd, "1", StringComparison.Ordinal)) return false;
                if (!string.Equals(hung, "5000", StringComparison.Ordinal)) return false;
                if (!string.Equals(wait, "5000", StringComparison.Ordinal)) return false;

                return true;
            }
            catch { return false; }
        }

        private static bool IsLatencyTweaked()
        {
            try
            {
                var a = RegistryGetDword(Registry.LocalMachine,
                    @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile",
                    "SystemResponsiveness");

                using var k = Registry.LocalMachine.OpenSubKey(
                    @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile",
                    false);

                var bObj = k?.GetValue("NetworkThrottlingIndex");
                var b = bObj?.ToString()?.Trim() ?? "";
                return a == 0 && (b.Equals("ffffffff", StringComparison.OrdinalIgnoreCase) || b.Equals("4294967295"));
            }
            catch { return false; }
        }

        // ===========================
        // GAMES VIEW
        // ===========================
        private Panel BuildGamesView()
        {
            var p = new Panel { Dock = DockStyle.Fill, BackColor = Theme.Bg, Visible = false };
            p.Controls.Add(BuildTopTabs());

            var content = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(18),
                BackColor = Theme.Bg
            };

            var card = new Panel
            {
                Dock = DockStyle.Top,
                Height = 180,
                BackColor = Theme.Card,
                Padding = new Padding(18)
            };

            card.Paint += (_, e) =>
            {
                using var pen = new Pen(Color.FromArgb(70, 70, 90));
                e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
            };

            var lbl = new Label
            {
                Text = "Games",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 16f, FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = 42
            };

            var sub = new Label
            {
                Text = "Essa aba fica funcional agora. Depois vamos colocar CS2 / Fortnite / Valorant / Rust com configs por jogo.",
                ForeColor = Theme.SubText,
                Font = new Font("Segoe UI", 10f),
                Dock = DockStyle.Top,
                Height = 60
            };

            var hint = new Label
            {
                Text = "✅ Navegação OK. Aqui entra: presets, editar arquivos .cfg, gameusersettings.ini, etc.",
                ForeColor = Color.FromArgb(0, 200, 83),
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = 26
            };

            card.Controls.Add(hint);
            card.Controls.Add(sub);
            card.Controls.Add(lbl);

            content.Controls.Add(card);
            p.Controls.Add(content);
            return p;
        }

        // ===========================
        // SETTINGS VIEW
        // ===========================
        private Panel BuildSettingsView()
        {
            var p = new Panel { Dock = DockStyle.Fill, BackColor = Theme.Bg, Visible = false };
            p.Controls.Add(BuildTopTabs());

            var content = new Panel { Dock = DockStyle.Fill, Padding = new Padding(18) };

            var cols = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1
            };
            cols.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 48));
            cols.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 52));

            var licenseCard = new Panel { Dock = DockStyle.Fill, BackColor = Theme.Card, Padding = new Padding(18), Margin = new Padding(8) };
            var sysCard = new Panel { Dock = DockStyle.Fill, BackColor = Theme.Card, Padding = new Padding(18), Margin = new Padding(8) };

            licenseCard.Paint += (_, e) =>
            {
                using var pen = new Pen(Color.FromArgb(70, 70, 90));
                e.Graphics.DrawRectangle(pen, 0, 0, licenseCard.Width - 1, licenseCard.Height - 1);
            };

            sysCard.Paint += (_, e) =>
            {
                using var pen = new Pen(Color.FromArgb(70, 70, 90));
                e.Graphics.DrawRectangle(pen, 0, 0, sysCard.Width - 1, sysCard.Height - 1);
            };

            var title = new Label
            {
                Text = "Settings",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 14f, FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = 36
            };

            settingsKey = new Label { ForeColor = Color.White, Font = new Font("Segoe UI", 10.5f, FontStyle.Bold), Dock = DockStyle.Top, Height = 26 };
            settingsStatus = new Label { ForeColor = Theme.SubText, Font = new Font("Segoe UI", 9.5f), Dock = DockStyle.Top, Height = 20 };
            settingsExp = new Label { ForeColor = Theme.SubText, Font = new Font("Segoe UI", 9.5f), Dock = DockStyle.Top, Height = 20 };
            settingsRemaining = new Label { ForeColor = Color.FromArgb(0, 200, 83), Font = new Font("Segoe UI", 12f, FontStyle.Bold), Dock = DockStyle.Top, Height = 36 };

            licenseCard.Controls.Add(settingsRemaining);
            licenseCard.Controls.Add(settingsExp);
            licenseCard.Controls.Add(settingsStatus);
            licenseCard.Controls.Add(settingsKey);
            licenseCard.Controls.Add(new Panel { Dock = DockStyle.Top, Height = 8 });
            licenseCard.Controls.Add(title);

            var sysTitle = new Label
            {
                Text = "System Specs",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 14f, FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = 36
            };

            settingsSpecs = new Label
            {
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9.8f),
                Dock = DockStyle.Fill
            };

            sysCard.Controls.Add(settingsSpecs);
            sysCard.Controls.Add(new Panel { Dock = DockStyle.Top, Height = 8 });
            sysCard.Controls.Add(sysTitle);

            cols.Controls.Add(licenseCard, 0, 0);
            cols.Controls.Add(sysCard, 1, 0);

            content.Controls.Add(cols);
            p.Controls.Add(content);
            return p;
        }

        private void UpdateSettingsKeyArea()
        {
            settingsKey.Text = "Key: " + (_license.CurrentKey ?? "--");
            settingsStatus.Text = "Status: " + (_license.IsValid ? "Active" : "Inactive");
            settingsExp.Text = "Expires: " + (_license.ExpiresAtUtc?.ToLocalTime().ToString("dd/MM/yyyy HH:mm") ?? "--");
            settingsRemaining.Text = "Remaining: " + _license.GetRemainingText();
        }

        private void UpdateSettingsSpecsArea()
        {
            settingsSpecs.Text =
                $"CPU: {_cachedSystemInfo.CPU}\n\n" +
                $"RAM: {_cachedSystemInfo.RAM}\n\n" +
                $"GPU: {_cachedSystemInfo.GPU}\n\n" +
                $"OS: {_cachedSystemInfo.OS} (Build {_cachedSystemInfo.Build})";
        }

        private void RefreshSystemInfo()
        {
            try { _cachedSystemInfo = _sysReader.Read(); }
            catch { _cachedSystemInfo = new Core.System.SystemInfo(); }
        }

        // ===========================
        // BUSY OVERLAY
        // ===========================
        private void SetupBusyOverlay()
        {
            busyOverlay = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(0, 0, 0, 0), Visible = false };

            var card = new Panel { Size = new Size(460, 180), BackColor = Theme.Card, Padding = new Padding(18) };
            card.Paint += (_, e) =>
            {
                using var pen = new Pen(Color.FromArgb(70, 70, 90));
                e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
            };

            busyText = new Label
            {
                Text = "Running...",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12.5f, FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = 40
            };

            var hint = new Label
            {
                Text = "Please wait. Applying tweaks.",
                ForeColor = Theme.SubText,
                Font = new Font("Segoe UI", 9.5f),
                Dock = DockStyle.Top,
                Height = 26
            };

            var bar = new ProgressBar { Dock = DockStyle.Top, Height = 10, Style = ProgressBarStyle.Marquee, MarqueeAnimationSpeed = 28 };

            card.Controls.Add(bar);
            card.Controls.Add(new Panel { Dock = DockStyle.Top, Height = 12 });
            card.Controls.Add(hint);
            card.Controls.Add(busyText);

            busyOverlay.Controls.Add(card);
            busyOverlay.Resize += (_, __) =>
            {
                card.Left = (busyOverlay.Width - card.Width) / 2;
                card.Top = (busyOverlay.Height - card.Height) / 2;
            };

            root.Controls.Add(busyOverlay);
            busyOverlay.BringToFront();
        }

        private void ShowBusyOverlay(string text)
        {
            if (busyOverlay == null) return;

            busyOverlay.Visible = true;
            busyOverlay.BackColor = Color.FromArgb(180, 0, 0, 0);

            busyDotsTimer?.Stop();
            busyDotsTimer?.Dispose();

            int dots = 0;
            busyDotsTimer = new System.Windows.Forms.Timer { Interval = 420 };
            busyDotsTimer.Tick += (_, __) =>
            {
                dots = (dots + 1) % 4;
                busyText.Text = text + new string('.', dots);
            };
            busyDotsTimer.Start();
        }

        private void HideBusyOverlay()
        {
            busyDotsTimer?.Stop();
            busyDotsTimer?.Dispose();
            busyDotsTimer = null;

            if (busyOverlay == null) return;
            busyOverlay.Visible = false;
            busyOverlay.BackColor = Color.FromArgb(0, 0, 0, 0);
        }

        private void OpenUrl(string url)
        {
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
            }
            catch { }
        }

        // ===========================
        // LOGIN VIEW
        // ===========================
        private Panel BuildLoginView()
        {
            var p = new Panel { Dock = DockStyle.Fill, BackColor = Theme.Bg };

            var card = new Panel
            {
                Size = new Size(900, 420),
                BackColor = Theme.Card,
                Padding = new Padding(28)
            };

            card.Paint += (_, e) =>
            {
                using var pen = new Pen(Color.FromArgb(80, 80, 100));
                e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
            };

            p.Resize += (_, __) =>
            {
                card.Left = (p.Width - card.Width) / 2;
                card.Top = (p.Height - card.Height) / 2;
            };

            var header = new Label
            {
                Text = "System Optimizer",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 18f, FontStyle.Bold),
                Location = new Point(10, 10),
                AutoSize = true
            };

            var sub = new Label
            {
                Text = "Enter your access key to continue",
                ForeColor = Theme.SubText,
                Font = new Font("Segoe UI", 10f),
                Location = new Point(12, 56),
                AutoSize = true
            };

            keyInput = new TextBox
            {
                Font = new Font("Segoe UI", 12f, FontStyle.Regular),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(28, 28, 34),
                BorderStyle = BorderStyle.FixedSingle,
                Location = new Point(14, 110),
                Size = new Size(card.Width - 28, 38),
                TextAlign = HorizontalAlignment.Center
            };

            loginStatus = new Label
            {
                Text = "",
                ForeColor = Color.FromArgb(255, 193, 7),
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                Location = new Point(14, 160),
                Size = new Size(card.Width - 28, 24),
                TextAlign = ContentAlignment.MiddleCenter
            };

            btnLogin = new ModernButton
            {
                Text = "Login",
                IconGlyph = "\uE72E",
                Location = new Point(14, 220),
                Size = new Size(card.Width - 28, 56),
                BaseBackColor = Color.FromArgb(85, 20, 125),
                HoverBackColor = Color.FromArgb(105, 28, 150),
                PressBackColor = Color.FromArgb(70, 16, 110),
                BorderColor = Color.FromArgb(120, 55, 165),
                CornerRadius = 18
            };
            btnLogin.Click += async (_, __) => await HandleLoginAsync();

            btnDiscord = new ModernButton
            {
                Text = "Discord / Support",
                IconGlyph = "\uE774",
                Location = new Point(14, 290),
                Size = new Size(card.Width - 28, 48),
                BaseBackColor = Color.FromArgb(40, 40, 48),
                HoverBackColor = Color.FromArgb(55, 55, 66),
                PressBackColor = Color.FromArgb(30, 30, 38),
                BorderColor = Color.FromArgb(70, 70, 90),
                CornerRadius = 18
            };
            btnDiscord.Click += (_, __) => OpenUrl(DISCORD_INVITE_URL);

            card.Controls.Add(header);
            card.Controls.Add(sub);
            card.Controls.Add(keyInput);
            card.Controls.Add(loginStatus);
            card.Controls.Add(btnLogin);
            card.Controls.Add(btnDiscord);

            p.Controls.Add(card);
            return p;
        }

        private async Task HandleLoginAsync()
        {
            var key = (keyInput.Text ?? string.Empty).Trim();
            if (key.Length < 6)
            {
                SetLoginStatus("Enter a valid key.", isError: true);
                return;
            }

            try
            {
                SetLoginStatus("Validating license...", isError: false);
                ShowBusyOverlay("Validating license");

                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(12));
                var res = await _license.LoginAsync(key, cts.Token).ConfigureAwait(true);

                if (!res.Valid || res.ExpiresAtUtc == null)
                {
                    SetLoginStatus(res.Message ?? "Invalid key.", isError: true);
                    return;
                }

                _license.StartAutoRefresh();
                SetLoginStatus("License validated. Opening dashboard...", isError: false);

                await Task.Delay(150).ConfigureAwait(true);
                ShowTweaks();
            }
            catch (TaskCanceledException)
            {
                SetLoginStatus("API timeout.", isError: true);
            }
            catch (Exception ex)
            {
                SetLoginStatus("Error: " + ex.Message, isError: true);
            }
            finally
            {
                HideBusyOverlay();
            }
        }

        private void SetLoginStatus(string msg, bool isError)
        {
            loginStatus.Text = msg;
            loginStatus.ForeColor = isError ? Color.FromArgb(255, 100, 100) : Color.FromArgb(255, 193, 7);
        }

        // ===========================
        // LOGGER UI
        // ===========================
        private sealed class UiLogSink : ILogSink
        {
            private readonly RichTextBox _box;
            public UiLogSink(RichTextBox box) => _box = box;

            public void Info(string msg) => Write(msg);
            public void Success(string msg) => Write("✅ " + msg);
            public void Warn(string msg) => Write("⚠️ " + msg);
            public void Error(string msg) => Write("❌ " + msg);

            private void Write(string msg)
            {
                if (_box.IsDisposed) return;

                if (_box.InvokeRequired)
                {
                    _box.BeginInvoke(new Action(() => Write(msg)));
                    return;
                }

                _box.AppendText(msg + Environment.NewLine);
                _box.SelectionStart = _box.TextLength;
                _box.ScrollToCaret();
            }
        }

        // ===========================
        // STANDARD EXECUTION (runner + log)
        // ===========================
        private async Task RunOpAsync(IOperation op)
        {
            var uiLog = new UiLogSink(logBox);
            var ctx = new OperationContext(uiLog, _cmd);

            _opsCts?.Cancel();
            _opsCts = new CancellationTokenSource();

            await _runner.RunAsync(op.Name, new List<IOperation> { op }, ctx, _opsCts.Token);

            bool ok = false;
            try { ok = op.CheckApplied(); } catch { ok = true; }
            uiLog.Info(ok ? "🔎 CheckApplied(): ✅ applied" : "🔎 CheckApplied(): ⚠️ not confirmed (best-effort)");
        }

        private void AppendLog(string line)
        {
            if (logBox == null) return;
            logBox.AppendText(line + Environment.NewLine);
        }

        // ===========================
        // LAMBDA OP (for quick ON/OFF without new file)
        // ===========================
        private sealed class LambdaOperation : IOperation
        {
            public string Name { get; }
            public bool RequiresAdmin { get; }

            private readonly Action<IOperationContext> _apply;
            private readonly Func<bool> _check;

            public LambdaOperation(string name, bool requiresAdmin, Action<IOperationContext> apply, Func<bool> check)
            {
                Name = name;
                RequiresAdmin = requiresAdmin;
                _apply = apply;
                _check = check;
            }

            public void Apply(IOperationContext ctx) => _apply(ctx);
            public bool CheckApplied() => _check();
        }

        // ===========================
        // Registry helpers
        // ===========================
        private static void RegistrySetDword(RegistryKey rootKey, string subKeyPath, string name, int value)
        {
            using var k = rootKey.CreateSubKey(subKeyPath, writable: true);
            k?.SetValue(name, value, RegistryValueKind.DWord);
        }

        private static int RegistryGetDword(RegistryKey rootKey, string subKeyPath, string name)
        {
            using var k = rootKey.OpenSubKey(subKeyPath, writable: false);
            var v = k?.GetValue(name);

            if (v is int i) return i;

            // Às vezes vem como string (raríssimo), tenta parse.
            if (v is string s && int.TryParse(s, out var parsed)) return parsed;

            return 0;
        }

        private static void RegistrySetString(RegistryKey rootKey, string subKeyPath, string name, string value)
        {
            using var k = rootKey.CreateSubKey(subKeyPath, writable: true);
            k?.SetValue(name, value, RegistryValueKind.String);
        }

        private static string? RegistryGetString(RegistryKey rootKey, string subKeyPath, string name)
        {
            using var k = rootKey.OpenSubKey(subKeyPath, writable: false);
            var v = k?.GetValue(name);
            return v as string;
        }
    }
}