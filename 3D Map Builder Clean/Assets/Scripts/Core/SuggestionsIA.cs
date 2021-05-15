using System.Collections.Generic;
using UnityEngine;

namespace MapTileGridCreator.Core
{
    public static class IA
    {
        public static List<WaypointCluster> GetSuggestionsClusters(WaypointCluster cluster, int nbSuggestions, EvolutionaryAlgoParams algoParams)
        {
            TestGenetics newGenetic = new TestGenetics();
            newGenetic.StartGenetics(cluster, algoParams);

            for (int j = 0; j < algoParams.generations; j++)
            {
                newGenetic.UpdateGenetics();
            }

            return newGenetic.GetBestClusters(nbSuggestions);
        }

        public static Phenotype GetPhenotype(int sizeX, int sizeY, int sizeZ, WaypointParams[][][] Genes, TypeParams[] typeParams)
        {
            Phenotype newPhenotype = new Phenotype();
            newPhenotype.cellsWalls = new int();
            newPhenotype.cellsWallsSolo = new int();
            newPhenotype.cellsWallsCrowded = new int();

            for (int x = 1; x < sizeX; x++)
            {
                for (int y = 1; y < sizeY; y++)
                {
                    for (int z = 1; z < sizeZ; z++)
                    {
                        if (typeParams[Genes[x][y][z].type].wall)
                        {
                            if ((!typeParams[Genes[x - 1][y][z].type].wall && !typeParams[Genes[x + 1][y][z].type].wall
                             && !typeParams[Genes[x][y][z - 1].type].wall && !typeParams[Genes[x][y][z + 1].type].wall)
                             || (!typeParams[Genes[x][y - 1][z].type].wall && !typeParams[Genes[x][y + 1][z].type].wall))
                            {
                                newPhenotype.cellsWallsSolo++;
                                //Genes[x][y][z].type = 0;
                            }
                            else if ((typeParams[Genes[x - 1][y][z].type].wall && typeParams[Genes[x - 1][y][z - 1].type].wall && typeParams[Genes[x][y][z - 1].type].wall)
                           || (typeParams[Genes[x - 1][y][z].type].wall && typeParams[Genes[x - 1][y][z + 1].type].wall && typeParams[Genes[x][y][z + 1].type].wall)
                           || (typeParams[Genes[x + 1][y][z].type].wall && typeParams[Genes[x + 1][y][z - 1].type].wall && typeParams[Genes[x][y][z - 1].type].wall)
                           || (typeParams[Genes[x + 1][y][z].type].wall && typeParams[Genes[x + 1][y][z + 1].type].wall && typeParams[Genes[x][y][z + 1].type].wall))
                            {
                                newPhenotype.cellsWallsCrowded++;
                                //Genes[x][y][z].type = 0;
                            }

                            if(typeParams[Genes[x][y][z].type].wall)
                                newPhenotype.cellsWalls++;
                        }
                    }
                }
            }

            return newPhenotype;
        }

        public static WaypointParams[][][] FloodFill(Vector3Int size, WaypointParams[][][] Genes, Vector3Int input, int newType)
        {
            if (Genes[input.x][input.y][input.z].type == newType)
                return Genes;

            int y = input.y;
            int targetType = Genes[input.x][input.y][input.z].type;

            Stack<Vector3Int> pixels = new Stack<Vector3Int>();

            pixels.Push(input);
            while (pixels.Count != 0)
            {
                Vector3Int temp = pixels.Pop();
                int z1 = temp.z;
                while (z1 >= 1 && Genes[temp.x][y][z1].type == targetType)
                {
                    z1--;
                }
                z1++;
                Debug.Log(z1);
                bool spanLeft = false;
                bool spanRight = false;

                while (z1 < size.z && Genes[temp.x][y][z1].type == targetType)
                {
                    Genes[temp.x][y][z1].type = newType;

                    if (!spanLeft && temp.x > 1 && Genes[temp.x-1][y][z1].type == targetType)
                    {
                        pixels.Push(new Vector3Int(temp.x - 1,y, z1));
                        spanLeft = true;
                    }
                    else if (spanLeft && temp.x - 1 == 1 && Genes[temp.x - 1][y][z1].type != targetType)
                    {
                        spanLeft = false;
                    }
                    if (!spanRight && temp.x < size.x - 1 && Genes[temp.x + 1][y][z1].type == targetType)
                    {
                        pixels.Push(new Vector3Int(temp.x + 1, y, z1));
                        spanRight = true;
                    }
                    else if (spanRight && temp.x < size.x - 1 && Genes[temp.x + 1][y][z1].type != targetType)
                    {
                        spanRight = false;
                    }
                    z1++;
                }
            }

            return Genes;
        }

