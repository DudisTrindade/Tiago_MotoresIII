using TMPro;
using UnityEngine;

public class CoinUI : MonoBehaviour
{
    public TMP_Text textoMoedas;

    private void OnEnable()
    {
        PlayerObserverManager.OnCoinsChanged += AtualizarMoedas;
    }

    private void OnDisable()
    {
        PlayerObserverManager.OnCoinsChanged -= AtualizarMoedas;
    }

    private void Start()
    {
        textoMoedas.text = "Moedas: 0";
    }

    private void AtualizarMoedas(int quantidade)
    {
        textoMoedas.text = "Moedas: " + quantidade;
    }
}