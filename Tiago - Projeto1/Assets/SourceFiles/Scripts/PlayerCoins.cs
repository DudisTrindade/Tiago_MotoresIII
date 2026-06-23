using UnityEngine;

public class PlayerCoins : MonoBehaviour
{
    private int coins = 0;

    public void CollectCoin()
    {
        coins++;

        Debug.Log("Moeda coletada! Total: " + coins);

        PlayerObserverManager.NotifyCoinsChanged(coins);
    }
}