using System.Collections.Generic;
using System.Linq;
using System;
using UtilitiesGenetic;
using mVectors;

namespace MapTileGridCreator.Core
{
    public static class IA
    {
        public static List<WaypointParams[][][]> GetSuggestionsClusters(int sizeX, int sizeY, int sizeZ, TypeParams[] cellsInfos, WaypointParams[][][] waypointParams, int nbSuggestions, EvolutionaryAlgoParams algoParams)
        {   
            List<WaypointParams[][][]> newWaypointsParams = new List<WaypointParams[][][]>();
            Vector3Int size = new Vector3Int(sizeX, sizeY, sizeZ);

            for (int i = 0; i < nbSuggestions; i++)
            {
                TestGenetics newGenetic = new TestGenetics();
                newGenetic.StartGenetics(size, cellsInfos, waypointParams, algoParams, i);
                
               for (int j = 0; j < algoParams.generations; j++)
               {
                   newGenetic.UpdateGenetics();
               }

                newWaypointsParams.Add(newGenetic.GetBestClusters());
            }

            return newWaypointsParams;
        }

        public static Phenotype GetPhenotype(int sizeX, int sizeY, int sizeZ, WaypointParams[][][] Genes, TypeParams[] typeParams)
        {
            Vector3Int size = new Vector3Int(sizeX, sizeY, sizeZ);
            Vector3Int max = new Vector3Int(sizeX, 1, sizeZ);
            Vector3Int min = new Vector3Int(1, 1, 1);
            HashSet<Vector3Int> cellsPicked = new HashSet<Vector3Int>(); 
            Phenotype newPhenotype = new Phenotype();
            newPhenotype.cuboids = new HashSet<Cuboid>();

            
            for (int x = 1; x < sizeX; x++)
            {
                for (int y = 1; y < sizeY; y++)
                {
                    for (int z = 1; z < sizeZ; z++)
                    {
                        if (typeParams[Genes[x][y][z].type].floor || typeParams[Genes[x][y][z].type].wall)
                        {
                            if (max.y < y + 1)
                                max.y = y + 1;
                        }
                    }
                }
            }

            for (int x = min.x; x < max.x; x++)
            {
                for (int y = min.y; y < max.y; y++)
                {
                    for (int z = min.z; z < max.z; z++)
                    {
                        Vector3Int input = new Vector3Int(x, y, z);

                        if (!typeParams[Genes[x][y][z].type].floor && !typeParams[Genes[x][y][z].type].wall && !cellsPicked.Contains(input))
                        {
                            Cuboid newCuboid = new Cuboid();
                            newCuboid.cells = GetCuboid(max, Genes, input, typeParams, ref cellsPicked);
                            newPhenotype.cuboids.Add(newCuboid);
                        }
                    }
                }
            }

            return newPhenotype;
        }

        public static HashSet<Vector3Int> GetCuboid(Vector3Int size, WaypointParams[][][] Genes, Vector3Int input, TypeParams[] typeParams, ref HashSet<Vector3Int> cellsPicked)
        {
            Vector3Int max = new Vector3Int(size.x, size.y, size.z);
            Vector3Int min = new Vector3Int(0, 0, 0);
            HashSet<Vector3Int> cellsCuboid = new HashSet<Vector3Int>();

            for (int x = input.x; x < max.x; x++)
            {
                for (int y = input.y; y < max.y; y++)
                {
                    for (int z = input.z; z < max.z; z++)
                    {
                        if (CellIsStruct(x, y, z, Genes, typeParams))
                            max.z = z;

                        if (z < max.z && CellIsStruct(x, y + 1, z, Genes, typeParams))
                            max.y = y + 1;

                        if (z < max.z && y < max.y && CellIsStruct(x + 1, y, z, Genes, typeParams))
                            max.x = x + 1;
                    }
                }
            }

            for (int i = input.x; i < max.x; i++)
            {
                for (int j = input.y; j < max.y; j++)
                {
                    for (int k = input.z; k < max.z; k++)
                    {
                        cellsCuboid.Add(new Vector3Int(i, j, k));
                        cellsPicked.Add(new Vector3Int(i, j, k));
                    }
                }
            }

            return cellsCuboid;
        }

        public static bool CellIsStruct(int x, int y, int z, WaypointParams[][][] Genes, TypeParams[] typeParams)
        {
            if (typeParams[Genes[x][y][z].type].wall || typeParams[Genes[x][y][z].type].floor)
                return true;
            else
                return false;
        }

