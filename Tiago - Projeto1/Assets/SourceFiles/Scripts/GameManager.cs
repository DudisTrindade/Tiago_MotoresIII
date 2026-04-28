using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
#endif

public class GameManager : MonoBehaviour
{
	// Singleton
	public static GameManager Instance { get; private set; }

	// Game states
	public enum GameState
	{
		Iniciando,
		MenuPrincipal,
		Gameplay
	}

	public GameState CurrentState { get; private set; }

	public event Action<GameState> OnStateChanged;

	// Prevent multiple boot sequences running
	private static bool bootSequenceStarted = false;

	void Awake()
	{
		if (Instance != null && Instance != this)
		{
			Destroy(gameObject);
			return;
		}

		Instance = this;
		DontDestroyOnLoad(gameObject);
	}

	void OnEnable()
	{
		SceneManager.sceneLoaded += OnSceneLoaded;
	}

	void OnDisable()
	{
		SceneManager.sceneLoaded -= OnSceneLoaded;
	}

	private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		// Map scene name to game state
		switch (scene.name)
		{
			case "_Boot":
				SetState(GameState.Iniciando);
				break;
			case "MenuPrincipal":
				SetState(GameState.MenuPrincipal);
				break;
			case "GetStarted_Scene":
				SetState(GameState.Gameplay);
				break;
			default:
				// don't change state for unknown scenes
				break;
		}

		// Try to allocate PlayerInput to the player in the newly loaded scene
		TryAttachPlayerInputToPlayerInScene(scene);
	}

	private void SetState(GameState newState)
	{
		if (CurrentState == newState) return;
		CurrentState = newState;
		Debug.Log($"[GameManager] Game state changed: {newState}");
		OnStateChanged?.Invoke(newState);
	}

	// Public scene management API - other scripts should call these instead of SceneManager directly
	public void LoadScene(string sceneName, LoadSceneMode mode = LoadSceneMode.Single)
	{
		StartCoroutine(LoadSceneCoroutine(sceneName, mode));
	}

	private IEnumerator LoadSceneCoroutine(string sceneName, LoadSceneMode mode)
	{
		var op = SceneManager.LoadSceneAsync(sceneName, mode);
		if (op == null)
		{
			Debug.LogError($"[GameManager] Failed to start loading scene '{sceneName}'");
			yield break;
		}

		while (!op.isDone) yield return null;
	}

	// Attach PlayerInput to a GameObject named or tagged "Player" when present in the scene
	private void TryAttachPlayerInputToPlayerInScene(Scene scene)
	{
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
		try
		{
			// Search by tag first, then by name
			GameObject player = null;
			foreach (var root in scene.GetRootGameObjects())
			{
				if (player == null && root.CompareTag("Player")) player = root;
			}

			if (player == null)
			{
				foreach (var root in scene.GetRootGameObjects())
				{
					if (player == null && string.Equals(root.name, "Player", StringComparison.OrdinalIgnoreCase)) player = root;
				}
			}

			if (player != null)
			{
				var pi = player.GetComponent<PlayerInput>();
				if (pi == null)
				{
					player.AddComponent<PlayerInput>();
					Debug.Log("[GameManager] PlayerInput added to Player GameObject.");
				}
				else
				{
					Debug.Log("[GameManager] Player already has PlayerInput.");
				}
			}
		}
		catch (Exception ex)
		{
			Debug.LogWarning($"[GameManager] Exception while trying to attach PlayerInput: {ex}");
		}
#else
		// Input System not available - do nothing
#endif
	}

	// --- Boot sequence: when play is pressed in the editor (or runtime started), ensure _Boot is loaded first,
	//     then the originally opened scene is loaded, then _Boot is unloaded. Implemented without using UnityEditor namespace.
	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
	private static void EnsureBootSequence()
	{
		if (bootSequenceStarted) return;

		Scene active = SceneManager.GetActiveScene();
		// If already in _Boot, nothing to do
		if (string.Equals(active.name, "_Boot", StringComparison.OrdinalIgnoreCase))
		{
			bootSequenceStarted = true;
			return;
		}

		// Start a helper GameObject to run the coroutine
		var go = new GameObject("__GameManager_BootLoader");
		DontDestroyOnLoad(go);
		var loader = go.AddComponent<BootLoader>();
		loader.StartCoroutine(loader.BootSequence(active.name));
		bootSequenceStarted = true;
	}

	// Helper MonoBehaviour to run coroutine from a static context
	private class BootLoader : MonoBehaviour
	{
		public IEnumerator BootSequence(string initialSceneName)
		{
			// Wait a frame to allow any immediate initialization on the original scene to run
			yield return null;

			// Load _Boot additively
			var loadBoot = SceneManager.LoadSceneAsync("_Boot", LoadSceneMode.Additive);
			if (loadBoot == null)
			{
				Debug.LogWarning("[GameManager] Could not find scene '_Boot' to load additively. Make sure it's in Build Settings.");
			}
			else
			{
				while (!loadBoot.isDone) yield return null;

				// Set active to _Boot
				var bootScene = SceneManager.GetSceneByName("_Boot");
				if (bootScene.IsValid() && bootScene.isLoaded)
				{
					SceneManager.SetActiveScene(bootScene);
				}
			}

			// Unload the originally opened scene (if loaded) so we can reload it cleanly after _Boot
			if (!string.IsNullOrEmpty(initialSceneName) && SceneManager.GetSceneByName(initialSceneName).isLoaded)
			{
				var unloadOp = SceneManager.UnloadSceneAsync(initialSceneName);
				if (unloadOp != null)
				{
					while (!unloadOp.isDone) yield return null;
				}
			}

			// Load the original scene additively (fresh)
			if (!string.IsNullOrEmpty(initialSceneName))
			{
				var loadInitial = SceneManager.LoadSceneAsync(initialSceneName, LoadSceneMode.Additive);
				if (loadInitial == null)
				{
					Debug.LogWarning($"[GameManager] Could not reload scene '{initialSceneName}'. Ensure it is added to Build Settings.");
				}
				else
				{
					while (!loadInitial.isDone) yield return null;

					var newInitialScene = SceneManager.GetSceneByName(initialSceneName);
					if (newInitialScene.IsValid() && newInitialScene.isLoaded)
					{
						SceneManager.SetActiveScene(newInitialScene);
					}
				}
			}

			// Finally unload _Boot
			if (SceneManager.GetSceneByName("_Boot").isLoaded)
			{
				var unloadBoot = SceneManager.UnloadSceneAsync("_Boot");
				if (unloadBoot != null)
				{
					while (!unloadBoot.isDone) yield return null;
				}
			}

			// Done - destroy loader
			Destroy(gameObject);
		}
	}
}
