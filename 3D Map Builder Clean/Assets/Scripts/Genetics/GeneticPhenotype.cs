using System.Collections.Generic;
using System.Linq;
using System;
using UtilitiesGenetic;
using mVectors;

namespace Genetics
{
    public static class PhenotypeCompute
    {
        public static Phenotype GetPhenotype(int sizeX, int sizeY, int sizeZ, int[][][] Genes, TypeParams[] typeParams)
        {
            Vector3Int size = new Vector3Int(sizeX, sizeY, sizeZ);
            HashSet<Vector3Int> cellsInCuboids = new HashSet<Vector3Int>();
            HashSet<Vector3Int> cellsInPaths = new HashSet<Vector3Int>();
            Phenotype newPhenotype = new Phenotype();
            newPhenotype.cuboids = new HashSet<Cuboid>();
            newPhenotype.paths = new HashSet<Path>();

            for (int y = 1; y < sizeY; y++)
            {
                for (int x = 1; x < sizeX; x++)
                {
                    for (int z = 1; z < sizeZ; z++)
                    {
                        Vector3Int input = new Vector3Int(x, y, z);

                        if (!CellIsStruct(x, y, z, Genes, typeParams) && !cellsInCuboids.Contains(input))
                        {
                            Cuboid newCuboid = new Cuboid();
                            newCuboid.min = input;
                            newCuboid.max = GetCuboid(size, Genes, input, typeParams, ref cellsInCuboids);
                            newPhenotype.cuboids.Add(newCuboid);
                        }

                        if (CellIsWalkable(x, y, z, Genes, typeParams) && !cellsInPaths.Contains(input))
                        {
                            Path newPath = new Path();
                            newPath.cells = GetPath(size, Genes, input, typeParams);
                            cellsInPaths.UnionWith(newPath.cells);
                            newPhenotype.paths.Add(newPath);
                        }
                    }
                }
            }

            return newPhenotype;
        }

        public static Vector3Int GetCuboid(Vector3Int size, int[][][] Genes, Vector3Int input, TypeParams[] typeParams, ref HashSet<Vector3Int> cellsInCuboids)
        {
            Vector3Int max = new Vector3Int(size.x, size.y, size.z);
            HashSet<Vector3Int> cellsCuboid = new HashSet<Vector3Int>();
            bool firstBlockDiago = true;
            for (int y = input.y; y < max.y; y++)
            {
                for (int x = input.x; x < max.x; x++)
                {
                    for (int z = input.z; z < max.z; z++)
                    {
                        if (x < max.x && y < max.y && CellIsStruct(x, y, z + 1, Genes, typeParams) || cellsInCuboids.Contains(new Vector3Int(x, y, z + 1)))
                            max.z = z + 1;

                        if (z < max.z && x < max.x && CellIsStruct(x, y + 1, z, Genes, typeParams) || cellsInCuboids.Contains(new Vector3Int(x, y + 1, z)))
                            max.y = y + 1;

                        if (z < max.z && y < max.y && CellIsStruct(x + 1, y, z, Genes, typeParams) || cellsInCuboids.Contains(new Vector3Int(x + 1, y, z)))
                        {
                            if (firstBlockDiago)
                            {
                                if (x + 1 != input.x)
                                {
                                    if (x + 1 - input.x < z - input.z)
                                    {
                                        max.z = z;
                                    }
                                    else
                                    {
                                        max.x = x + 1;
                                    }
                                }
                                else
                                    max.x = x + 1;

                                firstBlockDiago = false;
                            }
                        }
                    }
                }
            }

            int oldNumberEmpty = 999999;
            int numberEmpty = 0;

            for (int y = input.y; y < max.y; y++)
            {
                for (int x = input.x; x < max.x; x++)
                {
                    if (max.z < size.z && !CellIsStruct(x, y, max.z, Genes, typeParams))
                        numberEmpty++;

                    if (input.z > 1 && !CellIsStruct(x, y, input.z - 1, Genes, typeParams))
                        numberEmpty++;
                }

                for (int z = input.z; z < max.z; z++)
                {
                    if (max.x < size.x && !CellIsStruct(max.x, y, z, Genes, typeParams))
                        numberEmpty++;

                    if (input.x > 1 && !CellIsStruct(input.x - 1, y, z, Genes, typeParams))
                        numberEmpty++;
                }

                if (numberEmpty > oldNumberEmpty && max.y > y)
                {
                    max.y = y;
                }

                oldNumberEmpty = numberEmpty;
                numberEmpty = 0;
            }

            if (max.x < size.x)
            {
                int holeMaxX = 0;
                for (int z = input.z; z < max.z; z++)
                {
                    if (!CellIsStruct(max.x, input.y, z, Genes, typeParams) && !cellsInCuboids.Contains(new Vector3Int(max.x, input.y, z)))
                        holeMaxX++;
                    else
                        holeMaxX = 0;

                    if (holeMaxX >= max.x - input.x + 1 && max.z > z - holeMaxX + 1)
                    {
                        max.z = z - holeMaxX + 1;
                    }
                }
            }

            if (input.x > 1)
            {
                int holeMinX = 0;
                for (int z = input.z; z < max.z; z++)
                {
                    if (!CellIsStruct(input.x - 1, input.y, z, Genes, typeParams) && !cellsInCuboids.Contains(new Vector3Int(input.x - 1, input.y, z)))
                        holeMinX++;
                    else
                        holeMinX = 0;

                    if (holeMinX >= max.x - input.x + 1 && max.z > z - holeMinX + 1)
                    {
                        max.z = z - holeMinX + 1;
                    }
                }
            }

            if (max.z < size.z)
            {
                int holeMaxZ = 0;
                for (int x = input.x; x < max.x; x++)
                {
                    if (!CellIsStruct(x, input.y, max.z, Genes, typeParams) && !cellsInCuboids.Contains(new Vector3Int(x, input.y, max.z)))
                        holeMaxZ++;
                    else
                        holeMaxZ = 0;

                    if (holeMaxZ >= max.z - input.z + 1 && max.x > x - holeMaxZ + 1)
                    {
                        max.x = x - holeMaxZ + 1;
                    }
                }
            }

            if (input.z > 1)
            {
                int holeMinZ = 0;
                for (int x = input.x; x < max.x; x++)
                {
                    if (!CellIsStruct(x, input.y, input.z - 1, Genes, typeParams) && !cellsInCuboids.Contains(new Vector3Int(x, input.y, input.z - 1)))
                        holeMinZ++;
                    else
                        holeMinZ = 0;

                    if (holeMinZ >= max.z - input.z + 1 && max.x > x - holeMinZ + 1)
                    {
                        max.x = x - holeMinZ + 1;
                    }
                }
            }

            for (int i = input.x; i < max.x; i++)
            {
                for (int j = input.y; j < max.y; j++)
                {
                    for (int k = input.z; k < max.z; k++)
                    {
                        cellsInCuboids.Add(new Vector3Int(i, j, k));
                    }
                }
            }

            return max;
        }

