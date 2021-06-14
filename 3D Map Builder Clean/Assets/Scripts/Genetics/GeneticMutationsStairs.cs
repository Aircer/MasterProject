using System.Collections.Generic;
using System.Linq;
using System;
using UtilitiesGenetic;

namespace Genetics
{
    public static class MutationsStairs
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

        public static int[][][] CreateStair(int[][][] Genes, Vector3Int input, int newType)
        {
            int mutationType = random.Next(2);
            int direction = random.Next(2) > 0 ? 1 : -1;

            if (mutationType > 0)
            {
                FillStairX(ref Genes, input, newType, direction);
            }
            else
            {
                FillStairZ(ref Genes, input, newType, direction);
            }

            return Genes;
        }

        public static int[][][] DestroyStair(int[][][] Genes, Vector3Int input)
        {
            HashSet<Vector3Int> stairX = GetStairX(Genes, input);
            HashSet<Vector3Int> stairZ = GetStairZ(Genes, input);

            if (stairX.Count >= stairZ.Count)
            {
                RemoveStair(Genes, stairX);
            }
            else
                RemoveStair(Genes, stairZ);

            return Genes;
        }

        public static int[][][] MoveStair(int[][][] Genes, Vector3Int input, int newType)
        {
            HashSet<Vector3Int> stairX = GetStairX(Genes, input);
            HashSet<Vector3Int> stairZ = GetStairZ(Genes, input);

            if (stairX.Count > stairZ.Count)
            {
                RemoveStair(Genes, stairX);

                Vector3Int translation = new Vector3Int(0, 0, 0);

                if (input.x > 1 && input.x < size.x - 1)
                    translation.x = random.Next(2) > 0 ? -1 : 1;
                if (input.y > 1 && input.y < size.y - 1)
                    translation.y = random.Next(2) > 0 ? -1 : 1;
                if (input.z > 1 && input.z < size.z - 1)
                    translation.z = random.Next(2) > 0 ? -1 : 1;

                CreateStair(Genes, new Vector3Int(input.x + translation.x, input.y + translation.y, input.z + translation.z), newType);
            }
            else if (stairZ.Count > 1)
            {
                RemoveStair(Genes, stairZ);

                Vector3Int translation = new Vector3Int(0,0,0);

                if (input.x > 1 && input.x < size.x - 1)
                    translation.x = random.Next(2) > 0 ? -1 : 1;
                if (input.y > 1 && input.y < size.y - 1)
                    translation.y = random.Next(2) > 0 ? -1 : 1;
                if (input.z > 1 && input.z < size.z - 1)
                    translation.z = random.Next(2) > 0 ? -1 : 1;

                CreateStair(Genes, new Vector3Int(input.x + translation.x, input.y + translation.y, input.z + translation.z), newType);
            }

            return Genes;
        }

        public static int[][][] RemoveStair(int[][][] Genes, HashSet<Vector3Int> stair)
        {
            if (stair != null)
            {
                foreach (Vector3Int index in stair)
                {
                    Genes[index.x][index.y][index.z] = 0;

                    int mostCommunType = NeighborsMostCommunType(Genes, index);
                    if (!typeParams[mostCommunType].stair)
                        Genes[index.x][index.y][index.z] = mostCommunType;

                    mostCommunType = NeighborsMostCommunType(Genes, new Vector3Int(index.x, index.y + 1, index.z));
                    if (!typeParams[mostCommunType].stair)
                        Genes[index.x][index.y + 1][index.z] = mostCommunType;
                }
            }

            return Genes;
        }

        public static int NeighborsMostCommunType(int[][][] Genes, Vector3Int input)
        {
            List<int> typesAround = new List<int>();

            if(input.x - 1 > 0)
                typesAround.Add(Genes[input.x - 1][input.y][input.z]);
            if (input.x + 1 < size.x)
                typesAround.Add(Genes[input.x + 1][input.y][input.z]);
            if (input.z - 1 > 0)
                typesAround.Add(Genes[input.x][input.y][input.z - 1]);
            if (input.z + 1 < size.z)
                typesAround.Add(Genes[input.x][input.y][input.z + 1]);

            if(typesAround.Count > 0)
            {
                return (from i in typesAround
                        group i by i into grp
                        orderby grp.Count() descending
                        select grp.Key).First();
            }
            else
                return 0;
        }

        public static bool FloorAround(int[][][] Genes, int x, int y, int z)
        {
            if (x - 1 > 0 && typeParams[Genes[x - 1][y][z]].floor)
                return true;
            if (x + 1 < size.x && typeParams[Genes[x + 1][y][z]].floor)
                return true;
            if (z - 1 > 0 && typeParams[Genes[x][y][z - 1]].floor)
                return true;
            if (z + 1 < size.z && typeParams[Genes[x][y][z + 1]].floor)
                return true;

            return false;
        }

