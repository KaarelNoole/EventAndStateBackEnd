using EventAndStateBackEnd.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using VideoOS.Platform.EventsAndState;

namespace EventAndStateBackEnd.EventViewer
{
    class EventViewerViewModel : ViewModelBase
    {
        public string TabName => "Event viewer";

        public ICommand Clear { get; }

        public ObservableCollection<EventViewModel> Events { get; } = new ObservableCollection<EventViewModel>();

        public EventViewerViewModel()
        {
            App.DataModel.EventReceiver.EventsReceived += OnEventsReceived;
            Clear = new DelegateCommand(OnClearEvents);
        }

        private async void OnEventsReceived(object sender, IEnumerable<Event> events)
        {
            
            string connectionString = "Data Source=10.100.80.67;Initial Catalog=minubaas;User ID=minunimi;Password=test;";

            
            string insertQuery = "INSERT INTO Alarm (EventTime, Source, Event, CameraID) VALUES (@EventTime, @Source, @Event, @CameraID)";

            foreach (var @event in events)
            {
                Events.Add(new EventViewModel(@event));
                await DelayToDatabaseAsync();
                DateTime eventTime = @event.Time;
                var eventViewModel = new EventViewModel(@event);

                string cameraName = eventViewModel.Source;
                
                string source = cameraName;
                string EventName = eventViewModel.EventType;
                string eventText = EventName;

                string CameraID = @event.Source;

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    
                    using (SqlCommand command = new SqlCommand(insertQuery, connection))
                    {
                        
                        command.Parameters.AddWithValue("@EventTime", eventTime);
                        command.Parameters.AddWithValue("@Source", source);
                        command.Parameters.AddWithValue("@Event", eventText);
                        command.Parameters.AddWithValue("@CameraID", CameraID);

                        try
                        {
                            
                            int rowsAffected = command.ExecuteNonQuery();

                            
                            if (rowsAffected > 0)
                            {
                                Console.WriteLine("Record inserted successfully.");
                            }
                            else
                            {
                                Console.WriteLine("No records inserted.");
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Error: " + ex.Message);
                        }
                    }
                }
            }
        }

        private async Task DelayToDatabaseAsync()
        {
            await Task.Delay(750);

        }

        private void OnClearEvents()
        {
            Events.Clear();
        }
    }
}