        public static bool HasNeighborsX(int x, int y, int z, WaypointParams[][][] Genes, TypeParams[] typeParams)
        {
            if (typeParams[Genes[x][y - 1][z].type].wall || typeParams[Genes[x][y - 1][z].type].floor
             || typeParams[Genes[x][y + 1][z].type].wall || typeParams[Genes[x][y + 1][z].type].floor
             || typeParams[Genes[x][y][z - 1].type].wall || typeParams[Genes[x][y][z - 1].type].floor
             || typeParams[Genes[x][y][z + 1].type].wall || typeParams[Genes[x][y][z + 1].type].floor)
                return true;
            else
                return false;
        }

        public static int NewLimitX(int x, int y, int z, WaypointParams[][][] Genes, TypeParams[] typeParams, int limit, int type)
        {
            if (type == Constants.MAX)
            {
                while (x < limit)
                {
                    if (CellIsStruct(x, y, z, Genes, typeParams) || HasNeighborsX(x, y, z, Genes, typeParams) != HasNeighborsX(x - 1, y, z, Genes, typeParams))
                    {
                        if (x < limit)
                            limit = x;

                        break;
                    }

                    x++;
                }
            }
            else
            {
                while (x > limit)
                {
                    if (CellIsStruct(x, y, z, Genes, typeParams) || HasNeighborsX(x, y, z, Genes, typeParams) != HasNeighborsX(x + 1, y, z, Genes, typeParams))
                    {
                        if (x < limit)
                            limit = x;

                        break;
                    }

                    x--;
                }
            }

            return limit;
        }

        public static int NewLimitY(int x, int y, int z, WaypointParams[][][] Genes, TypeParams[] typeParams, int limit, int type)
        {
            if (type == Constants.MAX)
            {
                while (y < limit)
                {
                    if (CellIsStruct(x, y, z, Genes, typeParams) || HasNeighborsX(x, y, z, Genes, typeParams) != HasNeighborsX(x, y - 1, z, Genes, typeParams))
                    {
                        if (y < limit)
                            limit = y;

                        break;
                    }

                    y++;
                }
            }
            else
            {
                while (y > limit)
                {
                    if (CellIsStruct(x, y, z, Genes, typeParams) || HasNeighborsX(x, y, z, Genes, typeParams) != HasNeighborsX(x, y + 1, z, Genes, typeParams))
                    {
                        if (y < limit)
                            limit = y;

                        break;
                    }

                    y--;
                }
            }

            return limit;
        }

        public static int NewLimitZ(int x, int y, int z, WaypointParams[][][] Genes, TypeParams[] typeParams, int limit, int type)
        {
            if (type == Constants.MAX)
            {
                while (z < limit)
                {
                    if (CellIsStruct(x, y, z, Genes, typeParams) || HasNeighborsX(x, y, z, Genes, typeParams) != HasNeighborsX(x, y, z + 1, Genes, typeParams))
                    {
                        if (z < limit)
                            limit = z;

                        break;
                    }

                    z++;
                }
            }
            else
            {
                while (z > limit)
                {
                    if (CellIsStruct(x, y, z, Genes, typeParams) || HasNeighborsX(x, y, z, Genes, typeParams) != HasNeighborsX(x, y, z + 1, Genes, typeParams))
                    {
                        if (z < limit)
                            limit = z;

                        break;
                    }

                    z--;
                }
            }

            return limit;
        }

        public static WaypointParams[][][] TranslateDoor(Vector3Int size, WaypointParams[][][] Genes, Vector3Int input, TypeParams[] typeParams, SharpNeatLib.Maths.FastRandom randomFast)
        {
            if(typeParams[Genes[input.x][input.y][input.z].type].door)
            {
                int maxX = 0;
                int minX = 0;
                int maxZ = 0;
                int minZ = 0;

                if (input.x + 2 < size.x && typeParams[Genes[input.x + 1][input.y][input.z].type].wall && typeParams[Genes[input.x + 2][input.y][input.z].type].wall)
                    maxX = 1;
                if (input.x - 2 > 0 && typeParams[Genes[input.x - 1][input.y][input.z].type].wall && typeParams[Genes[input.x - 2][input.y][input.z].type].wall)
                    minX = -1;
                if (input.z + 2 < size.z && typeParams[Genes[input.x][input.y][input.z + 1].type].wall && typeParams[Genes[input.x][input.y][input.z].type + 2].wall)
                    maxZ = 1;
                if (input.z - 2 > 0 && typeParams[Genes[input.x][input.y][input.z - 1].type].wall && typeParams[Genes[input.x][input.y][input.z - 2].type].wall)
                    minZ = -1;

                Vector2Int translation = new Vector2Int(0, 0);
                if (minX != 0 || maxX != 0 || minZ != 0 || maxZ != 0)
                {
                    while (translation.x == 0 && translation.y == 0)
                    {
                        translation.x = randomFast.Next(minX, maxX + 1);
                        translation.y = randomFast.Next(minZ, maxZ + 1);
                    }

                    int temp;
                    temp = Genes[input.x][input.y][input.z].type;
                    Genes[input.x][input.y][input.z].type = Genes[input.x + translation.x][input.y][input.z + translation.y].type;
                    Genes[input.x + translation.x][input.y][input.z + translation.y].type = temp;

                    if (typeParams[Genes[input.x][input.y - 1][input.z].type].door)
                    {
                        temp = Genes[input.x][input.y - 1][input.z].type;
                        Genes[input.x][input.y - 1][input.z].type = Genes[input.x + translation.x][input.y - 1][input.z + translation.y].type;
                        Genes[input.x + translation.x][input.y - 1][input.z + translation.y].type = temp;
                    }

                    if (typeParams[Genes[input.x][input.y + 1][input.z].type].door)
                    {
                        temp = Genes[input.x][input.y + 1][input.z].type;
                        Genes[input.x][input.y + 1][input.z].type = Genes[input.x + translation.x][input.y + 1][input.z + translation.y].type;
                        Genes[input.x + translation.x][input.y + 1][input.z + translation.y].type = temp;
                    }
                }
            }

            return Genes;
        }

