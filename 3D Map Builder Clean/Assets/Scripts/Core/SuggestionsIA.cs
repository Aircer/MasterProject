using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor;

namespace MapTileGridCreator.Core
{
    public static class IA
    {
        public static List<WaypointCluster> GetSuggestionsClusters(WaypointCluster cluster, int nbSuggestions)
        {
            List<WaypointCluster> suggestionsClusters = new List<WaypointCluster>();
            float progressBarTime = 0;

            for (int i=0; i< nbSuggestions;i++)
            {
                suggestionsClusters.Add(TransformationCluster(cluster, nbSuggestions, ref progressBarTime));
            }

            return suggestionsClusters;
        }

        private static WaypointCluster TransformationCluster(WaypointCluster cluster, int nbSugg, ref float progressBarTime)
        {
            Dictionary<Vector3Int, Waypoint> newWaypoints = new Dictionary<Vector3Int, Waypoint>(cluster.waypoints);
            WaypointCluster newCluster = new WaypointCluster();

            foreach (KeyValuePair<Vector3Int, Waypoint> point in cluster.waypoints)
            {
                if (point.Value.type != null)
                {
                    Vector3Int newKey = new Vector3Int(point.Key.x + UnityEngine.Random.Range(-2, 2), point.Key.y, point.Key.z + UnityEngine.Random.Range(-2, 2));
                    if (newWaypoints.ContainsKey(newKey))
                    {
                        newWaypoints[newKey] = point.Value;
                        newWaypoints[newKey].type = point.Value.type;
                    }
                }
                progressBarTime++;
                EditorUtility.DisplayProgressBar("Transforming cells", "IA is working...", progressBarTime / (nbSugg*cluster.waypoints.Count));
            }

            newCluster.waypoints = newWaypoints;
            return newCluster;
        }
    }
}