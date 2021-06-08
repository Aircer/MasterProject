using System.Collections.Generic;
using System.Linq;
using System;
using UtilitiesGenetic;
using mVectors;

namespace Genetics
{
    public static class MutationsFloor
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

        public static int[][][] FillFloor(int[][][] Genes, Vector3Int input, int newType)
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
                    if (CanFillFloor(temp.x, y, z1, Genes))
                        Genes[temp.x][y][z1] = newType;
                    newFloor.Add(new Vector3Int(temp.x, y, z1));

                    if (!spanLeft && temp.x > 1 && CanFillFloor(temp.x - 1, y, z1, Genes) && !newFloor.Contains(new Vector3Int(temp.x - 1, y, z1)))
                    {
                        floor.Push(new Vector3Int(temp.x - 1, y, z1));
                        spanLeft = true;
                    }
                    else if (spanLeft && temp.x - 1 == 1 && CanFillFloor(temp.x - 1, y, z1, Genes))
                    {
                        spanLeft = false;
                    }
                    if (!spanRight && temp.x < size.x - 1 && CanFillFloor(temp.x + 1, y, z1, Genes) && !newFloor.Contains(new Vector3Int(temp.x + 1, y, z1)))
                    {
                        floor.Push(new Vector3Int(temp.x + 1, y, z1));
                        spanRight = true;
                    }
                    else if (spanRight && temp.x < size.x - 1 && CanFillFloor(temp.x + 1, y, z1, Genes))
                    {
                        spanRight = false;
                    }
                    z1++;
                }
            }

            return Genes;
        }

        public static int[][][] DeleteFloor(int[][][] Genes, Vector3Int input)
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

        private static bool CellIsStruct(int x, int y, int z, int[][][] Genes)
        {
            return Mutations.CellIsStruct(x, y, z, Genes);
        }

        private static bool CanFillFloor(int x, int y, int z, int[][][] Genes)
        {
            if ((Genes[x][y][z] == 0 || typeParams[Genes[x][y][z]].floor) && !typeParams[Genes[x][y - 1][z]].stair
                                && !typeParams[Genes[x][y - 1][z]].floor && !typeParams[Genes[x][y + 1][z]].floor)
                return true;
            else
                return false;
        }
    }
}