        public static WaypointParams[][][] CreateDoor(Vector3Int size, WaypointParams[][][] Genes, Vector3Int input, TypeParams[] typeParams, SharpNeatLib.Maths.FastRandom randomFast, int newType)
        {
            if (typeParams[Genes[input.x][input.y][input.z].type].wall && typeParams[Genes[input.x][input.y + 1][input.z].type].wall)
            {
                    Genes[input.x][input.y][input.z].type = newType;
                    Genes[input.x][input.y + 1][input.z].type = newType;
            }

            return Genes;
        }

        public static WaypointParams[][][] CollapseDoor(Vector3Int size, WaypointParams[][][] Genes, Vector3Int input, TypeParams[] typeParams, SharpNeatLib.Maths.FastRandom randomFast)
        {
            if (typeParams[Genes[input.x][input.y][input.z].type].door && typeParams[Genes[input.x][input.y + 1][input.z].type].door)
            {
                Dictionary<int, int> typesAround = new Dictionary<int, int>();

                for (int i = -1; i < 2; i++)
                {
                    for (int j = -1; j < 3; j++)
                    {
                        for (int k = -1; k < 2; k++)
                        {
                            if (input.y + j < size.y && typeParams[Genes[input.x + i][input.y + j][input.z + k].type].wall)
                            {
                                if (typesAround.ContainsKey(Genes[input.x + i][input.y + j][input.z + k].type))
                                    typesAround[Genes[input.x + i][input.y + j][input.z + k].type]++;
                                else
                                {
                                    typesAround[Genes[input.x + i][input.y + j][input.z + k].type] = 1;
                                }
                            }
                        }
                    }
                }

                int mostTypeAround = typesAround.ElementAt(0).Key;

                foreach (KeyValuePair<int, int> type in typesAround)
                {
                    if (type.Value > typesAround[mostTypeAround])
                        mostTypeAround = type.Key;
                }

                if (typeParams[mostTypeAround].wall && !typeParams[mostTypeAround].door)
                {
                    Genes[input.x][input.y][input.z].type = mostTypeAround;
                    Genes[input.x][input.y + 1][input.z].type = mostTypeAround;
                }
            }

            return Genes;
        }

        public static WaypointParams[][][] FillFloor(Vector3Int size, WaypointParams[][][] Genes, Vector3Int input, int newType, TypeParams[] typeParams)
        {
            if (Genes[input.x][input.y][input.z].type > 0 && !typeParams[Genes[input.x][input.y][input.z].type].floor)
                return Genes;

            int y = input.y;
            int targetType = Genes[input.x][input.y][input.z].type;

            Stack<Vector3Int> floor = new Stack<Vector3Int>();
            HashSet<Vector3Int> newFloor = new HashSet<Vector3Int>();

            floor.Push(input);
            while (floor.Count != 0)
            {
                Vector3Int temp = floor.Pop();
                int z1 = temp.z;
                while (z1 >= 1 && (Genes[temp.x][y][z1].type == 0 || typeParams[Genes[temp.x][y][z1].type].floor))
                {
                    z1--;
                }
                z1++;

                bool spanLeft = false;
                bool spanRight = false;

                while (z1 < size.z && (Genes[temp.x][y][z1].type == 0 || typeParams[Genes[temp.x][y][z1].type].floor))
                {
                    Genes[temp.x][y][z1].type = newType;
                    newFloor.Add(new Vector3Int(temp.x, y, z1));

                    if (!spanLeft && temp.x > 1 && (Genes[temp.x-1][y][z1].type == 0 || typeParams[Genes[temp.x - 1][y][z1].type].floor) && !newFloor.Contains(new Vector3Int(temp.x - 1, y, z1)))
                    {
                        floor.Push(new Vector3Int(temp.x - 1,y, z1));
                        spanLeft = true;
                    }
                    else if (spanLeft && temp.x - 1 == 1 && Genes[temp.x - 1][y][z1].type > 0 && !typeParams[Genes[temp.x - 1][y][z1].type].floor)
                    {
                        spanLeft = false;
                    }
                    if (!spanRight && temp.x < size.x - 1 && (Genes[temp.x + 1][y][z1].type == 0 || typeParams[Genes[temp.x + 1][y][z1].type].floor) && !newFloor.Contains(new Vector3Int(temp.x + 1, y, z1)))
                    {
                        floor.Push(new Vector3Int(temp.x + 1, y, z1));
                        spanRight = true;
                    }
                    else if (spanRight && temp.x < size.x - 1 && Genes[temp.x + 1][y][z1].type > 0 && !typeParams[Genes[temp.x + 1][y][z1].type].floor)
                    {
                        spanRight = false;
                    }
                    z1++;
                }
            }

            return Genes;
        }

