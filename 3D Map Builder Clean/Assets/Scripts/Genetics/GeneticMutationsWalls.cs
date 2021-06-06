using System.Collections.Generic;
using System.Linq;
using System;
using UtilitiesGenetic;
using mVectors;

namespace Genetics
{
    public static class MutationsWalls
    {
        public static Vector3Int size;
        public static SharpNeatLib.Maths.FastRandom random;
        public static TypeParams[] typeParams;

        public static void InitMutations(Vector3Int sizeDNA, SharpNeatLib.Maths.FastRandom rand, TypeParams[] tp)
        {
            size = sizeDNA;
            random = rand;
            typeParams = tp;
        }

        public static int[][][] FillWallX(int[][][] Genes, Vector3Int input, int newType)
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
                while (y1 >= 1 && !WallLimit(temp.x, y1, z, Genes))
                {
                    y1--;
                }
                y1++;

                bool spanLeft = false;
                bool spanRight = false;

                while (y1 < size.y && !WallLimit(temp.x, y1, z, Genes))
                {
                    Genes[temp.x][y1][z] = newType;

                    if (!spanLeft && temp.x > 1 && !WallLimit(temp.x - 1, y1, z, Genes))
                    {
                        wall.Push(new Vector3Int(temp.x - 1, y1, z));
                        spanLeft = true;
                    }
                    else if (spanLeft && temp.x - 1 == 1 && WallLimit(temp.x - 1, y1, z, Genes))
                    {
                        spanLeft = false;
                    }
                    if (!spanRight && temp.x < size.x - 1 && !WallLimit(temp.x + 1, y1, z, Genes))
                    {
                        wall.Push(new Vector3Int(temp.x + 1, y1, z));
                        spanRight = true;
                    }
                    else if (spanRight && temp.x < size.x - 1 && WallLimit(temp.x, y1, z, Genes))
                    {
                        spanRight = false;
                    }
                    y1++;
                }
            }

