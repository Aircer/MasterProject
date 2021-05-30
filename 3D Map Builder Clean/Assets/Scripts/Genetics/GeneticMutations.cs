using System.Collections.Generic;
using System.Linq;
using System;
using UtilitiesGenetic;
using mVectors;

namespace Genetics
{
    public static class Mutations
    {
        public static int[][][] TranslateDoor(Vector3Int size, int[][][] Genes, Vector3Int input, TypeParams[] typeParams, SharpNeatLib.Maths.FastRandom randomFast)
        {
            if (typeParams[Genes[input.x][input.y][input.z]].door)
            {
                int maxX = 0;
                int minX = 0;
                int maxZ = 0;
                int minZ = 0;

                if (input.x + 2 < size.x && typeParams[Genes[input.x + 1][input.y][input.z]].wall && typeParams[Genes[input.x + 2][input.y][input.z]].wall)
                    maxX = 1;
                if (input.x - 2 > 0 && typeParams[Genes[input.x - 1][input.y][input.z]].wall && typeParams[Genes[input.x - 2][input.y][input.z]].wall)
                    minX = -1;
                if (input.z + 2 < size.z && typeParams[Genes[input.x][input.y][input.z + 1]].wall && typeParams[Genes[input.x][input.y][input.z] + 2].wall)
                    maxZ = 1;
                if (input.z - 2 > 0 && typeParams[Genes[input.x][input.y][input.z - 1]].wall && typeParams[Genes[input.x][input.y][input.z - 2]].wall)
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
                    temp = Genes[input.x][input.y][input.z];
                    Genes[input.x][input.y][input.z] = Genes[input.x + translation.x][input.y][input.z + translation.y];
                    Genes[input.x + translation.x][input.y][input.z + translation.y] = temp;

                    if (typeParams[Genes[input.x][input.y - 1][input.z]].door)
                    {
                        temp = Genes[input.x][input.y - 1][input.z];
                        Genes[input.x][input.y - 1][input.z] = Genes[input.x + translation.x][input.y - 1][input.z + translation.y];
                        Genes[input.x + translation.x][input.y - 1][input.z + translation.y] = temp;
                    }

                    if (typeParams[Genes[input.x][input.y + 1][input.z]].door)
                    {
                        temp = Genes[input.x][input.y + 1][input.z];
                        Genes[input.x][input.y + 1][input.z] = Genes[input.x + translation.x][input.y + 1][input.z + translation.y];
                        Genes[input.x + translation.x][input.y + 1][input.z + translation.y] = temp;
                    }
                }
            }

            return Genes;
        }

        public static int[][][] CreateDoor(int[][][] Genes, Vector3Int input, TypeParams[] typeParams, SharpNeatLib.Maths.FastRandom randomFast, int newType)
        {
            if (typeParams[Genes[input.x][input.y][input.z]].wall && typeParams[Genes[input.x][input.y + 1][input.z]].wall)
            {
                Genes[input.x][input.y][input.z] = newType;
                Genes[input.x][input.y + 1][input.z] = newType;
            }

            return Genes;
        }

