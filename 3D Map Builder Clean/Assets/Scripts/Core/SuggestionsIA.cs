using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor;
using MapTileGridCreator.Utilities;

namespace MapTileGridCreator.Core
{
    public static class IA
    {
        public static List<WaypointCluster> GetSuggestionsClusters(WaypointCluster cluster, int nbSuggestions)
        {
            List<WaypointCluster> suggestionsClusters = new List<WaypointCluster>();
            Waypoint[,,] waypoints = cluster.GetWaypoints();
            Dictionary<CellInformation, List<Vector3Int>> waypointsDico = cluster.GetWaypointsDico();
            float progressBarTime = 0;

            for (int i=0; i< nbSuggestions;i++)
            {
                suggestionsClusters.Add(TransformationCluster(waypoints, waypointsDico, nbSuggestions, ref progressBarTime));
            }

            return suggestionsClusters;
        }

        private static WaypointCluster TransformationCluster(Waypoint[,,] waypoints, Dictionary<CellInformation, List<Vector3Int>> waypointsDico, int nbSugg, ref float progressBarTime)
        {
            WaypointCluster newCluster = new WaypointCluster(new Vector3Int(waypoints.GetLength(0), waypoints.GetLength(1), waypoints.GetLength(2)));

            foreach (CellInformation key in waypointsDico.Keys)
            {
                foreach (Vector3Int point in waypointsDico[key])
                {
                    int i = point.x; int j = point.y; int k = point.z;

                    if (waypoints[i, j, k] != null && waypoints[i, j, k].type != null && waypoints[i, j, k].baseType && waypoints[i, j, k].show)
                    {
                        Vector3Int newKey = new Vector3Int(i + UnityEngine.Random.Range(-2, 2), j, k + UnityEngine.Random.Range(-2, 2));
                        if (newKey.x >= 0 && newKey.x < waypoints.GetLength(0) && newKey.z >= 0 && newKey.z < waypoints.GetLength(2))
                        {
                            Vector3Int size = new Vector3Int(waypoints.GetLength(0), waypoints.GetLength(1), waypoints.GetLength(2));
                            newCluster.SetTypeAround(size, waypoints[i, j, k].rotation, waypoints[i, j, k].type, newKey);
                            newCluster.SetRotation(waypoints[i, j, k].rotation, newKey.x, newKey.y, newKey.z);
                        }
                    }

                    if (waypoints[i, j, k] != null && waypoints[i, j, k].type != null && waypoints[i, j, k].baseType && !waypoints[i, j, k].show)
                    {
                        Vector3Int newKey = new Vector3Int(i, j, k);
                        Vector3Int size = new Vector3Int(waypoints.GetLength(0), waypoints.GetLength(1), waypoints.GetLength(2));
                        newCluster.SetTypeAround(size, waypoints[i, j, k].rotation, waypoints[i, j, k].type, newKey);
                        newCluster.SetRotation(waypoints[i, j, k].rotation, newKey.x, newKey.y, newKey.z);
                        newCluster.GetWaypoints()[i, j, k].show = false;
                    }

                    progressBarTime++;
                    EditorUtility.DisplayProgressBar("Transforming cells", "IA is working...", progressBarTime / (nbSugg * waypoints.Length));
                }
            }

            /*
            for (int i = 0; i < waypoints.GetLength(0); i++)
            {
                for (int j = 0; j < waypoints.GetLength(1); j++)
                {
                    for (int k = 0; k < waypoints.GetLength(2); k++)
                    {
                        if (waypoints[i, j, k] != null && waypoints[i,j,k].type != null)
                        {
                            Vector3Int newKey = new Vector3Int(i + UnityEngine.Random.Range(-2, 2), j, k + UnityEngine.Random.Range(-2, 2));
                            if (newKey.x >= 0 && newKey.x < waypoints.GetLength(0) && newKey.z >= 0 && newKey.z < waypoints.GetLength(2))
                            {
                                newCluster.SetType(waypoints[i, j, k].type, newKey.x, newKey.y, newKey.z);
                            }
                        }
                        progressBarTime++;
                        EditorUtility.DisplayProgressBar("Transforming cells", "IA is working...", progressBarTime / (nbSugg * waypoints.Length));
                    }
                }
            }*/

            return newCluster;
        }
    }
}