using System.Collections.Generic;
using UnityEngine;

namespace MapTileGridCreator.Core
{
    public static class IA
    {
        public static List<WaypointCluster> GetSuggestionsClusters(WaypointCluster cluster, int nbSuggestions)
        {
            List<WaypointCluster> suggestionsClusters = new List<WaypointCluster>();

            for (int i=0; i< nbSuggestions;i++)
            {
                suggestionsClusters.Add(TransformationCluster(cluster));
            }

            return suggestionsClusters;
        }

        private static WaypointCluster TransformationCluster(WaypointCluster cluster)
        {
            Dictionary<Vector3Int, Waypoint> newWaypoints = new Dictionary<Vector3Int, Waypoint>(cluster.waypoints);
            WaypointCluster newCluster = new WaypointCluster();

            foreach (KeyValuePair<Vector3Int, Waypoint> point in cluster.waypoints)
            {
                Vector3Int newKey = new Vector3Int(point.Key.x + Random.Range(-1, 1), point.Key.y, point.Key.z + Random.Range(-1, 1));
                if(newWaypoints.ContainsKey(newKey))
                    newWaypoints[newKey] = point.Value;
            }

            newCluster.waypoints = newWaypoints;
            return newCluster;
        }
    }
}