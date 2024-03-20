using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VideoOS.Platform;
using VideoOS.Platform.JsonHandling;

namespace EventAndStateBackEnd.Subscription
{

    class SubscriptionItemProvider
    {
        private readonly CachedRestApiClient _restApiClient;

        public SubscriptionItemProvider()
        {
            _restApiClient = App.DataModel.RestApiClient;
        }


        public IEnumerable<Item> GetResourceTypes()
        {
            var recorderId = new Guid("80b7fb0a-1d71-4861-8dc9-12ac7031b00e");

            return new[]
            {
                new Item(new FQID(new ServerId() { ServerType = ServerId.CorporateManagementServerType }, Guid.Empty, Kind.Server, FolderType.No, Kind.Server) { ObjectIdString = "sites" }, "Management servers"),
                new Item(new FQID(new ServerId() { ServerType = ServerId.CorporateRecordingServerType }, Guid.Empty, recorderId, FolderType.No, Kind.Server) { ObjectIdString = "recordingServers" }, "Recording servers"),
                new Item(new FQID(new ServerId(), Guid.Empty, Kind.Hardware, FolderType.No, Kind.Hardware) { ObjectIdString = "hardware" }, "Hardware"),
                new Item(new FQID(new ServerId(), Guid.Empty, Kind.Camera, FolderType.No, Kind.Camera) { ObjectIdString = "cameras" }, "Cameras"),
                new Item(new FQID(new ServerId(), Guid.Empty, Kind.Microphone, FolderType.No, Kind.Microphone) { ObjectIdString = "microphones"}, "Microphones"),
                new Item(new FQID(new ServerId(), Guid.Empty, Kind.Speaker, FolderType.No, Kind.Speaker) { ObjectIdString = "speakers" }, "Speakers"),
                new Item(new FQID(new ServerId(), Guid.Empty, Kind.Output, FolderType.No, Kind.Output) { ObjectIdString = "outputs" }, "Outputs"),
                new Item(new FQID(new ServerId(), Guid.Empty, Kind.InputEvent, FolderType.No, Kind.InputEvent) { ObjectIdString = "inputEvents" }, "Input events"),
                new Item(new FQID(new ServerId(), Guid.Empty, Kind.TriggerEvent, FolderType.No, Kind.TriggerEvent) { ObjectIdString = "userDefinedEvents" }, "User-defined events"),
            };
        }


        public async Task<IEnumerable<Item>> GetSourcesAsync()
        {
            return await Task.Run(() => Configuration.Instance.GetItems());
        }


        public async Task<IEnumerable<Item>> GetEventTypesAsync()
        {
            var tasks = await Task.WhenAll(
                _restApiClient.LookupResourceAsync("eventTypes/"),
                _restApiClient.LookupResourceAsync("stateGroups/"),
                _restApiClient.LookupResourceAsync("eventTypeGroups/"));

            var eventTypes = tasks[0].GetChild("array").GetChildren().Select(x => ToItem(x));
            var stateGroups = tasks[1].GetChild("array").GetChildren().Select(x => ToItem(x)).ToDictionary(x => x.FQID.ObjectId);
            var eventTypeGroups = tasks[2].GetChild("array").GetChildren().Select(x => ToItem(x)).ToDictionary(x => x.FQID.ObjectId);

            foreach (var eventType in eventTypes)
            {
                var eventTypeGroup = eventTypeGroups[eventType.FQID.ParentId];
                if (Guid.TryParse(eventType.Properties["stateGroupId"], out var stateGroupId))
                {
                    var stateGroup = stateGroups[stateGroupId];
                    stateGroup.AddChild(eventType);
                    if (!eventTypeGroup.GetChildren().Contains(stateGroup))
                    {
                        eventTypeGroup.AddChild(stateGroup);
                    }
                }
                else
                {
                    eventTypeGroup.AddChild(eventType);
                }
            }
            return eventTypeGroups.Values.Where(x => x.GetChildren().Any());
        }
        private ConfigItem ToItem(JsonObject jsonObject)
        {
            Guid.TryParse(jsonObject.GetChild("relations")?.GetChild("parent")?.GetString("id"), out var parentId);
            Guid.TryParse(jsonObject.GetChild("relations")?.GetChild("self")?.GetString("id"), out var objectId);
            var folderType = parentId == Guid.Empty ? FolderType.SystemDefined : FolderType.No;
            var name = jsonObject.GetString("displayName");
            var item = new ConfigItem(new FQID(new ServerId(), parentId, objectId, folderType, Kind.TriggerEvent), name);
            if (parentId != Guid.Empty)
            {
                item.Properties.Add("stateGroupId", jsonObject.GetString("stateGroupId"));
            }
            return item;
        }


    }
}