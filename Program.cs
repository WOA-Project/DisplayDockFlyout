using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Device.Net;

#if (!LIBUSB)
using Usb.Net.Windows;
#else
using Device.Net.LibUsb;
#endif

namespace DisplayDockFlyout
{
    static class Program
    {
        public static MicrosoftDock msdock = new MicrosoftDock();
        public static NotifyIcon notifyIcon = null;

        private static readonly DebugLogger Logger = new DebugLogger();
        private static readonly DebugTracer Tracer = new DebugTracer();

        private static Flyout form;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {

#if (LIBUSB)
            LibUsbUsbDeviceFactory.Register(Logger, Tracer);
#else
            WindowsUsbDeviceFactory.Register(Logger, Tracer);
#endif
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            MenuItem exitMenuItem = new MenuItem("E&xit", new EventHandler(Exit));

            notifyIcon = new NotifyIcon();
            notifyIcon.Click += new EventHandler(OpenApp);
            notifyIcon.Icon = Properties.Resources.DevIcon_HD500;
            notifyIcon.ContextMenu = new ContextMenu(new MenuItem[] { exitMenuItem });
            notifyIcon.Text = "Display dock";

            notifyIcon.Visible = true;

            msdock.StartListening();
            msdock.DockDeviceDisconnected += Msdock_DockDeviceDisconnected;
            msdock.DockDeviceInitialized += Msdock_DockDeviceInitialized;

            Application.Run();
        }

        private static void Msdock_DockDeviceInitialized(object sender, EventArgs e)
        {
            notifyIcon.ShowBalloonTip(3000, "Display dock", "A display dock has been connected.", ToolTipIcon.None);
        }

        private static void Msdock_DockDeviceDisconnected(object sender, EventArgs e)
        {
            notifyIcon.ShowBalloonTip(3000, "Display dock disconnected", "A display dock has been disconnected.", ToolTipIcon.None);
        }

        private static void OpenApp(object sender, EventArgs e)
        {
            if (form == null)
            {
                form = new Flyout();
            }

            if (form.Visible)
            {
                form.Hide2();
            }
            else
            {
                form.ShowWithFocus();
            }
        }

        private static void Exit(object sender, EventArgs e)
        {
            msdock.Dispose();
            notifyIcon.Visible = false;
            notifyIcon.Dispose();
            Application.Exit();
        }
    }
}
