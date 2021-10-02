using Device.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DisplayDockFlyout
{
    internal sealed class MicrosoftDock : IDisposable
    {
        #region Fields
#if(LIBUSB)
        private const int PollMilliseconds = 6000;
#else
        private const int PollMilliseconds = 3000;
#endif
        private readonly List<FilterDeviceDefinition> _DeviceDefinitions = new List<FilterDeviceDefinition>
        {
            new FilterDeviceDefinition{ DeviceType= DeviceType.Usb, VendorId=1118, ProductId=2566 }
        };
        #endregion

        #region Events
        public event EventHandler DockDeviceInitialized;
        public event EventHandler DockDeviceDisconnected;
        #endregion

        #region Public Properties
        public IDevice DockDevice { get; private set; }
        public DeviceListener DeviceListener { get; }
        #endregion

        #region Constructor
        public MicrosoftDock()
        {
            DeviceListener = new DeviceListener(_DeviceDefinitions, PollMilliseconds) { Logger = new DebugLogger() };
        }
        #endregion

        #region Event Handlers
        private void DevicePoller_DeviceInitialized(object sender, DeviceEventArgs e)
        {
            DockDevice = e.Device;
            DockDeviceInitialized?.Invoke(this, new EventArgs());
        }

        private void DevicePoller_DeviceDisconnected(object sender, DeviceEventArgs e)
        {
            DockDevice = null;
            DockDeviceDisconnected?.Invoke(this, new EventArgs());
        }
        #endregion

        #region Public Methods
        public void StartListening()
        {
            DockDevice?.Close();
            DeviceListener.DeviceDisconnected += DevicePoller_DeviceDisconnected;
            DeviceListener.DeviceInitialized += DevicePoller_DeviceInitialized;
            DeviceListener.Start();
        }

        public async Task InitializeDockAsync()
        {
            //Get the first available device and connect to it
            var devices = await DeviceManager.Current.GetDevicesAsync(_DeviceDefinitions);
            DockDevice = devices.FirstOrDefault();

            if (DockDevice == null) throw new Exception("There were no devices found");

            await DockDevice.InitializeAsync();
        }


        public void Dispose()
        {
            DeviceListener.DeviceDisconnected -= DevicePoller_DeviceDisconnected;
            DeviceListener.DeviceInitialized -= DevicePoller_DeviceInitialized;
            DeviceListener.Dispose();
            DockDevice?.Dispose();
        }
        #endregion
    }
}