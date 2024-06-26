﻿using EventAndStateBackEnd.Mvvm;
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
                DateTime eventTimeUtc = @event.Time;
                DateTime eventTimeLocal = TimeZoneInfo.ConvertTimeFromUtc(eventTimeUtc, TimeZoneInfo.Local);
                var eventViewModel = new EventViewModel(@event);


                string cameraName = eventViewModel.Source;
                var eventId = @event.Id;
                string source = cameraName;
                string eventName = eventViewModel.EventType;
                string eventText = eventName;

                string cameraId = @event.Source;

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand(insertQuery, connection))
                    {
                        command.Parameters.AddWithValue("@EventTime", eventTimeLocal);
                        command.Parameters.AddWithValue("@Source", source);
                        command.Parameters.AddWithValue("@Event", eventText);
                        command.Parameters.AddWithValue("@CameraID", cameraId);

                        try
                        {
                            int rowsAffected = command.ExecuteNonQuery();

                            if (rowsAffected > 0)
                            {
                                Console.WriteLine("Alarmi sisestamine õnnestus.");
                            }
                            else
                            {
                                Console.WriteLine("Alarmi pole sisestatud.");
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Veateade: " + ex.Message);
                        }
                    }
                }
            }
        }

        private async Task DelayToDatabaseAsync()
        {
            await Task.Delay(1500);

        }

        private void OnClearEvents()
        {
            Events.Clear();
        }
    }
}