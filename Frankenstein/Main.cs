using System;
using System.Drawing;
using System.Windows.Forms;
using Frankenstein.Properties;
using log4net;

namespace Frankenstein
{
    public partial class Main : Form
    {
        private readonly Timer _timer = new Timer();

        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public Main()
        {
            InitializeComponent();
        }

        private void trayIcon_MouseClick(object sender, MouseEventArgs e)
        {
            WindowState = FormWindowState.Normal;
        }

        private void Main_Load(object sender, EventArgs e)
        {
            Log.Info("Entered");

            foreach (var key in Enum.GetValues(typeof(Keys)))
            {
                keysListBox.Items.Add(key);
            }

            var keyPress = Enum.Parse(typeof(Keys), Settings.Default.KeyPress);

            keysListBox.SelectedItem = keyPress;

            sendKeyPress.Checked = Settings.Default.SendKeyPress;

            moveMouse.Checked = Settings.Default.MoveMouse;

            var interval = Settings.Default.Interval;

            timerInterval.Value = interval;

            showBalloon.Checked = Settings.Default.ShowTipAtStart;

            if (showBalloon.Checked)
            {
                if (sendKeyPress.Checked || moveMouse.Checked)
                {
                    trayIcon.BalloonTipText = string.Format(
                        "Electrifying things every {0} second{1}",
                        timerInterval.Value,
                        timerInterval.Value == 1 ? string.Empty : "s");
                }
                 else
                {
                    trayIcon.BalloonTipText = "Sadly you have told me not to experiment";
                }

                trayIcon.ShowBalloonTip(500);
            }

            SetTimer();

            TimerEventProcessor(null, null);

            timerInterval.ValueChanged += new EventHandler(settings_Changed);
            sendKeyPress.CheckedChanged += new EventHandler(settings_Changed);
            showBalloon.CheckedChanged += new EventHandler(settings_Changed);
            moveMouse.CheckedChanged += new EventHandler(settings_Changed);
            keysListBox.SelectedIndexChanged += new EventHandler(settings_Changed);
        }

        private void MoveMousePosition()
        {
            Log.Info("Entered");

            this.Cursor = new Cursor(Cursor.Current.Handle);

            Cursor.Position = new Point(Cursor.Position.X - 1, Cursor.Position.Y - 1);

            Cursor.Position = new Point(Cursor.Position.X + 1, Cursor.Position.Y + 1);
        }

        private void SetTimer()
        {
            Log.Info("Entered");

            _timer.Stop();

            _timer.Tick -= TimerEventProcessor;

            _timer.Interval = Convert.ToInt32(timerInterval.Value) * 1000;

            _timer.Tick += TimerEventProcessor;

            _timer.Start();
        }

        private void TimerEventProcessor(object sender, EventArgs e)
        {
            Log.Info("Entered");

            try
            {
                NativeMethods.PreventSleep();

                if (moveMouse.Checked)
                {
                    MoveMousePosition();
                    NativeMethods.MoveMouse();
                }

                if (sendKeyPress.Checked)
                {
                    var key = string.Format("{{{0}}}", keysListBox.SelectedItem);

                    SendKeys.Send(key);
                }

                Text = string.Format(
                    "Frankenstein performed an experiment @ {0}",
                    DateTime.Now.ToLongTimeString());
            }
            catch(Exception ex)
            {
                Text = string.Format(
                    "Frankenstein failed @ {0}",
                    DateTime.Now.ToLongTimeString());

                Log.Error(Text, ex);
            }

            trayIcon.Text = Text;
        }


        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            Log.Info("Entered");

            _timer.Stop();
            _timer.Tick -= TimerEventProcessor;

            timerInterval.ValueChanged -= new EventHandler(settings_Changed);
            sendKeyPress.CheckedChanged -= new EventHandler(settings_Changed);
            showBalloon.CheckedChanged -= new EventHandler(settings_Changed);
            moveMouse.CheckedChanged -= new EventHandler(settings_Changed);
            keysListBox.SelectedIndexChanged -= new EventHandler(settings_Changed);

            NativeMethods.AllowSleep();

            SaveSettings();
        }

        private void SaveSettings()
        {
            Log.Info("Entered");

            Settings.Default.ShowTipAtStart = showBalloon.Checked;
            Settings.Default.Interval = Convert.ToInt32(timerInterval.Value);
            Settings.Default.KeyPress = keysListBox.SelectedItem.ToString();
            Settings.Default.MoveMouse = moveMouse.Checked;
            Settings.Default.SendKeyPress = sendKeyPress.Checked;

            Settings.Default.Save();
        }

        private void ok_Click(object sender, EventArgs e)
        {
            Log.Info("Entered");

            SaveSettings();
            WindowState = FormWindowState.Minimized;
        }

        private void settings_Changed(object sender, EventArgs e)
        {
            Log.Info("Entered");

            SetTimer();

            SaveSettings();
        }
    }
}