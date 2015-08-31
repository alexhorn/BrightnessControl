using Microsoft.Win32;

namespace BrightnessControl
{
    class AutostartManager
    {
        private string name, path, arguments;
        private RegistryKey runKey;

        public AutostartManager(string name, string path, string arguments, AutostartType type)
        {
            this.name = name;
            this.path = path;
            this.arguments = arguments;
            switch (type)
            {
                case AutostartType.CurrentUser:
                    runKey = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run");
                    break;
                case AutostartType.LocalMachine:
                    runKey = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run");
                    break;
            }
        }

        public bool IsRegistered
        {
            get
            {
                return runKey.GetValue(name, null) != null;
            }
        }

        public void Register()
        {
            runKey.SetValue(name, "\"" + path + "\" " + arguments);
        }

        public void Unregister()
        {
            if (IsRegistered)
            {
                runKey.DeleteValue(name);
            }
        }
    }

    enum AutostartType {
        CurrentUser, LocalMachine
    }
}
