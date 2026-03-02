using System.Windows;
using NodeNetwork;

namespace NodeNetworkDemo
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            NNViewRegistrar.RegisterSplat();
        }
    }
}
