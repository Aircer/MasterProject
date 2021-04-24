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
			foreach (CellInformation key in cluster.GetWaypointsDico().Keys)
			{
				listCellsInfo.Add(key);
			}
			ga = new GeneticAlgorithm(populationSize, size_grid, random, GetRandomCell, FitnessFunction, elitism, cluster.GetWaypoints(),mutationRate);
		}

		public void UpdateGenetics()
		{
			ga.NewGeneration();
		}

		public List<WaypointCluster> GetBestClusters(int nbSuggestions)
        {
			List<WaypointCluster> bestClusters = new List<WaypointCluster>();

			for (int i = 0; i < nbSuggestions; i++)
			{
				bestClusters.Add(new WaypointCluster(ga.Population[i].Genes));
			}

			return bestClusters;
		}

		private CellInformation GetRandomCell()
		{
			int i = random.Next(listCellsInfo.Count);
			return listCellsInfo[i];
		}

		private float FitnessFunction(int index)
		{
			float score = 0f;
			float finalScore = 0;
			float symTotal = 0.01f;
			DNA dna = ga.Population[index];

			for (int l = 0; l < sizeGrid.y; l++)
			{
				int k = sizeGrid.x - 1;
				for (int i = 0; i < sizeGrid.x / 2; i++, k--)
				{
					for (int j = 0; j < sizeGrid.z; j++)
					{

						if (dna.Genes[i, l, j] == dna.Genes[k, l, j])
						{
							score += 1;
						}

						symTotal++;
					}
				}
			}

			score /= symTotal;
			finalScore = score;
			score = 0f; symTotal = 0.01f;

			for (int l = 0; l < sizeGrid.y; l++)
			{
				int k = sizeGrid.z - 1;
				for (int i = 0; i < sizeGrid.z / 2; i++, k--)
				{
					for (int j = 0; j < sizeGrid.x; j++)
					{

						if (dna.Genes[j, l, i] == dna.Genes[j, l, k])
						{
							score += 1;
						}

						symTotal++;
					}
				}
			}

			score /= symTotal;
			finalScore = Mathf.Pow(2, (score + finalScore) / 2) - 1;

			return finalScore;
		}
	}
}
