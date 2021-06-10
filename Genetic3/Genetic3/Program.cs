using System;
using System.Collections.Generic;
using UtilitiesGenetic;
using mVectors;
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
		
		static void Main()
        {
            Console.WriteLine("START");
			Vector3Int size = new Vector3Int(7, 7, 7);
			string path = "D:\\MasterProject\\Genetic3\\Data\\DataFitData";

			TypeParams[] cellsInfos = SetTypesParams();
            EvolutionaryAlgoParams algoParams = SetAlgoParams(path);
            int[][][] waypointParams = SetWaypointsParams(size);

			Init.GetSuggestionsClusters(size.x, size.y, size.z, cellsInfos, waypointParams, 20, algoParams);

			WriteData(path, size, algoParams, Init.fitness);

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

		public static EvolutionaryAlgoParams SetAlgoParams(string path)
		{
			EvolutionaryAlgoParams algoParams = new EvolutionaryAlgoParams();

			algoParams.population = 100;
			algoParams.elitism = 4;
			algoParams.generations = 100;
			algoParams.mutationRate = 0.005f;
			algoParams.wEmptyCuboids = 0.2f;
			algoParams.wWallsCuboids = 1f;
			algoParams.wPathfinding = 0.2f;

			return algoParams;
		}

		public static int[][][] SetWaypointsParams(Vector3Int size)
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

		public static void WriteData(string path, Vector3Int size, EvolutionaryAlgoParams algoParams, List<List<float[]>> fitness)
		{
			int id = 0;

			for(int i = 0; i < fitness.Count; i++)
			{
				using (var w = new StreamWriter(path + "_" + id + ".csv"))
				{
					var line = string.Format("Size_x; Size_y; Size_z; Population; Elitism; Generations; MutationRate; wEmptyCuboids; wWallsCuboids; wPathfinding");
					w.WriteLine(line);
					line = string.Format(size.x + ";" + size.y + ";" + size.z + ";" + algoParams.population + ";" 
						+ algoParams.elitism + ";" + algoParams.generations + ";" + algoParams.mutationRate + ";"
						+ algoParams.wEmptyCuboids + ";" + algoParams.wWallsCuboids + ";" + algoParams.wPathfinding);
					w.WriteLine(line);

					for(int j = 0; j < fitness[i].Count; j++)
					{
						w.Write(fitness[i][j][0]);
						for (int k = 1; k < fitness[i][j].Length; k++)
						{
							w.Write(";");
							w.Write(fitness[i][j][k]);
						}
						w.WriteLine();
					}

					w.Flush();
				}
				id++;
			}
		}
	}
}
