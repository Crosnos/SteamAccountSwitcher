﻿#region

using System.Linq;
using System.Threading;
using System.Windows.Interop;
using SteamAccountSwitcher.Properties;

#endregion

namespace SteamAccountSwitcher
{
    internal static class AppInitHelper
    {
        public static bool Initialize()
        {
            if (IsExistingInstanceRunning())
            {
                //Popup.Show("You can only run one instance at a time.");
                return false;
            }
            LoadSettings();
            LoadAccounts();
            InitMainWindow();
            if (Settings.Default.AlwaysOn)
                TrayIconHelper.CreateTrayIcon();

            LaunchStartAccount();

            return true;
        }

        private static void LaunchStartAccount()
        {
            if (!string.IsNullOrWhiteSpace(Settings.Default.OnStartLoginName) && Settings.Default.AlwaysOn &&
                App.Arguments.Contains("-systemstartup"))
            {
                foreach (
                    var account in
                        App.Accounts.Where(x => x.Username == Settings.Default.OnStartLoginName)
                    )
                {
                    account.SwitchTo(onStart: true);
                    break;
                }
            }
        }

        private static void InitMainWindow()
        {
            App.SwitchWindow = new SwitchWindow();
            new WindowInteropHelper(App.SwitchWindow).EnsureHandle();
            if (Settings.Default.AlwaysOn)
            {
            }
            else
            {
                SwitchWindowHelper.ShowSwitcherWindow();
            }
        }

        private static bool IsExistingInstanceRunning()
        {
            bool isNewInstance;
            App.AppMutex = new Mutex(true, AssemblyInfo.Guid, out isNewInstance);
            if (App.Arguments.Contains("-restarting") || App.Arguments.Contains("-multiinstance") ||
                Settings.Default.MultiInstance || isNewInstance)
                return false;
            SingleInstanceHelper.ShowExistingInstance();
            AppHelper.ShutdownApplication();
            return true;
        }

        private static void LoadAccounts()
        {
            if (AppHelper.IsFirstLaunch && Settings.Default.Accounts == string.Empty)
                Settings.Default.Accounts = AccountDataHelper.DefaultData();

            AccountDataHelper.ReloadData();
        }

        private static void LoadSettings()
        {
            SettingsHelper.UpgradeSettings();

            if (App.Arguments.Contains("-reset"))
                SettingsHelper.ResetSettings();

            SettingsHelper.IncrementLaunches();
        }
    }
}