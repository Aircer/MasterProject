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

        public List<int[][][]> GetSuggestionsClusters(Vector3Int size, TypeParams[] cellsInfos,int[][][] waypointParams, 
            int nbSuggestions, EvolutionaryAlgoParams[] algoParams)
        {
            List<int[][][]> newWaypointsParams = new List<int[][][]>();
            fitness = new Fitness[nbSuggestions][][];
            populations = new Population[nbSuggestions][][];
            SharpNeatLib.Maths.FastRandom randomFast = new SharpNeatLib.Maths.FastRandom();

            for (int i = 0; i < algoParams.Length; i++)
            {
                GeneticController newGenetic = new GeneticController();
                newGenetic.StartGenetics(size, cellsInfos, waypointParams, algoParams[i], randomFast);

                int j = 0;
                while (j < algoParams[i].generations)
                {
                    newGenetic.UpdateGenetics();

                    if (newGenetic.GetBestTotalFitness(algoParams[i].nbBestFit) > algoParams[i].fitnessStop)
                        break;

                    j++;
                }
                
                newWaypointsParams.AddRange(newGenetic.GetBestClusters(algoParams[i].nbBestFit));
                fitness[i] = newGenetic.GetFitness();
                populations[i] = newGenetic.GetPopulations();
            }

            return newWaypointsParams;
        }
    }
}