        public static HashSet<Vector3Int> GetPath(Vector3Int size, int[][][] Genes, Vector3Int input, TypeParams[] typeParams)
        {
            Stack<Vector3Int> openSet = new Stack<Vector3Int>();
            HashSet<Vector3Int> newPath = new HashSet<Vector3Int>();
            openSet.Push(input);

            while (openSet.Count > 0)
            {
                Vector3Int currentCell = openSet.Pop();
                newPath.Add(currentCell);
                int x = currentCell.x; int y = currentCell.y; int z = currentCell.z;

                if(x - 1 > 0 && NeighborWalkable(x - 1, y, z, Genes, typeParams, newPath))
                    openSet.Push(new Vector3Int(x - 1, y, z));
                if (x + 1 < size.x && NeighborWalkable(x + 1, y, z, Genes, typeParams, newPath))
                    openSet.Push(new Vector3Int(x + 1, y, z));
                if (y - 1 > 0 && NeighborWalkable(x, y - 1, z, Genes, typeParams, newPath))
                    openSet.Push(new Vector3Int(x, y - 1, z));
                if (y + 1 < size.y && NeighborWalkable(x, y + 1, z, Genes, typeParams, newPath))
                    openSet.Push(new Vector3Int(x, y + 1, z));
                if (z - 1 > 0 && NeighborWalkable(x, y, z - 1, Genes, typeParams, newPath))
                    openSet.Push(new Vector3Int(x, y, z - 1));
                if (z + 1 < size.z && NeighborWalkable(x, y, z + 1, Genes, typeParams, newPath))
                    openSet.Push(new Vector3Int(x, y, z + 1));
            }

            return newPath;
        }

        public static bool CellIsStruct(int x, int y, int z, int[][][] Genes, TypeParams[] typeParams)
        {
            if (typeParams[Genes[x][y][z]].wall || typeParams[Genes[x][y][z]].floor)
                return true;
            else
                return false;
        }

        public static bool CellIsWalkable(int x, int y, int z, int[][][] Genes, TypeParams[] typeParams)
        {
            if ((!CellIsStruct(x, y, z, Genes, typeParams) && typeParams[Genes[x][y - 1][z]].floor)
              || (typeParams[Genes[x][y][z]].door && CellIsStruct(x, y - 1, z, Genes, typeParams))
              || (Genes[x][y][z] == 0 && typeParams[Genes[x][y - 1][z]].stair)
              || (Genes[x][y + 1][z] == 0 && typeParams[Genes[x][y][z]].stair)
              || (typeParams[Genes[x][y][z]].ladder || typeParams[Genes[x][y - 1][z]].ladder)
              || (typeParams[Genes[x][y][z]].door && Genes[x][y - 1][z] > 0))
                return true;
            else
                return false;
        }

        public static bool NeighborWalkable(int x, int y, int z, int[][][] Genes, TypeParams[] typeParams, HashSet<Vector3Int> newPath)
        {
            if (CellIsWalkable(x, y, z, Genes, typeParams) && !newPath.Contains(new Vector3Int(x, y, z)))
                return true;
            else
                return false;
        }
    }
}