        public static WaypointParams[][][] FillWallX(Vector3Int size, WaypointParams[][][] Genes, Vector3Int input, int newType, TypeParams[] typeParams)
        {
            if (Genes[input.x][input.y][input.z].type > 0 && !typeParams[Genes[input.x][input.y][input.z].type].floor)
                return Genes;

            int z = input.z;

            Stack<Vector3Int> wall = new Stack<Vector3Int>();

            wall.Push(input);
            while (wall.Count != 0)
            {
                Vector3Int temp = wall.Pop();
                int y1 = temp.y;
                while (y1 >= 1 && !WallLimit(temp.x, y1, z, Genes, typeParams))
                {
                    y1--;
                }
                y1++;

                bool spanLeft = false;
                bool spanRight = false;

                while (y1 < size.y && !WallLimit(temp.x, y1, z, Genes, typeParams))
                {
                    Genes[temp.x][y1][z].type = newType;

                    if (!spanLeft && temp.x > 1 && !WallLimit(temp.x - 1, y1, z, Genes, typeParams))
                    {
                        wall.Push(new Vector3Int(temp.x - 1, y1, z));
                        spanLeft = true;
                    }
                    else if (spanLeft && temp.x - 1 == 1 && WallLimit(temp.x - 1, y1, z, Genes, typeParams))
                    {
                        spanLeft = false;
                    }
                    if (!spanRight && temp.x < size.x - 1 && !WallLimit(temp.x + 1, y1, z, Genes, typeParams))
                    {
                        wall.Push(new Vector3Int(temp.x + 1, y1, z));
                        spanRight = true;
                    }
                    else if (spanRight && temp.x < size.x - 1 && WallLimit(temp.x, y1, z, Genes, typeParams))
                    {
                        spanRight = false;
                    }
                    y1++;
                }
            }

            return Genes;
        }

        public static WaypointParams[][][] FillWallZ(Vector3Int size, WaypointParams[][][] Genes, Vector3Int input, int newType, TypeParams[] typeParams)
        {
            if (Genes[input.x][input.y][input.z].type > 0 && !typeParams[Genes[input.x][input.y][input.z].type].floor)
                return Genes;

            int x = input.x;

            Stack<Vector3Int> wall = new Stack<Vector3Int>();

            wall.Push(input);
            while (wall.Count != 0)
            {
                Vector3Int temp = wall.Pop();
                int y1 = temp.y;
                while (y1 >= 1 && !WallLimit(x, y1, temp.z, Genes, typeParams))
                {
                    y1--;
                }
                y1++;

                bool spanLeft = false;
                bool spanRight = false;

                while (y1 < size.y && !WallLimit(x, y1, temp.z, Genes, typeParams))
                {
                    Genes[x][y1][temp.z].type = newType;

                    if (!spanLeft && temp.z > 1 && !WallLimit(x, y1, temp.z - 1, Genes, typeParams))
                    {
                        wall.Push(new Vector3Int(x, y1, temp.z - 1));
                        spanLeft = true;
                    }
                    else if (spanLeft && temp.z - 1 == 1 && WallLimit(x, y1, temp.z - 1, Genes, typeParams))
                    {
                        spanLeft = false;
                    }
                    if (!spanRight && temp.z < size.z - 1 && !WallLimit(x, y1, temp.z + 1, Genes, typeParams))
                    {
                        wall.Push(new Vector3Int(x, y1, temp.z + 1));
                        spanRight = true;
                    }
                    else if (spanRight && temp.x < size.x - 1 && WallLimit(x, y1, temp.z + 1, Genes, typeParams))
                    {
                        spanRight = false;
                    }
                    y1++;
                }
            }

            return Genes;
        }

