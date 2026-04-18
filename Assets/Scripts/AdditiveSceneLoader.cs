using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Carica scene aggiuntive in modalità Additive quando si entra in Play Mode.
/// Le scene selezionabili sono solo quelle presenti nella Build Settings list.
/// </summary>
public class AdditiveSceneLoader : MonoBehaviour
{
    [Tooltip("Lista delle scene da caricare in modalità Additive all'avvio.")]
    public List<SceneReference> additiveScenes = new List<SceneReference>();

    [Tooltip("Se true, aspetta che ogni scena sia caricata prima di passare alla successiva.")]
    public bool loadSequentially = false;

    private readonly List<AsyncOperation> _loadOperations = new List<AsyncOperation>();

    private void Start()
    {
        if (additiveScenes == null || additiveScenes.Count == 0)
        {
            Debug.LogWarning("[AdditiveSceneLoader] Nessuna scena configurata da caricare.");
            return;
        }

        if (loadSequentially)
            StartCoroutine(LoadScenesSequentially());
        else
            LoadScenesParallel();
    }

    private void LoadScenesParallel()
    {
        foreach (var sceneRef in additiveScenes)
        {
            if (!sceneRef.IsValid())
            {
                Debug.LogWarning($"[AdditiveSceneLoader] SceneReference non valida, skipping.");
                continue;
            }

            if (IsSceneAlreadyLoaded(sceneRef.SceneName))
            {
                Debug.Log($"[AdditiveSceneLoader] Scena '{sceneRef.SceneName}' già caricata, skipping.");
                continue;
            }

            Debug.Log($"[AdditiveSceneLoader] Caricamento parallelo: '{sceneRef.SceneName}'");
            var op = SceneManager.LoadSceneAsync(sceneRef.SceneName, LoadSceneMode.Additive);
            if (op != null)
                _loadOperations.Add(op);
        }
    }

    private IEnumerator LoadScenesSequentially()
    {
        foreach (var sceneRef in additiveScenes)
        {
            if (!sceneRef.IsValid())
            {
                Debug.LogWarning($"[AdditiveSceneLoader] SceneReference non valida, skipping.");
                continue;
            }

            if (IsSceneAlreadyLoaded(sceneRef.SceneName))
            {
                Debug.Log($"[AdditiveSceneLoader] Scena '{sceneRef.SceneName}' già caricata, skipping.");
                continue;
            }

            Debug.Log($"[AdditiveSceneLoader] Caricamento sequenziale: '{sceneRef.SceneName}'");
            var op = SceneManager.LoadSceneAsync(sceneRef.SceneName, LoadSceneMode.Additive);
            if (op != null)
                yield return op;
        }
    }

    private static bool IsSceneAlreadyLoaded(string sceneName)
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            if (SceneManager.GetSceneAt(i).name == sceneName)
                return true;
        }
        return false;
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }
}

/// <summary>
/// Wrapper serializzabile per un riferimento a una scena tramite nome e build index.
/// </summary>
[System.Serializable]
public class SceneReference
{
    [HideInInspector] public string SceneName;
    [HideInInspector] public int BuildIndex = -1;

    public bool IsValid() => BuildIndex >= 0 && !string.IsNullOrEmpty(SceneName);

    public override string ToString() => IsValid() ? $"{SceneName} (Build Index: {BuildIndex})" : "Non configurata";
}
