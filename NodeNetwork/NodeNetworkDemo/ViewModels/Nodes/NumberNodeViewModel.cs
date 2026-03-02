using System.Reactive.Linq;
using DynamicData;
using NodeNetwork.Toolkit.ValueNode;
using NodeNetwork.ViewModels;
using NodeNetwork.Views;
using ReactiveUI;

namespace NodeNetworkDemo.ViewModels.Nodes
{
    public class NumberNodeViewModel : NodeViewModel
    {
        static NumberNodeViewModel()
        {
            Splat.Locator.CurrentMutable.Register(() => new NodeView(), typeof(IViewFor<NumberNodeViewModel>));
        }

        public ValueNodeOutputViewModel<double?> Output { get; }

        private double _value = 0;
        public double Value
        {
            get => _value;
            set => this.RaiseAndSetIfChanged(ref _value, value);
        }

        public NumberNodeViewModel()
        {
            Name = "数字";

            Output = new ValueNodeOutputViewModel<double?>
            {
                Name = "输出",
                Value = this.WhenAnyValue(vm => vm.Value).Select(v => (double?)v)
            };
            Outputs.Add(Output);
        }
    }
}
