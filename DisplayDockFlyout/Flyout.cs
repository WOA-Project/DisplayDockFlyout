using Device.Net;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Windows.UI.Xaml.Markup;

namespace DisplayDockFlyout
{
    public partial class Flyout : Form
    {
        public enum QueryType
        {
            Undefined,
            ProtocolVersion,
            FrameSize,
            DeviceType,
            HwVersion,
            SwVersion,
            MunchkinSWLocation,
            Anx7737FWVersion = 8,
            Anx7750FWVersion
        }

        public enum MunchkinContent
        {
            Undefined,
            Low = 7,
            High,
            PD,
            ANX7737FW,
            ANX7750FW
        }

        public enum MessageType
        {
            QueryRequest,
            QueryResponse,
            DataPushStartRequest,
            DataPushStartResponse,
            DataPush,
            StatusRequest,
            StatusResponse,
            CommandRequest,
            CommandResponse,
            Unknown
        }

        public enum UpdateStepType
        {
            ResetCommand = 1,
            DataPush,
            Instruction
        }

        public enum ResDataType
        {
            Dword = 1,
            AsciiString
        }

        public enum DataType
        {
            Request = 0x10,
            Command = 0x14,
            Response
        }

        public Flyout()
        {
            Program.msdock.DockDeviceInitialized += Msdock_DockDeviceInitialized;
            Program.msdock.DockDeviceDisconnected += Msdock_DockDeviceDisconnected;

            InitializeComponent();

            if (Program.msdock.DockDevice == null)
            {
                Msdock_DockDeviceDisconnected(this, new EventArgs());
            }
            else
            {
                Msdock_DockDeviceInitialized(this, new EventArgs());
            }

            Deactivate += Form1_Deactivate;
        }

        internal void Hide2()
        {
            Deactivate -= Form1_Deactivate;
            Hide();
        }

        private void Form1_Deactivate(object sender, EventArgs e)
        {
            Deactivate -= Form1_Deactivate;
            Hide();
        }

        internal void ShowWithFocus()
        {
            var desktopWorkingArea = Screen.PrimaryScreen.WorkingArea;
            var taskbarsize = WindowsTaskbar.Current.Size;

            switch (WindowsTaskbar.Current.Location)
            {
                case WindowsTaskbar.Position.Bottom:
                    {
                        var loc = this.Location;
                        loc.X = desktopWorkingArea.Right - (int)(this.Width);
                        loc.Y = desktopWorkingArea.Bottom - (int)(this.Height);
                        this.Location = loc;

                        break;
                    }
                case WindowsTaskbar.Position.Left:
                    {
                        var loc = this.Location;
                        loc.X = taskbarsize.Right - taskbarsize.Left;
                        loc.Y = desktopWorkingArea.Bottom - (int)(this.Height);
                        this.Location = loc;

                        break;
                    }
                case WindowsTaskbar.Position.Right:
                    {
                        var loc = this.Location;
                        loc.X = desktopWorkingArea.Right - (int)(this.Width);
                        loc.Y = desktopWorkingArea.Bottom - (int)(this.Height);
                        this.Location = loc;

                        break;
                    }
                case WindowsTaskbar.Position.Top:
                    {
                        var loc = this.Location;
                        loc.X = desktopWorkingArea.Right - (int)(this.Width);
                        loc.Y = taskbarsize.Bottom - taskbarsize.Top;
                        this.Location = loc;

                        break;
                    }
            }

            Deactivate += Form1_Deactivate;
            Show();
            Focus();
        }

        private void Form1_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            RebootDock(Program.msdock.DockDevice);
        }