            return Genes;
        }

        public static int[][][] FillWallZ(int[][][] Genes, Vector3Int input, int newType)
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
                while (y1 >= 1 && !WallLimit(x, y1, temp.z, Genes))
                {
                    y1--;
                }
                y1++;

                bool spanLeft = false;
                bool spanRight = false;

                while (y1 < size.y && !WallLimit(x, y1, temp.z, Genes))
                {
                    Genes[x][y1][temp.z] = newType;

                    if (!spanLeft && temp.z > 1 && !WallLimit(x, y1, temp.z - 1, Genes))
                    {
                        wall.Push(new Vector3Int(x, y1, temp.z - 1));
                        spanLeft = true;
                    }
                    else if (spanLeft && temp.z - 1 == 1 && WallLimit(x, y1, temp.z - 1, Genes))
                    {
                        spanLeft = false;
                    }
                    if (!spanRight && temp.z < size.z - 1 && !WallLimit(x, y1, temp.z + 1, Genes))
                    {
                        wall.Push(new Vector3Int(x, y1, temp.z + 1));
                        spanRight = true;
                    }
                    else if (spanRight && temp.x < size.x - 1 && WallLimit(x, y1, temp.z + 1, Genes))
                    {
                        spanRight = false;
                    }
                    y1++;
                }
            }

            return Genes;
        }

        public static int[][][] DeleteWallX(int[][][] Genes, Vector3Int input)
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

        public static int[][][] DeleteWallZ(int[][][] Genes, Vector3Int input)
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

        public static int[][][] TranslationWall(int[][][] Genes, Vector3Int input)
        {
            int x = input.x; int y = input.y; int z = input.z;

            if (typeParams[Genes[x][y][z]].wall)
            {
                HashSet<Vector3Int> wall = new HashSet<Vector3Int>();

                if (typeParams[Genes[x][y][z - 1]].wall || typeParams[Genes[x][y][z + 1]].wall
                && (!typeParams[Genes[x - 1][y][z]].wall && !typeParams[Genes[x + 1][y][z]].wall))
                {
                    wall = TakeWallZ(Genes, input);
                    Genes = TranslateWallZ(input, wall, Genes);
                }
                if (typeParams[Genes[x - 1][y][z]].wall || typeParams[Genes[x + 1][y][z]].wall
                    && (!typeParams[Genes[x][y][z - 1]].wall && !typeParams[Genes[x][y][z + 1]].wall))
                {
                    wall = TakeWallX(Genes, input);
                    Genes = TranslateWallX(input, wall, Genes);
                }
                else if ((typeParams[Genes[x - 1][y][z]].wall || typeParams[Genes[x + 1][y][z]].wall)
                    && (typeParams[Genes[x][y][z - 1]].wall || typeParams[Genes[x][y][z + 1]].wall))
                {
                    if (random.Next(2) > 1 ? true : false)
                    {
                        wall = TakeWallZ(Genes, input);
                        Genes = TranslateWallZ(input, wall, Genes);
                    }
                    else
                    {
                        wall = TakeWallX(Genes, input);
                        Genes = TranslateWallX(input, wall, Genes);
                    }
                }
            }

            return Genes;
        }

        public static int[][][] RotationWall(int[][][] Genes, Vector3Int input)
        {
            int x = input.x; int y = input.y; int z = input.z;

            if (typeParams[Genes[x][y][z]].wall)
            {
                HashSet<Vector3Int> wall = new HashSet<Vector3Int>();

                if (typeParams[Genes[x][y][z - 1]].wall || typeParams[Genes[x][y][z + 1]].wall
                && (!typeParams[Genes[x - 1][y][z]].wall && !typeParams[Genes[x + 1][y][z]].wall))
                {
                    wall = TakeWallZ(Genes, input);
                    Genes = RotateWallZ(input, wall, Genes);
                }
                if (typeParams[Genes[x - 1][y][z]].wall || typeParams[Genes[x + 1][y][z]].wall
                    && (!typeParams[Genes[x][y][z - 1]].wall && !typeParams[Genes[x][y][z + 1]].wall))
                {
                    wall = TakeWallX(Genes, input);
                    Genes = RotateWallX(input, wall, Genes);
                }
                else if ((typeParams[Genes[x - 1][y][z]].wall || typeParams[Genes[x + 1][y][z]].wall)
                    && (typeParams[Genes[x][y][z - 1]].wall || typeParams[Genes[x][y][z + 1]].wall))
                {
                    if (random.Next(2) > 1 ? true : false)
                    {
                        wall = TakeWallZ(Genes, input);
                        Genes = RotateWallZ(input, wall, Genes);
                    }
                    else
                    {
                        wall = TakeWallX(Genes, input);
                        Genes = RotateWallX(input, wall, Genes);
                    }
                }
            }

            return Genes;
        }

        public static HashSet<Vector3Int> TakeWallX(int[][][] Genes, Vector3Int input)
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

                    if (!spanLeft && temp.x > 1 && typeParams[Genes[temp.x - 1][y1][z]].wall && !cellsChecked.Contains(new Vector3Int(temp.x - 1, y1, z)))
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

        public static HashSet<Vector3Int> TakeWallZ(int[][][] Genes, Vector3Int input)
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

        public static int[][][] TranslateWallX(Vector3Int input, HashSet<Vector3Int> wall, int[][][] Genes)
        {
            /*int upperTranslationLimit = size.z - input.z;
            int lowerTranslationLimit = input.z - 1;
            int translation = 0;

            while (translation == 0)
                translation = randomFast.Next(-lowerTranslationLimit, upperTranslationLimit);*/

            int translation = 0;

            if (input.z > 1 && input.z < size.z - 1)
            {
                translation = random.Next(2) > 0 ? -1 : 1;
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

                if (!typeParams[Genes[index.x][index.y][index.z - 1]].wall && !typeParams[Genes[index.x][index.y][index.z + 1]].wall)
                    Genes[index.x][index.y][index.z] = 0;

                Genes[index.x][index.y][index.z] = Genes[index.x][index.y][index.z + translation];
                Genes[index.x][index.y][index.z + translation] = temp;
            }

            return Genes;
        }

        public static int[][][] TranslateWallZ(Vector3Int input, HashSet<Vector3Int> wall, int[][][] Genes)
        {
            /*int upperTranslationLimit = size.x - input.x;
            int lowerTranslationLimit = input.x - 1;
            int translation = 0;

            while (translation == 0)
                translation = randomFast.Next(-lowerTranslationLimit, upperTranslationLimit);*/

            int translation = 0;

            if (input.x > 1 && input.x < size.x - 1)
            {
                translation = random.Next(2) > 0 ? -1 : 1;
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

        public static int[][][] RotateWallX(Vector3Int input, HashSet<Vector3Int> wall, int[][][] Genes)
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
                if (index.x != pivot)
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

        public static int[][][] RotateWallZ(Vector3Int input, HashSet<Vector3Int> wall, int[][][] Genes)
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

        private static bool WallLimit(int x, int y, int z, int[][][] Genes)
        {
            if ((Genes[x][y][z] == 0 || (typeParams[Genes[x][y][z]].floor && (y - 2 < 0 || typeParams[Genes[x][y - 2][z]].wall))
                                    || typeParams[Genes[x][y][z]].stair)
                                    && !typeParams[Genes[x][y][z]].floor)
                return false;
            else
                return true;
        }

        public static int NeighborsMostCommunType(int[][][] Genes, Vector3Int input)
        {
            List<int> typesAround = new List<int>();

            if (input.x - 1 > 0)
                typesAround.Add(Genes[input.x - 1][input.y][input.z]);
            if (input.x + 1 < size.x)
                typesAround.Add(Genes[input.x + 1][input.y][input.z]);
            if (input.z - 1 > 0)
                typesAround.Add(Genes[input.x][input.y][input.z - 1]);
            if (input.z + 1 < size.z)
                typesAround.Add(Genes[input.x][input.y][input.z + 1]);

            if (typesAround.Count > 0)
            {
                return (from i in typesAround
                        group i by i into grp
                        orderby grp.Count() descending
                        select grp.Key).First();
            }
            else
                return 0;
        }

        private static bool CellIsStruct(int x, int y, int z, int[][][] Genes)
        {
            return Mutations.CellIsStruct(x, y, z, Genes);
        }
    }
}