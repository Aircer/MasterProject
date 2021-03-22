using System.Collections.Generic;
using MapTileGridCreator.Core;
using UnityEditor;
using UnityEngine;
using MapTileGridCreator.Utilities;

public class GridsSuggestionsCreatorWindow : EditorWindow
{
    List<Editor> gameObjectsEditors = new List<Editor>();
    List<Grid3D> suggestionsGrids = new List<Grid3D>();
    List<WaypointCluster> suggestionsClusters = new List<WaypointCluster>();

    public int numberSuggestions { get; set; }

    [MenuItem("3D Map/GridsSuggestions")]
    static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(GridsSuggestionsCreatorWindow));
    }

    private void OnEnable()
    {
        foreach (Editor gameObjectEditor in gameObjectsEditors)
        {
            DestroyImmediate(gameObjectEditor);
        }

        gameObjectsEditors.Clear();
        suggestionsGrids.Clear();
    }

    private void OnGUI()
    {
        GUIStyle bgColor = new GUIStyle();
        bgColor.normal.background = EditorGUIUtility.whiteTexture;
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("New"))
        {
            while (gameObjectsEditors.Count != 0)
            {
                DestroyImmediate(gameObjectsEditors[0].target);
                DestroyImmediate(gameObjectsEditors[0]);
                gameObjectsEditors.RemoveAt(0);
            }

            suggestionsClusters.Clear();
            MapTileGridCreatorWindow[] editor = (MapTileGridCreatorWindow[])Resources.FindObjectsOfTypeAll(typeof(MapTileGridCreatorWindow));
            suggestionsClusters = IA.GetSuggestionsClusters(editor[0]._cluster, numberSuggestions);

            gameObjectsEditors.Clear();

            foreach(WaypointCluster newcluster in suggestionsClusters)
            {
                FuncEditor.TransformCellsFromCluster(editor[0]._suggestionCell, newcluster);
                GameObject newObject = Instantiate(editor[0]._suggestionsGrid.gameObject);
                MeshCombiner combiner = newObject.gameObject.GetComponent<MeshCombiner>();
                combiner.CreateMultiMaterialMesh = true;
                combiner.CombineMeshes(true);
                Transform newObjectTransform = newObject.transform;

                
                while (newObjectTransform.childCount != 0)
                {
                    DestroyImmediate(newObjectTransform.GetChild(0).gameObject);
                }

                gameObjectsEditors.Add(Editor.CreateEditor(newObject));
            }
        }

        numberSuggestions = EditorGUILayout.IntField("Number Suggestions: ", numberSuggestions);

        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        if (gameObjectsEditors != null)
        {
            for (int i = 0; i < gameObjectsEditors.Count; i++)
            {
                gameObjectsEditors[i].OnInteractivePreviewGUI(GUILayoutUtility.GetRect(100, 100), bgColor);
            }
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if (gameObjectsEditors != null)
        {
            for (int i = 0; i < gameObjectsEditors.Count; i++)
            {
                if(GUILayout.Button(" ^^ Swap ^^ "))
                {

                }
            }
        }
        GUILayout.EndHorizontal();
    }
}
