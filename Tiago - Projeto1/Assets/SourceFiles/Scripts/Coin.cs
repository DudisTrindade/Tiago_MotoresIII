using UnityEngine;

public class Coin : MonoBehaviour
{
    public float velocidadeRotacao = 180f;

    void Update()
    {
        transform.Rotate(velocidadeRotacao * Time.deltaTime, 0, 0);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerCoins playerCoins = other.GetComponent<PlayerCoins>();

            if (playerCoins != null)
            {
                playerCoins.CollectCoin();
            }

            Destroy(gameObject);
        }
    }
}