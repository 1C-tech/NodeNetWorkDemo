using DynamicData;
using NodeNetwork.Toolkit.ValueNode;
using NodeNetwork.ViewModels;
using NodeNetwork.Views;
using ReactiveUI;

namespace NodeNetworkDemo.ViewModels.Nodes
{
    public class OutputNodeViewModel : NodeViewModel
    {
        static OutputNodeViewModel()
        {
            Splat.Locator.CurrentMutable.Register(() => new NodeView(), typeof(IViewFor<OutputNodeViewModel>));
        }

        public ValueNodeInputViewModel<double?> Input { get; }

        public OutputNodeViewModel()
        {
            Name = "输出";

            Input = new ValueNodeInputViewModel<double?>
            {
                Name = "结果"
            };
            Inputs.Add(Input);
        }
    }
}