        public static int[][][] CollapseDoor(Vector3Int size, int[][][] Genes, Vector3Int input, TypeParams[] typeParams, SharpNeatLib.Maths.FastRandom randomFast)
        {
            if (typeParams[Genes[input.x][input.y][input.z]].door && typeParams[Genes[input.x][input.y + 1][input.z]].door)
            {
                Dictionary<int, int> typesAround = new Dictionary<int, int>();

                for (int i = -1; i < 2; i++)
                {
                    for (int j = -1; j < 3; j++)
                    {
                        for (int k = -1; k < 2; k++)
                        {
                            if (input.y + j < size.y && typeParams[Genes[input.x + i][input.y + j][input.z + k]].wall)
                            {
                                if (typesAround.ContainsKey(Genes[input.x + i][input.y + j][input.z + k]))
                                    typesAround[Genes[input.x + i][input.y + j][input.z + k]]++;
                                else
                                {
                                    typesAround[Genes[input.x + i][input.y + j][input.z + k]] = 1;
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
                    Genes[input.x][input.y][input.z] = mostTypeAround;
                    Genes[input.x][input.y + 1][input.z] = mostTypeAround;
                }
            }

            return Genes;
        }

        public static int[][][] FillFloor(Vector3Int size, int[][][] Genes, Vector3Int input, int newType, TypeParams[] typeParams)
        {
            if (Genes[input.x][input.y][input.z] > 0 && !typeParams[Genes[input.x][input.y][input.z]].floor
                && !typeParams[Genes[input.x][input.y - 1][input.z]].floor && !typeParams[Genes[input.x][input.y + 1][input.z]].floor)
                return Genes;

            int y = input.y;
            int targetType = Genes[input.x][input.y][input.z];

            Stack<Vector3Int> floor = new Stack<Vector3Int>();
            HashSet<Vector3Int> newFloor = new HashSet<Vector3Int>();

            floor.Push(input);
            while (floor.Count != 0)
            {
                Vector3Int temp = floor.Pop();
                int z1 = temp.z;
                while (z1 >= 1 && (Genes[temp.x][y][z1] == 0 || typeParams[Genes[temp.x][y][z1]].floor))
                {
                    z1--;
                }
                z1++;

                bool spanLeft = false;
                bool spanRight = false;

                while (z1 < size.z && (Genes[temp.x][y][z1] == 0 || typeParams[Genes[temp.x][y][z1]].floor))
                {
                    if(!typeParams[Genes[temp.x][y - 1][z1]].floor && !typeParams[Genes[temp.x][y + 1][z1]].floor)
                        Genes[temp.x][y][z1] = newType;
                    newFloor.Add(new Vector3Int(temp.x, y, z1));

                    if (!spanLeft && temp.x > 1 && (Genes[temp.x-1][y][z1] == 0 || typeParams[Genes[temp.x - 1][y][z1]].floor) && !newFloor.Contains(new Vector3Int(temp.x - 1, y, z1)))
                    {
                        floor.Push(new Vector3Int(temp.x - 1,y, z1));
                        spanLeft = true;
                    }
                    else if (spanLeft && temp.x - 1 == 1 && Genes[temp.x - 1][y][z1] > 0 && !typeParams[Genes[temp.x - 1][y][z1]].floor)
                    {
                        spanLeft = false;
                    }
                    if (!spanRight && temp.x < size.x - 1 && (Genes[temp.x + 1][y][z1] == 0 || typeParams[Genes[temp.x + 1][y][z1]].floor) && !newFloor.Contains(new Vector3Int(temp.x + 1, y, z1)))
                    {
                        floor.Push(new Vector3Int(temp.x + 1, y, z1));
                        spanRight = true;
                    }
                    else if (spanRight && temp.x < size.x - 1 && Genes[temp.x + 1][y][z1] > 0 && !typeParams[Genes[temp.x + 1][y][z1]].floor)
                    {
                        spanRight = false;
                    }
                    z1++;
                }
            }

            return Genes;
        }

        public static int[][][] DeleteFloor(Vector3Int size, int[][][] Genes, Vector3Int input, TypeParams[] typeParams)
        {
            if (!typeParams[Genes[input.x][input.y][input.z]].floor)
                return Genes;

            int y = input.y;
            int targetType = Genes[input.x][input.y][input.z];

            Stack<Vector3Int> floor = new Stack<Vector3Int>();
            HashSet<Vector3Int> newFloor = new HashSet<Vector3Int>();

            floor.Push(input);
            while (floor.Count != 0)
            {
                Vector3Int temp = floor.Pop();
                int z1 = temp.z;
                while (z1 >= 1 && typeParams[Genes[temp.x][y][z1]].floor)
                {
                    z1--;
                }
                z1++;

                bool spanLeft = false;
                bool spanRight = false;

                while (z1 < size.z && typeParams[Genes[temp.x][y][z1]].floor)
                {
                    Genes[temp.x][y][z1] = 0;
                    newFloor.Add(new Vector3Int(temp.x, y, z1));

                    if (!spanLeft && temp.x > 1 && typeParams[Genes[temp.x - 1][y][z1]].floor && !newFloor.Contains(new Vector3Int(temp.x - 1, y, z1)))
                    {
                        floor.Push(new Vector3Int(temp.x - 1, y, z1));
                        spanLeft = true;
                    }
                    else if (spanLeft && temp.x - 1 == 1 && !typeParams[Genes[temp.x - 1][y][z1]].floor)
                    {
                        spanLeft = false;
                    }
                    if (!spanRight && temp.x < size.x - 1 && typeParams[Genes[temp.x + 1][y][z1]].floor && !newFloor.Contains(new Vector3Int(temp.x + 1, y, z1)))
                    {
                        floor.Push(new Vector3Int(temp.x + 1, y, z1));
                        spanRight = true;
                    }
                    else if (spanRight && temp.x < size.x - 1 && !typeParams[Genes[temp.x + 1][y][z1]].floor)
                    {
                        spanRight = false;
                    }
                    z1++;
                }
            }

            return Genes;
        }

        public static int[][][] FillStairXPos(Vector3Int size, int[][][] Genes, Vector3Int input, int newType, TypeParams[] typeParams)
        {
            if (Genes[input.x][input.y][input.z] > 0 && !typeParams[Genes[input.x][input.y][input.z]].floor)
                return Genes;

            int xMin = input.x;
            int yMin = input.y;
            int z = input.z;

            while (xMin > 0 && yMin > 0 && !CellIsStruct(xMin, yMin, z, Genes, typeParams))
            {
                xMin--;
                yMin--;
            }
            xMin++;
            yMin++;

            int xTemp = xMin;
            int yTemp = yMin;

            while (xTemp < size.x && yTemp < size.y && !CellIsStruct(xTemp, yTemp, z, Genes, typeParams))
            {
                Genes[xTemp][yTemp][z] = newType;
                Genes[xTemp][yTemp + 1][z] = 0;
                xTemp++;
                yTemp++;
            }

            if (typeParams[Genes[xTemp][yTemp][z]].floor)
            {
                Genes[xTemp][yTemp][z] = newType;
            }
            else 
            {
                Genes[xTemp - 1][yTemp - 1][z] = 0;
                Genes[xTemp - 1][yTemp - 2][z] = newType;
            }

            return Genes;
        }

        public static int[][][] FillWallX(Vector3Int size, int[][][] Genes, Vector3Int input, int newType, TypeParams[] typeParams)
        {
            if (Genes[input.x][input.y][input.z] > 0 && !typeParams[Genes[input.x][input.y][input.z]].floor)
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
                    Genes[temp.x][y1][z] = newType;

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

        public static int[][][] FillWallZ(Vector3Int size, int[][][] Genes, Vector3Int input, int newType, TypeParams[] typeParams)
        {
            if (Genes[input.x][input.y][input.z] > 0 && !typeParams[Genes[input.x][input.y][input.z]].floor)
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
                    Genes[x][y1][temp.z] = newType;

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

        private static bool WallLimit(int x, int y, int z, int[][][] Genes, TypeParams[] typeParams)
        {
            if (Genes[x][y][z] == 0 || (typeParams[Genes[x][y][z]].floor && (y - 2 < 0 || typeParams[Genes[x][y - 2][z]].wall)))
                return false;
            else
                return true;
        }

        public static int[][][] DeleteWallX(Vector3Int size, int[][][] Genes, Vector3Int input, TypeParams[] typeParams)
        {
            if (!typeParams[Genes[input.x][input.y][input.z]].wall || (!typeParams[Genes[input.x - 1][input.y][input.z]].wall
                && !typeParams[Genes[input.x + 1][input.y][input.z]].wall))
                return Genes;

            int z = input.z;

            Stack<Vector3Int> wall = new Stack<Vector3Int>();

            wall.Push(input);
            while (wall.Count != 0)
            {
                Vector3Int temp = wall.Pop();
                int y1 = temp.y;
                while (y1 >= 1 && typeParams[Genes[temp.x][y1][z]].wall)
                {
                    y1--;
                }
                y1++;

                bool spanLeft = false;
                bool spanRight = false;
                int Xmin = 1; 
                int Xmax = size.x - 1;

                while (y1 < size.y && typeParams[Genes[temp.x][y1][z]].wall && !typeParams[Genes[temp.x][y1][z - 1]].wall && !typeParams[Genes[temp.x][y1][z + 1]].wall)
                {
                    Genes[temp.x][y1][z] = 0;

                    if (!spanLeft && temp.x > Xmin && typeParams[Genes[temp.x - 1][y1][z]].wall)
                    {
                        if (!typeParams[Genes[temp.x - 1][y1][z - 1]].wall && !typeParams[Genes[temp.x - 1][y1][z + 1]].wall)
                            wall.Push(new Vector3Int(temp.x - 1, y1, z));
                        else
                            Xmin = temp.x; 

                        spanLeft = true;
                    }
                    else if (spanLeft && temp.x - 1 == Xmin && !typeParams[Genes[temp.x - 1][y1][z]].wall)
                    {
                        spanLeft = false;
                    }
                    if (!spanRight && temp.x < Xmax && typeParams[Genes[temp.x + 1][y1][z]].wall)
                    {
                        if (!typeParams[Genes[temp.x + 1][y1][z - 1]].wall && !typeParams[Genes[temp.x + 1][y1][z + 1]].wall)
                            wall.Push(new Vector3Int(temp.x + 1, y1, z));
                        else
                            Xmax = temp.z + 1; 

                        spanRight = true;
                    }
                    else if (spanRight && temp.x < Xmax && !typeParams[Genes[temp.x + 1][y1][z]].wall)
                    {
                        spanRight = false;
                    }
                    y1++;
                }
            }

            return Genes;
        }

        public static int[][][] DeleteWallZ(Vector3Int size, int[][][] Genes, Vector3Int input, TypeParams[] typeParams)
        {
            if (!typeParams[Genes[input.x][input.y][input.z]].wall || (!typeParams[Genes[input.x][input.y][input.z - 1]].wall
                && !typeParams[Genes[input.x][input.y][input.z + 1]].wall))
                return Genes;

            int x = input.x;

            Stack<Vector3Int> wall = new Stack<Vector3Int>();

            wall.Push(input);
            while (wall.Count != 0)
            {
                Vector3Int temp = wall.Pop();
                int y1 = temp.y;
                while (y1 >= 1 && typeParams[Genes[x][y1][temp.z]].wall)
                {
                    y1--;
                }
                y1++;

                bool spanLeft = false;
                bool spanRight = false;
                int minZ = 1;
                int maxZ = size.z - 1; 

                while (y1 < size.y && typeParams[Genes[x][y1][temp.z]].wall && !typeParams[Genes[x - 1][y1][temp.z]].wall && !typeParams[Genes[x + 1][y1][temp.z]].wall)
                {
                    Genes[x][y1][temp.z] = 0;

                    if (!spanLeft && temp.z > minZ && typeParams[Genes[x][y1][temp.z - 1]].wall)
                    {
                        if (!typeParams[Genes[x - 1][y1][temp.z - 1]].wall && !typeParams[Genes[x + 1][y1][temp.z - 1]].wall)
                            wall.Push(new Vector3Int(x, y1, temp.z - 1));
                        else
                            minZ = temp.z; 

                        spanLeft = true;
                    }
                    else if (spanLeft && temp.z - 1 == minZ && !typeParams[Genes[x][y1][temp.z - 1]].wall)
                    {
                        spanLeft = false;
                    }
                    if (!spanRight && temp.z < maxZ && typeParams[Genes[x][y1][temp.z + 1]].wall)
                    {
                        if (!typeParams[Genes[x - 1][y1][temp.z + 1]].wall && !typeParams[Genes[x + 1][y1][temp.z + 1]].wall)
                            wall.Push(new Vector3Int(x, y1, temp.z + 1));
                        else
                            maxZ = temp.z + 1; 

                        spanRight = true;
                    }
                    else if (spanRight && temp.z < maxZ && !typeParams[Genes[x][y1][temp.z + 1]].wall)
                    {
                        spanRight = false;
                    }
                    y1++;
                }
            }

            return Genes;
        }

        public static int[][][] TranslationWall(Vector3Int size, int[][][] Genes, Vector3Int input, TypeParams[] typeParams, SharpNeatLib.Maths.FastRandom randomFast)
        {
            int x = input.x; int y = input.y; int z = input.z;

            if (typeParams[Genes[x][y][z]].wall)
            {
                HashSet<Vector3Int> wall = new HashSet<Vector3Int>(); 

                if (typeParams[Genes[x][y][z - 1]].wall || typeParams[Genes[x][y][z + 1]].wall
                && (!typeParams[Genes[x - 1][y][z]].wall && !typeParams[Genes[x + 1][y][z]].wall))
                {
                    wall = TakeWallZ(size, Genes, input, typeParams);
                    Genes = TranslateWallZ(size, input, wall, Genes, randomFast, typeParams); 
                }
                if (typeParams[Genes[x - 1][y][z]].wall || typeParams[Genes[x + 1][y][z]].wall
                    && (!typeParams[Genes[x][y][z - 1]].wall && !typeParams[Genes[x][y][z + 1]].wall))
                {
                    wall = TakeWallX(size, Genes, input, typeParams);
                    Genes = TranslateWallX(size, input, wall, Genes, randomFast, typeParams);
                }
                else if ((typeParams[Genes[x - 1][y][z]].wall || typeParams[Genes[x + 1][y][z]].wall)
                    && (typeParams[Genes[x][y][z - 1]].wall || typeParams[Genes[x][y][z + 1]].wall))
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

        public static int[][][] RotationWall(Vector3Int size, int[][][] Genes, Vector3Int input, TypeParams[] typeParams, SharpNeatLib.Maths.FastRandom randomFast)
        {
            int x = input.x; int y = input.y; int z = input.z;

            if (typeParams[Genes[x][y][z]].wall)
            {
                HashSet<Vector3Int> wall = new HashSet<Vector3Int>();

                if (typeParams[Genes[x][y][z - 1]].wall || typeParams[Genes[x][y][z + 1]].wall
                && (!typeParams[Genes[x - 1][y][z]].wall && !typeParams[Genes[x + 1][y][z]].wall))
                {
                    wall = TakeWallZ(size, Genes, input, typeParams);
                    Genes = RotateWallZ(size, input, wall, Genes, randomFast, typeParams);
                }
                if (typeParams[Genes[x - 1][y][z]].wall || typeParams[Genes[x + 1][y][z]].wall
                    && (!typeParams[Genes[x][y][z - 1]].wall && !typeParams[Genes[x][y][z + 1]].wall))
                {
                    wall = TakeWallX(size, Genes, input, typeParams);
                    Genes = RotateWallX(size, input, wall, Genes, randomFast, typeParams);
                }
                else if ((typeParams[Genes[x - 1][y][z]].wall || typeParams[Genes[x + 1][y][z]].wall)
                    && (typeParams[Genes[x][y][z - 1]].wall || typeParams[Genes[x][y][z + 1]].wall))
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

        public static HashSet<Vector3Int> TakeWallX(Vector3Int size, int[][][] Genes, Vector3Int input, TypeParams[] typeParams)
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
                while (y1 >= 1 && typeParams[Genes[temp.x][y1][z]].wall)
                {
                    y1--;
                }
                y1++;

                bool spanLeft = false;
                bool spanRight = false;

                while (y1 < size.y && typeParams[Genes[temp.x][y1][z]].wall)
                {   
                    cellsChecked.Add(new Vector3Int(temp.x, y1, z));
                    
                    if (typeParams[Genes[temp.x][y1][z - 1]].wall && typeParams[Genes[temp.x][y1][z + 1]].wall)
                    {
                        if (temp.x >= input.x && temp.x < maxX)
                            maxX = temp.x;

                        if (temp.x <= input.x && temp.x > minX)
                            minX = temp.x;
                    }
                    
                    if (!spanLeft && temp.x > 1 && typeParams[Genes[temp.x-1][y1][z]].wall && !cellsChecked.Contains(new Vector3Int(temp.x - 1, y1, z)))
                    {
                        cells.Push(new Vector3Int(temp.x - 1, y1, z));
                        spanLeft = true;
                    }
                    else if (spanLeft && temp.x - 1 == 1 && !typeParams[Genes[temp.x - 1][y1][z]].wall)
                    {
                        spanLeft = false;
                    }
                    if (!spanRight && temp.x < size.x - 1 && typeParams[Genes[temp.x + 1][y1][z]].wall && !cellsChecked.Contains(new Vector3Int(temp.x + 1, y1, z)))
                    {
                        cells.Push(new Vector3Int(temp.x + 1, y1, z));
                        spanRight = true;
                    }
                    else if (spanRight && temp.x < size.x - 1 && !typeParams[Genes[temp.x + 1][y1][z]].wall)
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

        public static HashSet<Vector3Int> TakeWallZ(Vector3Int size, int[][][] Genes, Vector3Int input, TypeParams[] typeParams)
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
                while (y1 >= 1 && typeParams[Genes[x][y1][temp.z]].wall)
                {
                    y1--;
                }
                y1++;

                bool spanLeft = false;
                bool spanRight = false;

                while (y1 < size.y && typeParams[Genes[x][y1][temp.z]].wall)
                {
                    cellsChecked.Add(new Vector3Int(x, y1, temp.z));

                    if (typeParams[Genes[x - 1][y1][temp.z]].wall && typeParams[Genes[x + 1][y1][temp.z]].wall)
                    { 
                        if (temp.z >= input.z && temp.z < maxZ)
                            maxZ = temp.z;

                        if (temp.z <= input.z && temp.z > minZ)
                            minZ = temp.z;
                    }

                    if (!spanLeft && temp.z > 1 && typeParams[Genes[x][y1][temp.z - 1]].wall && !cellsChecked.Contains(new Vector3Int(x, y1, temp.z - 1)))
                    {
                        cells.Push(new Vector3Int(x, y1, temp.z - 1));
                        spanLeft = true;
                    }
                    else if (spanLeft && temp.z - 1 == 1 && !typeParams[Genes[x][y1][temp.z - 1]].wall)
                    {
                        spanLeft = false;
                    }
                    if (!spanRight && temp.z < size.z - 1 && typeParams[Genes[x][y1][temp.z + 1]].wall && !cellsChecked.Contains(new Vector3Int(x, y1, temp.z + 1)))
                    {
                        cells.Push(new Vector3Int(x, y1, temp.z + 1));
                        spanRight = true;
                    }
                    else if (spanRight && temp.z < size.z - 1 && !typeParams[Genes[x][y1][temp.z + 1]].wall)
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

        public static int[][][] TranslateWallX(Vector3Int size, Vector3Int input, HashSet<Vector3Int> wall, int[][][] Genes, SharpNeatLib.Maths.FastRandom randomFast, TypeParams[] typeParams)
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
                temp = Genes[index.x][index.y][index.z];

                /*if (!typeParams[Genes[index.x][index.y][index.z - 1]].wall && !typeParams[Genes[index.x][index.y][index.z + 1]].wall)
                    Genes[index.x][index.y][index.z] = 0;*/

                Genes[index.x][index.y][index.z] = Genes[index.x][index.y][index.z + translation];
                Genes[index.x][index.y][index.z + translation] = temp;
            }

            return Genes;
        }

        public static int[][][] TranslateWallZ(Vector3Int size, Vector3Int input, HashSet<Vector3Int> wall, int[][][] Genes, SharpNeatLib.Maths.FastRandom randomFast, TypeParams[] typeParams)
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
                temp = Genes[index.x][index.y][index.z];

                /*
                if (!typeParams[Genes[index.x - 1][index.y][index.z]].wall && !typeParams[Genes[index.x + 1][index.y][index.z]].wall)
                    Genes[index.x][index.y][index.z] = 0;*/

                Genes[index.x][index.y][index.z] = Genes[index.x + translation][index.y][index.z];
                Genes[index.x + translation][index.y][index.z] = temp;
            }

            /*
            foreach (Vector3Int index in wall)
            {
                if ((typeParams[Genes[index.x + translation][index.y][index.z - 1]].wall && !typeParams[Genes[index.x + translation][index.y][index.z + 1]].wall)
                    && (typeParams[Genes[index.x + translation - 1][index.y][index.z + 1]].wall || typeParams[Genes[index.x + translation + 1][index.y][index.z + 1]].wall))
                    Genes[index.x + translation][index.y][index.z + 1] = Genes[index.x + translation][index.y][index.z];

                if ((typeParams[Genes[index.x + translation][index.y][index.z + 1]].wall && !typeParams[Genes[index.x + translation][index.y][index.z - 1]].wall)
                    && (typeParams[Genes[index.x + translation - 1][index.y][index.z - 1]].wall || typeParams[Genes[index.x + translation + 1][index.y][index.z - 1]].wall))
                    Genes[index.x + translation][index.y][index.z - 1] = Genes[index.x + translation][index.y][index.z];
            }*/

            return Genes;
        }

        public static int[][][] RotateWallX(Vector3Int size, Vector3Int input, HashSet<Vector3Int> wall, int[][][] Genes, SharpNeatLib.Maths.FastRandom randomFast, TypeParams[] typeParams)
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

                    if (newZ > 1 && newZ < size.z && pivot < size.x && pivot > 1)
                    {
                        temp = Genes[index.x][index.y][index.z];
                        Genes[index.x][index.y][index.z] = Genes[pivot][index.y][newZ];
                        Genes[pivot][index.y][newZ] = temp;
                    }
                }
            }

            return Genes;
        }

        public static int[][][] RotateWallZ(Vector3Int size, Vector3Int input, HashSet<Vector3Int> wall, int[][][] Genes, SharpNeatLib.Maths.FastRandom randomFast, TypeParams[] typeParams)
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

                    if (newX > 1 && newX < size.x && pivot < size.z && pivot > 1)
                    {
                        temp = Genes[index.x][index.y][index.z];
                        Genes[index.x][index.y][index.z] = Genes[newX][index.y][pivot];
                        Genes[newX][index.y][pivot] = temp;
                    }
                }
            }

            return Genes;
        }

        public static int[][][] CreateLadder(Vector3Int size, int[][][] Genes, Vector3Int input, int newType, TypeParams[] typeParams)
        {
            if (Genes[input.x][input.y][input.z] > 0)
                return Genes;

            int y1 = input.y;
            while (y1 >= 1 && !CellIsStruct(input.x, y1, input.z, Genes, typeParams))
            {
                y1--;
            }
            y1++;

            while (y1 < size.y && !CellIsStruct(input.x, y1, input.z, Genes, typeParams))
            {
                Genes[input.x][y1][input.z] = newType;
                y1++;
            }

            if(y1 < size.y && CellIsStruct(input.x, y1, input.z, Genes, typeParams))
                Genes[input.x][y1][input.z] = newType;

            return Genes;
        }

        public static int[][][] DeleteLadder(Vector3Int size, int[][][] Genes, Vector3Int input, TypeParams[] typeParams)
        {
            if (!typeParams[Genes[input.x][input.y][input.z]].ladder)
                return Genes;

            int y1 = input.y;
            while (y1 >= 1 && typeParams[Genes[input.x][y1][input.z]].ladder)
            {
                y1--;
            }
            y1++;

            while (y1 < size.y && typeParams[Genes[input.x][y1][input.z]].ladder)
            {
                Genes[input.x][y1][input.z] = 0;
                y1++;
            }

            return Genes;
        }

        public static int[][][] TranslateLadder(Vector3Int size, int[][][] Genes, Vector3Int input, TypeParams[] typeParams, SharpNeatLib.Maths.FastRandom randomFast)
        {

            if (!typeParams[Genes[input.x][input.y][input.z]].ladder)
                return Genes;

            bool emptyCellsXMinus = true;
            bool emptyCellsXPlus = true;
            bool emptyCellsZMinus = true;
            bool emptyCellsZPlus = true;

            int y1 = input.y;
            while (y1 >= 1 && typeParams[Genes[input.x][y1][input.z]].ladder)
            {
                y1--;
            }
            y1++;

            while (y1 < size.y && typeParams[Genes[input.x][y1][input.z]].ladder)
            {
                if (input.x < 2 || typeParams[Genes[input.x - 1][y1][input.z]].wall)
                    emptyCellsXMinus = false;
                if (input.x  + 1 > size.x - 1 || typeParams[Genes[input.x + 1][y1][input.z]].wall)
                    emptyCellsXPlus = false;
                if (input.z < 2 || typeParams[Genes[input.x][y1][input.z - 1]].wall)
                    emptyCellsZMinus = false;
                if (input.z + 1 > size.z - 1 || typeParams[Genes[input.x][y1][input.z + 1]].wall)
                    emptyCellsZPlus = false;

                y1++;
            }

            int translationX = randomFast.Next(emptyCellsXMinus == true ? -1 : 0, emptyCellsXPlus == true ? 2 : 1);
            int translationZ = randomFast.Next(emptyCellsZMinus == true ? -1 : 0, emptyCellsZPlus == true ? 2 : 1);

            if (translationX != 0 || translationZ != 0)
            {
                y1--;
                int temp;
                while (y1 >= 1 && typeParams[Genes[input.x][y1][input.z]].ladder)
                {
                    temp = Genes[input.x + translationX][y1][input.z + translationZ];
                    Genes[input.x + translationX][y1][input.z + translationZ] = Genes[input.x][y1][input.z];
                    Genes[input.x][y1][input.z] = temp;
                    y1--;
                }
            }

            return Genes;
        }

        private static bool CellIsStruct(int x, int y, int z, int[][][] Genes, TypeParams[] typeParams)
        {
            if (typeParams[Genes[x][y][z]].floor || typeParams[Genes[x][y][z]].wall)
                return true;
            else
                return false;
        }

    }
}