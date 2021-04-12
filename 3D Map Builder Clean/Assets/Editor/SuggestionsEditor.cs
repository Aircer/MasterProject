using System.Collections.Generic;
using MapTileGridCreator.Core;
using UnityEditor;
using UnityEngine;
using MapTileGridCreator.Utilities;
using System.Diagnostics;

[CanEditMultipleObjects]
public class SuggestionsEditor : EditorWindow
{
    private List<Editor> suggestionsEditors = new List<Editor>();
    private List<WaypointCluster> suggestionsClusters = new List<WaypointCluster>();

    private MapTileGridCreatorWindow mapWindow;
    private WaypointCluster mapCluster;
    private Cell[,,] mapCells;
    private Grid3D mapSuggestionGrid;
    private Cell[,,] mapSuggestionCells;

    private int numberSuggestions;
    private Vector2 scrollPos;
    private Texture2D bgTexture;

    [MenuItem("3D Map/SuggestionsEditor")]
    static void ShowWindow()
    {
        EditorWindow window = GetWindow(typeof(SuggestionsEditor));
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

        bgTexture = Resources.Load("Sprite/bg") as Texture2D;
    }

    private void OnGUI()
    {
        GUIStyle bg = new GUIStyle();
        //Initialize RectOffset object
        bg.border = new RectOffset(2, 2, 2, 2);
        bg.normal.background = bgTexture;

        GUILayout.BeginHorizontal();

        if (GUILayout.Button("New"))
        {
            NewSuggestions();
        }

        //Get number of suggestions
        numberSuggestions = EditorGUILayout.IntField("Number Suggestions: ", numberSuggestions);
        GUILayout.EndHorizontal();

        scrollPos =
            EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(position.height - 10));
        int i = 0;
        while (i < suggestionsEditors.Count)
        {
            GUILayout.BeginHorizontal();
            if (suggestionsEditors != null)
            {
                for (int j = i; j < i + 2 && j < suggestionsEditors.Count; j++)
                {
                    suggestionsEditors[j].OnInteractivePreviewGUI(GUILayoutUtility.GetRect(0.5f*position.width, 200), bg);
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (suggestionsEditors != null)
            {
                for (int j = i; j < i + 2 && j < suggestionsEditors.Count; j++)
                {
                    if (GUILayout.Button("Swap"))
                    {
                        DestroyImmediate(suggestionsEditors[j].target);
                        suggestionsEditors[j] = Editor.CreateEditor(CreateSuggestionObject(mapCluster));

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
        EditorGUILayout.EndScrollView();
        EditorUtility.ClearProgressBar();
    }

    public GameObject CreateSuggestionObject(WaypointCluster cluster)
    {
        Stopwatch stopWatch;
        stopWatch = new Stopwatch();
        stopWatch.Start();

        FuncEditor.TransformCellsFromWaypoints(mapSuggestionCells, cluster.GetWaypoints(), mapWindow.GetCellPrefabs());

        stopWatch.Stop();
        UnityEngine.Debug.Log("Time Taken By Transformation " + stopWatch.ElapsedMilliseconds + "ms");
        stopWatch.Reset();

        GameObject newObject = Instantiate(mapSuggestionGrid.gameObject);
        MeshCombiner combiner = newObject.gameObject.GetComponent<MeshCombiner>();
        combiner.CreateMultiMaterialMesh = true;
        combiner.CombineMeshes(true);
        Transform newObjectTransform = newObject.transform;

        while (newObjectTransform.childCount != 0)
        {
            DestroyImmediate(newObjectTransform.GetChild(0).gameObject);
        }

        //stopWatch.Stop();
        //UnityEngine.Debug.Log("Time Taken By MeshMerge " + stopWatch.ElapsedMilliseconds + "ms");
        //stopWatch.Reset();

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

    public void NewSuggestions()
    {
        while (suggestionsEditors.Count != 0)
        {
            DestroyImmediate(suggestionsEditors[0].target);
            DestroyImmediate(suggestionsEditors[0]);
            suggestionsEditors.RemoveAt(0);
        }

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
}