        public static WaypointParams[][][] MutationTranslateWall(Vector3Int size, WaypointParams[][][] Genes, Vector3Int input, TypeParams[] typeParams, SharpNeatLib.Maths.FastRandom randomFast)
        {
            int x = input.x; int y = input.y; int z = input.z;

            if (typeParams[Genes[x][y][z].type].wall)
            {
                HashSet<Vector3Int> wall = new HashSet<Vector3Int>(); 

                if ((typeParams[Genes[x][y][z - 1].type].wall || typeParams[Genes[x][y][z + 1].type].wall)
               && (!typeParams[Genes[x - 1][y][z].type].wall && !typeParams[Genes[x + 1][y][z].type].wall))
                {
                    wall = TakeWallZ(size, Genes, input, typeParams);
                    return TranslateWallZ(size, input, wall, Genes, randomFast);
                }
                else if ((typeParams[Genes[x - 1][y][z].type].wall || typeParams[Genes[x + 1][y][z].type].wall)
               && !typeParams[Genes[x][y][z - 1].type].wall && !typeParams[Genes[x][y][z + 1].type].wall)
                {
                    UnityEngine.Debug.Log("Will Take");
                    wall = TakeWallX(size, Genes, input, typeParams);
                    UnityEngine.Debug.Log("WallTaken");
                    return TranslateWallX(size, input, wall, Genes, randomFast);
                }
                else 
                {
                    if(randomFast.Next(2) < 1 ? true : false)
                    {
                        wall = TakeWallX(size, Genes, input, typeParams);
                        return TranslateWallX(size, input, wall, Genes, randomFast);
                    }
                    else
                    {
                        wall = TakeWallZ(size, Genes, input, typeParams);
                        return TranslateWallZ(size, input, wall, Genes, randomFast);
                    }
                }
            }

            return Genes;
        }

        public static HashSet<Vector3Int> TakeWallX(Vector3Int size, WaypointParams[][][] Genes, Vector3Int input, TypeParams[] typeParams)
        {
            HashSet<Vector3Int> wall = new HashSet<Vector3Int>();
            int x = input.x; int y = input.y; int z = input.z;

            Stack<Vector3Int> cells = new Stack<Vector3Int>();

            cells.Push(input);
            while (cells.Count != 0)
            {
                Vector3Int temp = cells.Pop();
                int y1 = temp.y;
                while (y1 >= 1 && typeParams[Genes[temp.x][y1][z].type].wall)
                {
                    y1--;
                }
                y1++;

                bool spanLeft = false;
                bool spanRight = false;

                while (y1 < size.y && typeParams[Genes[temp.x][y1][z].type].wall)
                {
                    if (!typeParams[Genes[temp.x][y1][z - 1].type].wall && !typeParams[Genes[temp.x][y1][z + 1].type].wall)
                        wall.Add(new Vector3Int(temp.x, y1, z));

                    if (!spanLeft && temp.x > 1 && typeParams[Genes[temp.x-1][y1][z].type].wall && !wall.Contains(new Vector3Int(temp.x - 1, y1, z)))
                    {
                        cells.Push(new Vector3Int(temp.x - 1, y1, z));
                        spanLeft = true;
                    }
                    else if (spanLeft && temp.x - 1 == 1 && !typeParams[Genes[temp.x - 1][y1][z].type].wall)
                    {
                        spanLeft = false;
                    }
                    if (!spanRight && temp.x < size.x - 1 && typeParams[Genes[temp.x + 1][y1][z].type].wall && !wall.Contains(new Vector3Int(temp.x + 1, y1, z)))
                    {
                        cells.Push(new Vector3Int(temp.x + 1, y1, z));
                        spanRight = true;
                    }
                    else if (spanRight && temp.x < size.x - 1 && !typeParams[Genes[temp.x + 1][y1][z].type].wall)
                    {
                        spanRight = false;
                    }
                    y1++;
                }
            }

            return wall;
        }

