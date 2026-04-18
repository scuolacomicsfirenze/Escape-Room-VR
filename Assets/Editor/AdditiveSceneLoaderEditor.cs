using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Custom Editor per AdditiveSceneLoader.
/// Espone la lista delle scene additive con un dropdown che mostra solo
/// le scene registrate nelle Build Settings.
/// </summary>
[CustomEditor(typeof(AdditiveSceneLoader))]
public class AdditiveSceneLoaderEditor : Editor
{
    private SerializedProperty _additiveScenesProp;
    private SerializedProperty _loadSequentiallyProp;

    // Cache delle scene disponibili in Build Settings
    private List<SceneInfo> _buildScenes = new List<SceneInfo>();
    private string[] _sceneDisplayNames;
    private int[] _sceneBuildIndexes;

    private struct SceneInfo
    {
        public string Name;
        public string Path;
        public int BuildIndex;
    }

    private void OnEnable()
    {
        _additiveScenesProp = serializedObject.FindProperty("additiveScenes");
        _loadSequentiallyProp = serializedObject.FindProperty("loadSequentially");
        RefreshBuildScenes();
    }

    /// <summary>
    /// Aggiorna la cache con le scene attualmente presenti nelle Build Settings (abilitate o no).
    /// </summary>
    private void RefreshBuildScenes()
    {
        _buildScenes.Clear();

        var scenes = EditorBuildSettings.scenes;
        for (int i = 0; i < scenes.Length; i++)
        {
            var scene = scenes[i];
            if (string.IsNullOrEmpty(scene.path)) continue;

            string name = System.IO.Path.GetFileNameWithoutExtension(scene.path);
            _buildScenes.Add(new SceneInfo
            {
                Name = name,
                Path = scene.path,
                BuildIndex = i
            });
        }

        // Prepara gli array per EditorGUI.Popup
        _sceneDisplayNames = new string[_buildScenes.Count + 1];
        _sceneBuildIndexes = new int[_buildScenes.Count + 1];

        _sceneDisplayNames[0] = "-- Seleziona scena --";
        _sceneBuildIndexes[0] = -1;

        for (int i = 0; i < _buildScenes.Count; i++)
        {
            var info = _buildScenes[i];
            // Mostra se la scena è abilitata o disabilitata nelle Build Settings
            bool enabled = EditorBuildSettings.scenes[info.BuildIndex].enabled;
            string label = enabled
                ? $"{info.Name}  [{info.BuildIndex}]"
                : $"{info.Name}  [{info.BuildIndex}]  (disabilitata)";

            _sceneDisplayNames[i + 1] = label;
            _sceneBuildIndexes[i + 1] = info.BuildIndex;
        }
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawHeader();
        EditorGUILayout.Space(4);

        DrawLoadMode();
        EditorGUILayout.Space(8);

        DrawSceneList();
        EditorGUILayout.Space(4);

        DrawFooterButtons();

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawHeader()
    {
        EditorGUILayout.LabelField("Additive Scene Loader", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Carica le scene selezionate in modalità Additive all'avvio (Play Mode).\n" +
            "Solo le scene presenti nelle Build Settings sono selezionabili.",
            MessageType.Info);
    }

    private void DrawLoadMode()
    {
        EditorGUILayout.PropertyField(_loadSequentiallyProp,
            new GUIContent("Caricamento Sequenziale",
                "Se attivo, attende il completamento di ogni scena prima di caricare la successiva."));
    }

    private void DrawSceneList()
    {
        EditorGUILayout.LabelField($"Scene Additive  ({_additiveScenesProp.arraySize})", EditorStyles.boldLabel);

        if (_buildScenes.Count == 0)
        {
            EditorGUILayout.HelpBox(
                "Nessuna scena trovata nelle Build Settings.\n" +
                "Aggiungi scene tramite File > Build Settings.",
                MessageType.Warning);
            return;
        }

        for (int i = 0; i < _additiveScenesProp.arraySize; i++)
        {
            DrawSceneElement(i);
        }
    }

    private void DrawSceneElement(int index)
    {
        var elementProp = _additiveScenesProp.GetArrayElementAtIndex(index);
        var sceneNameProp = elementProp.FindPropertyRelative("SceneName");
        var buildIndexProp = elementProp.FindPropertyRelative("BuildIndex");

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.BeginHorizontal();

        // Label indice
        EditorGUILayout.LabelField($"Scena {index + 1}", GUILayout.Width(60));

        // Trova l'indice corrente nel popup
        int currentBuildIndex = buildIndexProp.intValue;
        int popupIndex = 0;
        for (int j = 1; j < _sceneBuildIndexes.Length; j++)
        {
            if (_sceneBuildIndexes[j] == currentBuildIndex)
            {
                popupIndex = j;
                break;
            }
        }

        // Evidenzia in rosso se la scena non è più valida
        bool isOrphan = currentBuildIndex >= 0 && popupIndex == 0;
        if (isOrphan)
            GUI.color = new Color(1f, 0.5f, 0.5f);

        int newPopupIndex = EditorGUILayout.Popup(popupIndex, _sceneDisplayNames);

        GUI.color = Color.white;

        // Applica la selezione
        if (newPopupIndex != popupIndex)
        {
            int selectedBuildIdx = _sceneBuildIndexes[newPopupIndex];
            buildIndexProp.intValue = selectedBuildIdx;

            if (selectedBuildIdx >= 0 && selectedBuildIdx < _buildScenes.Count)
            {
                // Ricerca per build index reale
                string newName = "";
                foreach (var info in _buildScenes)
                {
                    if (info.BuildIndex == selectedBuildIdx)
                    {
                        newName = info.Name;
                        break;
                    }
                }
                sceneNameProp.stringValue = newName;
            }
            else
            {
                sceneNameProp.stringValue = "";
            }
        }

        // Bottone rimozione
        if (GUILayout.Button("✕", GUILayout.Width(24), GUILayout.Height(18)))
        {
            _additiveScenesProp.DeleteArrayElementAtIndex(index);
            serializedObject.ApplyModifiedProperties();
            GUIUtility.ExitGUI();
        }

        EditorGUILayout.EndHorizontal();

        // Warning se la scena non è più in Build Settings
        if (isOrphan)
        {
            EditorGUILayout.HelpBox(
                $"La scena con Build Index {currentBuildIndex} non è più nelle Build Settings.",
                MessageType.Error);
        }

        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(2);
    }

    private void DrawFooterButtons()
    {
        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("+ Aggiungi Scena"))
        {
            _additiveScenesProp.arraySize++;
            var newEl = _additiveScenesProp.GetArrayElementAtIndex(_additiveScenesProp.arraySize - 1);
            newEl.FindPropertyRelative("SceneName").stringValue = "";
            newEl.FindPropertyRelative("BuildIndex").intValue = -1;
        }

        if (GUILayout.Button("↺ Aggiorna Lista", GUILayout.Width(130)))
        {
            RefreshBuildScenes();
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(4);

        // Link rapido alle Build Settings
        if (GUILayout.Button("Apri Build Settings"))
        {
            EditorWindow.GetWindow(System.Type.GetType("UnityEditor.BuildPlayerWindow,UnityEditor"));
        }
    }
}
