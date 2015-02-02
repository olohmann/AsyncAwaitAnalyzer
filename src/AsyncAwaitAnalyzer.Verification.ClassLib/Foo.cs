using System;
using System.Threading.Tasks;

namespace AsyncAwaitAnalyzer.Verification.ClassLib
{
    public class Foo
    {
        public async Task BarAsync()
        {
            // Bad
            await Task.Delay(TimeSpan.FromSeconds(1)); 
            await Task.Delay(TimeSpan.FromSeconds(1)).ConfigureAwait(true);

            // Good
            await Task.Delay(TimeSpan.FromSeconds(1)).ConfigureAwait(false); 
        }
    }
}
