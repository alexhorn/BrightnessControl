using System.Reflection;
using System.Windows;
using System.Windows.Input;

namespace BrightnessControl
{
    /// <summary>
    /// Interaktionslogik für SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow
    {
        private static SettingsWindow instance;

        public static SettingsWindow ShowInstance()
        {
            if (instance != null)
            {
                instance.Focus();
            }
            else
            {
                instance = new SettingsWindow();
                instance.Closed += (sender, e) => instance = null;
                instance.Show();
            }
            return instance;
        }

        private ModifierKeys volumeUpModifiers, volumeDownModifiers;
        private Key volumeUpKey, volumeDownKey;
        private AutostartManager autostartManager;

        public SettingsWindow()
        {
            InitializeComponent();

            volumeUpModifiers = (ModifierKeys)Properties.Settings.Default.VolumeUpModifiers;
            volumeDownModifiers = (ModifierKeys)Properties.Settings.Default.VolumeDownModifiers;
            volumeUpKey = (Key)Properties.Settings.Default.VolumeUpKey;
            volumeDownKey = (Key)Properties.Settings.Default.VolumeDownKey;
            RefreshHotkeyTextBoxes();

            autostartManager = new AutostartManager("BrightnessControl", Assembly.GetExecutingAssembly().Location, "/autostart", AutostartType.CurrentUser);
            autostartCheckBox.IsChecked = autostartManager.IsRegistered;
        }

        private bool IsModifier(Key key)
        {
            return key == Key.LeftAlt
                || key == Key.RightAlt
                || key == Key.LeftCtrl
                || key == Key.RightCtrl
                || key == Key.LeftShift
                || key == Key.RightShift
                || key == Key.LWin
                || key == Key.RWin;
        }

        private void RefreshHotkeyTextBoxes()
        {
            volumeUpTextBox.Text = volumeUpModifiers.ToString() + " + " + volumeUpKey.ToString();
            volumeDownTextBox.Text = volumeDownModifiers.ToString() + " + " + volumeDownKey.ToString();
        }

        private void volumeUpTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = true;
        }

        private void volumeUpTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            Key key = e.Key == Key.System ? e.SystemKey : e.Key;
            if (!IsModifier(key))
            {
                volumeUpModifiers = Keyboard.Modifiers;
                volumeUpKey = key;
                RefreshHotkeyTextBoxes();
            }
            e.Handled = true;
        }

        private void volumeDownTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = true;
        }

        private void volumeDownTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            Key key = e.Key == Key.System ? e.SystemKey : e.Key;
            if (!IsModifier(key))
            {
                volumeDownModifiers = Keyboard.Modifiers;
                volumeDownKey = key;
                RefreshHotkeyTextBoxes();
            }
            e.Handled = true;
        }

        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.VolumeUpModifiers = (int)volumeUpModifiers;
            Properties.Settings.Default.VolumeUpKey = (int)volumeUpKey;
            Properties.Settings.Default.VolumeDownModifiers = (int)volumeDownModifiers;
            Properties.Settings.Default.VolumeDownKey = (int)volumeDownKey;
            Properties.Settings.Default.Save();

            if (autostartCheckBox.IsChecked == true && !autostartManager.IsRegistered)
            {
                autostartManager.Register();
            }
            else if (autostartCheckBox.IsChecked == false && autostartManager.IsRegistered)
            {
                autostartManager.Unregister();
            }

            ((MainWindow)Application.Current.MainWindow).UnregisterHotkeys();
            ((MainWindow)Application.Current.MainWindow).RegisterHotkeys();

            Close();
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

    }
}
