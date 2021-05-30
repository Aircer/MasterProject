using System.Collections.Generic;
using System;
using UtilitiesGenetic;
using mVectors;

namespace Genetics
{
	public class GeneticController
	{
		private GeneticAlgorithm ga;
		private TypeParams[] typeParams;
		private int fitnessType;
		private int numberBlocks;

		public void StartGenetics(Vector3Int size, TypeParams[] cellsInfos, int[][][] waypointParams, EvolutionaryAlgoParams algoParams, SharpNeatLib.Maths.FastRandom randomFast, int fitnessType)
		{
			this.fitnessType = fitnessType;
			numberBlocks = (size.x - 2)* (size.y - 2) * (size.z - 2);

			typeParams = new TypeParams[cellsInfos.Length + 1];
			SetTypeCellParams(cellsInfos);

			ga = new GeneticAlgorithm(algoParams, size, FitnessFunction, waypointParams, typeParams, randomFast);
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

			UnityEngine.Debug.Log("Fitness Type: " + fitnessType 
				+ "; Number walkable zones: " + ga.oldPopulation[0].phenotype.paths.Count 
				+ "; Fitness: " + ga.oldPopulation[0].Fitness);

			return ga.oldPopulation[0].Genes;
		}

		private float FitnessFunction(int index)
		{
			float finalScore = 0; float nbCuboidsCorrectSize = 0; float nbCuboids = 0;
			float ratioGoodCuboids = 0;  float sizeMin = 0; float nbCuboidsPossible;
			Phenotype phenotype = ga.oldPopulation[index].phenotype;

			switch(fitnessType)
            {
				case 0:
					sizeMin = 0.001f; 
					break;
				case 1:
					sizeMin = 0.005f;
					break;
				case 2:
					sizeMin = 0.01f;
					break;
				case 3:
					sizeMin = 0.2f;
					break;
			}

			nbCuboidsPossible = 1/sizeMin;

			foreach (Cuboid cuboid in phenotype.cuboids)
			{
				if ((cuboid.max.x - cuboid.min.x) * (cuboid.max.y - cuboid.min.y) * (cuboid.max.z - cuboid.min.z) > numberBlocks * sizeMin
					&& (cuboid.max.y - cuboid.min.y) > 1 && (cuboid.max.y - cuboid.min.y) < 5)
					nbCuboidsCorrectSize++;
				nbCuboids++;
			}

			if (nbCuboids > 0)
				ratioGoodCuboids = nbCuboidsCorrectSize / nbCuboidsPossible;
			else
				ratioGoodCuboids = 0;

			finalScore = ratioGoodCuboids; //Mathf.Pow(2, ratioGoodCuboids) - 1;

			foreach (Path path in phenotype.paths)
			{
				if (path.cells.Count < 3)
					finalScore = 0; 
			}

			if (phenotype.paths.Count == 0)
				finalScore = 0;

			return finalScore;
		}
	}
}
