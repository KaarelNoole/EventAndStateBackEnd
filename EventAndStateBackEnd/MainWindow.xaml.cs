using System.Windows;

namespace EventAndStateBackEnd
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            if (App.DataModel != null)
                DataContext = new MainViewModel();
        }

        private void TabControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel mainViewModel)
            {
                TabControl.SelectedItem = mainViewModel.SubscriptionViewModel;

                mainViewModel.SubscriptionViewModel.Subscribed += (s, _) => TabControl.SelectedItem = mainViewModel.StateViewerViewModel;
            }
        }
    }
}