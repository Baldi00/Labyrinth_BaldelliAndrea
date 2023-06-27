using UnityEngine;

namespace DBGA.EventSystem
{
    /// <summary>
    /// Generic event that can be sent and received inside the game
    /// 
    /// HOW TO USE:
    /// 
    /// Extend this class to create your custom game event with public parameters
    /// e.g.
    /// public class MyCustomGameEvent : GameEvent 
    /// { 
    ///     public int customParam;
    /// }
    /// 
    /// To send the event call GameEventsManager.Instance.DispatchEvent(new MyCustomGameEvent(){ customParam = ... });
    /// (If the event is sent very frequently consider creating only one event and send the same event 
    /// with different parameters each time)
    /// 
    /// To receive the event implement IGameEventsListener in your class and register your class
    /// as a listener for that type of event.
    /// e.g.
    /// void Start()
    /// { 
    ///     GameEventsManager.Instance.AddListener(this, typeof(MyCustomGameEvent));
    /// }
    /// 
    /// Inside the implemented method ReceiveEvent you will receive the events you are listening to
    /// If your class is listening to multiple types of events remember to check which event you received
    /// e.g.
    /// public void ReceiveEvent(GameEvent gameEvent)
    /// {
    ///     if(gameEvent is MyCustomGameEvent myCustomGameEvent)
    ///     {
    ///         DoSomething(myCustomGameEvent);
    ///     }
    ///     else if ...
    /// }
    /// </summary>
    public class GameEvent
    {
        public GameObject callerGameObject;
    }
}
