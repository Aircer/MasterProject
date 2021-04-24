using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor;
using MapTileGridCreator.Utilities;
using System.Diagnostics;

namespace MapTileGridCreator.Core
{
    public static class IA
    {
        public static List<WaypointCluster> GetSuggestionsClusters(WaypointCluster cluster, int nbSuggestions, EvolutionaryAlgoParams algoParams)
        {
            List<WaypointCluster> suggestionsClusters = new List<WaypointCluster>();
            Waypoint[,,] waypoints = cluster.GetWaypoints();
            Dictionary<CellInformation, List<Vector3Int>> waypointsDico = cluster.GetWaypointsDico();
            System.Random rand = new System.Random();

            TestGenetics newGenetic = new TestGenetics();
            newGenetic.elitism = algoParams.elitism;
            newGenetic.populationSize = algoParams.population;
            newGenetic.mutationRate = algoParams.mutationRate;
            newGenetic.StartGenetics(new Vector3Int(waypoints.GetLength(0), waypoints.GetLength(1), waypoints.GetLength(2)), cluster);

            for (int j = 0; j < algoParams.generations; j++)
            {
                newGenetic.UpdateGenetics();
            }

            for (int i=0; i< nbSuggestions;i++)
            {
                //suggestionsClusters.Add(TransformationCluster(rand, waypoints, waypointsDico));

            }

            suggestionsClusters = newGenetic.GetBestClusters(nbSuggestions);

            return suggestionsClusters;
        }

        private static WaypointCluster TransformationCluster(System.Random rand, Waypoint[,,] waypoints, Dictionary<CellInformation, List<Vector3Int>> waypointsDico)
        {
            Vector3Int size_grid = new Vector3Int(waypoints.GetLength(0), waypoints.GetLength(1), waypoints.GetLength(2));
            WaypointCluster newCluster = new WaypointCluster(size_grid);

            foreach (CellInformation key in waypointsDico.Keys)
            {
                foreach (Vector3Int oldKey in waypointsDico[key])
                {
                    if (waypoints[oldKey.x, oldKey.y, oldKey.z].baseType)
                    {
                        Vector3Int newKey = new Vector3Int(rand.Next(-2, 2) + oldKey.x, oldKey.y, rand.Next(-2, 2) + oldKey.z);

                        if (CheckNeighbordsFull(waypoints[oldKey.x, oldKey.y, oldKey.z]) 
                                || !waypoints[oldKey.x, oldKey.y, oldKey.z].show 
                                || !CanPaintHere(size_grid, newCluster.GetWaypoints(), newKey, waypoints[oldKey.x, oldKey.y, oldKey.z].type.size, waypoints[oldKey.x, oldKey.y, oldKey.z].rotation) 
                                || (!waypoints[newKey.x, newKey.y, newKey.z].show && waypoints[newKey.x, newKey.y, newKey.z].type != null))
                            newKey = oldKey;

                        if(CanPaintHere(size_grid, newCluster.GetWaypoints(), newKey, waypoints[oldKey.x, oldKey.y, oldKey.z].type.size, waypoints[oldKey.x, oldKey.y, oldKey.z].rotation))
                            MoveOldKey(newCluster, size_grid, waypoints, newKey, oldKey);
                    }
                }
            }

            return newCluster;
        }

        private static void MoveOldKey(WaypointCluster newCluster, Vector3Int size, Waypoint[,,] waypoints, Vector3Int newKey, Vector3Int oldKey)
        {
            newCluster.SetTypeAround(size, waypoints[oldKey.x, oldKey.y, oldKey.z].rotation, waypoints[oldKey.x, oldKey.y, oldKey.z].type, newKey);
            newCluster.SetRotation(waypoints[oldKey.x, oldKey.y, oldKey.z].rotation, newKey);
            newCluster.GetWaypoints()[newKey.x, newKey.y, newKey.z].show = waypoints[oldKey.x, oldKey.y, oldKey.z].show;
        }

        public static bool CheckNeighbordsFull(Waypoint waypoint)
        {
            foreach(Waypoint neighbord in waypoint.SideNeighbor)
            {
                if (neighbord.type == null)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Check if there is enough room to paint the new asset
        ///</summary>
        public static bool CanPaintHere(Vector3Int size_grid, Waypoint[,,] waypoints, Vector3Int index, Vector3Int size, Vector3 rotation)
        {

            Vector3Int lowerBound = default(Vector3Int);
            Vector3Int upperBound = default(Vector3Int);
            SetBounds(ref lowerBound, ref upperBound, index, size, rotation);

            for (int i = lowerBound.x; i <= upperBound.x; i++)
            {
                for (int j = lowerBound.y; j <= upperBound.y; j++)
                {
                    for (int k = lowerBound.z; k <= upperBound.z; k++)
                    {
                        if (!InputInGridBoundaries(new Vector3Int(i, j, k), size_grid) || waypoints[i, j, k].type != null)
                            return false;
                    }
                }
            }

            return true;
        }

        public static void SetBounds(ref Vector3Int lowerBound, ref Vector3Int upperBound, Vector3Int index, Vector3Int size, Vector3 rotation)
        {
            Vector3Int newSize = default(Vector3Int);
            newSize.x = (int)Mathf.Cos(rotation.y * Mathf.Deg2Rad) * size.x + (int)Mathf.Sin(rotation.x * Mathf.Deg2Rad) * (int)Mathf.Sin(rotation.y * Mathf.Deg2Rad) * size.y + (int)Mathf.Sin(rotation.y * Mathf.Deg2Rad) * (int)Mathf.Cos(rotation.x * Mathf.Deg2Rad) * size.z;
            newSize.y = (int)Mathf.Cos(rotation.x * Mathf.Deg2Rad) * size.y - (int)Mathf.Sin(rotation.x * Mathf.Deg2Rad) * size.z;
            newSize.z = -(int)Mathf.Sin(rotation.y * Mathf.Deg2Rad) * size.x + (int)Mathf.Cos(rotation.y * Mathf.Deg2Rad) * (int)Mathf.Sin(rotation.x * Mathf.Deg2Rad) * size.y + (int)Mathf.Cos(rotation.y * Mathf.Deg2Rad) * (int)Mathf.Cos(rotation.x * Mathf.Deg2Rad) * size.z;

            lowerBound.x = newSize.x < 0 ? index.x + newSize.x + 1 : index.x;
            lowerBound.y = newSize.y < 0 ? index.y + newSize.y + 1 : index.y;
            lowerBound.z = newSize.z < 0 ? index.z + newSize.z + 1 : index.z;

            upperBound.x = newSize.x > 0 ? index.x + newSize.x - 1 : index.x;
            upperBound.y = newSize.y > 0 ? index.y + newSize.y - 1 : index.y;
            upperBound.z = newSize.z > 0 ? index.z + newSize.z - 1 : index.z;
        }

        /// <summary>
        /// Check if input is in the grid.
        /// </summary>
        /// /// <returns>Return true if Input is in Boundaries.</returns>
        public static bool InputInGridBoundaries(Vector3 input, Vector3Int size_grid)
        {
            bool inBoundaries = true;

            if (input.x < 0 || input.y < 0 || input.z < 0 || input.x > size_grid.x - 1 || input.y > size_grid.y - 1 || input.z > size_grid.z - 1)
                inBoundaries = false;

            return inBoundaries;
        }
    }
}