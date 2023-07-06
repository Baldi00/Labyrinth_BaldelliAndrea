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

        private readonly Dictionary<Type, List<IGameEventsListener>> gameEventsListeners;

        private GameEventsManager()
        {
            gameEventsListeners = new Dictionary<Type, List<IGameEventsListener>>();
        }

        /// <summary>
        /// Register a listener for the given game event
        /// </summary>
        /// <param name="listener">The listener that will be called when the listened game event is raised</param>
        /// <param name="listenedEventType">The game event that triggers the listener</param>
        public void AddGameEventListener(IGameEventsListener listener, Type listenedEventType)
        {
            if (!gameEventsListeners.ContainsKey(listenedEventType))
                gameEventsListeners.Add(listenedEventType, new List<IGameEventsListener>());

            gameEventsListeners[listenedEventType].Add(listener);
        }

        /// <summary>
        /// Removes a listener from the given game event listeners list
        /// </summary>
        /// <param name="listener">The listener to remove</param>
        /// <param name="listenedEventType">The type of the event that is no more listened from the listener</param>
        public void RemoveGameEventListener(IGameEventsListener listener, Type listenedEventType)
        {
            if (gameEventsListeners.ContainsKey(listenedEventType))
                gameEventsListeners[listenedEventType].Remove(listener);
        }

        /// <summary>
        /// Removes a listener from all the listened game event types
        /// </summary>
        /// <param name="listener">The listener to remove</param>
        public void RemoveGameEventListener(IGameEventsListener listener)
        {
            foreach (Type listenedEventType in gameEventsListeners.Keys)
                RemoveGameEventListener(listener, listenedEventType);
        }

        /// <summary>
        /// Removes all the listeners for a type of game event
        /// </summary>
        /// <param name="listenedEventType">The type of the event that will have no more listeners</param>
        public void RemoveAllGameEventListeners(Type listenedEventType)
        {
            if (gameEventsListeners.ContainsKey(listenedEventType))
                gameEventsListeners[listenedEventType].Clear();
        }

        /// <summary>
        /// Removes all the listeners for all types of game events
        /// </summary>
        public void RemoveAllGameEventListeners()
        {
            foreach (Type listenedEventType in gameEventsListeners.Keys)
                RemoveAllGameEventListeners(listenedEventType);
        }

        /// <summary>
        /// Receives a game event and dispatches it to the correct listeners
        /// </summary>
        /// <param name="gameEvent">The received event</param>
        public void DispatchGameEvent(GameEvent gameEvent)
        {
            Type eventType = gameEvent.GetType();
            if (gameEventsListeners.ContainsKey(eventType))
                gameEventsListeners[eventType].ForEach(listener => listener.ReceiveGameEvent(gameEvent));
            else
                Debug.LogWarning("There are no listeners for " + eventType);
        }
    }
}