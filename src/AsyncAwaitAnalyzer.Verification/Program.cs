using System;
using System.Threading.Tasks;

namespace AsyncAwaitAnalyzer.Verification
{
    class Program
    {
        static void Main(string[] args)
        {
        }
    }

    public class BadClass
    {
        public async void FireAndForget()
        {
            await Task.Delay(TimeSpan.FromSeconds(10));
        }

        public async Task FireAndNoForgetWithoutSuffix() 
        {
            await Task.Delay(TimeSpan.FromSeconds(10));
        }

        public Task FireAndNoForgetWithoutSuffixAndKeyword()
        {
            return Task.Delay(TimeSpan.FromSeconds(10));
        }

        public void FireAndForgetLambda()
        {
            // Bad: Not returning a Task.
            Action fireAndForgetAction = async () => await Task.Delay(TimeSpan.FromMinutes(1));
            Action<int> onMoreFireAndForgetAction = async v => await Task.Delay(TimeSpan.FromMinutes(v));

            Action anotherFireAndForgetAction = async () =>
            {
                await Task.Delay(TimeSpan.FromDays(1));
                await Task.Delay(TimeSpan.FromDays(1));
            };
        }
    }

    public class GoodClass
    {
        public GoodClass()
        {
            var eventProvider = new EventProvider();
            eventProvider.UpdateTriggered += EventSubscription;

            var wpfBtn = new System.Windows.Controls.Button();
            var formsBtn = new System.Windows.Forms.Button();

            wpfBtn.Click += WpfBtn_Click;
            formsBtn.Click += FormsBtn_Click;

            // Good: Return Func of Task
            Func<Task> fireAndNotForgetAction = async () => await Task.Delay(TimeSpan.FromMinutes(1));
            Func<int, Task> fireAndNotForgetAction2 = async v => await Task.Delay(TimeSpan.FromMinutes(v));

            wpfBtn.Click += async (s, e) => await Task.Delay(TimeSpan.FromHours(1));
        }

        private async void FormsBtn_Click(object sender, EventArgs e)
        {
            await Task.Delay(TimeSpan.FromSeconds(10));
        }

        private async void WpfBtn_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            await Task.Delay(TimeSpan.FromSeconds(10));
        }

        public async Task FireAndNotForgetAsync()
        {
            await Task.Delay(TimeSpan.FromSeconds(10));
        }

        public async void EventSubscription(object sender, EventArgs args)
        {
            await Task.Delay(TimeSpan.FromSeconds(10));
        }
    }

    public class EventProvider
    {
        public event EventHandler UpdateTriggered;

        public void DoIt()
        {
            var handler = UpdateTriggered;

            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }
    }
}
