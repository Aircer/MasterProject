using System.Collections.Generic;
using System.Linq;
using System;
using UtilitiesGenetic;

namespace Genetics
{
    public static class GeneticGetCuboid
    {
        public static Vector3Int size;
        public static TypeParams[] typeParams;
        public static int[][][] Genes;
        public static HashSet<Vector3Int> cellsAlreadyPicked;
        public static void InitMutations(Vector3Int sizeDNA, TypeParams[] tp, int[][][] g)
        {
            size = sizeDNA;
            typeParams = tp;
            Genes = g;
        }

        public static Vector3Int Grow_ones(Vector3Int input, HashSet<Vector3Int> cellsPicked)
        {
            cellsAlreadyPicked = cellsPicked;
            Vector3Int urBot = new Vector3Int(0, 0, 0);
            Vector3Int urUp = new Vector3Int(0, 0, 0);

            int x_max = size.x - 1;
            int x; int y = input.y; int z = input.z;

            while (z < size.z && CellInWall(input.x, y, z))
            {
                x = input.x;

                while (x + 1 <= x_max && CellInWall(x + 1, y, z))
                {
                    x++;
                }

                x_max = x;

                if ((x < x_max && z > input.z) || urUp.x != 0)
                {
                    if (x_max < size.x && Volume(input, new Vector3Int(x+1, y+1, z+1)) > Volume(input, urUp))
                    {
                        urUp.x = x+1; urUp.y = y+1; urUp.z = z+1;
                    }
                }
                else
                {
                    if (x_max < size.x && Volume(input, new Vector3Int(x+1, y+1, z+1)) > Volume(input, urBot))
                    {
                        urBot.x = x+1; urBot.y = y+1; urBot.z = z+1;
                    }
                }
                z++;
            }

            urBot.y = GetYMax(input, urBot);
            urUp.y = GetYMax(input, urUp);

            Vector3Int ur = GetBestWall(input, urBot, urUp);

            UpdateCellsBorders(input, ref ur);

            //UpdateCellsUp(input, ref ur);

            return ur;
        }

        private static void UpdateCellsUp(Vector3Int input, ref Vector3Int ur)
        {
            HashSet<Vector3Int> cellsBorder = new HashSet<Vector3Int>();

            bool sideXPos; bool sideXNeg;
            bool sideZPos; bool sideZNeg;
            bool sideYPos;
            
            for (int x = input.x + 1; x < ur.x; x++)
            {
                sideXPos = true;
                sideXNeg = true;
                sideYPos = true;

                for (int y = input.y; y < ur.y; y++)
                {
                    if (!CellInWall(x, y, ur.z))
                        sideXPos = false;

                    if (!CellInWall(x, y, input.z - 1))
                        sideXNeg = false;
                }

                for (int z = input.z; z < ur.z; z++)
                {
                    if (!CellInWall(x, ur.y, z))
                        sideYPos = false;
                }

                if ((sideXPos || sideXNeg) && sideYPos)
                    ur.x = x - 1;
            }

            sideYPos = true;

            for (int z = input.z + 1; z < ur.z; z++)
            {
                sideZPos = true;
                sideZNeg = true;
                sideYPos = true;

                for (int y = input.y; y < ur.y; y++)
                {
                    if (!CellInWall(ur.x, y, z))
                        sideZPos = false;

                    if (!CellInWall(input.x - 1, y, z))
                        sideZNeg = false;
                }

                for (int x = input.x; x < ur.x; x++)
                {
                    if (!CellInWall(x, ur.y, z))
                        sideYPos = false;
                }

                if ((sideZPos || sideZNeg) && sideYPos)
                    ur.z = z;
            }
        }

