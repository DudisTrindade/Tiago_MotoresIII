using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadScene : MonoBehaviour
{
    public string sceneName;
   
    public void Load()
    {
        SceneManager.LoadScene(sceneName);
    }
}