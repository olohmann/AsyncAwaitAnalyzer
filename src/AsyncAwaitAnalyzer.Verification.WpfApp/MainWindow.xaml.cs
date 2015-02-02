using AsyncAwaitAnalyzer.Verification.ClassLib;
using System.Windows;

namespace AsyncAwaitAnalyzer.WpfApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            var foo = new Foo();
            
            // OK, no .ConfigureAwait required.
            await foo.BarAsync();
        }

        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var foo = new Foo();

            // OK, depends on the usage.
            await foo.BarAsync().ConfigureAwait(false);
        }
    }
}
