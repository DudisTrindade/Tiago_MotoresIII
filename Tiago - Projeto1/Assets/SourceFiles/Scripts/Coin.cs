using UnityEngine;

public class Coin : MonoBehaviour
{
    [SerializeField] private float velocidadeRotacao = 180f;

    private void Update()
    {
        transform.Rotate(0, velocidadeRotacao * Time.deltaTime, 0);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        PlayerCoins playerCoins =
            other.GetComponentInParent<PlayerCoins>();

        if (playerCoins == null)
        {
            Debug.Log("PlayerCoins não encontrado!");
            return;
        }

        playerCoins.CollectCoin();

        Destroy(gameObject);
    }
}