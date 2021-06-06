using System.Collections.Generic;
using System.Linq;
using System;
using UtilitiesGenetic;
using mVectors;

namespace Genetics
{
    public static class PhenotypeCompute
    {
        public static Vector3Int size;
        public static SharpNeatLib.Maths.FastRandom random;
        public static TypeParams[] typeParams;
        public static int[][][] Genes;

        public static void InitMutations(Vector3Int sizeDNA, SharpNeatLib.Maths.FastRandom rand, TypeParams[] tp)
        {
            size = sizeDNA;
            random = rand;
            typeParams = tp;
        }

        public static Phenotype GetPhenotype(int[][][] newGenes)
        {
            Genes = newGenes;
            HashSet<Vector3Int> cellsInEmptyCuboids = new HashSet<Vector3Int>();
            HashSet<Vector3Int> cellsInWalls= new HashSet<Vector3Int>();
            HashSet<Vector3Int> cellsInWalkableAreas = new HashSet<Vector3Int>();
            HashSet<Vector3Int> cellsInPaths = new HashSet<Vector3Int>();

            Phenotype newPhenotype = new Phenotype();
            newPhenotype.emptyCuboids = new HashSet<Cuboid>();
            newPhenotype.walls= new HashSet<Cuboid>();
            newPhenotype.walkableArea = new HashSet<WalkableArea>();
            newPhenotype.paths = new HashSet<Path>();

            for (int x = 1; x < size.x; x++)
            {
                for (int y = 1; y < size.y; y++)
                {
                    for (int z = 1; z < size.z; z++)
                    {
                        Vector3Int input = new Vector3Int(x, y, z);

                        if (!CellIsStruct(x, y, z) && !cellsInEmptyCuboids.Contains(input))
                        {
                            Cuboid newCuboid = GetCuboid(input, ref cellsInEmptyCuboids, "empty");
                            newPhenotype.emptyCuboids.Add(newCuboid);
                        }

                        if (typeParams[Genes[x][y][z]].wall && !cellsInWalls.Contains(input))
                        {
                            Cuboid newWall = GetCuboid(input, ref cellsInWalls, "wall");
                            newPhenotype.walls.Add(newWall);
                        }

                        if (CellIsWalkable(x, y, z) && !cellsInWalkableAreas.Contains(input))
                        {
                            WalkableArea newWalkableArea = GetWalkableArea(input);
                            cellsInWalkableAreas.UnionWith(newWalkableArea.cells);
                            newPhenotype.walkableArea.Add(newWalkableArea);
                        }

                        if (CellIsPath(x, y, z) && !cellsInPaths.Contains(input))
                        {
                            Path newPath = GetPath(input);
                            cellsInPaths.UnionWith(newPath.cells);
                            newPhenotype.paths.Add(newPath);
                        }
                    }
                }
            }

            foreach (Cuboid cuboidIn in newPhenotype.emptyCuboids)
            {
                foreach (Cuboid cuboidOut in newPhenotype.emptyCuboids)
                {
                    if (cuboidIn.cells.Overlaps(cuboidOut.cellsBorder))
                    {
                        cuboidIn.outCuboids.Add(cuboidOut);
                        cuboidOut.inCuboids.Add(cuboidIn);
                    }
                }
            }

            foreach (Path path in newPhenotype.paths)
            {
                List<WalkableArea> tempWalkableArea = new List<WalkableArea>();
                foreach (WalkableArea wa in newPhenotype.walkableArea)
                {
                    if (path.cells.Overlaps(wa.paths))
                    {
                        wa.neighborsPaths.Add(path);
                        tempWalkableArea.Add(wa);
                    }
                }

                for (int i = 0; i < tempWalkableArea.Count; i++)
                {
                    for (int j = 0; j < tempWalkableArea.Count; j++)
                    {
                        if (j != i && (typeParams[path.type].door || tempWalkableArea[i].yPos != tempWalkableArea[j].yPos))
                        {
                            tempWalkableArea[i].neighborsArea.Add(tempWalkableArea[j]);
                            tempWalkableArea[j].neighborsArea.Add(tempWalkableArea[i]);
                            path.neighborsConnected.Add(tempWalkableArea[i]);
                            path.neighborsConnected.Add(tempWalkableArea[j]);
                        }
                    }
                    path.neighbors.Add(tempWalkableArea[i]);
                }
            }

            return newPhenotype;
        }

