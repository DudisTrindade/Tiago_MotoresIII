using System;

public static class PlayerObserverManager
{
    // Evento para avisar quando a quantidade de moedas mudar
    public static event Action<int> OnCoinsChanged;

    // Método responsável por disparar o evento
    public static void NotifyCoinsChanged(int quantidade)
    {
        OnCoinsChanged?.Invoke(quantidade);
    }
}