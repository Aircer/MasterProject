using System.Collections.Generic;
using UnityEngine;

namespace MapTileGridCreator.Core
{
    public static class IA
    {
        public static List<WaypointCluster> GetSuggestionsClusters(WaypointCluster cluster, int nbSuggestions, EvolutionaryAlgoParams algoParams)
        {
            TestGenetics newGenetic = new TestGenetics();
            newGenetic.StartGenetics(cluster, algoParams);

            for (int j = 0; j < algoParams.generations; j++)
            {
                newGenetic.UpdateGenetics();
            }

            return newGenetic.GetBestClusters(nbSuggestions);
        }

        public static Phenotype GetPhenotype(int sizeX, int sizeY, int sizeZ, WaypointParams[][][] Genes, TypeParams[] typeParams)
        {
            Phenotype newPhenotype = new Phenotype();
            newPhenotype.cellsWalls = new int();
            newPhenotype.cellsWallsSolo = new int();
            newPhenotype.cellsWallsCrowded = new int();
            
            for (int x = 1; x < sizeX; x++)
            {
                for (int y = 1; y < sizeY; y++)
                {
                    for (int z = 1; z < sizeZ; z++)
                    {
                        if (typeParams[Genes[x][y][z].type].wall)
                        {
                            if (!typeParams[Genes[x - 1][y][z].type].wall && !typeParams[Genes[x + 1][y][z].type].wall
                             && !typeParams[Genes[x][y][z - 1].type].wall && !typeParams[Genes[x][y][z + 1].type].wall)
                            {
                                newPhenotype.cellsWallsSolo++;
                            }

                            if ((typeParams[Genes[x - 1][y][z].type].wall && typeParams[Genes[x - 1][y][z - 1].type].wall && typeParams[Genes[x][y][z - 1].type].wall)
                             || (typeParams[Genes[x - 1][y][z].type].wall && typeParams[Genes[x - 1][y][z + 1].type].wall && typeParams[Genes[x][y][z + 1].type].wall)
                             || (typeParams[Genes[x + 1][y][z].type].wall && typeParams[Genes[x + 1][y][z - 1].type].wall && typeParams[Genes[x][y][z - 1].type].wall)
                             || (typeParams[Genes[x + 1][y][z].type].wall && typeParams[Genes[x + 1][y][z + 1].type].wall && typeParams[Genes[x][y][z + 1].type].wall))
                            {
                                newPhenotype.cellsWallsCrowded++;
                            }

                            newPhenotype.cellsWalls++;
                        }
                    }
                }
            }

            return newPhenotype;
        }
    }
}