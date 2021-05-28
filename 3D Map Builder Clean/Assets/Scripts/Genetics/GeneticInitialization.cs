using System.Collections.Generic;
using System.Linq;
using System;
using UtilitiesGenetic;
using mVectors;

namespace Genetics
{
    public static class Init
    {
        public static List<int[][][]> GetSuggestionsClusters(int sizeX, int sizeY, int sizeZ, TypeParams[] cellsInfos, int[][][] waypointParams, int nbSuggestions, EvolutionaryAlgoParams algoParams)
        {
            List<int[][][]> newWaypointsParams = new List<int[][][]>();
            Vector3Int size = new Vector3Int(sizeX, sizeY, sizeZ);

            for (int i = 0; i < nbSuggestions; i++)
            {
                GeneticController newGenetic = new GeneticController();
                newGenetic.StartGenetics(size, cellsInfos, waypointParams, algoParams, i);

                for (int j = 0; j < algoParams.generations; j++)
                {
                    newGenetic.UpdateGenetics();
                }

                newWaypointsParams.Add(newGenetic.GetBestClusters());
            }

            return newWaypointsParams;
        }
    }
}