        private static Cuboid GetCuboid(Vector3Int input, ref HashSet<Vector3Int> cellsInCuboids)
        {
            Cuboid newCuboid = new Cuboid();
            Vector3Int max = new Vector3Int(size.x, size.y, size.z);
            HashSet<Vector3Int> cellsCuboid = new HashSet<Vector3Int>();
            HashSet<Vector3Int> cells = new HashSet<Vector3Int>();
            bool firstBlockDiago = true;

            for (int y = input.y; y < max.y; y++)
            {
                for (int x = input.x; x < max.x; x++)
                {
                    for (int z = input.z; z < max.z; z++)
                    {
                        if (x < max.x && y < max.y && CellIsStruct(x, y, z + 1) || cellsInCuboids.Contains(new Vector3Int(x, y, z + 1)))
                            max.z = z + 1;

                        if (z < max.z && x < max.x && CellIsStruct(x, y + 1, z) || cellsInCuboids.Contains(new Vector3Int(x, y + 1, z)))
                            max.y = y + 1;

                        if (z < max.z && y < max.y && CellIsStruct(x + 1, y, z) || cellsInCuboids.Contains(new Vector3Int(x + 1, y, z)))
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

            if (max.x < size.x)
            {
                int holeMaxX = 0;
                for (int z = input.z; z < max.z; z++)
                {
                    if (!CellIsStruct(max.x, input.y, z) && !cellsInCuboids.Contains(new Vector3Int(max.x, input.y, z)))
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
                    if (!CellIsStruct(input.x - 1, input.y, z) && !cellsInCuboids.Contains(new Vector3Int(input.x - 1, input.y, z)))
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
                    if (!CellIsStruct(x, input.y, max.z) && !cellsInCuboids.Contains(new Vector3Int(x, input.y, max.z)))
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
                    if (!CellIsStruct(x, input.y, input.z - 1) && !cellsInCuboids.Contains(new Vector3Int(x, input.y, input.z - 1)))
                        holeMinZ++;
                    else
                        holeMinZ = 0;

                    if (holeMinZ >= max.z - input.z + 1 && max.x > x - holeMinZ + 1)
                    {
                        max.x = x - holeMinZ + 1;
                    }
                }
            }

            int oldNumberEmpty = 999999;
            int numberEmpty = 0;

            for (int y = input.y; y < max.y; y++)
            {
                for (int x = input.x; x < max.x; x++)
                {
                    if (max.z < size.z && !CellIsStruct(x, y, max.z))
                        numberEmpty++;

                    if (input.z > 1 && !CellIsStruct(x, y, input.z - 1))
                        numberEmpty++;
                }

                for (int z = input.z; z < max.z; z++)
                {
                    if (max.x < size.x && !CellIsStruct(max.x, y, z))
                        numberEmpty++;

                    if (input.x > 1 && !CellIsStruct(input.x - 1, y, z))
                        numberEmpty++;
                }

                if (numberEmpty > oldNumberEmpty && max.y > y)
                {
                    max.y = y;
                }

                oldNumberEmpty = numberEmpty;
                numberEmpty = 0;
            }

            for (int i = input.x; i < max.x; i++)
            {
                for (int j = input.y; j < max.y; j++)
                {
                    for (int k = input.z; k < max.z; k++)
                    {
                        cellsInCuboids.Add(new Vector3Int(i, j, k));
                        cells.Add(new Vector3Int(i, j, k));
                    }
                }
            }

            newCuboid.min = input;
            newCuboid.max = max;
            newCuboid.cells = cells;
            newCuboid.cellsBorder = ConnectCuboid(newCuboid);
            newCuboid.inCuboids = new HashSet<Cuboid>();
            newCuboid.outCuboids = new HashSet<Cuboid>();

            return newCuboid;
        }

        private static Cuboid GetCuboid(Vector3Int input, ref HashSet<Vector3Int> cellsAlreadyPicked, string type)
        {
            Cuboid newCuboid = new Cuboid();
            Vector3Int max = Grow_ones(input, cellsAlreadyPicked, type);
            HashSet<Vector3Int> cells = new HashSet<Vector3Int>();

            if ((max.x - input.x) < (max.z - input.z))
            {
                if (max.x < size.x)
                {
                    for (int y = input.y; y < max.y; y++)
                    {
                        int holeMaxX = 0;
                        int newMax = 0;

                        for (int z = input.z; z < max.z; z++)
                        {
                            if (((typeParams[Genes[max.x][y][z]].wall && type == "wall") || (!CellIsStruct(max.x, y, z) && type == "empty"))
                                && !cellsAlreadyPicked.Contains(new Vector3Int(max.x, y, z)))
                                holeMaxX++;
                            else
                            {
                                holeMaxX = 0;
                                newMax = z + 1;
                            }

                            if (holeMaxX >= max.x - input.x && newMax < max.z && newMax > input.z)
                            {
                                max.z = newMax;
                                break;
                            }
                        }
                    }
                }
            }
            else
            {
                if (max.z < size.z)
                {
                    for (int y = input.y; y < max.y; y++)
                    {
                        int holeMaxZ = 0;
                        int newMax = 0;

                        for (int x = input.x; x < max.x; x++)
                        {
                            if (((typeParams[Genes[x][y][max.z]].wall && type == "wall") || (!CellIsStruct(x, y, max.z) && type == "empty"))
                                && !cellsAlreadyPicked.Contains(new Vector3Int(x, y, max.z)))
                                holeMaxZ++;
                            else
                            {
                                holeMaxZ = 0;
                                newMax = x + 1;
                            }

                            if (holeMaxZ >= max.z - input.z && newMax < max.x && newMax > input.x)
                            {
                                max.x = newMax;
                                break;
                            }
                        }
                    }
                }
            }
            
            int oldNumberEmpty = 0;
            int numberEmpty = 0;

            for (int y = input.y; y < max.y; y++)
            {
                
                for (int x = input.x; x < max.x; x++)
                {
                    if (max.z < size.z && ((typeParams[Genes[x][y][max.z]].wall && type == "wall") || (!CellIsStruct(x, y, max.z) && type == "empty")))
                        numberEmpty++;
                }
                
                for (int z = input.z; z < max.z; z++)
                {
                    if (max.x < size.x && ((typeParams[Genes[max.x][y][z]].wall && type == "wall") || (!CellIsStruct(max.x, y, z) && type == "empty")))
                        numberEmpty++;
                }

                if (numberEmpty != oldNumberEmpty && max.y > y && y > input.y)
                {
                    max.y = y;
                }

                oldNumberEmpty = numberEmpty;
                numberEmpty = 0;
            }

            for (int i = input.x; i < max.x; i++)
            {
                for (int j = input.y; j < max.y; j++)
                {
                    for (int k = input.z; k < max.z; k++)
                    {
                        cellsAlreadyPicked.Add(new Vector3Int(i, j, k));
                        cells.Add(new Vector3Int(i, j, k));
                    }
                }
            }

            newCuboid.min = input;
            newCuboid.max = max;
            newCuboid.cells = cells;
            newCuboid.cellsBorder = ConnectCuboid(newCuboid);
            newCuboid.inCuboids = new HashSet<Cuboid>();
            newCuboid.outCuboids = new HashSet<Cuboid>();

            return newCuboid;
        }

        private static HashSet<Vector3Int> ConnectCuboid(Cuboid cuboid)
        {

            HashSet<Vector3Int> borderCells  = new HashSet<Vector3Int>();
            borderCells.UnionWith(NewBorderCellsYPos(cuboid));
            borderCells.UnionWith(NewBorderCellsYNeg(cuboid));
            borderCells.UnionWith(NewBorderCellsXPos(cuboid));
            borderCells.UnionWith(NewBorderCellsXNeg(cuboid));
            borderCells.UnionWith(NewBorderCellsZPos(cuboid));
            borderCells.UnionWith(NewBorderCellsZNeg(cuboid));

            return borderCells;
        }

        private static HashSet<Vector3Int> NewBorderCellsYPos(Cuboid cuboid)
        {
            HashSet<Vector3Int> newBorderCells = new HashSet<Vector3Int>();

            if (cuboid.max.y < size.y)
            {
                for (int x = cuboid.min.x; x < cuboid.max.x; x++)
                {
                    for (int z = cuboid.min.z; z < cuboid.max.z; z++)
                    {
                        if (!CellIsStruct(x, cuboid.max.y, z))
                            newBorderCells.Add(new Vector3Int(x, cuboid.max.y, z));
                        else
                            return new HashSet<Vector3Int>();
                    }
                }

                return newBorderCells;
            }

            return new HashSet<Vector3Int>();
        }

        private static Vector3Int Grow_ones(Vector3Int ll, HashSet<Vector3Int> cellsAlreadyPicked, string type)
        {
            Vector3Int urBot = new Vector3Int(0, 0, 0);
            Vector3Int urUp = new Vector3Int(0, 0, 0);
            Vector3Int ur = new Vector3Int(0, 0, 0);

            int x_max = size.x - 1; 
            int z = ll.z - 1; 
            int y = ll.y;
            int x;

            while (z + 1 < size.z 
                && ((typeParams[Genes[ll.x][y][z + 1]].wall && type == "wall") || (!CellIsStruct(ll.x, y, z + 1) && type == "empty"))
                && !cellsAlreadyPicked.Contains(new Vector3Int(ll.x, y, z + 1)))
            {
                z = z + 1; x = ll.x;
                while (x + 1 <= x_max 
                    && ((typeParams[Genes[x + 1][y][z]].wall && type == "wall") || (!CellIsStruct(x + 1, y, z) && type == "empty"))
                    && !cellsAlreadyPicked.Contains(new Vector3Int(x + 1, y, z)))
                {
                    x = x + 1;
                }

                if ((x < x_max && z > ll.z) || urUp.x != 0)
                {
                    x_max = x;
                    if (x_max < size.x && Volume(ll, new Vector3Int(x + 1, y + 1, z + 1)) > Volume(ll, urUp))
                    {
                        urUp.x = x + 1; urUp.y = y + 1; urUp.z = z + 1;
                    }
                }
                else
                {
                    x_max = x;
                    if (x_max < size.x && Volume(ll, new Vector3Int(x + 1, y + 1, z + 1)) > Volume(ll, urBot))
                    {
                        urBot.x = x + 1; urBot.y = y + 1; urBot.z = z + 1;
                    }
                }
            }

            int thicknessBot = 0;
            int thicknessUp = 0;

            if (urBot.x > 0)
                thicknessBot = (urBot.x - ll.x) > (urBot.z - ll.z) ? (urBot.z - ll.z): (urBot.x - ll.x);
            if (urUp.x > 0)
                thicknessUp = (urUp.x - ll.x) > (urUp.z - ll.z) ? (urUp.z - ll.z) : (urUp.x - ll.x);

            if (thicknessBot == thicknessUp)
            {
                if(Volume(ll, urBot) > Volume(ll, urUp))
                    ur = urBot;
                else
                    ur = urUp;
            }
            else
            {
                if (thicknessBot > thicknessUp)
                    ur = urBot;
                else
                    ur = urUp;
            }

            ur.y = GetYMax(ll, ur, cellsAlreadyPicked, type);

            return ur;
        }

        private static int GetYMax(Vector3Int ll, Vector3Int ur, HashSet<Vector3Int> cellsAlreadyPicked, string type)
        {
            for (int y = ll.y; y < size.y; y++)
            {
                for (int x = ll.x; x < ur.x; x++)
                {
                    for (int z = ll.z; z < ur.z; z++)
                    {
                        if (((!typeParams[Genes[x][y][z]].wall && type == "wall") || (CellIsStruct(x, y, z) && type == "empty"))
                            || cellsAlreadyPicked.Contains(new Vector3Int(x, y, z)))
                            return y;
                    }
                }
            }

            return size.y;
        }

        private static int Volume(Vector3Int ll, Vector3Int ur)
        {
            if ((ur.x - ll.x) > 0 && (ur.y - ll.y) > 0 && (ur.z - ll.z) > 0)
                return (ur.x - ll.x) * (ur.y - ll.y) * (ur.z - ll.z);
            else
                return 0;
        }

        private static HashSet<Vector3Int> NewBorderCellsYNeg(Cuboid cuboid)
        {
            HashSet<Vector3Int> newBorderCells = new HashSet<Vector3Int>();

            if (cuboid.min.y - 1 > 0)
            {
                for (int x = cuboid.min.x; x < cuboid.max.x; x++)
                {
                    for (int z = cuboid.min.z; z < cuboid.max.z; z++)
                    {
                        if (!CellIsStruct(x, cuboid.min.y - 1, z))
                            newBorderCells.Add(new Vector3Int(x, cuboid.min.y - 1, z));
                        else
                            return new HashSet<Vector3Int>();
                    }
                }

                return newBorderCells;
            }

            return new HashSet<Vector3Int>();
        }

        private static HashSet<Vector3Int> NewBorderCellsXPos(Cuboid cuboid)
        {
            HashSet<Vector3Int> newBorderCells = new HashSet<Vector3Int>();

            if (cuboid.max.x < size.x)
            {
                for (int y = cuboid.min.y; y < cuboid.max.y; y++)
                {
                    for (int z = cuboid.min.z; z < cuboid.max.z; z++)
                    {
                        if (!CellIsStruct(cuboid.max.x, y, z))
                            newBorderCells.Add(new Vector3Int(cuboid.max.x, y, z));
                        else
                            return new HashSet<Vector3Int>();
                    }
                }

                return newBorderCells;
            }

            return new HashSet<Vector3Int>();
        }

        private static HashSet<Vector3Int> NewBorderCellsXNeg(Cuboid cuboid)
        {
            HashSet<Vector3Int> newBorderCells = new HashSet<Vector3Int>();

            if (cuboid.min.x - 1 > 0)
            {
                for (int y = cuboid.min.y; y < cuboid.max.y; y++)
                {
                    for (int z = cuboid.min.z; z < cuboid.max.z; z++)
                    {
                        if (!CellIsStruct(cuboid.min.x - 1, y, z))
                            newBorderCells.Add(new Vector3Int(cuboid.min.x - 1, y, z));
                        else
                            return new HashSet<Vector3Int>();
                    }
                }

                return newBorderCells;
            }

            return new HashSet<Vector3Int>();
        }

        private static HashSet<Vector3Int> NewBorderCellsZPos(Cuboid cuboid)
        {
            HashSet<Vector3Int> newBorderCells = new HashSet<Vector3Int>();

            if (cuboid.max.z < size.z)
            {
                for (int x = cuboid.min.x; x < cuboid.max.x; x++)
                {
                    for (int y = cuboid.min.y; y < cuboid.max.y; y++)
                    {
                        if (!CellIsStruct(x, y, cuboid.max.z))
                            newBorderCells.Add(new Vector3Int(x, y, cuboid.max.z));
                        else
                            return new HashSet<Vector3Int>();
                    }
                }

                return newBorderCells;
            }

            return new HashSet<Vector3Int>();
        }

        private static HashSet<Vector3Int> NewBorderCellsZNeg(Cuboid cuboid)
        {
            HashSet<Vector3Int> newBorderCells = new HashSet<Vector3Int>();

            if (cuboid.min.z - 1 > 0)
            {
                for (int x = cuboid.min.x; x < cuboid.max.x; x++)
                {
                    for (int y = cuboid.min.y; y < cuboid.max.y; y++)
                    {
                        if (!CellIsStruct(x,y, cuboid.min.z - 1))
                            newBorderCells.Add(new Vector3Int(x, y, cuboid.min.z - 1));
                        else
                            return new HashSet<Vector3Int>();
                    }
                }

                return newBorderCells;
            }

            return new HashSet<Vector3Int>();
        }

        private static WalkableArea GetWalkableArea(Vector3Int input)
        {
            WalkableArea newWalkableArea = new WalkableArea();
            newWalkableArea.neighborsArea = new HashSet<WalkableArea>();
            newWalkableArea.neighborsPaths = new HashSet<Path>();
            newWalkableArea.yPos = input.y;

            Stack<Vector3Int> openSet = new Stack<Vector3Int>();
            HashSet<Vector3Int> cells = new HashSet<Vector3Int>();
            HashSet<Vector3Int> path = new HashSet<Vector3Int>();

            openSet.Push(input);
            if (CellIsPath(input.x, input.y, input.z))
                path.Add(input);

            while (openSet.Count > 0)
            {
                Vector3Int currentCell = openSet.Pop();
                cells.Add(currentCell);
                int x = currentCell.x; int y = currentCell.y; int z = currentCell.z;

                if (x - 1 > 0)
                {
                    if(CellIsWalkable(x - 1, y, z) && !cells.Contains(new Vector3Int(x - 1, y, z)))
                        openSet.Push(new Vector3Int(x - 1, y, z));
                    if (CellIsPath(x - 1, y, z))
                        path.Add(new Vector3Int(x - 1, y, z));
                }

                if (x + 1 < size.x)
                {
                    if (CellIsWalkable(x + 1, y, z) && !cells.Contains(new Vector3Int(x + 1, y, z)))
                        openSet.Push(new Vector3Int(x + 1, y, z));
                    if (CellIsPath(x + 1, y, z))
                        path.Add(new Vector3Int(x + 1, y, z));
                }

                if (z - 1 > 0)
                {
                    if (CellIsWalkable(x, y, z - 1) && !cells.Contains(new Vector3Int(x, y, z - 1)))
                        openSet.Push(new Vector3Int(x, y, z - 1));
                    if (CellIsPath(x, y, z - 1))
                        path.Add(new Vector3Int(x, y, z - 1));
                }

                if (z + 1 < size.z)
                {
                    if (CellIsWalkable(x, y, z + 1) && !cells.Contains(new Vector3Int(x, y, z + 1)))
                        openSet.Push(new Vector3Int(x, y, z + 1));
                    if (CellIsPath(x, y, z + 1))
                        path.Add(new Vector3Int(x, y, z + 1));
                }

            }

            newWalkableArea.cells = cells;
            newWalkableArea.paths = path;

            return newWalkableArea;
        }

        private static Path GetPath(Vector3Int input)
        {
            Path newPath = new Path();
            newPath.type = Genes[input.x][input.y][input.z];
            newPath.neighbors = new HashSet<WalkableArea>();
            newPath.neighborsConnected = new HashSet<WalkableArea>();
            newPath.cells = new HashSet<Vector3Int>();
            newPath.cells.Add(input);

            if (typeParams[Genes[input.x][input.y][input.z]].stair)
                newPath = GetStairPath(input, newPath);

            if (typeParams[Genes[input.x][input.y][input.z]].ladder)
                newPath = GetLadderPath(input, newPath);

            return newPath;
        }

        private static Path GetStairPath(Vector3Int input, Path newPath)
        {
            HashSet<Vector3Int> stairX = MutationsStairs.GetStairX(Genes, input);
            HashSet<Vector3Int> stairZ = MutationsStairs.GetStairZ(Genes, input);

            if (stairX.Count > stairZ.Count)
                newPath.cells = stairX;
            else
                newPath.cells = stairZ;

            return newPath;
        }

        private static Path GetLadderPath(Vector3Int input, Path newPath)
        {
            Stack<Vector3Int> openSet = new Stack<Vector3Int>();
            HashSet<Vector3Int> cells = new HashSet<Vector3Int>();

            openSet.Push(input);

            while (openSet.Count > 0)
            {
                Vector3Int currentCell = openSet.Pop();
                cells.Add(currentCell);
                int x = currentCell.x; int y = currentCell.y; int z = currentCell.z;

                if (y - 1 > 0)
                {
                    if (Genes[x][y - 1][z] == newPath.type && CellIsPath(x, y - 1, z) && !cells.Contains(new Vector3Int(x, y - 1, z)))
                        openSet.Push(new Vector3Int(x, y - 1, z));
                }

                if (y + 1 < size.y)
                {
                    if (Genes[x][y + 1][z] == newPath.type && CellIsPath(x, y + 1, z) && !cells.Contains(new Vector3Int(x, y + 1, z)))
                        openSet.Push(new Vector3Int(x, y + 1, z));
                }

            }

            newPath.cells = cells;

            return newPath;
        }

        private static bool CellIsStruct(int x, int y, int z)
        {
            if (typeParams[Genes[x][y][z]].wall || typeParams[Genes[x][y][z]].floor)
                return true;
            else
                return false;
        }

        private static bool CellIsWalkable(int x, int y, int z)
        {
            if (!CellIsStruct(x, y, z) && typeParams[Genes[x][y - 1][z]].floor)
                return true;
            else
                return false;
        }

        private static bool CellIsPath(int x, int y, int z)
        {
            if ((Genes[x][y][z] == 0 && typeParams[Genes[x][y - 1][z]].stair)
              || (Genes[x][y + 1][z] == 0 && typeParams[Genes[x][y][z]].stair)
              || (typeParams[Genes[x][y][z]].ladder || typeParams[Genes[x][y - 1][z]].ladder)
              || (typeParams[Genes[x][y][z]].door && Genes[x][y - 1][z] > 0 && !typeParams[Genes[x][y - 1][z]].door))
                return true;
            else
                return false;
        }
    }
}