        private static bool WallLimit(int x, int y, int z, WaypointParams[][][] Genes, TypeParams[] typeParams)
        {
            if (Genes[x][y][z].type == 0 || (typeParams[Genes[x][y][z].type].floor && (y - 2 < 0 || typeParams[Genes[x][y - 2][z].type].wall)))
                return false;
            else
                return true;
        }

        public static WaypointParams[][][] DeleteWallX(Vector3Int size, WaypointParams[][][] Genes, Vector3Int input, TypeParams[] typeParams)
        {
            if (!typeParams[Genes[input.x][input.y][input.z].type].wall || (!typeParams[Genes[input.x - 1][input.y][input.z].type].wall
                && !typeParams[Genes[input.x + 1][input.y][input.z].type].wall))
                return Genes;

            int z = input.z;

            Stack<Vector3Int> wall = new Stack<Vector3Int>();

            wall.Push(input);
            while (wall.Count != 0)
            {
                Vector3Int temp = wall.Pop();
                int y1 = temp.y;
                while (y1 >= 1 && typeParams[Genes[temp.x][y1][z].type].wall)
                {
                    y1--;
                }
                y1++;

                bool spanLeft = false;
                bool spanRight = false;
                int Xmin = 1; 
                int Xmax = size.x - 1;

                while (y1 < size.y && typeParams[Genes[temp.x][y1][z].type].wall && !typeParams[Genes[temp.x][y1][z - 1].type].wall && !typeParams[Genes[temp.x][y1][z + 1].type].wall)
                {
                    Genes[temp.x][y1][z].type = 0;

                    if (!spanLeft && temp.x > Xmin && typeParams[Genes[temp.x - 1][y1][z].type].wall)
                    {
                        if (!typeParams[Genes[temp.x - 1][y1][z - 1].type].wall && !typeParams[Genes[temp.x - 1][y1][z + 1].type].wall)
                            wall.Push(new Vector3Int(temp.x - 1, y1, z));
                        else
                            Xmin = temp.x; 

                        spanLeft = true;
                    }
                    else if (spanLeft && temp.x - 1 == Xmin && !typeParams[Genes[temp.x - 1][y1][z].type].wall)
                    {
                        spanLeft = false;
                    }
                    if (!spanRight && temp.x < Xmax && typeParams[Genes[temp.x + 1][y1][z].type].wall)
                    {
                        if (!typeParams[Genes[temp.x + 1][y1][z - 1].type].wall && !typeParams[Genes[temp.x + 1][y1][z + 1].type].wall)
                            wall.Push(new Vector3Int(temp.x + 1, y1, z));
                        else
                            Xmax = temp.z + 1; 

                        spanRight = true;
                    }
                    else if (spanRight && temp.x < Xmax && !typeParams[Genes[temp.x + 1][y1][z].type].wall)
                    {
                        spanRight = false;
                    }
                    y1++;
                }
            }

            return Genes;
        }

        public static WaypointParams[][][] DeleteWallZ(Vector3Int size, WaypointParams[][][] Genes, Vector3Int input, TypeParams[] typeParams)
        {
            if (!typeParams[Genes[input.x][input.y][input.z].type].wall || (!typeParams[Genes[input.x][input.y][input.z - 1].type].wall
                && !typeParams[Genes[input.x][input.y][input.z + 1].type].wall))
                return Genes;

            int x = input.x;

            Stack<Vector3Int> wall = new Stack<Vector3Int>();

            wall.Push(input);
            while (wall.Count != 0)
            {
                Vector3Int temp = wall.Pop();
                int y1 = temp.y;
                while (y1 >= 1 && typeParams[Genes[x][y1][temp.z].type].wall)
                {
                    y1--;
                }
                y1++;

                bool spanLeft = false;
                bool spanRight = false;
                int minZ = 1;
                int maxZ = size.z - 1; 

                while (y1 < size.y && typeParams[Genes[x][y1][temp.z].type].wall && !typeParams[Genes[x - 1][y1][temp.z].type].wall && !typeParams[Genes[x + 1][y1][temp.z].type].wall)
                {
                    Genes[x][y1][temp.z].type = 0;

                    if (!spanLeft && temp.z > minZ && typeParams[Genes[x][y1][temp.z - 1].type].wall)
                    {
                        if (!typeParams[Genes[x - 1][y1][temp.z - 1].type].wall && !typeParams[Genes[x + 1][y1][temp.z - 1].type].wall)
                            wall.Push(new Vector3Int(x, y1, temp.z - 1));
                        else
                            minZ = temp.z; 

                        spanLeft = true;
                    }
                    else if (spanLeft && temp.z - 1 == minZ && !typeParams[Genes[x][y1][temp.z - 1].type].wall)
                    {
                        spanLeft = false;
                    }
                    if (!spanRight && temp.z < maxZ && typeParams[Genes[x][y1][temp.z + 1].type].wall)
                    {
                        if (!typeParams[Genes[x - 1][y1][temp.z + 1].type].wall && !typeParams[Genes[x + 1][y1][temp.z + 1].type].wall)
                            wall.Push(new Vector3Int(x, y1, temp.z + 1));
                        else
                            maxZ = temp.z + 1; 

                        spanRight = true;
                    }
                    else if (spanRight && temp.z < maxZ && !typeParams[Genes[x][y1][temp.z + 1].type].wall)
                    {
                        spanRight = false;
                    }
                    y1++;
                }
            }

            return Genes;
        }

