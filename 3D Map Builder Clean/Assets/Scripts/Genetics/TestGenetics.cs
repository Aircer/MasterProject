using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NumSharp;
using MathNet.Numerics.LinearAlgebra;

namespace MapTileGridCreator.Core
{
	public class TestGenetics 
	{
		public int populationSize = 20;
		public float mutationRate = 0.01f;
		public int elitism = 5;

		public GeneticAlgorithm ga;
		private System.Random random;
		private int numberTypeCells;
		private float numberCells;
		private Vector3Int sizeGrid;
		private TypeParams[] typeParams;
		private List<CellInformation> cellsInfos;
		public void StartGenetics(Vector3Int size_grid, WaypointCluster cluster)
		{
			numberCells = size_grid.x * size_grid.y * size_grid.z;
			sizeGrid = new Vector3Int(size_grid.x+2, size_grid.y+2, size_grid.z+2);
			random = new System.Random();
			numberTypeCells = cluster.cellInfos.Count;
			typeParams = new TypeParams[numberTypeCells + 1];

			typeParams[0].ground = false;
			typeParams[0].size = new Vector3Int(1,1,1);
			typeParams[0].blockPath = false;
			typeParams[0].wall = false;
			typeParams[0].floor = false;

			for (int i=0; i < numberTypeCells; i++)
			{
				typeParams[i + 1].ground = cluster.cellInfos[i].ground;
				typeParams[i + 1].size = cluster.cellInfos[i].size;
				typeParams[i + 1].blockPath = cluster.cellInfos[i].blockPath;
				typeParams[i + 1].wall = cluster.cellInfos[i].wall;
				typeParams[i + 1].floor = cluster.cellInfos[i].floor;
			}
			this.cellsInfos = cluster.cellInfos;

			ga = new GeneticAlgorithm(populationSize, size_grid, random, GetRandomType, FitnessFunction, elitism, cluster, typeParams, mutationRate);
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
				bestClusters.Add(new WaypointCluster(ga.oldPopulation[j].sizeDNA, ga.oldPopulation[j].Genes, cellsInfos));
				j++;
			}
			
			Debug.Log("Walls X: " + ga.oldPopulation[0].phenotype.walls_x.Count);
			Debug.Log("Walls Z: " + ga.oldPopulation[0].phenotype.walls_z.Count);
			Debug.Log("Blocks Solo: " + ga.oldPopulation[0].phenotype.blocksSolo.Count);
			//Debug.Log("Layer Types: " + ga.oldPopulation[0].typeMatrix[1]);

			var m = ga.oldPopulation[0].typeMatrix[1];
			var m_twist = Matrix<double>.Build.Random(sizeGrid.x, sizeGrid.z);

			int xBis = sizeGrid.x - 1;
			for (int x = 0; x < sizeGrid.x/2; x++, xBis--)
			{
				m_twist.SetRow(x, m.Row(xBis));
				m_twist.SetRow(xBis, m.Row(x));
			}

			if(sizeGrid.x % 2 == 1)
				m_twist.SetRow(sizeGrid.x/2, m.Row(sizeGrid.x / 2));

			m_twist = m_twist.PointwiseDivide(m);

			UnityEngine.Debug.Log(m_twist);

			return bestClusters;
		}

		private int GetRandomType()
		{
			return random.Next(numberTypeCells);
		}

		private float FitnessFunction(int index)
		{
			float finalScore = 0;
			float scoreX = 0f; float symTotalX = sizeGrid.x*sizeGrid.y*sizeGrid.z/2;
			DNA dna = ga.oldPopulation[index];

			//Debug.Log(dna.phenotype.walls_x.Count);
			
			for (int l = 1; l < sizeGrid.y-1; l++)
			{
				int k = sizeGrid.x - 1;
				for (int i = 0; i < sizeGrid.x / 2; i++, k--)
				{
					for (int j = 1; j < sizeGrid.z-1; j++)
					{
						if (dna.Genes[i][l][j].type == dna.Genes[k][l][j].type)
						{
								scoreX += 1;
						}
					}
				}
			}

			scoreX /= symTotalX;
			//scoreX = Mathf.Pow(2, scoreX) - 1;

			float scoreZ = 0f; int symTotalZ = sizeGrid.x * sizeGrid.y * sizeGrid.z/2;

			for (int l = 1; l < sizeGrid.y-1; l++)
			{
				int k = sizeGrid.z - 1;
				for (int i = 1; i < sizeGrid.z / 2; i++, k--)
				{
					for (int j = 1; j < sizeGrid.x-1; j++)
					{
						if (dna.Genes[j][l][i].type == dna.Genes[j][l][k].type)
						{
								scoreZ += 1;
						}
					}
				}
			}

			scoreZ /= symTotalZ;
			//scoreZ = Mathf.Pow(2, scoreZ) - 1;

			finalScore = Mathf.Pow(2, (scoreX + scoreZ) / 2) - 1;

			
			int nbWalls = dna.phenotype.walls_x.Count + dna.phenotype.walls_z.Count;
			int bigWalls = 0;

			foreach (Wall wallX in dna.phenotype.walls_x)
			{
				if (wallX.indexes.Count > 3) bigWalls++;
			}

			foreach (Wall wallZ in dna.phenotype.walls_z)
			{
				if (wallZ.indexes.Count > 3) bigWalls++;
			}
			
			if (nbWalls > 4)
				finalScore = finalScore/2;

			return finalScore;
		}
	}
}
