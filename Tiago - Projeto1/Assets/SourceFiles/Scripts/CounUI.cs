using TMPro;
using UnityEngine;

public class CoinUI : MonoBehaviour
{
    [Header("Configuração do Jogador")]
    [SerializeField] private int playerID = 1;

    [Header("Texto")]
    [SerializeField] private TMP_Text textoMoedas;

    private void OnEnable()
    {
        PlayerObserverManager.OnCoinsChanged += AtualizarMoedas;
    }

    private void OnDisable()
    {
        PlayerObserverManager.OnCoinsChanged -= AtualizarMoedas;
    }

    private void AtualizarMoedas(int jogadorQueMudou, int quantidade)
    {
        // Só atualiza o texto do jogador correto
        if (jogadorQueMudou != playerID)
            return;

        textoMoedas.text = "Jogador " + playerID + ": " + quantidade;
    }
}