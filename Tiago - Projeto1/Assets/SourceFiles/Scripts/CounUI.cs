using TMPro;
using UnityEngine;

public class CoinUI : MonoBehaviour
{
    [SerializeField] private TMP_Text textoMoedas;

    private void OnEnable()
    {
        PlayerObserverManager.OnCoinsChanged += AtualizarMoedas;
    }

    private void OnDisable()
    {
        PlayerObserverManager.OnCoinsChanged -= AtualizarMoedas;
    }

    private void AtualizarMoedas(int quantidade)
    {
        textoMoedas.text = "Moedas: " + quantidade;
    }
}