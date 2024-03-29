﻿using EventAndStateBackEnd.EventViewer;
using EventAndStateBackEnd.StateViewer;
using EventAndStateBackEnd.Subscription;
using System.Collections;

namespace EventAndStateBackEnd
{
    class MainViewModel
    {
        public ConnectionStateViewModel ConnectionStateViewModel { get; } = new ConnectionStateViewModel();
        public SubscriptionViewModel SubscriptionViewModel { get; } = new SubscriptionViewModel();
        public StateViewerViewModel StateViewerViewModel { get; } = new StateViewerViewModel();
        public EventViewerViewModel EventViewerViewModel { get; } = new EventViewerViewModel();

        public ICollection Tabs => new object[] { SubscriptionViewModel, StateViewerViewModel, EventViewerViewModel };

        public MainViewModel()
        {
            SubscriptionViewModel.Subscribed += (s, e) => StateViewerViewModel.Refresh.Execute(null);
            SubscriptionViewModel.Subscribed += (s, e) => EventViewerViewModel.Clear.Execute(null);
        }
    }
}