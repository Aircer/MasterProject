using System.Collections.Generic;
using System.Linq;
using System;
using UtilitiesGenetic;

namespace Genetics
{
    public static class MutationsLadders
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

        public static int[][][] CreateLadder(int[][][] Genes, Vector3Int input, int newType)
        {
            if (Genes[input.x][input.y][input.z] > 0)
                return Genes;

            int y1 = input.y;
            while (y1 >= 1 && !CellIsStruct(input.x, y1, input.z, Genes))
            {
                y1--;
            }
            y1++;

            while (y1 < size.y && !CellIsStruct(input.x, y1, input.z, Genes))
            {
                Genes[input.x][y1][input.z] = newType;
                y1++;
            }

            if (y1 < size.y && CellIsStruct(input.x, y1, input.z, Genes))
                Genes[input.x][y1][input.z] = newType;

            return Genes;
        }

        public static int[][][] DeleteLadder(int[][][] Genes, Vector3Int input)
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

        public static int[][][] TranslateLadder(int[][][] Genes, Vector3Int input)
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
                if (input.x + 1 > size.x - 1 || typeParams[Genes[input.x + 1][y1][input.z]].wall)
                    emptyCellsXPlus = false;
                if (input.z < 2 || typeParams[Genes[input.x][y1][input.z - 1]].wall)
                    emptyCellsZMinus = false;
                if (input.z + 1 > size.z - 1 || typeParams[Genes[input.x][y1][input.z + 1]].wall)
                    emptyCellsZPlus = false;

                y1++;
            }

            int translationX = random.Next(emptyCellsXMinus == true ? -1 : 0, emptyCellsXPlus == true ? 2 : 1);
            int translationZ = random.Next(emptyCellsZMinus == true ? -1 : 0, emptyCellsZPlus == true ? 2 : 1);

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

        private static bool CellIsStruct(int x, int y, int z, int[][][] Genes)
        {
            return Mutations.CellIsStruct(x, y, z, Genes);
        }
    }
}