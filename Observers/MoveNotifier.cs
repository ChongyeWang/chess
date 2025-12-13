using System.Collections.Generic;

public class MoveNotifier
{
    private List<IMoveObserver> _observers = new List<IMoveObserver>();

    public void Attach(IMoveObserver observer)
    {
        if (!_observers.Contains(observer))
        {
            _observers.Add(observer);
        }
    }

    public void Detach(IMoveObserver observer)
    {
        _observers.Remove(observer);
    }

    public void Notify(MoveRecord move)
    {
        foreach (var observer in _observers)
        {
            observer.OnMoveMade(move);
        }
    }
}

