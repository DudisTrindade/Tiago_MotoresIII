using System;

public static class PlayerObserverManager
{
    public static Action<int> OnCoinsChanged;

    public static void NotifyCoinsChanged(int amount)
    {
        OnCoinsChanged?.Invoke(amount);
    }
}