namespace DBGA.EventSystem
{
    /// <summary>
    /// Generic game events listener
    /// 
    /// HOW TO USE:
    /// 
    /// To receive an event implement IGameEventsListener in your class and register your class
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
    public interface IGameEventsListener
    {
        public void ReceiveEvent(GameEvent gameEvent);
    }
}