using System.Reactive.Linq;
using System.Windows;
using NodeNetworkDemo.ViewModels;
using ReactiveUI;

namespace NodeNetworkDemo.Views
{
    public partial class MainWindow : Window, IViewFor<MainViewModel>
    {
        public MainViewModel ViewModel
        {
            get => (MainViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register(nameof(ViewModel), typeof(MainViewModel), typeof(MainWindow));

        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (MainViewModel)value;
        }

        public MainWindow()
        {
            InitializeComponent();

            ViewModel = new MainViewModel();

            this.OneWayBind(ViewModel, vm => vm.ListViewModel, v => v.nodeList.ViewModel);
            this.OneWayBind(ViewModel, vm => vm.NetworkViewModel, v => v.viewHost.ViewModel);
            this.OneWayBind(ViewModel, vm => vm.ResultLabel, v => v.resultText.Text);
        }
    }
}
