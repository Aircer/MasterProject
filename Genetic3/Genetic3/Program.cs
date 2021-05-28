using System;
using System.Collections.Generic;
using MapTileGridCreator.Core;
using UtilitiesGenetic;
using mVectors;
using CsvHelper;
using System.IO;
using System.Globalization;


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
			string path = "D:\\MasterProject\\Genetic3\\Data\\DataFitData.csv";

			TypeParams[] cellsInfos = SetTypesParams();
            EvolutionaryAlgoParams algoParams = SetAlgoParams(path);
            WaypointParams[][][] waypointParams = SetWaypointsParams(size);

			WriteData(path, size, algoParams);

			List<WaypointParams[][][]> newWaypointsParams = IA.GetSuggestionsClusters(size.x, size.y, size.z, cellsInfos, waypointParams, 1, algoParams);
		}

		public static TypeParams[] SetTypesParams()
		{
			TypeParams[] newTypesParams = new TypeParams[1];

			newTypesParams[0] = new TypeParams();
			newTypesParams[0].blockPath = true;
			newTypesParams[0].door = false;
			newTypesParams[0].floor = false;
			newTypesParams[0].ground = true;
			newTypesParams[0].wall = true;

			return newTypesParams;
		}

		public static EvolutionaryAlgoParams SetAlgoParams(string path)
		{
			EvolutionaryAlgoParams algoParams = new EvolutionaryAlgoParams();

			algoParams.population = 100;
			algoParams.elitism = 0;
			algoParams.generations = 100;
			algoParams.mutationRate = 0.005f;

			return algoParams;
		}

		public static WaypointParams[][][] SetWaypointsParams(Vector3Int size)
		{
			WaypointParams[][][] waypointsParamsXYZ = new WaypointParams[size.x][][];

			for (int x = 0; x < size.x; x++)
			{
				WaypointParams[][] waypointsParamsYZ = new WaypointParams[size.y][];
				for (int y = 0; y < size.y; y++)
				{
					WaypointParams[] waypointsParamsZ = new WaypointParams[size.z];
					for (int z = 0; z < size.z; z++)
					{
						waypointsParamsZ[z].type = 0;
					}
					waypointsParamsYZ[y] = waypointsParamsZ;
				}
				waypointsParamsXYZ[x] = waypointsParamsYZ;
			}

			return waypointsParamsXYZ;
		}

		public static void WriteData(string path, Vector3Int size, EvolutionaryAlgoParams algoParams)
		{
			using (var w = new StreamWriter(path))
			{
				var line = string.Format("Size_x; Size_y; Size_z; Population; Elitism; Generations; MutationRate");
				w.WriteLine(line);
				line = string.Format(size.x + ";" + size.y + ";" + size.z + ";" + algoParams.population + ";" + algoParams.elitism + ";" + algoParams.generations + ";" + algoParams.mutationRate);
				w.WriteLine(line);
				w.Flush();
			}
		}
	}
}
