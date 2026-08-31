using System;

public static class PlayerObserverManager
{
 
    public static event Action<int, int> OnCoinsChanged;

    public static void NotifyCoinsChanged(int playerID, int quantidade)
    {
        OnCoinsChanged?.Invoke(playerID, quantidade);
    }
}