        private async void Msdock_DockDeviceDisconnected(object sender, EventArgs e)
        {
            var xaml = Properties.Resources.Flyout;

            xaml = xaml.Replace("$CONNECTIONSTATUS", "Disconnected");
            xaml = xaml.Replace("$PROTOCOLVERSION", "N/A");
            xaml = xaml.Replace("$DEVICENAME", "N/A");
            xaml = xaml.Replace("$DEVICENAME", "N/A");
            xaml = xaml.Replace("$HWVERSION", "N/A");
            xaml = xaml.Replace("$SWLOCATION", "N/A");
            xaml = xaml.Replace("$SWVERSION", "N/A");
            xaml = xaml.Replace("$ANX7737FWVERSION", "N/A");
            xaml = xaml.Replace("$ANX7750FWVERSION", "N/A");

            if (sender == this)
            {
                Windows.UI.Xaml.Controls.Page control = (Windows.UI.Xaml.Controls.Page)XamlReader.Load(xaml);

                var maingrid = control.Content as Windows.UI.Xaml.Controls.Grid;
                var lastgridinmaingrid = maingrid.Children.Last() as Windows.UI.Xaml.Controls.Grid;
                var button = lastgridinmaingrid.Children[0] as Windows.UI.Xaml.Controls.Button;

                button.IsEnabled = false;
                button.Click += Form1_Click;

                windowsXamlHost1.Child = control;
            }
            else
            {
                await windowsXamlHost1.Child.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    Windows.UI.Xaml.Controls.Page control = (Windows.UI.Xaml.Controls.Page)XamlReader.Load(xaml);

                    var maingrid = control.Content as Windows.UI.Xaml.Controls.Grid;
                    var lastgridinmaingrid = maingrid.Children.Last() as Windows.UI.Xaml.Controls.Grid;
                    var button = lastgridinmaingrid.Children[0] as Windows.UI.Xaml.Controls.Button;

                    button.IsEnabled = false;
                    button.Click += Form1_Click;

                    windowsXamlHost1.Child = control;
                });
            }
        }

        private async void Msdock_DockDeviceInitialized(object sender, EventArgs e)
        {
            var dockDevice = Program.msdock.DockDevice;

            string protocolver = await DoStringRequest(dockDevice, DataType.Request, 0x01, QueryType.ProtocolVersion);

            if (protocolver != "1.1")
            {
                //Console.WriteLine("Unsupported protocol version! Update your device.");
                return;
            }

            int frameSize = await DoIntRequest(dockDevice, DataType.Request, 0x01, QueryType.FrameSize);

            if (frameSize != 64)
            {
                //Console.WriteLine("Unsupported frame size! Update your device.");
                return;
            }

            string devicetype = await DoStringRequest(dockDevice, DataType.Request, 0x02, QueryType.DeviceType);

            string hwversion = await DoStringRequest(dockDevice, DataType.Request, 0x03, QueryType.HwVersion);
            hwversion = hwversion.Substring(0, hwversion.Length - 1);

            int swloc = await DoIntRequest(dockDevice, DataType.Request, 0x04, QueryType.MunchkinSWLocation);

            MunchkinContent content = (MunchkinContent)swloc;

            string swver = await DoVersionRequest(dockDevice, DataType.Request, 0x05, QueryType.SwVersion);

            string Anx7737FW = await DoVersionRequest(dockDevice, DataType.Request, 0x06, QueryType.Anx7737FWVersion);

            string Anx7750FW = await DoVersionRequest(dockDevice, DataType.Request, 0x07, QueryType.Anx7750FWVersion);

            var xaml = Properties.Resources.Flyout;

            xaml = xaml.Replace("$CONNECTIONSTATUS", "Connected");
            xaml = xaml.Replace("$PROTOCOLVERSION", protocolver);
            xaml = xaml.Replace("$DEVICENAME", devicetype);
            xaml = xaml.Replace("$DEVICENAME", devicetype);
            xaml = xaml.Replace("$HWVERSION", hwversion);
            xaml = xaml.Replace("$SWLOCATION", content.ToString());
            xaml = xaml.Replace("$SWVERSION", swver);
            xaml = xaml.Replace("$ANX7737FWVERSION", Anx7737FW);
            xaml = xaml.Replace("$ANX7750FWVERSION", Anx7750FW);

            if (sender == this)
            {
                Windows.UI.Xaml.Controls.Page control = (Windows.UI.Xaml.Controls.Page)XamlReader.Load(xaml);

                var maingrid = control.Content as Windows.UI.Xaml.Controls.Grid;
                var lastgridinmaingrid = maingrid.Children.Last() as Windows.UI.Xaml.Controls.Grid;
                var button = lastgridinmaingrid.Children[0] as Windows.UI.Xaml.Controls.Button;

                button.Click += Form1_Click;

                windowsXamlHost1.Child = control;
            }
            else
            {
                await windowsXamlHost1.Child.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    Windows.UI.Xaml.Controls.Page control = (Windows.UI.Xaml.Controls.Page)XamlReader.Load(xaml);

                    var maingrid = control.Content as Windows.UI.Xaml.Controls.Grid;
                    var lastgridinmaingrid = maingrid.Children.Last() as Windows.UI.Xaml.Controls.Grid;
                    var button = lastgridinmaingrid.Children[0] as Windows.UI.Xaml.Controls.Button;

                    button.Click += Form1_Click;

                    windowsXamlHost1.Child = control;
                });
            }
        }

        static async void RebootDock(IDevice device)
        {
            var req = new byte[] { 0x4D, (byte)DataType.Command, 0x00, 0x0F, 0x00, 0x00, 0x00, 0x08,
                                0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00,
                                0x00, 0x00, 0x00, 0x52, 0x00, 0x00, 0x00, 0x00,
                                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

            await device.WriteAsync(req);
        }

        static byte[] GenerateRequest(DataType type, int num1, QueryType request)
        {
            return new byte[] { 0x4D, (byte)type, 0x00, (byte)num1, 0x00, 0x00, 0x00, 0x01,
                                0x00, 0x00, 0x00, (byte)request, 0x00, 0x00, 0x00, 0x52,
                                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }; ;
        }

        static async Task<string> DoStringRequest(IDevice device, DataType type, int num1, QueryType request)
        {
            var req = GenerateRequest(type, num1, request);
            var result = await device.WriteAndReadAsync(req);
            string res = (string)ParseResponse(result.Data);
            return res;
        }

        static async Task<int> DoIntRequest(IDevice device, DataType type, int num1, QueryType request)
        {
            var req = GenerateRequest(type, num1, request);
            var result = await device.WriteAndReadAsync(req);
            int res = (int)ParseResponse(result.Data);
            return res;
        }

        static async Task<byte[]> DoRawRequest(IDevice device, DataType type, int num1, QueryType request)
        {
            var req = GenerateRequest(type, num1, request);
            var result = await device.WriteAndReadAsync(req);
            var res = ParseResponseRaw(result.Data);
            return res;
        }

        static async Task<string> DoVersionRequest(IDevice device, DataType type, int num1, QueryType request)
        {
            var res = await DoRawRequest(device, type, num1, request);

            string ver = res[2] + "." + res[1] + "." + res[0];

            return ver;
        }

        static object ParseResponse(byte[] buffer)
        {
            DataType type = (DataType)buffer[1];
            int num1 = buffer[3];
            QueryType request = (QueryType)buffer[11];

            if (buffer[15] == (byte)ResDataType.Dword && buffer[20] == 0x52)
            {
                return BitConverter.ToInt32(buffer, 16);
            }

            if (buffer[15] == (byte)ResDataType.AsciiString)
            {
                var length = buffer[16];
                if (buffer[length + 17] == 0x52)
                {
                    return Encoding.ASCII.GetString(buffer, 17, length);
                }
            }

            //Console.WriteLine("WARNING: Unknown response provided");
            return null;
        }

        static byte[] ParseResponseRaw(byte[] buffer)
        {
            DataType type = (DataType)buffer[1];
            int num1 = buffer[3];
            QueryType request = (QueryType)buffer[11];

            if (buffer[15] == (byte)ResDataType.Dword && buffer[20] == 0x52)
            {
                byte[] buffern = new byte[4];
                Array.Copy(buffer, 16, buffern, 0, 4);
                return buffern;
            }

            if (buffer[15] == (byte)ResDataType.AsciiString)
            {
                var length = buffer[16];
                if (buffer[length + 17] == 0x52)
                {
                    byte[] buffern = new byte[length];
                    Array.Copy(buffer, 17, buffern, 0, length);
                    return buffern;
                }
            }

            //Console.WriteLine("WARNING: Unknown response provided");
            return null;
        }
    }
}
