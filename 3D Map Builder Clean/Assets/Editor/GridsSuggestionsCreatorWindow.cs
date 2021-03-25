using System.Collections.Generic;
using MapTileGridCreator.Core;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using MapTileGridCreator.Utilities;

public class GridsSuggestionsCreatorWindow : EditorWindow
{
    List<Editor> suggestionsEditors = new List<Editor>();
    Editor editor = new Editor();
    List<Grid3D> suggestionsGrids = new List<Grid3D>();
    List<WaypointCluster> suggestionsClusters = new List<WaypointCluster>();
    MapTileGridCreatorWindow[] mapWindow;
    float progressBarTime;
    public int numberSuggestions { get; set; }

    [MenuItem("3D Map/GridsSuggestions")]
    static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(GridsSuggestionsCreatorWindow));
    }

    private void OnEnable()
    {
        foreach (Editor gameObjectEditor in suggestionsEditors)
        {
            DestroyImmediate(gameObjectEditor);
        }

        suggestionsEditors.Clear();
        suggestionsGrids.Clear();
        mapWindow = (MapTileGridCreatorWindow[])Resources.FindObjectsOfTypeAll(typeof(MapTileGridCreatorWindow));
    }

    private void OnGUI()
    {
        GUIStyle bgColor = new GUIStyle();
        bgColor.normal.background = EditorGUIUtility.whiteTexture;
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("New"))
        {
            progressBarTime = 0.0f; 

            while (suggestionsEditors.Count != 0)
            {
                DestroyImmediate(suggestionsEditors[0].target);
                DestroyImmediate(suggestionsEditors[0]);
                suggestionsEditors.RemoveAt(0);
            }

            if (editor)
                DestroyImmediate(editor.target);

            if (suggestionsClusters != null)
                suggestionsClusters.Clear();
            mapWindow = (MapTileGridCreatorWindow[])Resources.FindObjectsOfTypeAll(typeof(MapTileGridCreatorWindow));
            WaypointCluster mapCluster = new WaypointCluster(mapWindow[0]._size_grid, mapWindow[0]._cells);
            suggestionsClusters = IA.GetSuggestionsClusters(mapCluster, numberSuggestions);

            suggestionsEditors.Clear();

            foreach (WaypointCluster newcluster in suggestionsClusters)
            {
                FuncEditor.TransformCellsFromCluster(mapWindow[0]._suggestionCell, newcluster);
                GameObject newObject = Instantiate(mapWindow[0]._suggestionsGrid.gameObject);
                MeshCombiner combiner = newObject.gameObject.GetComponent<MeshCombiner>();
                combiner.CreateMultiMaterialMesh = true;
                combiner.CombineMeshes(true);
                Transform newObjectTransform = newObject.transform;


                while (newObjectTransform.childCount != 0)
                {
                    DestroyImmediate(newObjectTransform.GetChild(0).gameObject);
                }

                suggestionsEditors.Add(Editor.CreateEditor(newObject));
            }
        }

        numberSuggestions = EditorGUILayout.IntField("Number Suggestions: ", numberSuggestions);

        GUILayout.EndHorizontal();
        int i = 0;
        while (i < suggestionsEditors.Count)
        {
            GUILayout.BeginHorizontal();
            if (suggestionsEditors != null)
            {
                for (int j = i; j < i + 2 && j < suggestionsEditors.Count; j++)
                {
                    suggestionsEditors[j].OnInteractivePreviewGUI(GUILayoutUtility.GetRect(100, 100), bgColor);
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (suggestionsEditors != null)
            {
                for (int j = i; j < i + 2 && j < suggestionsEditors.Count; j++)
                {
                    if (GUILayout.Button(" ^^ Swap ^^ "))
                    {
                        if (editor)
                            DestroyImmediate(editor.target);
                        mapWindow = (MapTileGridCreatorWindow[])Resources.FindObjectsOfTypeAll(typeof(MapTileGridCreatorWindow));
                        FuncEditor.TransformCellsFromCluster(mapWindow[0]._suggestionCell, mapWindow[0]._cluster);
                        GameObject newObject = Instantiate(mapWindow[0]._suggestionsGrid.gameObject);
                        MeshCombiner combiner = newObject.gameObject.GetComponent<MeshCombiner>();
                        combiner.CreateMultiMaterialMesh = true;
                        combiner.CombineMeshes(true);
                        Transform newObjectTransform = newObject.transform;

                        while (newObjectTransform.childCount != 0)
                        {
                            DestroyImmediate(newObjectTransform.GetChild(0).gameObject);
                        }

                        editor = Editor.CreateEditor(newObject);
                        Editor newEditor = editor;
                        WaypointCluster newCluster = new WaypointCluster(mapWindow[0]._size_grid, mapWindow[0]._cells);
                        mapWindow[0]._cluster = suggestionsClusters[j];
                        editor = suggestionsEditors[j];
                        suggestionsClusters[j] = newCluster;
                        suggestionsEditors[j] = newEditor;

                        FuncEditor.TransformCellsFromCluster(mapWindow[0]._cells, mapWindow[0]._cluster);
                    }
                }
            }
            GUILayout.EndHorizontal();
            i += 2;
        }

        EditorUtility.ClearProgressBar();
    }
}
