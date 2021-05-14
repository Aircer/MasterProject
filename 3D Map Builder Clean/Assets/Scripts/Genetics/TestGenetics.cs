using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
		private CellInformation[] cellsInfos;

		public void StartGenetics(WaypointCluster cluster, EvolutionaryAlgoParams algoParams)
		{
			// Size of the DNA is size + 2 to have empty borders
			sizeDNA = new Vector3Int(cluster.size.x+2, cluster.size.y+2, cluster.size.z+2);
			randomFast = new SharpNeatLib.Maths.FastRandom();
			randomSystem = new System.Random();
			numberTypeCells = cluster.cellInfos.Count;
			cellsInfos = cluster.cellInfos.ToArray();
			typeParams = new TypeParams[numberTypeCells + 1];
			this.algoParams = algoParams;

			SetTypeCellParams(cluster);

			ga = new GeneticAlgorithm(algoParams, sizeDNA, randomSystem, randomFast, FitnessFunction, cluster, typeParams);
		}

		private void SetTypeCellParams(WaypointCluster cluster)
		{
			typeParams[0] = new TypeParams();

			for (int i = 0; i < numberTypeCells; i++)
			{
				typeParams[i + 1] = cluster.cellInfos[i].typeParams;
			}
		}


		public void UpdateGenetics()
		{
			ga.NewGeneration();
		}

		public List<WaypointCluster> GetBestClusters(int nbSuggestions)
        {
			List<WaypointCluster> bestClusters = new List<WaypointCluster>();
			int j = 0;
			ga.ClassifyPopulation();
			for (int i = 0; i < nbSuggestions; i++)
			{
				if (j > 0)
					while (j < ga.oldPopulation.Length && ga.oldPopulation[j].Fitness == ga.oldPopulation[j - 1].Fitness)
						j++;

				if (j == ga.oldPopulation.Length)
					j = ga.oldPopulation.Length - 1;

				bestClusters.Add(new WaypointCluster(new Vector3Int(ga.oldPopulation[j].sizeDNAx_wtBorder+1, ga.oldPopulation[j].sizeDNAy_wtBorder+1,
																	ga.oldPopulation[j].sizeDNAz_wtBorder+1), ga.oldPopulation[j].Genes, cellsInfos));
				j++;
			}

			return bestClusters;
		}

		private float FitnessFunction(int index)
		{
			float finalScore = 0;
			float scoreX = 0f; float symTotal = sizeDNA.x*sizeDNA.y*sizeDNA.z/2;
			DNA dna = ga.oldPopulation[index];
			Phenotype phenotype = ga.oldPopulation[index].phenotype;

			if (dna.phenotype.cellsWalls == 0)
				return 0;

			int sizeDNAx_minus = sizeDNA.x - 1; int sizeDNAy_minus = sizeDNA.y - 1; int sizeDNAz_minus = sizeDNA.z - 1;
			int sizeDNAx_half = sizeDNA.x / 2; int sizeDNAz_half = sizeDNA.z / 2;

			for (int l = 1; l < sizeDNAy_minus; l++)
			{
				int k = sizeDNAx_minus;
				for (int i = 0; i < sizeDNAx_half; i++, k--)
				{
					for (int j = 1; j < sizeDNAz_minus; j++)
					{
						if (dna.Genes[i][l][j].type != 0 && (dna.Genes[i][l][j].type == dna.Genes[k][l][j].type))
						{
								scoreX += 2;
						}
					}
				}
			}

			scoreX /= dna.phenotype.cellsWalls;

			float scoreZ = 0f; 

			for (int l = 1; l < sizeDNAy_minus; l++)
			{
				int k = sizeDNA.z - 1;
				for (int i = 1; i < sizeDNAz_half; i++, k--)
				{
					for (int j = 1; j < sizeDNAx_minus; j++)
					{
						if (dna.Genes[i][l][j].type != 0 && (dna.Genes[j][l][i].type == dna.Genes[j][l][k].type))
						{
								scoreZ += 2;
						}
					}
				}
			}

			scoreZ /= dna.phenotype.cellsWalls;

			int goodWallCells = dna.phenotype.cellsWalls - (dna.phenotype.cellsWallsCrowded + dna.phenotype.cellsWallsSolo);
			float goodWallCellsRatio = goodWallCells / dna.phenotype.cellsWalls;

			if (goodWallCellsRatio < 0.9f)
				finalScore = 0;
			else
				finalScore = Mathf.Pow(2, (scoreX + scoreZ) / 2) - 1;

			return finalScore;
		}
	}
}
