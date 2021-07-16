using System;
using System.Drawing;
using System.Windows.Forms;
using Frankenstein.Properties;

namespace Frankenstein
{
    public partial class Main : Form
    {
        private readonly Timer _timer = new Timer();

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
            foreach (var key in Enum.GetValues(typeof(Keys)))
            {
                keysListBox.Items.Add(key);
            }

            var keyPress = Enum.Parse(typeof(Keys), Settings.Default.KeyPress);

            keysListBox.SelectedItem = keyPress;
            
            showBalloon.Checked = Settings.Default.ShowTipAtStart;

            if (showBalloon.Checked)
            {
                trayIcon.BalloonTipText = string.Format(
                    "Pressing '{0}' every {1} second{2}",
                    keysListBox.SelectedItem,
                    timerInterval.Value,
                    timerInterval.Value == 1 ? string.Empty : "s");

                trayIcon.ShowBalloonTip(500);
            }

            moveMouse.Checked = Settings.Default.MoveMouse;

            var interval = Settings.Default.Interval;

            timerInterval.Value = interval;

            SetTimer();

            TimerEventProcessor(null, null);
        }

        private void MoveMousePosition()
        {
            this.Cursor = new Cursor(Cursor.Current.Handle);

            Cursor.Position = new Point(Cursor.Position.X - 1, Cursor.Position.Y - 1);

            Cursor.Position = new Point(Cursor.Position.X + 1, Cursor.Position.Y + 1);
        }

        private void SetTimer()
        {
            _timer.Stop();

            _timer.Tick += TimerEventProcessor;

            _timer.Interval = Convert.ToInt32(timerInterval.Value) * 1000;

            _timer.Tick += TimerEventProcessor;

            _timer.Start();
        }

        private void TimerEventProcessor(object sender, EventArgs e)
        {
            try
            {
                var key = string.Format("{{{0}}}", keysListBox.SelectedItem);

                SendKeys.Send(key);

                if (moveMouse.Checked)
                    MoveMousePosition();

                Text = string.Format(
                    "Frankenstein pressed {0} @ {1}",
                    keysListBox.SelectedItem,
                    DateTime.Now.ToLongTimeString());
            }
            catch
            {
                Text = string.Format(
                    "Frankenstein Failed @ {0}",
                    DateTime.Now.ToLongTimeString());
            }

            trayIcon.Text = Text;
        }

        private void timerInterval_ValueChanged(object sender, EventArgs e)
        {
            SetTimer();
        }

        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            _timer.Stop();
            _timer.Tick -= TimerEventProcessor;

            SaveSettings();
        }

        private void SaveSettings()
        {
            Settings.Default.ShowTipAtStart = showBalloon.Checked;
            Settings.Default.Interval = Convert.ToInt32(timerInterval.Value);
            Settings.Default.KeyPress = keysListBox.SelectedItem.ToString();
            Settings.Default.MoveMouse = moveMouse.Checked;
            Settings.Default.Save();
        }

        private void ok_Click(object sender, EventArgs e)
        {
            SaveSettings();
            WindowState = FormWindowState.Minimized;
        }
    }
}