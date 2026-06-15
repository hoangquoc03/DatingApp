using System;
using System.Windows;
using DatingApp.Desktop.ViewModels;
using WinForms = System.Windows.Forms;
using System.Drawing;

namespace DatingApp.Desktop;

public partial class MainWindow : Window
{
    private WinForms.NotifyIcon? _notifyIcon;
    private bool _isExit;

    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        InitializeTray();
    }

    private void InitializeTray()
    {
        _notifyIcon = new WinForms.NotifyIcon();
        _notifyIcon.Icon = SystemIcons.Application;
        _notifyIcon.Text = "Aura Dating";
        _notifyIcon.Visible = true;

        // Double-click to restore
        _notifyIcon.DoubleClick += (s, e) => RestoreWindow();

        // Create Context Menu for Tray Icon
        var contextMenu = new WinForms.ContextMenuStrip();
        contextMenu.Items.Add("Mở Aura Dating", null, (s, e) => RestoreWindow());
        contextMenu.Items.Add("Thoát", null, (s, e) => ExitApplication());
        _notifyIcon.ContextMenuStrip = contextMenu;
    }

    protected override void OnStateChanged(EventArgs e)
    {
        base.OnStateChanged(e);
        if (WindowState == WindowState.Minimized)
        {
            Hide(); // Hide from taskbar
            if (_notifyIcon != null)
            {
                _notifyIcon.ShowBalloonTip(2000, "Aura Dating", "Ứng dụng đã được thu nhỏ xuống khay hệ thống.", WinForms.ToolTipIcon.Info);
            }
        }
    }

    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
        if (!_isExit)
        {
            e.Cancel = true;
            Hide();
            WindowState = WindowState.Minimized;
        }
        else
        {
            if (_notifyIcon != null)
            {
                _notifyIcon.Visible = false;
                _notifyIcon.Dispose();
            }
            base.OnClosing(e);
        }
    }

    public void RestoreWindow()
    {
        Show();
        WindowState = WindowState.Normal;
        Activate();
    }

    public void ExitApplication()
    {
        _isExit = true;
        System.Windows.Application.Current.Shutdown();
    }

    public void ShowToastNotification(string title, string content)
    {
        if (_notifyIcon != null)
        {
            _notifyIcon.ShowBalloonTip(3000, title, content, WinForms.ToolTipIcon.Info);
        }
    }
}