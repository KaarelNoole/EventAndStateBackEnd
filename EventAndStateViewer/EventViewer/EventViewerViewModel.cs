using EventAndStateViewer.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Windows.Input;
using VideoOS.Platform.EventsAndState;

namespace EventAndStateViewer.EventViewer
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

        private void OnEventsReceived(object sender, IEnumerable<Event> events)
        {
            
            string connectionString = "Data Source=10.100.80.67;Initial Catalog=minubaas;User ID=minunimi;Password=test;";

            
            string insertQuery = "INSERT INTO Camera (EventTime, Source, Event) VALUES (@EventTime, @Source, @Event)";

            foreach (var @event in events)
            {
                Events.Add(new EventViewModel(@event));
                DateTime eventTime = @event.Time;
                string source = @event.Source;
                string eventText = @event.Type.ToString(); 
        
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    
                    using (SqlCommand command = new SqlCommand(insertQuery, connection))
                    {
                        
                        command.Parameters.AddWithValue("@EventTime", eventTime);
                        command.Parameters.AddWithValue("@Source", source);
                        command.Parameters.AddWithValue("@Event", eventText);

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

        private void OnClearEvents()
        {
            Events.Clear();
        }
    }
}