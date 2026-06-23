using UnityEngine;

public class PlayerCoins : MonoBehaviour
{
    private int coins = 0;

    private void Start()
    {
        // Atualiza a UI com o valor inicial
        PlayerObserverManager.NotifyCoinsChanged(coins);
    }

    public void CollectCoin()
    {
        coins++;

        Debug.Log("Moeda coletada! Total: " + coins);

        PlayerObserverManager.NotifyCoinsChanged(coins);
    }
}