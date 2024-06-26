﻿using EventAndStateBackEnd.Mvvm;
using System;
using VideoOS.Platform.EventsAndState;
using Event = VideoOS.Platform.EventsAndState.Event;

namespace EventAndStateBackEnd.EventViewer
{
    class EventViewModel : ViewModelBase
    {
        private readonly CachedRestApiClient _restApiClient;
        private readonly Event _event;

        public DateTime Timestamp => _event.Time;
        public string Source => LoadProperty(_restApiClient.LookupResourceNameAsync(_event.Source, "displayName"));
        public string EventType => LoadProperty(_restApiClient.LookupResourceNameAsync($"eventTypes/{_event.Type}", "displayName"));

        public EventViewModel(Event @event)
        {
            _restApiClient = App.DataModel.RestApiClient;
            _event = @event;
        }
    }
}