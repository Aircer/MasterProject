﻿using System.Collections.Generic;
using System.Linq;
using System;
using UtilitiesGenetic;

namespace Genetics
{
    public class MutationsDoors
    {
        public static Vector3Int size;
        public static SharpNeatLib.Maths.FastRandom random;
        public static TypeParams[] typeParams;

        public void InitMutations(Vector3Int sizeDNA, SharpNeatLib.Maths.FastRandom rand, TypeParams[] tp)
        {
            size = sizeDNA;
            random = rand;
            typeParams = tp;
        }

        public int[][][] TranslateDoor(int[][][] Genes, Vector3Int input)
        {
            if (typeParams[Genes[input.x][input.y][input.z]].door)
            {
                int maxX = 0;
                int minX = 0;
                int maxY = 0;
                int minY = 0;
                int maxZ = 0;
                int minZ = 0;

                if (input.x + 2 < size.x && typeParams[Genes[input.x + 1][input.y][input.z]].wall && typeParams[Genes[input.x + 2][input.y][input.z]].wall)
                    maxX = 1;
                if (input.x - 2 > 0 && typeParams[Genes[input.x - 1][input.y][input.z]].wall && typeParams[Genes[input.x - 2][input.y][input.z]].wall)
                    minX = -1;
                if (input.y + 2 < size.y && typeParams[Genes[input.x][input.y + 1][input.z]].wall && typeParams[Genes[input.x][input.y + 2][input.z]].wall)
                    maxY = 1;
                if (input.y - 2 > 0 && typeParams[Genes[input.x][input.y - 1][input.z]].wall && typeParams[Genes[input.x][input.y - 2][input.z]].wall)
                    minY = -1;
                if (input.z + 2 < size.z && typeParams[Genes[input.x][input.y][input.z + 1]].wall && typeParams[Genes[input.x][input.y][input.z] + 2].wall)
                    maxZ = 1;
                if (input.z - 2 > 0 && typeParams[Genes[input.x][input.y][input.z - 1]].wall && typeParams[Genes[input.x][input.y][input.z - 2]].wall)
                    minZ = -1;

                Vector3Int translation = new Vector3Int(0, 0, 0);
                if (minX != 0 || maxX != 0 || minY != 0 || maxY != 0 || minZ != 0 || maxZ != 0)
                {
                    while (translation.x == 0 && translation.y == 0 && translation.z == 0)
                    {
                        translation.x = random.Next(minX, maxX + 1);
                        translation.y = random.Next(minY, maxY + 1);
                        translation.z = random.Next(minZ, maxZ + 1);
                    }

                    int temp;
                    temp = Genes[input.x][input.y][input.z];
                    Genes[input.x][input.y][input.z] = Genes[input.x + translation.x][input.y + translation.y][input.z + translation.z];
                    Genes[input.x + translation.x][input.y + translation.y][input.z + translation.z] = temp;

                    if (typeParams[Genes[input.x][input.y - 1][input.z]].door)
                    {
                        temp = Genes[input.x][input.y - 1][input.z];
                        Genes[input.x][input.y - 1][input.z] = Genes[input.x + translation.x][input.y + translation.y - 1][input.z + translation.z];
                        Genes[input.x + translation.x][input.y + translation.y - 1][input.z + translation.z] = temp;
                    }

                    if (typeParams[Genes[input.x][input.y + 1][input.z]].door)
                    {
                        temp = Genes[input.x][input.y + 1][input.z];
                        Genes[input.x][input.y + 1][input.z] = Genes[input.x + translation.x][input.y + translation.y + 1][input.z + translation.z];
                        Genes[input.x + translation.x][input.y + translation.y + 1][input.z + translation.z] = temp;
                    }
                }
            }

            return Genes;
        }

        public int[][][] CreateDoor(int[][][] Genes, Vector3Int input, int newType)
        {
            if (typeParams[Genes[input.x][input.y][input.z]].wall && typeParams[Genes[input.x][input.y + 1][input.z]].wall
                &&(!typeParams[Genes[input.x][input.y][input.z]].door && !typeParams[Genes[input.x][input.y + 1][input.z]].door))
            {
                Genes[input.x][input.y][input.z] = newType;
                Genes[input.x][input.y + 1][input.z] = newType;
            }

            return Genes;
        }

        public int[][][] CollapseDoor(int[][][] Genes, Vector3Int input)
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
    }
}