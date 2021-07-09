using System.Collections.Generic;
using System;
using UtilitiesGenetic;

namespace Genetics
{
	public class GeneticController
	{
		private GeneticAlgorithm ga;
		private TypeParams[] typeParams;

		public void StartGenetics(Vector3Int size, TypeParams[] cellsInfos, int[][][] waypointParams, EvolutionaryAlgoParams algoParams,
			SharpNeatLib.Maths.FastRandom randomFast)
		{
			typeParams = cellsInfos;

			Mutations.InitMutations(size, randomFast, typeParams, algoParams.mutationType);

			ga = new GeneticAlgorithm(algoParams, size, waypointParams, typeParams, randomFast);
		}

		public void UpdateGenetics()
		{
			ga.NewGeneration();
		}

		public List<int[][][]> GetBestClusters(int nbBestFit)
        {
			List<int[][][]> bestClusters = new List<int[][][]>();

			ga.ClassifyPopulation();
			float previousFitness = 2;
			int j = 0;
			for (int i = 0; i < nbBestFit; i++)
			{
				while(ga.oldPopulation[j].fitness.total == previousFitness && j < ga.oldPopulation.Length - nbBestFit)
                {
					j++;
                }

				previousFitness = ga.oldPopulation[j].fitness.total;
				UnityEngine.Debug.Log("Total: " + ga.oldPopulation[j].fitness.total
					+ "Walls: " + ga.oldPopulation[j].fitness.walls
					+ "WA: " + ga.oldPopulation[j].fitness.walkingAreas
					+ "Paths: " + ga.oldPopulation[j].fitness.pathfinding);
				bestClusters.Add(ga.oldPopulation[j].Genes);
				j++;
			}

			return bestClusters;
		}

		public float GetBestTotalFitness(int nbBestFit)
		{
			ga.ClassifyPopulation();

			return ga.oldPopulation[nbBestFit - 1].fitness.total;
		}

		public Fitness[][] GetFitness()
		{
			return ga.fitnessPopulation;
		}

		public Population[][] GetPopulations()
		{
			return ga.populations;
		}
	}
}
