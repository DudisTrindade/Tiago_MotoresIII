using UnityEngine;
using UnityEngine.SceneManagement;

public class BootManager : MonoBehaviour
{
    void Start()
    {
        Invoke("IrParaSplashScene", 2f); // espera 2 segundos
    }

    void IrParaSplashScene()
    {
        SceneManager.LoadScene("SplashScene");
    }
}