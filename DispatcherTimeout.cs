using System;
using System.Windows.Threading;

namespace BrightnessControl
{
    class DispatcherTimeout
    {
        DispatcherTimer timer;
        Action callback;

        public DispatcherTimeout(Action callback, TimeSpan delay)
        {
            this.callback = callback;

            timer = new DispatcherTimer();
            timer.Interval = delay;
            timer.Tick += timer_Tick;
            timer.Start();
        }

        public void Cancel()
        {
            timer.Stop();
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            timer.Stop();
            callback();
        }
    }
}
