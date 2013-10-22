using System;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using ULTRAMONLib;

namespace ScreenSwitcher
{
    public partial class MainForm : Form
    {
        private readonly NotifyIcon _trayIcon;
        private readonly ContextMenu _trayMenu;
        private readonly Hotkey _key;
        private readonly IUltraMonSystem2 _system = new UltraMonSystem();

        [DllImport("user32.dll", EntryPoint = "GetForegroundWindow")]
        public static extern IntPtr GetForegroundWindow();

        public MainForm()
        {
            InitializeComponent();

            _trayMenu = new ContextMenu();
            _trayMenu.MenuItems.Add("Switch", SwitchScreen);
            _trayMenu.MenuItems.Add("Exit", OnExit);
            _trayIcon = new NotifyIcon
                            {
                                Text = "ScreenSwitcher",
                                Icon = new Icon(GetType(), "icon.ico"),
                                ContextMenu = _trayMenu,
                                Visible = true
                            };

            _key = new Hotkey { KeyCode = Keys.Escape, Shift = true };
            _key.Pressed += delegate { SwitchScreen(null, null); };
            _key.Register(); 
        }

        private void SwitchScreen(object sender, EventArgs e)
        {
            foreach (IUltraMonMonitor2 scr in _system.Monitors.Cast<object>().Where(scr => ((IUltraMonMonitor2)scr).CanChangeOrientation && Screen.FromHandle(GetForegroundWindow()).Primary == ((IUltraMonMonitor2)scr).Primary))
            {
                scr.Orientation = scr.Orientation == ORIENTATION.OR_PORTRAIT ? ORIENTATION.OR_LANDSCAPE : ORIENTATION.OR_PORTRAIT;

                _system.ApplyMonitorChanges();
            }
        }

        private void OnExit(object sender, EventArgs e)
        {
            _key.Unregister();
            _trayIcon.Dispose();
            Dispose();
            Application.Exit();
        }
    }
}
