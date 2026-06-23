using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public string sceneName;

    public enum GameState
    {
        Iniciando,
        MenuPrincipal,
        Gameplay
    }

    public GameState currentState;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        SetState(GameState.Iniciando);
        LoadScene("SplashScene");
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("Cena carregada: " + scene.name);

        if (scene.name == "SplashScene")
        {
            SetState(GameState.Iniciando);
        }
        else if (scene.name == "MenuPrincipal")
        {
            SetState(GameState.MenuPrincipal);
        }
        else if (scene.name == "GetStarted_Scene")
        {
            SetState(GameState.Gameplay);

            // Carrega a GUI junto da Gameplay
            if (!SceneManager.GetSceneByName("GUI").isLoaded)
            {
                SceneManager.LoadScene("GUI", LoadSceneMode.Additive);
            }
        }
    }

    public void SetState(GameState newState)
    {
        currentState = newState;
        Debug.Log("Estado atual: " + currentState);
    }

    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void SetupPlayerInput(PlayerInput playerInput)
    {
        Debug.Log("Input atribuído ao jogador: " + playerInput.name);
    }

    public void Load()
    {
        SceneManager.LoadScene(sceneName);
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}