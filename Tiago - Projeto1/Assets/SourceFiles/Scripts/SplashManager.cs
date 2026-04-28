using UnityEngine;
using UnityEngine.SceneManagement;

public class SplashManager : MonoBehaviour
{
    void Start()
    {
        Invoke("CarregarMenu", 2f); // espera 2 segundos
    }

    void CarregarMenu()
    {
        SceneManager.LoadScene("_MenuPrincipal");
    }
}