using System;
using System.Collections.Generic;
using UtilitiesGenetic;
using CsvHelper;
using System.IO;
using System.Globalization;
using Genetics;
using Genetic3;
using static Genetic3.Program;
using static Genetic3.Utilities;

namespace Genetic3
{
	public class Experiment
	{
		public int numberCandidates;
		public Candidate[] candidates;
		public int experimentID;

		private int[,,] randomInt;

		public Vector3Int[] sizeGrid;
		public EvolutionaryAlgoParams[] algoParams;
		public TypeSetUp[] typeSetUp;

		public Experiment(int ID, int nC, Vector3Int sizeGridCommun, EvolutionaryAlgoParams algoParamsCommun, TypeSetUp typeSetUpCommun)
		{
			randomInt = RandomInt(new System.Random());

			numberCandidates = nC;
			experimentID = ID;

			sizeGrid = SetSize(numberCandidates, sizeGridCommun);
			algoParams = SetAlgoParams(numberCandidates, algoParamsCommun);
			typeSetUp = SetTypeSetUp(numberCandidates, typeSetUpCommun);

			//candidates = SetCandidates(numberCandidates, sizeGrid, algoParams, typeSetUp, randomInt);
		}

		private int[,,] RandomInt(System.Random rand)
		{
			int[,,] randomInt = new int[100, 100, 100];

			for (int x = 0; x < 100; x++)
			{
				for (int y = 0; y < 100; y++)
				{
					for (int z = 0; z < 100; z++)
					{
						randomInt[x, y, z] = rand.Next(0, typeParams.Length + 1);
					}
				}
			}

			return randomInt;
		}

		private Vector3Int[] SetSize(int numberCandidates, Vector3Int sizeCommun)
		{
			Vector3Int[] size = new Vector3Int[numberCandidates];

			for (int i = 0; i < numberCandidates; i++)
			{
				size[i] = new Vector3Int(sizeCommun.x, sizeCommun.y, sizeCommun.z);
			}

			return size;
		}

		private EvolutionaryAlgoParams[] SetAlgoParams(int numberCandidates, EvolutionaryAlgoParams algoCommun)
		{
			EvolutionaryAlgoParams[] algoParams = new EvolutionaryAlgoParams[numberCandidates];

			for (int i = 0; i < numberCandidates; i++)
			{
				algoParams[i].generations = algoCommun.generations;
				algoParams[i].fitnessStop = algoCommun.fitnessStop;
				algoParams[i].mutationType = algoCommun.mutationType;
				algoParams[i].crossoverType = algoCommun.crossoverType;
				algoParams[i].elitism = algoCommun.elitism;
				algoParams[i].population = algoCommun.population;
				algoParams[i].mutationRate = algoCommun.mutationRate;

				algoParams[i].wDifference = algoCommun.wDifference;
				algoParams[i].wWalkingAreas = algoCommun.wWalkingAreas;
				algoParams[i].wWallsCuboids = algoCommun.wWallsCuboids;
				algoParams[i].wPathfinding = algoCommun.wPathfinding;
			}

			return algoParams;
		}

		private TypeSetUp[] SetTypeSetUp(int numberCandidates, TypeSetUp typeSetUpCommun)
		{
			TypeSetUp[] typeSetUp = new TypeSetUp[numberCandidates];

			for (int i = 0; i < numberCandidates; i++)
			{
				typeSetUp[i] = typeSetUpCommun;
			}

			return typeSetUp;
		}

		public Candidate[] SetCandidates()
		{
			candidates = new Candidate[numberCandidates];
			for (int i = 0; i < numberCandidates; i++)
			{
				candidates[i] = new Candidate(randomInt, sizeGrid[i], typeSetUp[i], algoParams[i]);
			}

			return candidates;
		}
	}

	public class Candidate
	{
		public Vector3Int sizeGrid;
		public Vector3Int sizeDNA;
		public int[][][] wayPointsInit;
		public EvolutionaryAlgoParams algoParams;
		public TypeSetUp typeSetUp;
		public int numberCandidates;

		public Candidate(int[,,] randomInt, Vector3Int size_grid, TypeSetUp typeSU, EvolutionaryAlgoParams algo)
		{
			sizeGrid = size_grid;
			sizeDNA = new Vector3Int(sizeGrid.x + 2, sizeGrid.y + 2, sizeGrid.z + 2);

			typeSetUp = typeSU;
			algoParams = algo;

			wayPointsInit = SetWaypointsParams(typeSetUp, randomInt);
		}

		private int[][][] SetWaypointsParams(TypeSetUp typeSetUp, int[,,] randomInt)
		{
			int[][][] waypointsParamsXYZ = new int[sizeDNA.x][][];

			for (int x = 0; x < sizeDNA.x; x++)
			{
				int[][] waypointsParamsYZ = new int[sizeDNA.y][];
				for (int y = 0; y < sizeDNA.y; y++)
				{
					int[] waypointsParamsZ = new int[sizeDNA.z];
					for (int z = 0; z < sizeDNA.z; z++)
					{
						if (x == 0 || y == 0 || z == 0
						|| x == sizeDNA.x - 1 || y == sizeDNA.y - 1 || z == sizeDNA.z - 1)
							waypointsParamsZ[z] = 0;
						else
						{
							if (typeSetUp == TypeSetUp.Chaos)
								waypointsParamsZ[z] = randomInt[x, y, z];
							else
								waypointsParamsZ[z] = 0;
						}
					}
					waypointsParamsYZ[y] = waypointsParamsZ;
				}
				waypointsParamsXYZ[x] = waypointsParamsYZ;
			}

			if (typeSetUp == TypeSetUp.Wall_and_Floor)
			{
				for (int x = 1; x < sizeDNA.x - 1; x++)
				{
					for (int z = 1; z < sizeDNA.z - 1; z++)
					{
						waypointsParamsXYZ[x][1][z] = 2;
					}
				}

				for (int y = 1; y < sizeDNA.y - 1; y++)
				{
					for (int z = 1; z < sizeDNA.z - 1; z++)
					{
						waypointsParamsXYZ[sizeDNA.x/2][y][z] = 5;
					}
				}
			}

			if (typeSetUp == TypeSetUp.Floor_and_Floor)
			{
				for (int x = 1; x < sizeDNA.x - 1; x++)
				{
					for (int z = 1; z < sizeDNA.z - 1; z++)
					{
						waypointsParamsXYZ[x][1][z] = 2;
						waypointsParamsXYZ[x][3][z] = 2;
					}
				}
			}

			return waypointsParamsXYZ;
		}
	}
}
