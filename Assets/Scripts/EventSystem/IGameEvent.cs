namespace DBGA.EventSystem
{
    /// <summary>
    /// Generic event that can be sent and received inside the game
    /// 
    /// HOW TO USE:
    /// 
    /// Implement this interface to create your custom game event with public parameters
    /// e.g.
    /// public class MyCustomGameEvent : IGameEvent 
    /// { 
    ///     public int customParam;
    /// }
    /// 
    /// To send the event call GameEventsManager.Instance.DispatchGameEvent(new MyCustomGameEvent(){ customParam = ... });
    /// (If the event is sent very frequently consider caching the event and send the same event 
    /// with different parameters each time)
    /// 
    /// To receive the event implement IGameEventsListener in your class and register your class
    /// as a listener for that type of event.
    /// e.g.
    /// void Awake()
    /// { 
    ///     GameEventsManager.Instance.AddGameEventListener(this, typeof(MyCustomGameEvent));
    /// }
    /// 
    /// Inside the implemented method ReceiveGameEvent you will receive the events you are listening to
    /// If your class is listening to multiple types of events remember to check which event you received
    /// e.g.
    /// public void ReceiveGameEvent(GameEvent gameEvent)
    /// {
    ///     if(gameEvent is MyCustomGameEvent myCustomGameEvent)
    ///     {
    ///         DoSomething(myCustomGameEvent);
    ///     }
    ///     else if ...
    /// }
    /// </summary>
    public interface IGameEvent
    {

    }
}
