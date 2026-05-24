// SPDX-License-Identifier: GPL-3.0-or-later
using System.Windows;
using System.Windows.Threading;

namespace PtpLabClock.App;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        DispatcherUnhandledException += OnDispatcherUnhandledException;
        base.OnStartup(e);
    }

    private static void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        MessageBox.Show(
            "PTP Lab Clock Simulator caught a startup/runtime error.\n\n" +
            e.Exception.Message +
            "\n\nTip: install Npcap and run Visual Studio/app as Administrator for RAW packet mode. Demo Mode can run without Npcap.",
            "PTP Lab Clock Simulator",
            MessageBoxButton.OK,
            MessageBoxImage.Warning);
        e.Handled = true;
    }
}