        private static void UpdateCellsBorders(Vector3Int input, ref Vector3Int ur)
        {
            HashSet<Vector3Int> cellsBorder = new HashSet<Vector3Int>();

            if ((ur.x - input.x) < (ur.z - input.z))
            {
                if (ur.x < size.x)
                {
                    for (int y = input.y; y < ur.y; y++)
                    {
                        int holeMaxX = 0;
                        int newMax = 0;

                        for (int z = input.z; z < ur.z; z++)
                        {
                            if (CellInWall(ur.x, y, z))
                                holeMaxX++;
                            else
                            {
                                holeMaxX = 0;
                                newMax = z + 1;
                            }

                            if (holeMaxX >= ur.x - input.x && newMax < ur.z && newMax > input.z)
                            {
                                ur.z = newMax;
                                break;
                            }
                        }
                    }
                }

                if (input.x > 1)
                {
                    for (int y = input.y; y < ur.y; y++)
                    {
                        int holeMaxX = 0;
                        int newMax = 0;

                        for (int z = input.z; z < ur.z; z++)
                        {
                            if (CellInWall(input.x - 1, y, z))
                                holeMaxX++;
                            else
                            {
                                holeMaxX = 0;
                                newMax = z + 1;
                            }

                            if (holeMaxX >= ur.x - input.x && newMax < ur.z && newMax > input.z)
                            {
                                ur.z = newMax;
                                break;
                            }
                        }
                    }
                }
            }
            else
            {
                if (ur.z < size.z)
                {
                    for (int y = input.y; y < ur.y; y++)
                    {
                        int holeMaxZ = 0;
                        int newMax = 0;

                        for (int x = input.x; x < ur.x; x++)
                        {
                            if (CellInWall(x, y, ur.z))
                                holeMaxZ++;
                            else
                            {
                                holeMaxZ = 0;
                                newMax = x + 1;
                            }

                            if (holeMaxZ >= ur.z - input.z && newMax < ur.x && newMax > input.x)
                            {
                                ur.x = newMax;
                                break;
                            }
                        }
                    }
                }

                if (input.z > 1)
                {
                    for (int y = input.y; y < ur.y; y++)
                    {
                        int holeMaxZ = 0;
                        int newMax = 0;

                        for (int x = input.x; x < ur.x; x++)
                        {
                            if (CellInWall(x, y, input.z - 1))
                                holeMaxZ++;
                            else
                            {
                                holeMaxZ = 0;
                                newMax = x + 1;
                            }

                            if (holeMaxZ >= ur.z - input.z && newMax < ur.x && newMax > input.x)
                            {
                                ur.x = newMax;
                                break;
                            }
                        }
                    }
                }
            }

            int oldNumberEmpty = 0;
            int numberEmpty = 0;

            for (int y = input.y; y < ur.y; y++)
            {

                for (int x = input.x; x < ur.x; x++)
                {
                    if (ur.z < size.z && CellInWall(x, y, ur.z))
                        numberEmpty++;

                    if (input.z > 1 && CellInWall(x, y, input.z - 1))
                        numberEmpty++;
                }

                for (int z = input.z; z < ur.z; z++)
                {
                    if (ur.x < size.x && CellInWall(ur.x, y, z))
                        numberEmpty++;

                    if (input.x > 1 && CellInWall(input.x - 1, y, z))
                        numberEmpty++;
                }

                if (numberEmpty != oldNumberEmpty && ur.y > y && y > input.y)
                {
                    ur.y = y;
                }

                oldNumberEmpty = numberEmpty;
                numberEmpty = 0;
            }
        }

        private static int GetYMax(Vector3Int input, Vector3Int ur)
        {

            for (int y = input.y + 1; y < size.y; y++)
            {
                for (int x = input.x; x < ur.x; x++)
                {
                    for (int z = input.z; z < ur.z; z++)
                    {
                        if (!CellInWall(x, y, z))
                            return y;
                    }
                }
            }

            return size.y;
        }

        private static Vector3Int GetBestWall(Vector3Int input, Vector3Int urBot, Vector3Int urUp)
        {
            Vector3Int ur;

            int sliceBot = GetSlice(input, urBot);
            int sliceUp = GetSlice(input, urUp);

            if (sliceBot == sliceUp)
            {
                if (Volume(input, urBot) > Volume(input, urUp))
                    ur = urBot;
                else
                    ur = urUp;
            }
            else
            {
                if (sliceBot > sliceUp)
                    ur = urBot;
                else
                    ur = urUp;
            }

            return ur;
        }

        private static int GetSlice(Vector3Int input, Vector3Int ur)
        {
            int thickness = 0;
            
            if(ur.x - input.x > 0 || ur.z - input.z > 0)
                thickness = (ur.x - input.x) > (ur.z - input.z) ? (ur.z - input.z) : (ur.x - input.x);

            return thickness * (ur.y - input.y);
        }

        private static int Volume(Vector3Int ll, Vector3Int ur)
        {
            if ((ur.x - ll.x) > 0 && (ur.y - ll.y) > 0 && (ur.z - ll.z) > 0)
                return (ur.x - ll.x) * (ur.y - ll.y) * (ur.z - ll.z);
            else
                return 0;
        }

        private static bool CellInWall(int x, int y, int z)
        {
            if (typeParams[Genes[x][y][z]].wall && !cellsAlreadyPicked.Contains(new Vector3Int(x, y, z)))
                return true;
            else
                return false;
        }
    }
}


