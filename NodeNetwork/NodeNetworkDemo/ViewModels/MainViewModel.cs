using System.Linq;
using System.Reactive.Linq;
using DynamicData;
using NodeNetwork;
using NodeNetwork.Toolkit;
using NodeNetwork.Toolkit.NodeList;
using NodeNetwork.ViewModels;
using NodeNetworkDemo.ViewModels.Nodes;
using ReactiveUI;

namespace NodeNetworkDemo.ViewModels
{
    public class MainViewModel : ReactiveObject
    {
        public NodeListViewModel ListViewModel { get; } = new NodeListViewModel();
        public NetworkViewModel NetworkViewModel { get; } = new NetworkViewModel();

        private string _resultLabel = "无";
        public string ResultLabel
        {
            get => _resultLabel;
            set => this.RaiseAndSetIfChanged(ref _resultLabel, value);
        }

        public MainViewModel()
        {
            ListViewModel.AddNodeType(() => new NumberNodeViewModel());
            ListViewModel.AddNodeType(() => new AddNodeViewModel());
            ListViewModel.AddNodeType(() => new MultiplyNodeViewModel());
            ListViewModel.AddNodeType(() => new OutputNodeViewModel());

            var outputNode = new OutputNodeViewModel();
            NetworkViewModel.Nodes.Add(outputNode);

            NetworkViewModel.Validator = network =>
            {
                bool containsLoops = GraphAlgorithms.FindLoops(network).Any();
                if (containsLoops)
                {
                    return new NetworkValidationResult(false, false, 
                        new ErrorMessageViewModel("网络包含循环!"));
                }
                return new NetworkValidationResult(true, true, null);
            };

            outputNode.Input.ValueChanged
                .Select(v => v?.ToString() ?? "无")
                .BindTo(this, vm => vm.ResultLabel);
        }
    }
}