        public static WaypointParams[][][] TranslationWall(Vector3Int size, WaypointParams[][][] Genes, Vector3Int input, TypeParams[] typeParams, SharpNeatLib.Maths.FastRandom randomFast)
        {
            int x = input.x; int y = input.y; int z = input.z;

            if (typeParams[Genes[x][y][z].type].wall)
            {
                HashSet<Vector3Int> wall = new HashSet<Vector3Int>(); 

                if (typeParams[Genes[x][y][z - 1].type].wall || typeParams[Genes[x][y][z + 1].type].wall
                && (!typeParams[Genes[x - 1][y][z].type].wall && !typeParams[Genes[x + 1][y][z].type].wall))
                {
                    wall = TakeWallZ(size, Genes, input, typeParams);
                    Genes = TranslateWallZ(size, input, wall, Genes, randomFast, typeParams); 
                }
                if (typeParams[Genes[x - 1][y][z].type].wall || typeParams[Genes[x + 1][y][z].type].wall
                    && (!typeParams[Genes[x][y][z - 1].type].wall && !typeParams[Genes[x][y][z + 1].type].wall))
                {
                    wall = TakeWallX(size, Genes, input, typeParams);
                    Genes = TranslateWallX(size, input, wall, Genes, randomFast, typeParams);
                }
                else if ((typeParams[Genes[x - 1][y][z].type].wall || typeParams[Genes[x + 1][y][z].type].wall)
                    && (typeParams[Genes[x][y][z - 1].type].wall || typeParams[Genes[x][y][z + 1].type].wall))
                {
                    if (randomFast.Next(2) > 1 ? true : false)
                    {
                        wall = TakeWallZ(size, Genes, input, typeParams);
                        Genes = TranslateWallZ(size, input, wall, Genes, randomFast, typeParams);
                    }
                    else
                    {
                        wall = TakeWallX(size, Genes, input, typeParams);
                        Genes = TranslateWallX(size, input, wall, Genes, randomFast, typeParams);
                    }
                }
            }

            return Genes;
        }

        public static WaypointParams[][][] RotationWall(Vector3Int size, WaypointParams[][][] Genes, Vector3Int input, TypeParams[] typeParams, SharpNeatLib.Maths.FastRandom randomFast)
        {
            int x = input.x; int y = input.y; int z = input.z;

            if (typeParams[Genes[x][y][z].type].wall)
            {
                HashSet<Vector3Int> wall = new HashSet<Vector3Int>();

                if (typeParams[Genes[x][y][z - 1].type].wall || typeParams[Genes[x][y][z + 1].type].wall
                && (!typeParams[Genes[x - 1][y][z].type].wall && !typeParams[Genes[x + 1][y][z].type].wall))
                {
                    wall = TakeWallZ(size, Genes, input, typeParams);
                    Genes = RotateWallZ(size, input, wall, Genes, randomFast, typeParams);
                }
                if (typeParams[Genes[x - 1][y][z].type].wall || typeParams[Genes[x + 1][y][z].type].wall
                    && (!typeParams[Genes[x][y][z - 1].type].wall && !typeParams[Genes[x][y][z + 1].type].wall))
                {
                    wall = TakeWallX(size, Genes, input, typeParams);
                    Genes = RotateWallX(size, input, wall, Genes, randomFast, typeParams);
                }
                else if ((typeParams[Genes[x - 1][y][z].type].wall || typeParams[Genes[x + 1][y][z].type].wall)
                    && (typeParams[Genes[x][y][z - 1].type].wall || typeParams[Genes[x][y][z + 1].type].wall))
                {
                    if (randomFast.Next(2) > 1 ? true : false)
                    {
                        wall = TakeWallZ(size, Genes, input, typeParams);
                        Genes = RotateWallZ(size, input, wall, Genes, randomFast, typeParams);
                    }
                    else
                    {
                        wall = TakeWallX(size, Genes, input, typeParams);
                        Genes = RotateWallX(size, input, wall, Genes, randomFast, typeParams);
                    }
                }
            }

            return Genes;
        }

