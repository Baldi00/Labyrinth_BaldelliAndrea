using CodiceApp.EventTracking.Plastic;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace DBGA.EventSystem
{
    /// <summary>
    /// Generic singleton game events manager
    /// </summary>
    public class GameEventsManager
    {
        private static GameEventsManager _instance = null;
        public static GameEventsManager Instance
        {
            get
            {
                _instance ??= new GameEventsManager();
                return _instance;
            }
        }

        private readonly Dictionary<string, List<Action<GameEvent>>> eventsCallbacks;

        private GameEventsManager()
        {
            eventsCallbacks = new Dictionary<string, List<Action<GameEvent>>>();
        }

        public void AddEventCallback(string eventName, Action<GameEvent> triggeredCallback)
        {
            if (!eventsCallbacks.ContainsKey(eventName))
                eventsCallbacks.Add(eventName, new List<Action<GameEvent>>());

            eventsCallbacks[eventName].Add(triggeredCallback);
        }

        public void RemoveEventCallback(string eventName)
        {
            if (eventsCallbacks.ContainsKey(eventName))
            {
                eventsCallbacks[eventName].Clear();
                eventsCallbacks.Remove(eventName);
            }
        }

        public void RemoveEventCallback(string eventName, Action<GameEvent> triggeredCallback)
        {
            if (eventsCallbacks.ContainsKey(eventName))
                eventsCallbacks[eventName].Remove(triggeredCallback);
        }

        public void RemoveEventCallbacks(Action<GameEvent> triggeredCallback)
        {
            foreach (string eventName in eventsCallbacks.Keys)
                RemoveEventCallback(eventName, triggeredCallback);
        }

        public void RemoveEventCallbacks()
        {
            foreach (string eventName in eventsCallbacks.Keys)
                RemoveEventCallback(eventName);
        }

        public void DispatchGameEvent(GameEvent gameEvent)
        {
            string eventName = gameEvent.Name;
            if (eventsCallbacks.ContainsKey(eventName))
                eventsCallbacks[eventName].ForEach(callback => callback?.Invoke(gameEvent));
            else
                Debug.LogWarning("There are no listeners for " + eventName);
        }
    }
}