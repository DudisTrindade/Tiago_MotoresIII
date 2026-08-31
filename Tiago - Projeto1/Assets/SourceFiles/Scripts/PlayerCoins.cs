using UnityEngine;

public class PlayerCoins : MonoBehaviour
{
    [Header("Identificação do Jogador")]
    [SerializeField] private int playerID = 1;

    private int coins = 0;

    public int PlayerID => playerID;
    public int Coins => coins;

    private void Start()
    {
       
        PlayerObserverManager.NotifyCoinsChanged(playerID, coins);
    }

    public void CollectCoin()
    {
        coins++;

        Debug.Log(
            "Jogador " + playerID +
            " coletou uma moeda! Total: " + coins
        );

      
        PlayerObserverManager.NotifyCoinsChanged(playerID, coins);
    }
}