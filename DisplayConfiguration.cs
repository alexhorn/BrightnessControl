using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace BrightnessControl
{
    class DisplayConfiguration
    {
        private const int MONITOR_DEFAULTTONEAREST = 2;

        private const int PHYSICAL_MONITOR_DESCRIPTION_SIZE = 128;

        private const int MC_CAPS_BRIGHTNESS = 0x2;
        private const int MC_CAPS_CONTRAST = 0x4;

        [StructLayout(LayoutKind.Sequential)]
        public struct PHYSICAL_MONITOR
        {
            public IntPtr hPhysicalMonitor;
            [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U2, SizeConst = PHYSICAL_MONITOR_DESCRIPTION_SIZE)]
            public char[] szPhysicalMonitorDescription;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            int x;
            int y;
        }

        [DllImport("user32.dll", SetLastError = true)]
        private extern static bool GetCursorPos(out POINT lpPoint);

        [DllImport("user32.dll", SetLastError = false)]
        private extern static IntPtr MonitorFromPoint(POINT pt, uint dwFlags);

        [DllImport("dxva2.dll", SetLastError = true)]
        private extern static bool GetPhysicalMonitorsFromHMONITOR(IntPtr hMonitor, uint dwPhysicalMonitorArraySize, [Out] PHYSICAL_MONITOR[] pPhysicalMonitorArray);

        [DllImport("dxva2.dll", SetLastError = true)]
        private extern static bool GetNumberOfPhysicalMonitorsFromHMONITOR(IntPtr hMonitor, out uint pdwNumberOfPhysicalMonitors);

        [DllImport("dxva2.dll", SetLastError = true)]
        private extern static bool DestroyPhysicalMonitors(uint dwPhysicalMonitorArraySize, PHYSICAL_MONITOR[] pPhysicalMonitorArray);

        [DllImport("dxva2.dll", SetLastError = true)]
        private extern static bool GetMonitorCapabilities(IntPtr hMonitor, out uint pdwMonitorCapabilities, out uint pdwSupportedColorTemperatures);

        [DllImport("dxva2.dll", SetLastError = true)]
        private extern static bool GetMonitorBrightness(IntPtr hMonitor, out uint pdwMinimumBrightness, out uint pdwCurrentBrightness, out uint pdwMaximumBrightness);

        [DllImport("dxva2.dll", SetLastError = true)]
        private extern static bool SetMonitorBrightness(IntPtr hMonitor, uint dwNewBrightness);

        [DllImport("dxva2.dll", SetLastError = true)]
        private extern static bool GetMonitorContrast(IntPtr hMonitor, out uint pdwMinimumContrast, out uint pdwCurrentContrast, out uint pdwMaximumContrast);

        [DllImport("dxva2.dll", SetLastError = true)]
        private extern static bool SetMonitorContrast(IntPtr hMonitor, uint dwNewContrast);

        public static IntPtr GetCurrentMonitor()
        {
            POINT point = new POINT();
            if (!GetCursorPos(out point))
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
            return MonitorFromPoint(point, MONITOR_DEFAULTTONEAREST);
        }

        public static PHYSICAL_MONITOR[] GetPhysicalMonitors(IntPtr hMonitor)
        {
            uint dwNumberOfPhysicalMonitors;
            if (!GetNumberOfPhysicalMonitorsFromHMONITOR(hMonitor, out dwNumberOfPhysicalMonitors))
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
            PHYSICAL_MONITOR[] physicalMonitorArray = new PHYSICAL_MONITOR[dwNumberOfPhysicalMonitors];
            if (!GetPhysicalMonitorsFromHMONITOR(hMonitor, dwNumberOfPhysicalMonitors, physicalMonitorArray))
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
            return physicalMonitorArray;
        }

        public static void DestroyPhysicalMonitors(PHYSICAL_MONITOR[] physicalMonitorArray)
        {
            if (!DestroyPhysicalMonitors((uint)physicalMonitorArray.Length, physicalMonitorArray))
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
        }

        private static uint GetMonitorCapabilities(PHYSICAL_MONITOR physicalMonitor)
        {
            uint dwMonitorCapabilities, dwSupportedColorTemperatures;
            if (!GetMonitorCapabilities(physicalMonitor.hPhysicalMonitor, out dwMonitorCapabilities, out dwSupportedColorTemperatures))
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
            return dwMonitorCapabilities;
        }

        public static bool GetBrightnessSupport(PHYSICAL_MONITOR physicalMonitor)
        {
            return (GetMonitorCapabilities(physicalMonitor) & MC_CAPS_BRIGHTNESS) != 0;
        }

        public static bool GetContrastSupport(PHYSICAL_MONITOR physicalMonitor)
        {
            return (GetMonitorCapabilities(physicalMonitor) & MC_CAPS_CONTRAST) != 0;
        }

        public static double GetMonitorBrightness(PHYSICAL_MONITOR physicalMonitor)
        {
            uint dwMinimumBrightness, dwCurrentBrightness, dwMaximumBrightness;
            if (!GetMonitorBrightness(physicalMonitor.hPhysicalMonitor, out dwMinimumBrightness, out dwCurrentBrightness, out dwMaximumBrightness))
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
            return (double)(dwCurrentBrightness - dwMinimumBrightness) / (double)(dwMaximumBrightness - dwMinimumBrightness);
        }

        public static void SetMonitorBrightness(PHYSICAL_MONITOR physicalMonitor, double brightness)
        {
            uint dwMinimumBrightness, dwCurrentBrightness, dwMaximumBrightness;
            if (!GetMonitorBrightness(physicalMonitor.hPhysicalMonitor, out dwMinimumBrightness, out dwCurrentBrightness, out dwMaximumBrightness))
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
            if (!SetMonitorBrightness(physicalMonitor.hPhysicalMonitor, (uint)(dwMinimumBrightness + (dwMaximumBrightness - dwMinimumBrightness) * brightness)))
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
        }

        public static double GetMonitorContrast(PHYSICAL_MONITOR physicalMonitor)
        {
            uint dwMinimumContrast, dwCurrentContrast, dwMaximumContrast;
            if (!GetMonitorContrast(physicalMonitor.hPhysicalMonitor, out dwMinimumContrast, out dwCurrentContrast, out dwMaximumContrast))
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
            return (double)(dwCurrentContrast - dwMinimumContrast) / (double)(dwMaximumContrast - dwMinimumContrast);
        }

        public static void SetMonitorContrast(PHYSICAL_MONITOR physicalMonitor, double contrast)
        {
            uint dwMinimumContrast, dwCurrentContrast, dwMaximumContrast;
            if (!GetMonitorContrast(physicalMonitor.hPhysicalMonitor, out dwMinimumContrast, out dwCurrentContrast, out dwMaximumContrast))
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
            if (!SetMonitorContrast(physicalMonitor.hPhysicalMonitor, (uint)(dwMinimumContrast + (dwMaximumContrast - dwMinimumContrast) * contrast)))
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
        }
    }
}
