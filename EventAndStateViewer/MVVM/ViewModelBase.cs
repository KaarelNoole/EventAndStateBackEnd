using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace EventAndStateViewer.Mvvm
{

    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
        {
            field = value;
            InvokePropertyChanged(propertyName);
        }

        protected void InvokePropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected string LoadProperty(Task<string> lookupTask, [CallerMemberName] string propertyName = "")
        {
            if (lookupTask.IsFaulted)
                return "(Unknown)";
            if (lookupTask.IsCompleted)
                return lookupTask.Result;

            UpdateAfterCompletion(lookupTask, propertyName);

            return "Loading...";
        }

        private async void UpdateAfterCompletion(Task task, string propertyName)
        {
            try { await task; }
            catch { }
            InvokePropertyChanged(propertyName);
        }
    }
}