        public static HashSet<Vector3Int> TakeWallX(Vector3Int size, WaypointParams[][][] Genes, Vector3Int input, TypeParams[] typeParams)
        {
            HashSet<Vector3Int> wall = new HashSet<Vector3Int>();
            HashSet<Vector3Int> cellsChecked = new HashSet<Vector3Int>();
            int x = input.x; int y = input.y; int z = input.z;
            int minX = 0;
            int maxX = size.x;

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
                    cellsChecked.Add(new Vector3Int(temp.x, y1, z));
                    
                    if (typeParams[Genes[temp.x][y1][z - 1].type].wall && typeParams[Genes[temp.x][y1][z + 1].type].wall)
                    {
                        if (temp.x >= input.x && temp.x < maxX)
                            maxX = temp.x;

                        if (temp.x <= input.x && temp.x > minX)
                            minX = temp.x;
                    }
                    
                    if (!spanLeft && temp.x > 1 && typeParams[Genes[temp.x-1][y1][z].type].wall && !cellsChecked.Contains(new Vector3Int(temp.x - 1, y1, z)))
                    {
                        cells.Push(new Vector3Int(temp.x - 1, y1, z));
                        spanLeft = true;
                    }
                    else if (spanLeft && temp.x - 1 == 1 && !typeParams[Genes[temp.x - 1][y1][z].type].wall)
                    {
                        spanLeft = false;
                    }
                    if (!spanRight && temp.x < size.x - 1 && typeParams[Genes[temp.x + 1][y1][z].type].wall && !cellsChecked.Contains(new Vector3Int(temp.x + 1, y1, z)))
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

            foreach (Vector3Int index in cellsChecked)
            {
                if (index.x > minX && index.x < maxX)
                    wall.Add(index);
            }
            return wall;
        }

        public static HashSet<Vector3Int> TakeWallZ(Vector3Int size, WaypointParams[][][] Genes, Vector3Int input, TypeParams[] typeParams)
        {
            HashSet<Vector3Int> wall = new HashSet<Vector3Int>();
            HashSet<Vector3Int> cellsChecked = new HashSet<Vector3Int>();
            int x = input.x; int y = input.y; int z = input.z;
            int minZ = 0;
            int maxZ = size.z;

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
                    cellsChecked.Add(new Vector3Int(x, y1, temp.z));

                    if (typeParams[Genes[x - 1][y1][temp.z].type].wall && typeParams[Genes[x + 1][y1][temp.z].type].wall)
                    {
                        if (temp.z >= input.z && temp.z < maxZ)
                            maxZ = temp.z;

                        if (temp.z <= input.z && temp.z > minZ)
                            minZ = temp.z;
                    }

                    if (!spanLeft && temp.z > 1 && typeParams[Genes[x][y1][temp.z - 1].type].wall && !cellsChecked.Contains(new Vector3Int(x, y1, temp.z - 1)))
                    {
                        cells.Push(new Vector3Int(x, y1, temp.z - 1));
                        spanLeft = true;
                    }
                    else if (spanLeft && temp.z - 1 == 1 && !typeParams[Genes[x][y1][temp.z - 1].type].wall)
                    {
                        spanLeft = false;
                    }
                    if (!spanRight && temp.z < size.z - 1 && typeParams[Genes[x][y1][temp.z + 1].type].wall && !cellsChecked.Contains(new Vector3Int(x, y1, temp.z + 1)))
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

            foreach (Vector3Int index in cellsChecked)
            {
                if (index.z > minZ && index.z < maxZ)
                    wall.Add(index);
            }
            return wall;
        }

        public static WaypointParams[][][] TranslateWallX(Vector3Int size, Vector3Int input, HashSet<Vector3Int> wall, WaypointParams[][][] Genes, SharpNeatLib.Maths.FastRandom randomFast, TypeParams[] typeParams)
        {
            /*int upperTranslationLimit = size.z - input.z;
            int lowerTranslationLimit = input.z - 1;
            int translation = 0;

            while (translation == 0)
                translation = randomFast.Next(-lowerTranslationLimit, upperTranslationLimit);*/

            int translation = 0;

            if (input.z > 1 && input.z < size.z - 1)
            {
                translation = randomFast.Next(2) > 0 ? -1 : 1;
            }
            else if (input.z == 1)
            {
                translation = 1;
            }
            else if (input.z == size.z - 1)
            {
                translation = -1;
            }

            int temp; 

            foreach (Vector3Int index in wall)
            {
                temp = Genes[index.x][index.y][index.z].type;

                /*if (!typeParams[Genes[index.x][index.y][index.z - 1].type].wall && !typeParams[Genes[index.x][index.y][index.z + 1].type].wall)
                    Genes[index.x][index.y][index.z].type = 0;*/

                Genes[index.x][index.y][index.z].type = Genes[index.x][index.y][index.z + translation].type;
                Genes[index.x][index.y][index.z + translation].type = temp;
            }

            return Genes;
        }

