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

			//float goodWallCells = ga.oldPopulation[j].phenotype.cellsWalls - (ga.oldPopulation[j].phenotype.cellsWallsCrowded + ga.oldPopulation[j].phenotype.cellsWallsSolo);
			//float goodWallCellsRatio = goodWallCells / ga.oldPopulation[j].phenotype.cellsWalls;
			UnityEngine.Debug.Log(ga.oldPopulation[0].Fitness);
			//UnityEngine.Debug.Log(goodWallCellsRatio);

			return bestClusters;
		}

		private float FitnessFunction(int index)
		{
			float finalScore = 0;
			float scoreX = 0f; float symTotal = sizeDNA.x*sizeDNA.y*sizeDNA.z/2;
			DNA dna = ga.oldPopulation[index];

			if (dna.phenotype.cellsWalls == 0)
				return 0;

			int l = sizeDNA.x -2;
			for (int i = 1; i < sizeDNA.x - 1; i++, l--)
			{
				for (int j = 1; j < sizeDNA.y - 1; j++)
				{
					for (int k = 1; k < sizeDNA.z - 1; k++)
					{
						if (dna.Genes[i][j][k].type != 0 && (dna.Genes[i][j][k].type == dna.Genes[l][j][k].type))
						{
								scoreX++;
						}
					}
				}
			}

			scoreX /= dna.phenotype.cellsWalls;

			float scoreZ = 0f;

			l = sizeDNA.z - 2;
			for (int i = 1; i < sizeDNA.x - 1; i++, l--)
			{
				for (int j = 1; j < sizeDNA.y - 1; j++)
				{
					for (int k = 1; k < sizeDNA.z - 1; k++)
					{
						if (dna.Genes[i][j][k].type != 0 && (dna.Genes[i][j][k].type == dna.Genes[i][j][l].type))
						{
							scoreZ++;
						}
					}
				}
			}

			scoreZ /= dna.phenotype.cellsWalls;

			float sim = scoreX > scoreZ ? scoreX : scoreZ;
			//float sim = scoreX*scoreZ;
			float goodWallCells = dna.phenotype.cellsWalls - (dna.phenotype.cellsWallsCrowded + dna.phenotype.cellsWallsSolo);
			float goodWallCellsRatio = goodWallCells / dna.phenotype.cellsWalls;


			//finalScore = Mathf.Pow(2, Mathf.Pow(goodWallCellsRatio, 2) * sim) - 1;

			//finalScore = Mathf.Pow(2, sim) - 1;

			return sim;
		}
	}
}
