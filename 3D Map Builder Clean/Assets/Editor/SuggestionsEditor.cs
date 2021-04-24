using System.Collections.Generic;
using MapTileGridCreator.Core;
using UnityEditor;
using UnityEngine;
using MapTileGridCreator.Utilities;
using System.Diagnostics;

[CanEditMultipleObjects]
public class SuggestionsEditor : EditorWindow
{
    private MapTileGridCreatorWindow mapWindow;
    private WaypointCluster mapCluster;
    private List<WaypointCluster> suggestionsClusters = new List<WaypointCluster>();
    private Cell[,,] mapCells;
    private List<Cell[,,]> mapSuggestionCells;
    private List<Grid3D> mapSuggestionGrid;
    private Vector2 scrollPos;

    public int numberSuggestions;
    public EvolutionaryAlgoParams evolAlgoParams;

    [MenuItem("3D Map/SuggestionsEditor")]
    static void ShowWindow()
    {
        EditorWindow window = GetWindow(typeof(SuggestionsEditor));
        window.autoRepaintOnSceneChange = true;
        window.Show();
    }

    private void OnEnable()
    {
        suggestionsClusters.Clear();

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
        EditorWindow window = GetWindow(typeof(SuggestionsEditor));
        window.minSize = new Vector2(0, 0);
        window.maxSize = new Vector2(500, 400);

        //Get number of suggestions
        //GUILayout.BeginHorizontal();
        //numberSuggestions = EditorGUILayout.IntField("Number Suggestions: ", numberSuggestions);
        //GUILayout.EndHorizontal();
        numberSuggestions = 4;

        //Get parameters of Evolutionary the Algorithm
        GUILayout.BeginHorizontal();
        evolAlgoParams.mutationRate = EditorGUILayout.FloatField("Mutation Rate ", evolAlgoParams.mutationRate);
        evolAlgoParams.population = EditorGUILayout.IntField("Population ", evolAlgoParams.population);
        evolAlgoParams.population = evolAlgoParams.population > numberSuggestions ? evolAlgoParams.population:numberSuggestions;
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        evolAlgoParams.generations = EditorGUILayout.IntField("Generations ", evolAlgoParams.generations);
        evolAlgoParams.generations = evolAlgoParams.generations > 0 ? evolAlgoParams.generations : 1;
        evolAlgoParams.elitism = EditorGUILayout.IntField("Elitism ", evolAlgoParams.elitism);
        evolAlgoParams.elitism = evolAlgoParams.generations > 0 ? evolAlgoParams.elitism : 1;
        GUILayout.EndHorizontal();

        if (mapSuggestionGrid == null || mapSuggestionGrid.Count == 0 || mapSuggestionGrid[0] == null)
            NewSuggestionsClusters();

        //scrollPos =
            //EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(position.height - 10));
        int i = 0;
        while (i < numberSuggestions && mapSuggestionGrid != null)
        {
            GUILayout.BeginHorizontal();
            if (mapSuggestionGrid.Count == numberSuggestions)
            {
                /*for (int j = i; j < i + 2 && j < numberSuggestions; j++)
                {
                    //Handles.DrawCamera(GUILayoutUtility.GetRect(50, 200), mapSuggestionGrid[j].transform.GetComponentInChildren<Camera>());
                    //Handles.DrawCamera(GUILayoutUtility.GetRect(0.5f * position.width, 2 * position.height / numberSuggestions-40), mapSuggestionGrid[j].transform.GetComponentInChildren<Camera>());
                }*/

                //GUILayoutUtility.GetRect(0.5f * position.width, 0.5f * position.height);
                //GUI.DrawTexture(new Rect(0.5f * position.width, i * position.height / numberSuggestions, 0.5f*position.width, 0.5f * position.height), renderTexture);

                
                GUILayoutUtility.GetRect(0.5f*position.width, position.height*0.5f - 50);
                //GUI.BeginGroup(new Rect(5, 5, 0.5f * position.width, position.height));
                Camera previewCam = mapSuggestionGrid[i].transform.GetComponentInChildren<Camera>();
                previewCam.hideFlags = HideFlags.HideAndDontSave;
                Rect cameraRect = new Rect(5, 50 + i*(position.height * 0.25f-15f), 0.5f * position.width-10, position.height * 0.5f - 60);
                if (previewCam)
                {
                    Handles.DrawCamera(cameraRect, previewCam, DrawCameraMode.Normal);
                }

                GUILayoutUtility.GetRect(0.5f * position.width, position.height * 0.5f - 50);
                previewCam = mapSuggestionGrid[i+1].transform.GetComponentInChildren<Camera>();
                previewCam.hideFlags = HideFlags.HideAndDontSave;
                cameraRect = new Rect(0.5f*position.width+5, 50 + i *(position.height * 0.25f-15f), 0.5f * position.width-10, position.height * 0.5f - 60);
                if (previewCam)
                {
                    Handles.DrawCamera(cameraRect, previewCam, DrawCameraMode.Normal);
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

                        FuncEditor.TransformCellsFromWaypoints(mapSuggestionCells[j], suggestionsClusters[j].GetWaypoints());
                        FuncEditor.TransformCellsFromWaypoints(mapCells, mapCluster.GetWaypoints());

                    }
                }
            }
            GUILayout.EndHorizontal();
            i += 2;
        }
        //EditorGUILayout.EndScrollView();
        EditorUtility.ClearProgressBar();
    }

    public void SwapCluster(ref WaypointCluster cluster1, ref WaypointCluster cluster2)
    {
        WaypointCluster swapCluster = cluster1;
        cluster1 = cluster2;
        cluster2 = swapCluster;
    }

    public void NewSuggestionsClusters()
    {
        if (suggestionsClusters != null)
            suggestionsClusters.Clear();

        mapCluster = mapWindow.GetCluster();
        mapCells = mapWindow.GetCells();
        mapSuggestionGrid = mapWindow.GetSuggestionGrid();
        mapSuggestionCells = mapWindow.GetSuggestionCells();
    }

    public void NewSuggestionsIA()
    {
        //Create new clusters from the current sketch 
        suggestionsClusters = IA.GetSuggestionsClusters(mapCluster, numberSuggestions, evolAlgoParams);
    }

    public void NewSuggestionsCells()
    {
        //Create GameObject from the newly created clusters and create editors 
        for (int i = 0; i < numberSuggestions; i++)
            FuncEditor.TransformCellsFromWaypoints(mapSuggestionCells[i], suggestionsClusters[i].GetWaypoints());
    }
}
