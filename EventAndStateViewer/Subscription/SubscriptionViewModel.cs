using EventAndStateViewer.Mvvm;
using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using VideoOS.Platform.EventsAndState;

namespace EventAndStateViewer.Subscription
{
    /// <summary>
    /// View model for SubscriptionControl.xaml
    /// </summary>
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
            Console.WriteLine("SubscriptionViewModel constructor called");

            _session = App.DataModel.Session;

            Subscribe = new DelegateCommand(OnSubscribeAsync);
            AddRule = new DelegateCommand(OnAddRule);

            // Load saved rules or add a default one
            LoadRules();

        }

        private async Task OnSubscribeAsync()
        {
            Console.WriteLine("OnSubscribeAsync method called");
            // Unsubscribe, if needed
            if (_subscriptionId != Guid.Empty)
            {
                await _session.RemoveSubscriptionAsync(_subscriptionId, default);
            }

            // Subscribe
            var rules = Rules.Select(r => r.ToRule());
            _subscriptionId = await _session.AddSubscriptionAsync(rules, default);
            IsDirty = false;

            // Save rules
            SaveRules();

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
                // Add a new rule to prevent 0 rules
                AddRule.Execute(null);
            }
            IsDirty = true;
        }

        private void OnRuleChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            IsDirty = true;
        }


        private void LoadRules()
        {
            Console.WriteLine("LoadRules method called");
            var savedRules = Properties.Settings.Default.SavedRules;
            if (!string.IsNullOrEmpty(savedRules))
            {
                try
                {
                    // Deserialize the saved rules and add them to the collection
                    var deserializedRules = JsonConvert.DeserializeObject<ObservableCollection<SubscriptionRuleViewModel>>(savedRules);

                    // Clear existing rules and add the loaded rules
                    Rules.Clear();
                    foreach (var rule in deserializedRules)
                    {
                        rule.Removed += OnRuleRemoved;
                        rule.PropertyChanged += OnRuleChanged;
                        Rules.Add(rule);
                    }
                }
                catch (Exception ex)
                {
                    // Handle the exception (e.g., log or display an error)
                    Console.WriteLine($"Error deserializing rules: {ex.Message}");
                }
            }
            else
            {
                // Add a default rule if no saved rules are found
                AddRule.Execute(null);
            }
        }

        public string SavedRules
        {
            get => Properties.Settings.Default.SavedRules;
            set
            {
                Properties.Settings.Default.SavedRules = value;
                Properties.Settings.Default.Save();
            }
        }

        private void SaveRules()
        {
            // Save rules to settings
            var serializedRules = SerializeRules(); // Serialize Rules collection to a string
            Properties.Settings.Default.SavedRules = serializedRules;
            Properties.Settings.Default.Save();

            Console.WriteLine($"SavedRules after save: {Properties.Settings.Default.SavedRules}");
        }

        private string SerializeRules()
        {
            // Serialize Rules collection to a JSON string
            try
            {
                var serializedRules = JsonConvert.SerializeObject(Rules);
                return serializedRules;
            }
            catch (Exception ex)
            {
                // Handle the exception (e.g., log or display an error)
                Console.WriteLine($"Error serializing rules: {ex.Message}");
                return string.Empty;
            }
        }
    }
}