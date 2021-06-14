using System;
using System.Collections.Generic;
using UtilitiesGenetic;
using CsvHelper;
using System.IO;
using System.Globalization;
using Genetics;

namespace Genetic3
{
    static class Program
    {
		/// <summary>
		/// The main entry point for the application.
		/// </summary>

		public static Vector3Int size;
		public static string path;
		public static int IDFile;
		public static int nbRuns;
		public static EvolutionaryAlgoParams algoParams;

		public static Fitness[][][] fitness;
		public static Population[][][] populations;

		static void Main()
        {
			nbRuns = 20;
			IDFile = 1;
			size = new Vector3Int(7, 7, 7);
			path= "D:\\MasterProject\\Genetic3\\Data\\Experiment_";
			algoParams = SetAlgoParams();

			TypeParams[] cellsInfos = SetTypesParams();
            int[][][] waypointParams = SetWaypointsParams();

			Genetics.Init newInitGenetic = new Genetics.Init();
			newInitGenetic.GetSuggestionsClusters(size.x, size.y, size.z, cellsInfos, waypointParams, nbRuns, algoParams);
			fitness = newInitGenetic.fitness;
			populations = newInitGenetic.populations;

			WriteData();
		}

		public static TypeParams[] SetTypesParams()
		{
			int nbType = 12;
			TypeParams[] newTypesParams = new TypeParams[nbType];

			for (int i = 0; i < nbType; i++)
			{
				newTypesParams[i] = new TypeParams();
				newTypesParams[i].SetEmpty();
			}

			newTypesParams[10].blockPath = true;
			newTypesParams[10].wall = true;

			newTypesParams[2].door = true;
			newTypesParams[2].wall = true;

			newTypesParams[4].blockPath = true;
			newTypesParams[4].floor = true;
			newTypesParams[4].ground = true;

			newTypesParams[5].ladder = true;

			newTypesParams[7].blockPath = true;
			newTypesParams[7].ground = true;
			newTypesParams[7].stair = true;

			return newTypesParams;
		}

		public static EvolutionaryAlgoParams SetAlgoParams()
		{
			EvolutionaryAlgoParams algoParams = new EvolutionaryAlgoParams();

			algoParams.population = 30;
			algoParams.elitism = 4;
			algoParams.generations = 20;
			algoParams.mutationRate = 0.005f;

			algoParams.wDifference = 0.2f;
			algoParams.wWallsCuboids = 0.2f;
			algoParams.wWalkingAreas = 0.2f;
			algoParams.wPathfinding = 1f;

			return algoParams;
		}

		public static int[][][] SetWaypointsParams()
		{
			int[][][] waypointsParamsXYZ = new int[size.x][][];

			for (int x = 0; x < size.x; x++)
			{
				int[][] waypointsParamsYZ = new int[size.y][];
				for (int y = 0; y < size.y; y++)
				{
					int[] waypointsParamsZ = new int[size.z];
					for (int z = 0; z < size.z; z++)
					{
						waypointsParamsZ[z] = 0;
					}
					waypointsParamsYZ[y] = waypointsParamsZ;
				}
				waypointsParamsXYZ[x] = waypointsParamsYZ;
			}

			return waypointsParamsXYZ;
		}

		public static void WriteData()
		{
			System.IO.Directory.CreateDirectory(path + IDFile);

			using (var w = new StreamWriter(path + IDFile + "\\" + "ExperimentSetUp.csv"))
			{
				var line = string.Format("Size_x; Size_y; Size_z; Population; Elitism; Generations; MutationRate; numberRuns; wDifference; wWalkingAreas; wWallsCuboids; wPathfinding");
				w.WriteLine(line);
				line = string.Format(size.x + ";" + size.y + ";" + size.z + ";" + algoParams.population + ";"
					+ algoParams.elitism + ";" + algoParams.generations + ";" + algoParams.mutationRate + ";" + nbRuns + ";"
					+ algoParams.wDifference + ";" + algoParams.wWalkingAreas + ";" + algoParams.wWallsCuboids + ";" + algoParams.wPathfinding);
				w.WriteLine(line);

				w.Flush();
			}

			for (int i = 0; i < nbRuns; i++)
			{
				using (var w = new StreamWriter(path + IDFile + "\\" + "Fitness" + "_" + i + ".csv"))
				{
					for (int j = 0; j < algoParams.generations; j++)
					{
						for (int k = 0; k < algoParams.population; k++)
						{
							WriteFitness(w, i, j, k);
						}
					}
				}
			}

			for (int i = 0; i < nbRuns; i++)
			{
				using (var w = new StreamWriter(path + IDFile + "\\" + "Cells" + "_" + i + ".csv"))
				{
					for (int j = 0; j < algoParams.generations; j++)
					{
						for (int k = 0; k < algoParams.population; k++)
						{
							WritePop(w, i, j, k);
						}
					}
				}
			}
		}

		public static void WritePop(StreamWriter w, int i, int j, int k)
		{
			for (int x = 0; x < size.x; x++)
			{
				for (int y = 0; y < size.y; y++)
				{
					for (int z = 0; z < size.z; z++)
					{
						w.Write(populations[i][j][k].genes[x][y][z]);
						w.Write(";");
					}
				}
			}
			w.WriteLine();
		}

		public static void WriteFitness(StreamWriter w, int i, int j, int k)
		{
			w.Write(fitness[i][j][k].total);
			w.Write(";");
			w.Write(fitness[i][j][k].difference);
			w.Write(";");
			w.Write(fitness[i][j][k].walkingAreas);
			w.Write(";");
			w.Write(fitness[i][j][k].walls);
			w.Write(";");
			w.Write(fitness[i][j][k].pathfinding);
			w.Write(";");

			w.WriteLine();
		}
	}
}
