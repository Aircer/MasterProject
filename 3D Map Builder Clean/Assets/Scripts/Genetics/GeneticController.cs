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

		public int[][][] GetBestClusters()
        {
			ga.ClassifyPopulation();

			int nbNeighbors = 0;
			
			foreach (WalkableArea wa in ga.oldPopulation[0].phenotype.walkableArea)
			{
				nbNeighbors += wa.neighborsArea.Count; 
			}

			UnityEngine.Debug.Log("Number walkable zones: " + ga.oldPopulation[0].phenotype.walkableArea.Count
				+ "; Number neighbors: " + nbNeighbors + "; Gen: " + ga.generation
				+ "; Fitness: " + ga.oldPopulation[0].fitness.total);

			return ga.oldPopulation[0].Genes;
		}

		public float GetBestTotalFitness()
		{
			ga.ClassifyPopulation();

			return ga.oldPopulation[0].fitness.total;
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
