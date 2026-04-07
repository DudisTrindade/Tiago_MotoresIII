using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;

// Script de Editor: gerencia carregamento da cena _Boot ao apertar Play
[InitializeOnLoad]
public static class BootSceneLoader
{
    private const string SessionKeyShouldUnload = "BootSceneLoader_ShouldUnloadBoot";
    private const string SessionKeyOriginalScene = "BootSceneLoader_OriginalScenePath";

    static BootSceneLoader()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        try
        {
            switch (state)
            {
                case PlayModeStateChange.ExitingEditMode:
                    HandleExitingEditMode();
                    break;
                case PlayModeStateChange.EnteredPlayMode:
                    HandleEnteredPlayMode();
                    break;
                case PlayModeStateChange.EnteredEditMode:
                case PlayModeStateChange.ExitingPlayMode:
                    // Garantir limpeza em qualquer transição que não deva manter a flag
                    ClearSessionState();
                    break;
            }

        }
        catch (System.Exception ex)
        {
            Debug.LogError($"BootSceneLoader: exceção durante mudança de PlayMode - {ex}");
            ClearSessionState();
            // Em caso de erro durante ExitingEditMode, cancelar o Play
            if (state == PlayModeStateChange.ExitingEditMode)
                EditorApplication.isPlaying = false;
        }
    }

    private static void HandleExitingEditMode()
    {
        // Pergunta ao usuário para salvar cenas modificadas
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            // Usuário cancelou o diálogo de salvar -> cancelar Play
            EditorApplication.isPlaying = false;
            ClearSessionState();
            return;
        }

        var activeScene = SceneManager.GetActiveScene();
        var originalPath = activeScene.path;

        if (string.IsNullOrEmpty(originalPath))
        {
            // Cena não tem path (não salva) -> pedir para salvar explicitamente
            EditorUtility.DisplayDialog("BootSceneLoader", "A cena ativa não foi salva. Salve a cena antes de executar Play.", "OK");
            EditorApplication.isPlaying = false;
            ClearSessionState();
            return;
        }

        // Localiza a cena _Boot no projeto
        var bootPath = FindBootScenePath();
        if (string.IsNullOrEmpty(bootPath))
        {
            EditorUtility.DisplayDialog("BootSceneLoader", "Cena '_Boot' não encontrada no projeto. Cancelando Play.", "OK");
            EditorApplication.isPlaying = false;
            ClearSessionState();
            return;
        }

        // Abre _Boot em Single e reabre a cena original em Additive
        var openedBoot = EditorSceneManager.OpenScene(bootPath, OpenSceneMode.Single);

        if (!openedBoot.IsValid())
        {
            Debug.LogError($"BootSceneLoader: falha ao abrir a cena _Boot em '{bootPath}'");
            EditorApplication.isPlaying = false;
            ClearSessionState();
            return;
        }

        var reopened = EditorSceneManager.OpenScene(originalPath, OpenSceneMode.Additive);
        if (!reopened.IsValid())
        {
            Debug.LogError($"BootSceneLoader: falha ao reabrir a cena original em '{originalPath}'");
            EditorApplication.isPlaying = false;
            ClearSessionState();
            return;
        }

        // Marca que devemos descarregar _Boot ao entrar em Play
        SessionState.SetInt(SessionKeyShouldUnload, 1);
        SessionState.SetString(SessionKeyOriginalScene, originalPath);
    }

    private static void HandleEnteredPlayMode()
    {
        if (SessionState.GetInt(SessionKeyShouldUnload, 0) != 1)
            return;

        // Tenta obter a cena _Boot e descarregá-la
        var bootScene = SceneManager.GetSceneByName("_Boot");
        if (bootScene.IsValid())
        {
            // Descarrega de forma assíncrona
            var unloadOp = SceneManager.UnloadSceneAsync(bootScene);
            if (unloadOp == null)
                Debug.LogWarning("BootSceneLoader: UnloadSceneAsync retornou null ao tentar descarregar _Boot.");
        }

        ClearSessionState();
    }

    private static string FindBootScenePath()
    {
        // Procurar por todos os assets do tipo Scene e comparar o nome do arquivo com _Boot
        var guids = AssetDatabase.FindAssets("t:Scene");
        foreach (var g in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(g);
            if (string.Equals(Path.GetFileNameWithoutExtension(path), "_Boot", System.StringComparison.OrdinalIgnoreCase))
                return path;
        }
        return null;
    }

    private static void ClearSessionState()
    {
        if (SessionState.GetInt(SessionKeyShouldUnload, 0) != 0)
            SessionState.EraseInt(SessionKeyShouldUnload);
        if (!string.IsNullOrEmpty(SessionState.GetString(SessionKeyOriginalScene, string.Empty)))
            SessionState.EraseString(SessionKeyOriginalScene);
    }
}
