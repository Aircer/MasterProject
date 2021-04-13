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
    private List<GameObject> suggestionsEditorsTargets = new List<GameObject>();
    private List<WaypointCluster> suggestionsClusters = new List<WaypointCluster>();

    private MapTileGridCreatorWindow mapWindow;
    private WaypointCluster mapCluster;
    private Cell[,,] mapCells;
    private List<Grid3D> mapSuggestionGrid;
    private List<Cell[,,]> mapSuggestionCells;

    public int numberSuggestions;
    private Vector2 scrollPos;
    private Texture2D bgTexture;

    [MenuItem("3D Map/SuggestionsEditor")]
    static void ShowWindow()
    {
        EditorWindow window = GetWindow(typeof(SuggestionsEditor));
        window.autoRepaintOnSceneChange = true;
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

        if (mapWindow == null)
            mapWindow = (MapTileGridCreatorWindow)Resources.FindObjectsOfTypeAll(typeof(MapTileGridCreatorWindow))[0];
    }
    public void Update()
    {
        // This is necessary to make the framerate normal for the editor window.
        Repaint();
    }

    private void OnGUI()
    {
        GUIStyle bg = new GUIStyle();
        //Initialize RectOffset object
        bg.border = new RectOffset(2, 2, 2, 2);
        bg.normal.background = bgTexture;

        GUILayout.BeginHorizontal();

        //Get number of suggestions
        numberSuggestions = EditorGUILayout.IntField("Number Suggestions: ", numberSuggestions);

        GUILayout.EndHorizontal();

        //scrollPos =
            //EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(position.height - 10));
        int i = 0;
        while (i < numberSuggestions)
        {
            GUILayout.BeginHorizontal();
            if (mapSuggestionGrid.Count == numberSuggestions)
            {
                for (int j = i; j < i + 2 && j < numberSuggestions; j++)
                {
                    //suggestionsEditors[j].OnInteractivePreviewGUI(GUILayoutUtility.GetRect(0.5f*position.width, 200), bg);
                    //GUI.DrawTexture(new Rect(0.0f, 0.0f, 0.5f * position.width, 200), renderTexture);

                    //Handles.DrawCamera(GUILayoutUtility.GetRect(50, 200), mapSuggestionGrid[j].transform.GetComponentInChildren<Camera>());
                    Handles.DrawCamera(GUILayoutUtility.GetRect(0.5f * position.width, 2 * position.height / numberSuggestions-40), mapSuggestionGrid[j].transform.GetComponentInChildren<Camera>());
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (mapSuggestionGrid.Count == numberSuggestions)
            {
                for (int j = i; j < i + 2 && j < numberSuggestions; j++)
                {
                    if (GUILayout.Button("Swap"))
                    {
                        //Swap Map cluster and suggestion cluster
                        mapWindow.SetCluster(suggestionsClusters[j]);
                        WaypointCluster newCluster = suggestionsClusters[j];
                        SwapCluster(ref mapCluster, ref newCluster);
                        suggestionsClusters[j] = newCluster;

                        FuncEditor.TransformCellsFromWaypoints(mapSuggestionCells[j], suggestionsClusters[j].GetWaypoints(), mapWindow.GetCellPrefabs());
                        FuncEditor.TransformCellsFromWaypoints(mapCells, mapCluster.GetWaypoints(), mapWindow.GetCellPrefabs());

                    }
                }
            }
            GUILayout.EndHorizontal();
            i += 2;
        }
        //EditorGUILayout.EndScrollView();
        EditorUtility.ClearProgressBar();
    }

    /*public GameObject CreateSuggestionObject()
    {
        FuncEditor.TransformCellsFromWaypoints(mapCells, mapCluster.GetWaypoints(), mapWindow.GetCellPrefabs());

        Stopwatch stopWatch;
        stopWatch = new Stopwatch();
        stopWatch.Start();

        GameObject newObject = Instantiate(mapSuggestionGrid.gameObject);

        stopWatch.Stop();
        UnityEngine.Debug.Log("Time Taken By Instantiate " + stopWatch.ElapsedMilliseconds + "ms");
        stopWatch.Reset();

        MeshCombiner combiner = newObject.gameObject.GetComponent<MeshCombiner>();
        combiner.CreateMultiMaterialMesh = true;
        combiner.CombineMeshes(true);
        Transform newObjectTransform = newObject.transform;

        
        while (newObjectTransform.childCount != 0)
        {
            DestroyImmediate(newObjectTransform.GetChild(0).gameObject);
        }

        return newObject;
    }*/

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

    public void NewSuggestionsClusters()
    {
        /*while (suggestionsEditors.Count != 0)
        {
            DestroyImmediate(suggestionsEditors[0].target);
            DestroyImmediate(suggestionsEditors[0]);
            suggestionsEditors.RemoveAt(0);
        }*/

        if (suggestionsClusters != null)
            suggestionsClusters.Clear();

        mapCluster = mapWindow.GetCluster();
        mapCells = mapWindow.GetCells();
        mapSuggestionGrid = mapWindow.GetSuggestionGrid();
        mapSuggestionCells = mapWindow.GetSuggestionCells();
    }

    public void NewSuggestionsIA()
    {
        Stopwatch stopWatch;
        stopWatch = new Stopwatch();
        stopWatch.Start();

        //Create new clusters from the current sketch 
        suggestionsClusters = IA.GetSuggestionsClusters(mapCluster, numberSuggestions);

        stopWatch.Stop();
        UnityEngine.Debug.Log("Time Taken By IA " + stopWatch.ElapsedMilliseconds + "ms");
        stopWatch.Reset();
    }

    public void NewSuggestionsEditors()
    {
        //Create GameObject from the newly created clusters and create editors 
        /*suggestionsEditors.Clear();
        foreach (WaypointCluster newcluster in suggestionsClusters)
        {
            suggestionsEditors.Add(Editor.CreateEditor(CreateSuggestionObject(newcluster)));
        }*/

        if (suggestionsEditors == null)
            suggestionsEditors = new List<Editor>();


        /*while (suggestionsEditors.Count != 0)
        {
            //DestroyImmediate(suggestionsEditors[0].target);
            DestroyImmediate(suggestionsEditors[0]);
            suggestionsEditors.RemoveAt(0);
        }

        suggestionsEditors.Clear(); */

        for (int i = 0; i < numberSuggestions; i++)
        {
            //DestroyImmediate(suggestionsEditors[i].target);
            //suggestionsEditorsTargets[i] = CreateSuggestionObject();

            FuncEditor.TransformCellsFromWaypoints(mapSuggestionCells[i], suggestionsClusters[i].GetWaypoints(), mapWindow.GetCellPrefabs());

            //stopWatch.Stop();
            //UnityEngine.Debug.Log("Time Taken By Transform " + stopWatch.ElapsedMilliseconds + "ms");
            //stopWatch.Reset();

            //GameObject newTarget = Instantiate(mapSuggestionGrid.gameObject);

            //stopWatch.Stop();
            //UnityEngine.Debug.Log("Time Taken By Instantiate " + stopWatch.ElapsedMilliseconds + "ms");
            //stopWatch.Reset();
            /*
            MeshCombiner combiner = mapSuggestionGrid.GetComponent<MeshCombiner>();
            combiner.CreateMultiMaterialMesh = true;
            combiner.CombineMeshes(true);
            Transform newTargetTransform = mapSuggestionGrid.transform;

            while(newTargetTransform.childCount != 0)
            {
                DestroyImmediate(newTargetTransform.GetChild(0).gameObject);
            }*/

            //suggestionMesh = combiner;

            //Editor oldEditor = suggestionsEditors[i];
            //Editor.CreateCachedEditor(mapSuggestionGrid[i].gameObject, null, ref oldEditor);
            //suggestionsEditors[i] = oldEditor;
            //UnityEngine.Debug.Log(suggestionsEditors[i].target);
            //suggestionsEditors[i] = oldEditor;
            //suggestionsEditors[i].Repaint();
        }
    }

    public void SetSuggestionsEditors()
    {
        while (suggestionsEditors.Count != 0)
        {
            DestroyImmediate(suggestionsEditors[0]);
            suggestionsEditors.RemoveAt(0);
        }

        suggestionsEditors.Clear();

        for (int i = 0; i < numberSuggestions; i++)
        {
            suggestionsEditors.Add(Editor.CreateEditor(mapSuggestionGrid[i].gameObject));
        }
    }
}
