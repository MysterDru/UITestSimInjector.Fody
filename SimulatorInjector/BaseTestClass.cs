using System;
using Xamarin.UITest;
using Xamarin.UITest.Utils;
using Xamarin.UITest.Configuration;

namespace SimulatorInjector
{
    public class BaseTestClass
    {
        private string deviceId;
        private Platform platform;

        public BaseTestClass(Platform platform, string deviceId = null)
        {
            this.platform = platform;
            this.deviceId = deviceId;
        }

        protected IApp StartAppOnCurrentDevice(iOSAppConfigurator iosApp, AppDataMode appDataMode = AppDataMode.Auto)
        {
            return iosApp
                .DeviceIdentifier(this.deviceId)
                .StartApp(appDataMode);
        }

        protected IApp StartAppOnCurrentDevice(AndroidAppConfigurator androidApp, AppDataMode appDataMode = AppDataMode.Auto)
        {
            return androidApp
                .DeviceSerial(this.deviceId)
                .StartApp(appDataMode);
        }
    }
}