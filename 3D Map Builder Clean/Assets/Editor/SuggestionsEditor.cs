using System.Collections.Generic;
using MapTileGridCreator.Core;
using UnityEditor;
using UnityEngine;
using MapTileGridCreator.UtilitiesMain;
using System.Collections;
//using EditorCoroutines.Editor;
using UtilitiesGenetic;
using Genetics;

[CanEditMultipleObjects]
public class SuggestionsEditor : EditorWindow
{
    private MapTileGridCreatorWindow mapWindow;
    private WaypointCluster mapCluster;
    private List<int[][][]> suggestionsInt = new List<int[][][]>();
    private Cell[,,] mapCells;
    private List<Cell[,,]> mapSuggestionCells;
    private Grid3D mapGrid;
    private List<Grid3D> mapSuggestionGrid;
    private Vector2 scrollPos;
    private EditorWindow window;

    public int numberSuggestions;
    public EvolutionaryAlgoParams[] evolAlgoParams;

    public void OpenSuggestions()
    {
        EditorWindow window = GetWindow(typeof(SuggestionsEditor));
        window.autoRepaintOnSceneChange = true;
        window.Show();
    }

    private void OnEnable()
    {
        suggestionsInt.Clear();
        numberSuggestions = 4;

        if (mapWindow == null)
            mapWindow = (MapTileGridCreatorWindow)Resources.FindObjectsOfTypeAll(typeof(MapTileGridCreatorWindow))[0];

        window = GetWindow(typeof(SuggestionsEditor));

        int nbAlgos = 2;
        evolAlgoParams = new EvolutionaryAlgoParams[nbAlgos];

        for(int i = 0; i < nbAlgos; i++)
        {
            evolAlgoParams[i].crossoverType = CrossoverType.Copy;
            evolAlgoParams[i].population = 50;
            evolAlgoParams[i].elitism = 1;
            evolAlgoParams[i].generations = 10;
            evolAlgoParams[i].mutationRate = 0.005f;
            evolAlgoParams[i].fitnessStop = 0.99f;

            evolAlgoParams[i].wDifference = 0f;
            evolAlgoParams[i].wWalkingAreas = 1f;
            evolAlgoParams[i].wWallsCuboids = 1f;
            evolAlgoParams[i].wPathfinding = 1f;

            evolAlgoParams[i].nbBestFit = 1;
            evolAlgoParams[i].mutationType = MutationsType.Normal;
        }

        evolAlgoParams[0].nbBestFit = 1;
        evolAlgoParams[0].mutationType = MutationsType.NoCreateDeleteFloorAndWalls;

        evolAlgoParams[1].nbBestFit = 3;
        evolAlgoParams[1].mutationType = MutationsType.Normal;
    }
    
