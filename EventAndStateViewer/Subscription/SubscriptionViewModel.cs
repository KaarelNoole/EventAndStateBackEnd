using EventAndStateBackEnd.Mvvm;
using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using VideoOS.Platform.EventsAndState;

namespace EventAndStateBackEnd.Subscription
{
    class SubscriptionViewModel : ViewModelBase
    {
        private readonly IEventsAndStateSession _session;
        private Guid _subscriptionId;
        private bool _isDirty;

        public string TabName => "Subscription";

        public event EventHandler Subscribed;

        public bool IsDirty
        {
            get => _isDirty;
            set => SetProperty(ref _isDirty, value);
        }

        public ICommand Subscribe { get; }
        public ICommand AddRule { get; }

        public ObservableCollection<SubscriptionRuleViewModel> Rules { get; } = new ObservableCollection<SubscriptionRuleViewModel>();

        public SubscriptionViewModel()
        {
            _session = App.DataModel.Session;

            Subscribe = new DelegateCommand(OnSubscribeAsync);
            AddRule = new DelegateCommand(OnAddRule);

            AddRule.Execute(null);

            IsDirty = false;
        }

        private async Task OnSubscribeAsync()
        {
            if (_subscriptionId != Guid.Empty)
            {
                await _session.RemoveSubscriptionAsync(_subscriptionId, default);
            }

            var rules = Rules.Select(r => r.ToRule());
            _subscriptionId = await _session.AddSubscriptionAsync(rules, default);
            IsDirty = false;

            Subscribed?.Invoke(this, EventArgs.Empty);
        }

        private void OnAddRule()
        {
            var rule = new SubscriptionRuleViewModel();
            rule.Removed += OnRuleRemoved;
            rule.PropertyChanged += OnRuleChanged;
            Rules.Add(rule);
            IsDirty = true;
        }

        private void OnRuleRemoved(object sender, EventArgs e)
        {
            if (sender is SubscriptionRuleViewModel rule)
            {
                rule.Removed -= OnRuleRemoved;
                rule.PropertyChanged -= OnRuleChanged;
                Rules.Remove(rule);
            }
            if (Rules.Count == 0)
            {
                AddRule.Execute(null);
            }
            IsDirty = true;
        }

        private void OnRuleChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            IsDirty = true;
        }
    }
}