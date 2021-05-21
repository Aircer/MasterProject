using System.Collections.Generic;
using System;
using UtilitiesGenetic;
using mVectors;

namespace MapTileGridCreator.Core
{
	public class TestGenetics
	{
		public EvolutionaryAlgoParams algoParams;
		public GeneticAlgorithm ga;

		private SharpNeatLib.Maths.FastRandom randomFast;
		private System.Random randomSystem;
		private int numberTypeCells;
		private Vector3Int sizeDNA;
		private TypeParams[] typeParams;
		private TypeParams[] cellsInfos;
		private int fitnessType;
		private int numberBlocks;

		public void StartGenetics(Vector3Int size, TypeParams[] cellsInfos, WaypointParams[][][] waypointParams, EvolutionaryAlgoParams algoParams, int fitnessType)
		{
			sizeDNA = size;
			randomFast = new SharpNeatLib.Maths.FastRandom();
			randomSystem = new System.Random();
			numberTypeCells = cellsInfos.Length;
			this.cellsInfos = cellsInfos;
			typeParams = new TypeParams[numberTypeCells + 1];
			this.algoParams = algoParams;
			this.fitnessType = fitnessType;
			numberBlocks = (size.x - 2)* (size.y - 2) * (size.z - 2);

			SetTypeCellParams(cellsInfos);

			ga = new GeneticAlgorithm(algoParams, sizeDNA, randomSystem, randomFast, FitnessFunction, waypointParams, typeParams);
		}

		private void SetTypeCellParams(TypeParams[] cellsInfos)
		{
			typeParams[0] = new TypeParams();

			for (int i = 0; i < numberTypeCells; i++)
			{
				typeParams[i + 1] = cellsInfos[i];
			}
		}


		public void UpdateGenetics()
		{
			ga.NewGeneration();
		}

		public WaypointParams[][][] GetBestClusters()
        {
			ga.ClassifyPopulation();

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
					sizeMin = 3; 
					break;
				case 1:
					sizeMin = 12;
					break;
				case 2:
					sizeMin = 24;
					break;
				case 3:
					sizeMin = 48;
					break;
			}

			nbCuboidsPossible = numberBlocks / sizeMin;

			foreach (Cuboid cuboid in phenotype.cuboids)
			{
				if (cuboid.cells.Count > sizeMin)
					nbCuboidsCorrectSize++;
				nbCuboids++;
			}

			if (nbCuboids > 0)
				ratioGoodCuboids = nbCuboidsCorrectSize / nbCuboidsPossible;
			else
				ratioGoodCuboids = 0;

			finalScore = ratioGoodCuboids; //Mathf.Pow(2, ratioGoodCuboids) - 1;

			return finalScore;
		}
	}
}