        public static WaypointParams[][][] TranslateWallZ(Vector3Int size, Vector3Int input, HashSet<Vector3Int> wall, WaypointParams[][][] Genes, SharpNeatLib.Maths.FastRandom randomFast, TypeParams[] typeParams)
        {
            /*int upperTranslationLimit = size.x - input.x;
            int lowerTranslationLimit = input.x - 1;
            int translation = 0;

            while (translation == 0)
                translation = randomFast.Next(-lowerTranslationLimit, upperTranslationLimit);*/

            int translation = 0;

            if (input.x > 1 && input.x < size.x - 1)
            {
                translation = randomFast.Next(2) > 0 ? -1 : 1;
            }
            else if (input.x == 1)
            {
                translation = 1;
            }
            else if (input.x == size.x - 1)
            {
                translation = -1;
            }

            int temp;
            foreach (Vector3Int index in wall)
            {
                temp = Genes[index.x][index.y][index.z].type;

                /*
                if (!typeParams[Genes[index.x - 1][index.y][index.z].type].wall && !typeParams[Genes[index.x + 1][index.y][index.z].type].wall)
                    Genes[index.x][index.y][index.z].type = 0;*/

                Genes[index.x][index.y][index.z].type = Genes[index.x + translation][index.y][index.z].type;
                Genes[index.x + translation][index.y][index.z].type = temp;
            }

            return Genes;
        }

        public static WaypointParams[][][] RotateWallX(Vector3Int size, Vector3Int input, HashSet<Vector3Int> wall, WaypointParams[][][] Genes, SharpNeatLib.Maths.FastRandom randomFast, TypeParams[] typeParams)
        {
            int pivot = 0;
            int Xmin = size.x; int Xmax = 0; 

            foreach (Vector3Int index in wall)
            {
                if (index.x < Xmin)
                    Xmin = index.x;
                if (index.x > Xmax)
                    Xmax = index.x;
            }

            int spaceLeft = input.z - 1;
            int spaceRight = size.z - input.z - 1;
            int sizeWall = Xmax - Xmin + 1;

            if (spaceLeft > spaceRight)
            {
                pivot = Xmax;

                if (sizeWall - 1 > spaceLeft)
                    pivot -= sizeWall - 1 - spaceLeft;
            }
            else
            {
                pivot = Xmin;

                if (sizeWall - 1 > spaceRight)
                    pivot += sizeWall - 1 - spaceRight;
            }

            int temp;
            int newZ;
            foreach (Vector3Int index in wall)
            {
                if(index.x != pivot)
                {
                    newZ = (index.x - pivot) + input.z;
                    temp = Genes[index.x][index.y][index.z].type; 
                    Genes[index.x][index.y][index.z].type = Genes[pivot][index.y][newZ].type;
                    Genes[pivot][index.y][newZ].type = temp;
                }
            }

            return Genes;
        }

        public static WaypointParams[][][] RotateWallZ(Vector3Int size, Vector3Int input, HashSet<Vector3Int> wall, WaypointParams[][][] Genes, SharpNeatLib.Maths.FastRandom randomFast, TypeParams[] typeParams)
        {
            int pivot = 0;
            int Zmin = size.z; int Zmax = 0;

            foreach (Vector3Int index in wall)
            {
                if (index.z < Zmin)
                    Zmin = index.z;
                if (index.z > Zmax)
                    Zmax = index.z;
            }

            int spaceLeft = input.x - 1;
            int spaceRight = size.x - input.x - 1;
            int sizeWall = Zmax - Zmin + 1;

            if (spaceLeft > spaceRight)
            {
                pivot = Zmax;

                if (sizeWall - 1 > spaceLeft)
                    pivot -= sizeWall - 1 - spaceLeft;
            }
            else
            {
                pivot = Zmin;

                if (sizeWall - 1 > spaceRight)
                    pivot += sizeWall - 1 - spaceRight;
            }

            int temp;
            int newX;
            foreach (Vector3Int index in wall)
            {
                if (index.z != pivot)
                {
                    newX = (index.z - pivot) + input.x;
                    temp = Genes[index.x][index.y][index.z].type;
                    Genes[index.x][index.y][index.z].type = Genes[newX][index.y][pivot].type;
                    Genes[newX][index.y][pivot].type = temp;
                }
            }

            return Genes;
        }

    }
}