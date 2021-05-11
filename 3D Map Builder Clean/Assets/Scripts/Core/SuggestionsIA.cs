using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor;
using MapTileGridCreator.Utilities;
using System.Diagnostics;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace MapTileGridCreator.Core
{
    public static class IA
    {
        public static List<WaypointCluster> GetSuggestionsClusters(WaypointCluster cluster, int nbSuggestions, EvolutionaryAlgoParams algoParams)
        {
            List<WaypointCluster> suggestionsClusters = new List<WaypointCluster>();
            Waypoint[,,] waypoints = cluster.GetWaypoints();
            Vector3Int size = new Vector3Int(waypoints.GetLength(0), waypoints.GetLength(1), waypoints.GetLength(2));

            TestGenetics newGenetic = new TestGenetics();
            newGenetic.elitism = algoParams.elitism;
            newGenetic.populationSize = algoParams.population;
            newGenetic.mutationRate = algoParams.mutationRate;
            newGenetic.StartGenetics(size, cluster);

            for (int j = 0; j < algoParams.generations; j++)
            {
                newGenetic.UpdateGenetics();
            }

            for (int i = 0; i < nbSuggestions; i++)
            {
                //suggestionsClusters.Add(TransformationCluster(rand, waypoints, waypointsDico));

            }

            suggestionsClusters = newGenetic.GetBestClusters(nbSuggestions);

            return suggestionsClusters;
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

        public static void SetWalls(ref HashSet<Vector3Int> blocksSolos, ref HashSet<Wall> walls_x, ref HashSet<Wall> walls_z, WaypointParams[][][] Genes, TypeParams[] typeParams, Vector3Int index)
        {
            int x = index.x; int y = index.y; int z = index.z;

            if (typeParams[Genes[x][y][z].type].wall)
            {
                bool inAWallX = false;
                bool inAWallZ = false;
                HashSet<Vector3Int> indexesX = new HashSet<Vector3Int>();
                HashSet<Vector3Int> indexesZ = new HashSet<Vector3Int>();
                HashSet<Wall> wallsMerged = new HashSet<Wall>();
                Wall wallToMerge = new Wall();

                CheckIndexInWall(ref walls_x, ref walls_z, ref wallToMerge, ref wallsMerged, ref inAWallX, Genes[x - 1][y][z], new Vector3Int(x - 1, y, z), typeParams, index);
                CheckIndexInWall(ref walls_x, ref walls_z, ref wallToMerge, ref wallsMerged, ref inAWallX, Genes[x + 1][y][z], new Vector3Int(x + 1, y, z), typeParams, index);
                CheckIndexInWall(ref walls_x, ref walls_z, ref wallToMerge, ref wallsMerged, ref inAWallX, Genes[x][y - 1][z], new Vector3Int(x, y - 1, z), typeParams, index);
                CheckIndexInWall(ref walls_x, ref walls_z, ref wallToMerge, ref wallsMerged, ref inAWallX, Genes[x][y + 1][z], new Vector3Int(x, y + 1, z), typeParams, index);

                wallToMerge = new Wall();
                wallsMerged.Clear();
                
                CheckIndexInWall(ref walls_z, ref walls_x, ref wallToMerge, ref wallsMerged, ref inAWallZ, Genes[x][y][z - 1], new Vector3Int(x, y, z - 1), typeParams, index);
                CheckIndexInWall(ref walls_z, ref walls_x, ref wallToMerge, ref wallsMerged, ref inAWallZ, Genes[x][y][z + 1], new Vector3Int(x, y, z + 1), typeParams, index);
                CheckIndexInWall(ref walls_z, ref walls_x, ref wallToMerge, ref wallsMerged, ref inAWallZ, Genes[x][y - 1][z], new Vector3Int(x, y - 1, z), typeParams, index);
                CheckIndexInWall(ref walls_z, ref walls_x, ref wallToMerge, ref wallsMerged, ref inAWallZ, Genes[x][y + 1][z], new Vector3Int(x, y + 1, z), typeParams, index);

                if (!inAWallX)
                {
                    AddIndexInWall(Genes[x - 1][y][z], new Vector3Int(x - 1, y, z), typeParams, ref indexesX);
                    AddIndexInWall(Genes[x + 1][y][z], new Vector3Int(x + 1, y, z), typeParams, ref indexesX);

                    if (indexesX.Count > 0)
                    {
                        AddIndexInWall(Genes[x][y - 1][z], new Vector3Int(x, y - 1, z), typeParams, ref indexesX);
                        AddIndexInWall(Genes[x][y + 1][z], new Vector3Int(x, y + 1, z), typeParams, ref indexesX);

                        indexesX.Add(new Vector3Int(x, y, z));
                        Wall newWall = new Wall();
                        newWall.indexes = indexesX;
                        newWall.position = z;
                        walls_x.Add(newWall);
                        inAWallX = true;
                    }
                }
                
                if (!inAWallZ)
                {
                    AddIndexInWall(Genes[x][y][z - 1], new Vector3Int(x, y, z - 1), typeParams, ref indexesZ);
                    AddIndexInWall(Genes[x][y][z + 1], new Vector3Int(x, y, z + 1), typeParams, ref indexesZ);

                    if (indexesZ.Count > 0)
                    {
                        AddIndexInWall(Genes[x][y - 1][z], new Vector3Int(x, y - 1, z), typeParams, ref indexesX);
                        AddIndexInWall(Genes[x][y + 1][z], new Vector3Int(x, y + 1, z), typeParams, ref indexesX);

                        indexesZ.Add(new Vector3Int(x, y, z));
                        Wall newWall = new Wall();
                        newWall.indexes = indexesZ;
                        newWall.position = x;
                        walls_z.Add(newWall);
                        inAWallZ = true;
                    }
                }

                if (!typeParams[Genes[x - 1][y][z].type].wall && !typeParams[Genes[x + 1][y][z].type].wall
                 && !typeParams[Genes[x][y - 1][z].type].wall && !typeParams[Genes[x][y + 1][z].type].wall
                 && !typeParams[Genes[x][y][z - 1].type].wall && !typeParams[Genes[x][y][z + 1].type].wall)
                    blocksSolos.Add(new Vector3Int(x, y, z));

                HashSet<Vector3Int> nomoreBlockSolo = new HashSet<Vector3Int>();

                foreach (Vector3Int ind in blocksSolos)
                {
                    if (typeParams[Genes[x-1][y][z].type].wall) nomoreBlockSolo.Add(ind);
                    if (typeParams[Genes[x+1][y][z].type].wall) nomoreBlockSolo.Add(ind);
                    if (typeParams[Genes[x][y - 1][z].type].wall) nomoreBlockSolo.Add(ind);
                    if (typeParams[Genes[x][y + 1][z].type].wall) nomoreBlockSolo.Add(ind);
                    if (typeParams[Genes[x][y][z-1].type].wall) nomoreBlockSolo.Add(ind);
                    if (typeParams[Genes[x][y][z+1].type].wall) nomoreBlockSolo.Add(ind);
                }

                foreach (Vector3Int rem in nomoreBlockSolo)
                {
                    //blocksSolos.Remove(rem);
                }
            }
        }

        public static void UnsetWalls(ref HashSet<Vector3Int> blocksSolo, ref HashSet<Wall> walls_x, ref HashSet<Wall> walls_z, WaypointParams[][][] Genes, TypeParams[] typeParams, Vector3Int index)
        {
            int x = index.x; int y = index.y; int z = index.z;

            if (!typeParams[Genes[x][y][z].type].wall)
            {
                Wall wallRebuild =  new Wall();

                foreach (Wall wall in walls_x)
                {
                    if (wall.indexes.Contains(index))
                    {
                        wallRebuild = wall;
                        break;
                    }
                }

                if (wallRebuild.indexes != null)
                {
                    walls_x.Remove(wallRebuild);

                    foreach (Vector3Int indexRebuild in wallRebuild.indexes)
                    {
                        SetWalls(ref blocksSolo, ref walls_x, ref walls_z, Genes, typeParams, indexRebuild);
                    }
                }

                wallRebuild = new Wall();

                foreach (Wall wall in walls_z)
                {
                    if (wall.indexes.Contains(index))
                    {
                        wallRebuild = wall;
                        break;
                    }
                }

                if (wallRebuild.indexes != null)
                {
                    walls_z.Remove(wallRebuild);

                    foreach (Vector3Int indexRebuild in wallRebuild.indexes)
                    {
                        SetWalls(ref blocksSolo, ref walls_x, ref walls_z, Genes, typeParams, indexRebuild);
                    }
                }

                blocksSolo.Remove(index);
            }
        }

        private static void CheckIndexInWall(ref HashSet<Wall> walls_check, ref HashSet<Wall> walls_other, ref Wall wallToMerge, ref HashSet<Wall> wallsMerged, ref bool inAWall, WaypointParams neighbor, Vector3Int indexNeighbor, TypeParams[] typeParams, Vector3Int index)
        {
            if (neighbor.type > 0 && typeParams[neighbor.type].wall)
            {
                foreach (Wall wallCheck in walls_check)
                {
                    if (wallCheck.indexes.Contains(indexNeighbor) && !wallCheck.indexes.Contains(index))
                    {
                        wallCheck.indexes.Add(index);
                        if (wallToMerge.indexes != null)
                            wallsMerged.Add(wallToMerge);
                        wallToMerge = wallCheck;
                        inAWall = true;
                    }

                    if (wallCheck.indexes.Contains(index))
                    {
                        inAWall = true;
                    }   
                }
                
                foreach (Wall wall in wallsMerged)
                {
                    wallToMerge.indexes.UnionWith(wall.indexes);
                    walls_check.Remove(wall);
                }
            }
        }

        private static void AddIndexInWall(WaypointParams neighbor, Vector3Int indexNeighbor, TypeParams[] typeParams, ref HashSet<Vector3Int> indexesX)
        {
            if (neighbor.type > 0 && typeParams[neighbor.type].wall)
            {
                indexesX.Add(indexNeighbor);
            }
        }

        public static Phenotype GetPhenotype(Vector3Int size, WaypointParams[][][] Genes, TypeParams[] typeParams)
        {
            Phenotype newPhenotype = new Phenotype();
            newPhenotype.walls_x = new HashSet<Wall>();
            newPhenotype.walls_z = new HashSet<Wall>();
            newPhenotype.blocksSolo = new HashSet<Vector3Int>();

            for (int x = 1; x < size.x-1; x++)
            {
                for (int y = 1; y < size.y-1; y++)
                {
                    for (int z = 1; z < size.z-1; z++)
                    {
                        SetWalls(ref newPhenotype.blocksSolo, ref newPhenotype.walls_x, ref newPhenotype.walls_z, Genes, typeParams, new Vector3Int(x, y, z));
                    }
                }
            }

            return newPhenotype;
        }
    }
}