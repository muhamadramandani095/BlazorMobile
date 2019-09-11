﻿using BlazorMobile.Common.Helpers;
using BlazorMobile.Common.Interfaces;
using BlazorMobile.Common.Services;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace BlazorMobile.Services
{
    internal static class OperatingSystem
    {
        public static bool IsWindows() =>
            RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        public static bool IsMacOS() =>
            RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

        public static bool IsLinux() =>
            RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
    }

    public class BlazorXamarinDeviceService : IBlazorXamarinDeviceService
    {
        private static bool _init = false;

        internal static void InitRuntimePlatform()
        {
            if (_init)
            {
                return;
            }

            string device = BlazorMobile.Common.BlazorDevice.Unknown;

            if (ContextHelper.IsBlazorMobile())
            {
                device = Device.RuntimePlatform;
            }
            else if (ContextHelper.IsElectronNET())
            {
                if (OperatingSystem.IsWindows())
                {
                    device = BlazorMobile.Common.BlazorDevice.Windows;
                }
                else if (OperatingSystem.IsLinux())
                {
                    device = BlazorMobile.Common.BlazorDevice.Linux;
                }
                else if (OperatingSystem.IsMacOS())
                {
                    device = BlazorMobile.Common.BlazorDevice.macOS;
                }
            }

            BlazorMobile.Common.BlazorDevice.RuntimePlatform = device;

            _init = true;
        }

        public Task<string> GetRuntimePlatform()
        {
            return Task.FromResult(BlazorMobile.Common.BlazorDevice.RuntimePlatform);
        }

        public Task WriteLine(string message)
        {
            ConsoleHelper.WriteLine(message);
            return Task.CompletedTask;
        }
    }
}
