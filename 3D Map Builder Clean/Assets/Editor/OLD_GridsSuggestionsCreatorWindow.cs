using System.Collections.Generic;
using MapTileGridCreator.Core;
using UnityEditor;
using UnityEngine;
using MapTileGridCreator.Utilities;

[CanEditMultipleObjects]
public class GridsSuggestionsCreatorWindow : EditorWindow
{
    private List<Editor> suggestionsEditors = new List<Editor>();
    private List<WaypointCluster> suggestionsClusters = new List<WaypointCluster>();

    private MapTileGridCreatorWindow mapWindow;
    private Editor editor = new Editor();
    private WaypointCluster mapCluster;
    private Cell[,,] mapCells;
    private Grid3D mapSuggestionGrid;
    private Cell[,,] mapSuggestionCells;

    private int numberSuggestions;


    static void ShowWindow()
    {
        EditorWindow window = GetWindow(typeof(GridsSuggestionsCreatorWindow));
        window.Show();
    }

    private void OnEnable()
    {
        foreach (Editor gameObjectEditor in suggestionsEditors)
        {
            DestroyImmediate(gameObjectEditor);
        }

        suggestionsEditors.Clear();
        suggestionsClusters.Clear();
    }

    private void OnGUI()
    {
        GUIStyle bgColor = new GUIStyle();
        bgColor.normal.background = EditorGUIUtility.whiteTexture;
        GUILayout.BeginHorizontal();

        if (GUILayout.Button("New"))
        {
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

            if (mapWindow == null)
                mapWindow = (MapTileGridCreatorWindow)Resources.FindObjectsOfTypeAll(typeof(MapTileGridCreatorWindow))[0];

            mapCluster = mapWindow.GetCluster();
            mapCells = mapWindow.GetCells();
            mapSuggestionGrid = mapWindow.GetSuggestionGrid();
            mapSuggestionCells = mapWindow.GetSuggestionCells();

            //Create new clusters from the current sketch 
            suggestionsClusters = IA.GetSuggestionsClusters(mapCluster, numberSuggestions);

            //Create GameObject from the newly created clusters and create editors 
            suggestionsEditors.Clear();
            foreach (WaypointCluster newcluster in suggestionsClusters)
            {
                suggestionsEditors.Add(Editor.CreateEditor(CreateSuggestionObject(newcluster)));
            }
        }

        //Get number of suggestions
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
                    suggestionsEditors[j].OnInteractivePreviewGUI(GUILayoutUtility.GetRect(200, 200), bgColor);
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

                        //Create new editor from Map Cluster and swap it with current Suggestion Editor
                        editor = Editor.CreateEditor(CreateSuggestionObject(mapCluster));
                        Editor suggestionEditor = suggestionsEditors[j];
                        SwapEditor(ref editor, ref suggestionEditor);
                        suggestionsEditors[j] = suggestionEditor;

                        //Swap Map cluster and suggestion cluster
                        mapWindow.SetCluster(suggestionsClusters[j]);
                        WaypointCluster newCluster = suggestionsClusters[j];
                        SwapCluster(ref mapCluster, ref newCluster);
                        suggestionsClusters[j] = newCluster;

                        FuncEditor.TransformCellsFromWaypoints(mapCells, mapCluster.GetWaypoints(), mapWindow.GetCellPrefabs());
                    }
                }
            }
            GUILayout.EndHorizontal();
            i += 2;
        }

        EditorUtility.ClearProgressBar();
    }

    public GameObject CreateSuggestionObject(WaypointCluster cluster)
    {
        FuncEditor.TransformCellsFromWaypoints(mapSuggestionCells, cluster.GetWaypoints(), mapWindow.GetCellPrefabs());
        GameObject newObject = Instantiate(mapSuggestionGrid.gameObject);
        MeshCombiner combiner = newObject.gameObject.GetComponent<MeshCombiner>();
        combiner.CreateMultiMaterialMesh = true;
        combiner.CombineMeshes(true);
        Transform newObjectTransform = newObject.transform;

        while (newObjectTransform.childCount != 0)
        {
            DestroyImmediate(newObjectTransform.GetChild(0).gameObject);
        }

        return newObject;
    }

    public void SwapEditor(ref Editor editor1, ref Editor editor2)
    {
        Editor swapEditor = editor1;
        editor1 = editor2;
        editor2 = swapEditor;
    }

    public void SwapCluster(ref WaypointCluster cluster1, ref WaypointCluster cluster2)
    {
        WaypointCluster swapCluster = cluster1;
        cluster1 = cluster2;
        cluster2 = swapCluster;
    }
}
