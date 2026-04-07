using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class BootSceneLoader : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void LoadBootScene()
    {
        string initialScene = SceneManager.GetActiveScene().name;
        
        SceneManager.LoadScene("_Boot", LoadSceneMode.Additive);
        
        SceneManager.sceneLoaded += OnBootSceneLoaded;
        
        void OnBootSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == "_Boot")
            {
                SceneManager.sceneLoaded -= OnBootSceneLoaded;
                MonoBehaviour tempBehaviour = new GameObject("_BootLoader").AddComponent<BootSequence>();
                tempBehaviour.GetComponent<BootSequence>().StartBootSequence(initialScene);
            }
        }
    }
}

public class BootSequence : MonoBehaviour
{
    public void StartBootSequence(string initialScene)
    {
        StartCoroutine(BootSequenceRoutine(initialScene));
    }

    private IEnumerator BootSequenceRoutine(string initialScene)
    {
        yield return new WaitForEndOfFrame();
        
        SceneManager.LoadScene(initialScene, LoadSceneMode.Additive);
        
        yield return new WaitForEndOfFrame();
        
        SceneManager.UnloadSceneAsync("_Boot");
        
        Destroy(gameObject);
    }
}