// SPDX-License-Identifier: Apache-2.0
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using PtpLabClock.App.ViewModels;

namespace PtpLabClock.App;

public partial class MainWindow : Window
{
    private readonly MainViewModel _vm = new();

    public MainWindow()
    {
        InitializeComponent();
        DataContext = _vm;
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        UpdateProfileIndicator(false);
    }

    private void PresetSegmentHost_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        UpdateProfileIndicator(false);
    }

    private void ProfileRadioButton_Checked(object sender, RoutedEventArgs e)
    {
        UpdateProfileIndicator(true);
    }

    private void UpdateProfileIndicator(bool animate)
    {
        if (!IsLoaded || PresetSegmentHost.ActualWidth <= 0)
            return;

        var segmentWidth = PresetSegmentHost.ActualWidth / 3.0;
        if (segmentWidth <= 0)
            return;

        PresetIndicator.Width = Math.Max(0, segmentWidth);

        var selectedIndex = 0;
        if (AnalyzerProfileButton.IsChecked == true)
            selectedIndex = 1;
        else if (GenericProfileButton.IsChecked == true)
            selectedIndex = 2;

        var targetX = segmentWidth * selectedIndex;

        if (!animate)
        {
            PresetIndicatorTransform.X = targetX;
            return;
        }

        var animation = new DoubleAnimation
        {
            To = targetX,
            Duration = TimeSpan.FromMilliseconds(250),
            EasingFunction = new BackEase
            {
                Amplitude = 0.16,
                EasingMode = EasingMode.EaseOut
            }
        };

        PresetIndicatorTransform.BeginAnimation(TranslateTransform.XProperty, animation);
    }

    private void AdapterDropDownButton_Click(object sender, RoutedEventArgs e)
    {
        if (AdapterPopup.IsOpen)
        {
            AdapterPopup.IsOpen = false;
            AdapterDropDownButton.IsChecked = false;
            AnimateChevron(0);
            return;
        }

        AdapterPopup.IsOpen = true;
        AdapterDropDownButton.IsChecked = true;
        AnimateChevron(180);
    }

    private void AdapterPopup_Closed(object? sender, EventArgs e)
    {
        AdapterDropDownButton.IsChecked = false;
        AnimateChevron(0);
    }

    private void AdapterListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (AdapterPopup.IsOpen)
            AdapterPopup.IsOpen = false;

        AdapterDropDownButton.IsChecked = false;
        AnimateChevron(0);
    }

    private void AnimateChevron(double angle)
    {
        var animation = new DoubleAnimation
        {
            To = angle,
            Duration = TimeSpan.FromMilliseconds(170),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };

        AdapterChevronRotate.BeginAnimation(RotateTransform.AngleProperty, animation);
    }

    protected override async void OnClosed(EventArgs e)
    {
        await _vm.DisposeAsync();
        base.OnClosed(e);
    }
}
