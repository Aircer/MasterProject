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
			typeParams = new TypeParams[cellsInfos.Length + 1];
			SetTypeCellParams(cellsInfos);

			Mutations.InitMutations(size, randomFast, typeParams);

			ga = new GeneticAlgorithm(algoParams, size, waypointParams, typeParams, randomFast);
		}

		private void SetTypeCellParams(TypeParams[] cellsInfos)
		{
			typeParams[0] = new TypeParams();
			typeParams[0].SetEmpty();

			for (int i = 0; i < typeParams.Length - 1; i++)
			{
				typeParams[i + 1] = cellsInfos[i];
			}
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
				+ "; Number neighbors: " + nbNeighbors
				+ "; Fitness: " + ga.oldPopulation[0].fitness.difference);

			return ga.oldPopulation[0].Genes;
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
