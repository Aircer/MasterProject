using System.Collections.Generic;
using System.Linq;
using System;
using UtilitiesGenetic;

namespace Genetics
{
    public class Init
    {
        //[run][generation][population]
        public Fitness[][][] fitness;
        public Population[][][] populations;

        public List<int[][][]> GetSuggestionsClusters(int sizeX, int sizeY, int sizeZ, TypeParams[] cellsInfos,int[][][] waypointParams, 
            int nbSuggestions, EvolutionaryAlgoParams algoParams)
        {
            List<int[][][]> newWaypointsParams = new List<int[][][]>();
            fitness = new Fitness[nbSuggestions][][];
            populations = new Population[nbSuggestions][][];
            Vector3Int size = new Vector3Int(sizeX, sizeY, sizeZ);
            SharpNeatLib.Maths.FastRandom randomFast = new SharpNeatLib.Maths.FastRandom();

            for (int i = 0; i < nbSuggestions; i++)
            {
                GeneticController newGenetic = new GeneticController();
                newGenetic.StartGenetics(size, cellsInfos, waypointParams, algoParams, randomFast);

                for (int j = 0; j < algoParams.generations; j++)
                {
                    newGenetic.UpdateGenetics();
                }

                newWaypointsParams.Add(newGenetic.GetBestClusters());
                fitness[i] = newGenetic.GetFitness();
                populations[i] = newGenetic.GetPopulations();
            }

            return newWaypointsParams;
        }
    }
}