using System.Collections.Generic;
using System.Linq;
using System;
using UtilitiesGenetic;
using System.Threading;
using System.Threading.Tasks;

namespace Genetics
{
    public class Init
    {
        //[run][generation][population]
        public Fitness[][][] fitness;
        public Population[][][] populations;

        private Vector3Int size;
        private TypeParams[] cellsInfos;
        private int[][][] waypointParams;
        private EvolutionaryAlgoParams[] algoParams;
        private List<int[][][]> newWaypointsParams;
        private int nbSuggestions;
        private GeneticController[] newGenetics;

        public Init(Vector3Int size, TypeParams[] cellsInfos, int[][][] waypointParams, 
            EvolutionaryAlgoParams[] algoParams, int numberSuggestions)
        {
            this.size = size;
            this.cellsInfos = cellsInfos;
            this.waypointParams = waypointParams;
            this.algoParams = algoParams;
            nbSuggestions = 4;

            fitness = new Fitness[nbSuggestions][][];
            populations = new Population[nbSuggestions][][];

            newGenetics = new GeneticController[numberSuggestions];

            SharpNeatLib.Maths.FastRandom randomFast = new SharpNeatLib.Maths.FastRandom();

            for (int i = 0; i < numberSuggestions; i++)
            {
                newGenetics[i] = new GeneticController();
                newGenetics[i].StartGenetics(size, cellsInfos, waypointParams, algoParams[i], randomFast);
            }
        }

        public int[][][] GetSuggestionsClusters(int i)
        {
            newWaypointsParams = new List<int[][][]>();

            int j = 0;
            while (j < algoParams[i].generations)
            {
                newGenetics[i].UpdateGenetics();

                if (newGenetics[i].GetBestTotalFitness(algoParams[i].nbBestFit) > algoParams[i].fitnessStop)
                    break;

                j++;
            }


            fitness[i] = newGenetics[i].GetFitness();
            populations[i] = newGenetics[i].GetPopulations();

            return newGenetics[i].GetBestClusters(algoParams[i].nbBestFit)[0];
        }

    }
}