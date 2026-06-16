using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadScene : MonoBehaviour
{
    
    public string sceneName;
   
    public void Load()
    {
        GameManager.Instance.LoadScene(sceneName);
    }

    // Função para o botão "Sair"
    public void QuitGame()
    {
        Debug.Log("saiu do jogo");

        // Fecha o jogo (só funciona no build)
        Application.Quit();
    }
}