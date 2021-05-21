using System;
using System.Collections.Generic;
using MapTileGridCreator.Core;
using UtilitiesGenetic;
using mVectors; 
namespace Genetic3
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            /*
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());*/
            Console.WriteLine("START");

			Vector3Int size = new Vector3Int(7, 7, 7);

			TypeParams[] cellsInfos = SetTypesParams();
            EvolutionaryAlgoParams algoParams = SetAlgoParams();
            WaypointParams[][][] waypointParams = SetWaypointsParams(size);

            List<WaypointParams[][][]> newWaypointsParams = IA.GetSuggestionsClusters(size.x, size.y, size.z, cellsInfos, waypointParams, 1, algoParams);

			WaypointParams[][][] nw = newWaypointsParams[0];

			for (int y = 0; y < size.y; y++)
			{
				for (int x = 0; x < size.x; x++)
				{
					for (int z = 0; z < size.z; z++)
					{
						Console.Write(nw[x][y][z].type + " ");
					}
					Console.WriteLine();
				}
				Console.WriteLine("______________________________");
			}
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

		public static EvolutionaryAlgoParams SetAlgoParams()
		{
			EvolutionaryAlgoParams algoParams = new EvolutionaryAlgoParams();

			algoParams.population = 20;
			algoParams.elitism = 5;
			algoParams.generations = 200;
			algoParams.mutationRate = 0.05f;

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
	}
}