    private void OnGUI()
    {
        /*
        EditorGUIUtility.labelWidth = 100;
        //Get parameters of Evolutionary the Algorithm
        GUILayout.BeginHorizontal();
        evolAlgoParams.mutationRate = EditorGUILayout.FloatField("Mutation Rate ", evolAlgoParams.mutationRate, GUILayout.Width(0.5f * position.width));
        evolAlgoParams.population = EditorGUILayout.IntField("Population ", evolAlgoParams.population, GUILayout.Width(0.5f * position.width));
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        evolAlgoParams.generations = EditorGUILayout.IntField("Generations ", evolAlgoParams.generations, GUILayout.Width(0.5f * position.width));
        evolAlgoParams.elitism = EditorGUILayout.IntField("Elitism ", evolAlgoParams.elitism, GUILayout.Width(0.5f * position.width));
        evolAlgoParams.fitnessStop = EditorGUILayout.FloatField("FitStop ", evolAlgoParams.fitnessStop, GUILayout.Width(0.5f * position.width));
        GUILayout.EndHorizontal();

        EditorGUIUtility.labelWidth = 70;
        GUILayout.BeginHorizontal();
        evolAlgoParams.wDifference = EditorGUILayout.FloatField("wDiff ", evolAlgoParams.wDifference, GUILayout.Width(0.25f * position.width));
        evolAlgoParams.wWalkingAreas = EditorGUILayout.FloatField("wWA ", evolAlgoParams.wWalkingAreas, GUILayout.Width(0.25f * position.width));
        evolAlgoParams.wWallsCuboids = EditorGUILayout.FloatField("wWalls ", evolAlgoParams.wWallsCuboids, GUILayout.Width(0.25f * position.width));
        evolAlgoParams.wPathfinding = EditorGUILayout.FloatField("wPath ", evolAlgoParams.wPathfinding, GUILayout.Width(0.25f * position.width));
        GUILayout.EndHorizontal();*/

        if (mapSuggestionGrid == null || mapSuggestionGrid.Count == 0 || mapSuggestionGrid[0] == null)
            NewSuggestionsInt();

        //scrollPos =
            //EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(position.height - 10));
        int i = 0;
        while (i < numberSuggestions && mapSuggestionGrid != null)
        {
            GUILayout.BeginHorizontal();
            if (mapSuggestionGrid.Count == numberSuggestions)
            {
                GUILayoutUtility.GetRect(0.5f*position.width, position.height*0.45f - 20);
                Camera previewCam = mapSuggestionGrid[i].transform.GetComponentInChildren<Camera>();
                previewCam.hideFlags = HideFlags.HideAndDontSave;
                Rect cameraRect = new Rect(5, 5 + i*(position.height * 0.25f), 0.5f * position.width-10, position.height * 0.49f - 30);
                if (previewCam)
                {
                    Handles.DrawCamera(cameraRect, previewCam, DrawCameraMode.Normal);
                }

                GUILayoutUtility.GetRect(0.5f * position.width, position.height * 0.49f - 20);
                previewCam = mapSuggestionGrid[i+1].transform.GetComponentInChildren<Camera>();
                previewCam.hideFlags = HideFlags.HideAndDontSave;
                cameraRect = new Rect(0.5f*position.width+5, 5 + i *(position.height * 0.25f), 0.5f * position.width-10, position.height * 0.49f - 30);
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
                        int[][][] suggestionInt = mapSuggestionGrid[j].ConvertCellsToInt();
                        int[][][] mapInt = mapGrid.ConvertCellsToInt();
                        mapSuggestionGrid[j].ConvertIntToCells(mapInt);
                        mapGrid.ConvertIntToCells(suggestionInt);
                    }
                }
            }
            GUILayout.EndHorizontal();
            i += 2;
        }
        EditorUtility.ClearProgressBar();
    }

    public void SwapCluster(ref WaypointCluster cluster1, ref WaypointCluster cluster2)
    {
        WaypointCluster swapCluster = cluster1;
        cluster1 = cluster2;
        cluster2 = swapCluster;
    }

    public void NewSuggestionsInt()
    {
        if (suggestionsInt != null)
            suggestionsInt.Clear();

        mapGrid = mapWindow.GetGrid();
        mapSuggestionGrid = mapWindow.GetSuggestionGrid();
    }

    public void NewSuggestionsIA()
    {
        //Create new clusters from the current sketch 
        int[][][] genesInitialPopulation = mapWindow.GetGrid().ConvertCellsToInt();

        int sizeDNDA_X = mapGrid.size.x + 2; int sizeDNDA_Y = mapGrid.size.y + 2; int sizeDNDA_Z = mapGrid.size.z + 2;
        TypeParams[] typeParams = new TypeParams[mapWindow.GetCellInfos().Count];

        for (int i = 0; i < typeParams.Length; i++)
        {
            typeParams[i] = mapWindow.GetCellInfos()[i].typeParams;
        }

        Genetics.Init geneticInit = new Genetics.Init();

        suggestionsInt = geneticInit.GetSuggestionsClusters(new UtilitiesGenetic.Vector3Int(sizeDNDA_X, sizeDNDA_Y, sizeDNDA_Z), typeParams, genesInitialPopulation, numberSuggestions, evolAlgoParams);
    }

    public void NewSuggestionsCells()
    {
        //Create GameObject from the newly created clusters and create editors 
        for (int i = 0; i < numberSuggestions; i++)
        {
            mapSuggestionGrid[i].ConvertIntToCells(suggestionsInt[i]);
        }
    }
}