        public static HashSet<Vector3Int> GetStairX(int[][][] Genes, Vector3Int input)
        {
            int direction = 0;
            HashSet<Vector3Int> stair = new HashSet<Vector3Int>();
            stair.Add(input);

            if (typeParams[Genes[input.x - 1][input.y - 1][input.z]].stair || typeParams[Genes[input.x + 1][input.y + 1][input.z]].stair)
                direction = 1;
            if (typeParams[Genes[input.x + 1][input.y - 1][input.z]].stair || typeParams[Genes[input.x - 1][input.y + 1][input.z]].stair)
                direction = -1;

            if (direction == 0)
                return stair;

            int x = input.x;
            int y = input.y;
            int z = input.z;

            while (x > 0 && x < size.x && y > 0 && typeParams[Genes[x][y][z]].stair)
            {
                stair.Add(new Vector3Int(x, y, z));

                if (typeParams[Genes[x][y - 1][z]].floor)   //FloorAround(Genes, x, y, z))
                    break;

                x -= direction;
                y--;
            }

            x = input.x;
            y = input.y;

            while (x > 0 && x < size.x && y < size.y && typeParams[Genes[x][y][z]].stair)
            {
                stair.Add(new Vector3Int(x, y, z));

                if(FloorAround(Genes, x, y, z))
                    break;

                x += direction;
                y++;
            }

            return stair;
        }

        public static HashSet<Vector3Int> GetStairZ(int[][][] Genes, Vector3Int input)
        {
            int direction = 0;
            HashSet<Vector3Int> stair = new HashSet<Vector3Int>();
            stair.Add(input);

            if (typeParams[Genes[input.x][input.y - 1][input.z - 1]].stair || typeParams[Genes[input.x][input.y + 1][input.z + 1]].stair)
                direction = 1;
            if (typeParams[Genes[input.x][input.y - 1][input.z + 1]].stair || typeParams[Genes[input.x][input.y + 1][input.z - 1]].stair)
                direction = -1;

            if (direction == 0)
                return stair;

            int z = input.z;
            int y = input.y;
            int x = input.x;

            while (z > 0 && z < size.z && y > 0 && typeParams[Genes[x][y][z]].stair)
            {
                stair.Add(new Vector3Int(x, y, z));

                if (typeParams[Genes[x][y - 1][z]].floor)
                    break;

                z -= direction;
                y--;
            }

            z = input.z;
            y = input.y;

            while (z > 0 && z < size.z && y < size.y && typeParams[Genes[x][y][z]].stair)
            {
                stair.Add(new Vector3Int(x, y, z));

                if (FloorAround(Genes, x, y, z))
                    break;

                z += direction;
                y++;
            }

            return stair;
        }

        public static void FillStairX(ref int[][][] Genes, Vector3Int input, int newType, int direction)
        {
            if (Genes[input.x][input.y][input.z] > 0 && !typeParams[Genes[input.x][input.y][input.z]].floor
                && !typeParams[Genes[input.x][input.y - 1][input.z]].stair && !typeParams[Genes[input.x][input.y + 1][input.z]].stair)
                return;

            int xMin = input.x;
            int yMin = input.y;
            int z = input.z;

            while (xMin < size.x && yMin > 0 && xMin > 0 && yMin > 0 && !CellIsStruct(xMin, yMin, z, Genes))
            {
                xMin -= direction;
                yMin--;
            }
            xMin += direction;
            yMin++;

            int xTemp = xMin;
            int yTemp = yMin;
            int temp = 0;
            while (xTemp > 0 && yTemp > 0 && xTemp < size.x && yTemp < size.y && !CellIsStruct(xTemp, yTemp, z, Genes))
            {
                Genes[xTemp][yTemp][z] = newType;
                temp = Genes[xTemp][yTemp + 1][z];
                Genes[xTemp][yTemp + 1][z] = 0;
                xTemp += direction;
                yTemp++;
            }

            if (typeParams[Genes[xTemp][yTemp][z]].floor)
            {
                Genes[xTemp][yTemp][z] = newType;
            }
        }

        public static void FillStairZ(ref int[][][] Genes, Vector3Int input, int newType, int direction)
        {
            if (Genes[input.x][input.y][input.z] > 0 && !typeParams[Genes[input.x][input.y][input.z]].floor
                && !typeParams[Genes[input.x][input.y - 1][input.z]].stair && !typeParams[Genes[input.x][input.y + 1][input.z]].stair)
                return;

            int x = input.x;
            int yMin = input.y;
            int zMin = input.z;

            while (zMin < size.z && yMin > 0 && zMin > 0 && yMin > 0 && !CellIsStruct(x, yMin, zMin, Genes))
            {
                zMin -= direction;
                yMin--;
            }
            zMin += direction;
            yMin++;

            int zTemp = zMin;
            int yTemp = yMin;
            int temp = 0;
            while (zTemp > 0 && yTemp > 0 && zTemp < size.z && yTemp < size.y && !CellIsStruct(x, yTemp, zTemp, Genes))
            {
                Genes[x][yTemp][zTemp] = newType;
                temp = Genes[x][yTemp + 1][zTemp];
                Genes[x][yTemp + 1][zTemp] = 0;
                zTemp += direction;
                yTemp++;
            }

            if (typeParams[Genes[x][yTemp][zTemp]].floor)
            {
                Genes[x][yTemp][zTemp] = newType;
            }
        }

        private static bool CellIsStruct(int x, int y, int z, int[][][] Genes)
        {
            return Mutations.CellIsStruct(x, y, z, Genes);
        }
    }
}