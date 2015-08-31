using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.IO;

namespace BrightnessControl
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        const int ERROR_GEN_FAILURE = 0x1F;
        static TimeSpan HIDING_DELAY = TimeSpan.FromSeconds(5);
        static TimeSpan CHANGE_DELAY = TimeSpan.FromMilliseconds(250);

        DispatcherTimeout brightnessTimeout, contrastTimeout, hideTimeout;
        DisplayConfiguration.PHYSICAL_MONITOR[] physicalMonitors;
        HotkeyManager hotkeys;
        Storyboard hideStoryboard;
        System.Windows.Forms.NotifyIcon notifyIcon;

        public MainWindow()
        {
            InitializeComponent();
            physicalMonitors = DisplayConfiguration.GetPhysicalMonitors(DisplayConfiguration.GetCurrentMonitor());
            hideStoryboard = (Storyboard)FindResource("hide");
            notifyIcon = new System.Windows.Forms.NotifyIcon();
            notifyIcon.Text = "Brightness and Contrast";
            using (Stream stream = Application.GetResourceStream(new Uri("brightness.ico", UriKind.Relative)).Stream)
            {
                notifyIcon.Icon = new System.Drawing.Icon(stream);
            }
            notifyIcon.Visible = true;
            notifyIcon.MouseClick += notifyIcon_MouseClick;
            notifyIcon.ContextMenu = new System.Windows.Forms.ContextMenu();
            notifyIcon.ContextMenu.MenuItems.Add(new System.Windows.Forms.MenuItem("Settings", (sender, e) => SettingsWindow.ShowInstance()));
            notifyIcon.ContextMenu.MenuItems.Add(new System.Windows.Forms.MenuItem("Quit", (sender, e) => Close()));
            brightnessSlider.Value = DisplayConfiguration.GetMonitorBrightness(physicalMonitors[0]) * 100;
            contrastSlider.Value = DisplayConfiguration.GetMonitorContrast(physicalMonitors[0]) * 100;
        }

        public void RegisterHotkeys()
        {
            hotkeys = new HotkeyManager(this);
            hotkeys.Register(0, (ModifierKeys)Properties.Settings.Default.VolumeDownModifiers, (Key)Properties.Settings.Default.VolumeDownKey);
            hotkeys.Register(1, (ModifierKeys)Properties.Settings.Default.VolumeUpModifiers, (Key)Properties.Settings.Default.VolumeUpKey);
            hotkeys.Pressed += hotkeys_Pressed;
        }

        public void UnregisterHotkeys()
        {
            hotkeys.Dispose();
        }

        private void ScheduleHiding()
        {
            hideStoryboard.Stop(this);
            if (hideTimeout != null)
            {
                hideTimeout.Cancel();
            }
            hideTimeout = new DispatcherTimeout(() => hideStoryboard.Begin(this, true), HIDING_DELAY);
        }

        private void RefreshBrightness()
        {
            foreach (DisplayConfiguration.PHYSICAL_MONITOR physicalMonitor in physicalMonitors)
            {
                try
                {
                    DisplayConfiguration.SetMonitorBrightness(physicalMonitor, brightnessSlider.Value / brightnessSlider.Maximum);
                }
                catch (Win32Exception e_)
                {
                    // LG Flatron W2443T sometimes causes ERROR_GEN_FAILURE when rapidly changing brightness or contrast
                    if (e_.NativeErrorCode == ERROR_GEN_FAILURE)
                    {
                        Debug.WriteLine("ERROR_GEN_FAILURE while setting brightness, rescheduling");
                        brightnessTimeout = new DispatcherTimeout(RefreshBrightness, CHANGE_DELAY);
                        break;
                    }
                    else
                    {
                        throw;
                    }
                }
            }
        }

        private void RefreshContrast()
        {
            foreach (DisplayConfiguration.PHYSICAL_MONITOR physicalMonitor in physicalMonitors)
            {
                try
                {
                    DisplayConfiguration.SetMonitorContrast(physicalMonitor, contrastSlider.Value / contrastSlider.Maximum);
                }
                catch (Win32Exception e_)
                {
                    // LG Flatron W2443T sometimes causes ERROR_GEN_FAILURE when rapidly changing brightness or contrast
                    if (e_.NativeErrorCode == ERROR_GEN_FAILURE)
                    {
                        Debug.WriteLine("ERROR_GEN_FAILURE while setting contrast, rescheduling");
                        contrastTimeout = new DispatcherTimeout(RefreshContrast, CHANGE_DELAY);
                        break;
                    }
                    else
                    {
                        throw;
                    }
                }
            }
        }

        private void brightnessSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (brightnessTimeout != null)
            {
                brightnessTimeout.Cancel();
            }
            brightnessTimeout = new DispatcherTimeout(RefreshBrightness, CHANGE_DELAY);
            ScheduleHiding();
        }

        private void contrastSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (contrastTimeout != null)
            {
                contrastTimeout.Cancel();
            }
            contrastTimeout = new DispatcherTimeout(RefreshContrast, CHANGE_DELAY);
            ScheduleHiding();
        }

        private void hotkeys_Pressed(object sender, PressedEventArgs e)
        {
            Visibility = Visibility.Visible;
            switch (e.Id)
            {
                case 0:
                    brightnessSlider.Value -= 5;
                    break;
                case 1:
                    brightnessSlider.Value += 5;
                    break;
            }
        }

        private void notifyIcon_MouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                Visibility = Visibility.Visible;
                ScheduleHiding();
            }
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            Visibility = Visibility.Hidden;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Visibility = Visibility.Hidden;

            RegisterHotkeys();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            UnregisterHotkeys();

            DisplayConfiguration.DestroyPhysicalMonitors(physicalMonitors);
        }
    }
}
