using System.Reactive.Linq;
using DynamicData;
using NodeNetwork.Toolkit.ValueNode;
using NodeNetwork.ViewModels;
using NodeNetwork.Views;
using ReactiveUI;

namespace NodeNetworkDemo.ViewModels.Nodes
{
    public class MultiplyNodeViewModel : NodeViewModel
    {
        static MultiplyNodeViewModel()
        {
            Splat.Locator.CurrentMutable.Register(() => new NodeView(), typeof(IViewFor<MultiplyNodeViewModel>));
        }

        public ValueNodeInputViewModel<double?> InputA { get; }
        public ValueNodeInputViewModel<double?> InputB { get; }
        public ValueNodeOutputViewModel<double?> Output { get; }

        public MultiplyNodeViewModel()
        {
            Name = "乘法";

            InputA = new ValueNodeInputViewModel<double?>
            {
                Name = "A"
            };
            Inputs.Add(InputA);

            InputB = new ValueNodeInputViewModel<double?>
            {
                Name = "B"
            };
            Inputs.Add(InputB);

            var result = this.WhenAnyValue(
                vm => vm.InputA.Value,
                vm => vm.InputB.Value)
                .Select(t =>
                {
                    if (t.Item1.HasValue && t.Item2.HasValue)
                        return t.Item1 * t.Item2;
                    return null;
                });

            Output = new ValueNodeOutputViewModel<double?>
            {
                Name = "A × B",
                Value = result
            };
            Outputs.Add(Output);
        }
    }
}
