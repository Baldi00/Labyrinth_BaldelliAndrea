using System;
using System.Collections.Generic;
using UnityEngine;

namespace DBGA.EventSystem
{
    public class GameEventsManager
    {
        private static GameEventsManager _instance = null;
        public static GameEventsManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new GameEventsManager();

                return _instance;
            }
        }

        private readonly Dictionary<Type, List<IGameEventsListener>> gameEventsListeners;

        private GameEventsManager()
        {
            gameEventsListeners = new Dictionary<Type, List<IGameEventsListener>>();
        }

        /// <summary>
        /// Register a listener for the given event
        /// </summary>
        /// <param name="listener">The listener that will be called when the listened event is raised</param>
        /// <param name="listenedEventType">The event that triggers the listener</param>
        public void AddListener(IGameEventsListener listener, Type listenedEventType)
        {
            if (!gameEventsListeners.ContainsKey(listenedEventType))
                gameEventsListeners.Add(listenedEventType, new List<IGameEventsListener>());

            gameEventsListeners[listenedEventType].Add(listener);
        }

        /// <summary>
        /// Removes a listener from the given event listeners list
        /// </summary>
        /// <param name="listener">The listener to remove</param>
        /// <param name="listenedEventType">The type of the event that is no more listened from the listener</param>
        public void RemoveListener(IGameEventsListener listener, Type listenedEventType)
        {
            if (gameEventsListeners.ContainsKey(listenedEventType))
                gameEventsListeners[listenedEventType].Remove(listener);
        }

        /// <summary>
        /// Removes a listener from all the listened event types
        /// </summary>
        /// <param name="listener">The listener to remove</param>
        public void RemoveListener(IGameEventsListener listener)
        {
            foreach (Type listenedEventType in gameEventsListeners.Keys)
                RemoveListener(listener, listenedEventType);
        }

        /// <summary>
        /// Removes all the listeners for a type of event
        /// </summary>
        /// <param name="listenedEventType">The type of the event that will have no more listeners</param>
        public void RemoveAllListeners(Type listenedEventType)
        {
            if (gameEventsListeners.ContainsKey(listenedEventType))
                gameEventsListeners[listenedEventType].Clear();
        }

        /// <summary>
        /// Removes all the listeners for all types of events
        /// </summary>
        public void RemoveAllListeners()
        {
            foreach (Type listenedEventType in gameEventsListeners.Keys)
                RemoveAllListeners(listenedEventType);
        }

        /// <summary>
        /// Receives an event and dispatches it to the correct listeners
        /// </summary>
        /// <param name="gameEvent">The received event</param>
        public void DispatchEvent(GameEvent gameEvent)
        {
            Type eventType = gameEvent.GetType();
            if (gameEventsListeners.ContainsKey(eventType))
                gameEventsListeners[eventType].ForEach(listener => listener.ReceiveEvent(gameEvent));
            else
                Debug.LogWarning("There are no listeners for " + eventType);
        }
    }
}