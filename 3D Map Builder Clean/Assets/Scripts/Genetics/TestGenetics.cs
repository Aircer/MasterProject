using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MapTileGridCreator.Core
{
	public class TestGenetics 
	{
		public int populationSize = 200;
		public float mutationRate = 0.05f;
		public int elitism = 5;

		public GeneticAlgorithm ga;
		private System.Random random;
		private List<CellInformation> listCellsInfo = new List<CellInformation>();
		private CellInformation targetCellInfo = null;
		private float numberCells;
		private Vector3Int sizeGrid;

		public void StartGenetics(Vector3Int size_grid, WaypointCluster cluster)
		{
			numberCells = size_grid.x * size_grid.y * size_grid.z;
			sizeGrid = new Vector3Int(size_grid.x, size_grid.y, size_grid.z);
			random = new System.Random();
			listCellsInfo.Clear();
			listCellsInfo.Add(null);
			foreach (Waypoint wp in cluster.GetWaypoints())
			{
				if(!listCellsInfo.Contains(wp.type))
					listCellsInfo.Add(wp.type);
			}
			ga = new GeneticAlgorithm(populationSize, size_grid, random, GetRandomType, FitnessFunction, elitism, cluster, mutationRate);
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

				//UnityEngine.Debug.Log(ga.oldPopulation[j].Fitness + "  " +  j);
				bestClusters.Add(new WaypointCluster(ga.oldPopulation[j].Genes));
				j++;
			}

			return bestClusters;
		}

		private CellInformation GetRandomType()
		{
			int i = random.Next(listCellsInfo.Count);
			return listCellsInfo[i];
		}

		private float FitnessFunction(int index)
		{
			float finalScore = 0;
			float scoreX = 0f; float symTotalX = sizeGrid.x*sizeGrid.y*sizeGrid.z/2;
			DNA dna = ga.oldPopulation[index];

			for (int l = 0; l < sizeGrid.y; l++)
			{
				int k = sizeGrid.x - 1;
				for (int i = 0; i < sizeGrid.x / 2; i++, k--)
				{
					for (int j = 0; j < sizeGrid.z; j++)
					{
						if (dna.Genes[i, l, j].type == dna.Genes[k, l, j].type || (dna.Genes[i, l, j].type == null && dna.Genes[k, l, j].type == null))
						{
								scoreX += 1;
						}
					}
				}
			}

			scoreX /= symTotalX;
			//scoreX = Mathf.Pow(2, scoreX) - 1;

			float scoreZ = 0f; int symTotalZ = sizeGrid.x * sizeGrid.y * sizeGrid.z/2;

			for (int l = 0; l < sizeGrid.y; l++)
			{
				int k = sizeGrid.z - 1;
				for (int i = 0; i < sizeGrid.z / 2; i++, k--)
				{
					for (int j = 0; j < sizeGrid.x; j++)
					{
						if (dna.Genes[j, l, i].type == dna.Genes[j, l, k].type || (dna.Genes[j, l, i].type == null && dna.Genes[j, l, k].type == null))
						{
								scoreZ += 1;
						}
					}
				}
			}

			scoreZ /= symTotalZ;
			//scoreZ = Mathf.Pow(2, scoreZ) - 1;

			finalScore = Mathf.Pow(2, (scoreX + scoreZ) / 2) - 1;

			return finalScore;
		}
	}
}