        public static HashSet<Vector3Int> TakeWallZ(Vector3Int size, WaypointParams[][][] Genes, Vector3Int input, TypeParams[] typeParams)
        {
            HashSet<Vector3Int> wall = new HashSet<Vector3Int>();
            int x = input.x; int y = input.y; int z = input.z;

            Stack<Vector3Int> cells = new Stack<Vector3Int>();

            cells.Push(input);
            while (cells.Count != 0)
            {
                Vector3Int temp = cells.Pop();
                int y1 = temp.y;
                while (y1 >= 1 && typeParams[Genes[x][y1][temp.z].type].wall)
                {
                    y1--;
                }
                y1++;

                bool spanLeft = false;
                bool spanRight = false;

                while (y1 < size.y && typeParams[Genes[x][y1][temp.z].type].wall)
                {
                    if(!typeParams[Genes[x - 1][y1][temp.z].type].wall && !typeParams[Genes[x + 1][y1][temp.z].type].wall)
                        wall.Add(new Vector3Int(x, y1, temp.z));

                    if (!spanLeft && temp.z > 1 && typeParams[Genes[x][y1][temp.z - 1].type].wall && !wall.Contains(new Vector3Int(x, y1, temp.z - 1)))
                    {
                        cells.Push(new Vector3Int(x, y1, temp.z - 1));
                        spanLeft = true;
                    }
                    else if (spanLeft && temp.z - 1 == 1 && !typeParams[Genes[x][y1][temp.z - 1].type].wall)
                    {
                        spanLeft = false;
                    }
                    if (!spanRight && temp.z < size.z - 1 && typeParams[Genes[x][y1][temp.z + 1].type].wall && !wall.Contains(new Vector3Int(x, y1, temp.z + 1)))
                    {
                        cells.Push(new Vector3Int(x, y1, temp.z + 1));
                        spanRight = true;
                    }
                    else if (spanRight && temp.z < size.z - 1 && !typeParams[Genes[x][y1][temp.z + 1].type].wall)
                    {
                        spanRight = false;
                    }
                    y1++;
                }
            }

            return wall;
        }

        public static WaypointParams[][][] TranslateWallX(Vector3Int size, Vector3Int input, HashSet<Vector3Int> wall, WaypointParams[][][] Genes, SharpNeatLib.Maths.FastRandom randomFast)
        {
            int upperTranslationLimit = size.z - input.z;
            int lowerTranslationLimit = input.z - 1;
            int translation = randomFast.Next(-lowerTranslationLimit, upperTranslationLimit);

            foreach(Vector3Int index in wall)
            {
                Genes[index.x][index.y][index.z + translation].type = Genes[index.x][index.y][index.z].type;
                Genes[index.x][index.y][index.z].type = 0; 
            }

            return Genes;
        }

        public static WaypointParams[][][] TranslateWallZ(Vector3Int size, Vector3Int input, HashSet<Vector3Int> wall, WaypointParams[][][] Genes, SharpNeatLib.Maths.FastRandom randomFast)
        {
            int upperTranslationLimit = size.x - input.x;
            int lowerTranslationLimit = input.x - 1;
            int translation = randomFast.Next(-lowerTranslationLimit, upperTranslationLimit);

            foreach (Vector3Int index in wall)
            {
                Genes[index.x + translation][index.y][index.z].type = Genes[index.x][index.y][index.z].type;
                Genes[index.x][index.y][index.z].type = 0;
            }

            return Genes;
        }
    }
}