﻿using System.Collections.Generic;
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
    private List<WaypointCluster> suggestionsClusters = new List<WaypointCluster>();
    private Cell[,,] mapCells;
    private List<Cell[,,]> mapSuggestionCells;
    private List<Grid3D> mapSuggestionGrid;
    private Vector2 scrollPos;
    private EditorWindow window;

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
        numberSuggestions = 4;

        if (mapWindow == null)
            mapWindow = (MapTileGridCreatorWindow)Resources.FindObjectsOfTypeAll(typeof(MapTileGridCreatorWindow))[0];

        window = GetWindow(typeof(SuggestionsEditor));
        evolAlgoParams.population = 50;
        evolAlgoParams.elitism = 4;
        evolAlgoParams.generations = 20;
        evolAlgoParams.mutationRate = 0.005f;

        evolAlgoParams.wDifference = 0.2f;
        evolAlgoParams.wWalkingAreas = 0.2f;
        evolAlgoParams.wWallsCuboids = 0.2f;
        evolAlgoParams.wPathfinding = 1f;
    }
    
    private void OnGUI()
    {
        EditorGUIUtility.labelWidth = 100;
        //Get parameters of Evolutionary the Algorithm
        GUILayout.BeginHorizontal();
        evolAlgoParams.mutationRate = EditorGUILayout.FloatField("Mutation Rate ", evolAlgoParams.mutationRate, GUILayout.Width(0.5f * position.width));
        evolAlgoParams.population = EditorGUILayout.IntField("Population ", evolAlgoParams.population, GUILayout.Width(0.5f * position.width));
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        evolAlgoParams.generations = EditorGUILayout.IntField("Generations ", evolAlgoParams.generations, GUILayout.Width(0.5f * position.width));
        evolAlgoParams.elitism = EditorGUILayout.IntField("Elitism ", evolAlgoParams.elitism, GUILayout.Width(0.5f * position.width));
        GUILayout.EndHorizontal();

        EditorGUIUtility.labelWidth = 70;
        GUILayout.BeginHorizontal();
        evolAlgoParams.wDifference = EditorGUILayout.FloatField("wDiff ", evolAlgoParams.wDifference, GUILayout.Width(0.25f * position.width));
        evolAlgoParams.wWalkingAreas = EditorGUILayout.FloatField("wWA ", evolAlgoParams.wWalkingAreas, GUILayout.Width(0.25f * position.width));
        evolAlgoParams.wWallsCuboids = EditorGUILayout.FloatField("wWalls ", evolAlgoParams.wWallsCuboids, GUILayout.Width(0.25f * position.width));
        evolAlgoParams.wPathfinding = EditorGUILayout.FloatField("wPath ", evolAlgoParams.wPathfinding, GUILayout.Width(0.25f * position.width));
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
                GUILayoutUtility.GetRect(0.5f*position.width, position.height*0.45f - 50);
                Camera previewCam = mapSuggestionGrid[i].transform.GetComponentInChildren<Camera>();
                previewCam.hideFlags = HideFlags.HideAndDontSave;
                Rect cameraRect = new Rect(5, 70 + i*(position.height * 0.25f-17f), 0.5f * position.width-10, position.height * 0.49f - 60);
                if (previewCam)
                {
                    Handles.DrawCamera(cameraRect, previewCam, DrawCameraMode.Normal);
                }

                GUILayoutUtility.GetRect(0.5f * position.width, position.height * 0.49f - 50);
                previewCam = mapSuggestionGrid[i+1].transform.GetComponentInChildren<Camera>();
                previewCam.hideFlags = HideFlags.HideAndDontSave;
                cameraRect = new Rect(0.5f*position.width+5, 70 + i *(position.height * 0.25f-17f), 0.5f * position.width-10, position.height * 0.49f - 60);
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

                        FuncMain.TransformCellsFromWaypoints(mapSuggestionCells[j], suggestionsClusters[j].GetWaypoints());
                        FuncMain.TransformCellsFromWaypoints(mapCells, mapCluster.GetWaypoints());

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
        int[][][] wp =  mapCluster.GetWaypointsParams();
        int sizeDNDA_X = mapCluster.size.x + 2; int sizeDNDA_Y = mapCluster.size.y + 2; int sizeDNDA_Z = mapCluster.size.z + 2;
        TypeParams[] typeParams = new TypeParams[mapCluster.cellInfos.Count];

        for (int i = 0; i < typeParams.Length; i++)
        {
            typeParams[i] = mapCluster.cellInfos[i].typeParams;
        }

        Genetics.Init geneticInit = new Genetics.Init();

        List<int[][][]> newWpList = geneticInit.GetSuggestionsClusters(sizeDNDA_X, sizeDNDA_Y, sizeDNDA_Z, typeParams, wp, numberSuggestions, evolAlgoParams);
        List<WaypointCluster> newSuggestionClusters = new List<WaypointCluster>();
        for (int i = 0; i < numberSuggestions; i++)
        {
            newSuggestionClusters.Add(new WaypointCluster(mapCluster.size + UnityEngine.Vector3Int.one*2, newWpList[i], mapCluster.cellInfos.ToArray()));
        }

        suggestionsClusters = newSuggestionClusters; 
    }

    public void NewSuggestionsCells()
    {
        //Create GameObject from the newly created clusters and create editors 
        for (int i = 0; i < numberSuggestions; i++)
        {
            //FuncMain.EditorCoroutines.Execute(FuncMain.CoroutineTransformCellsFromWaypoints(mapSuggestionCells[i], suggestionsClusters[i].GetWaypoints()));
            FuncMain.TransformCellsFromWaypoints(mapSuggestionCells[i], suggestionsClusters[i].GetWaypoints());
        }
    }
}
