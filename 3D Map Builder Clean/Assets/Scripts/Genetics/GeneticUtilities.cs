using System;
using System.Collections.Generic;
using Genetics;

namespace UtilitiesGenetic
{
	[System.Serializable]
	public class TypeParams
	{
		public bool ground;
		public bool blockPath;
		public bool wall;
		public bool floor;
		public bool door;
		public bool stair;
		public bool ladder;

		public void SetEmpty()
		{
			ground = false;
			blockPath = false;
			wall = false;
			floor = false;
			door = false;
			stair = false;
			ladder = false;
		}
	}

	public struct EvolutionaryAlgoParams
	{
		public int nbBestFit;
		public CrossoverType crossoverType;
		public MutationsType mutationType;
		public float mutationRate;
		public int population;
		public int generations;
		public int elitism;
		public float fitnessStop;

		public float wDifference;
		public float wWalkingAreas;
		public float wWallsCuboids;
		public float wPathfinding;
	}

	public enum MutationsType
	{
		Normal, NoWalls, NoFloor, NoDoors, NoPathsUp, NoTransformations, OnlyTransformations, NoCreateDeleteFloorAndWalls
	}

	public enum CrossoverType
	{
		Swap, Copy
	}

	public class Phenotype
	{
		public HashSet<Cuboid> emptyCuboids;
		public HashSet<Cuboid> walls;
		public HashSet<WalkableArea> walkableArea;
		public HashSet<Path> paths;

		public Population population;


		public void Init(int lengthTypes, int numberCells)
		{
			emptyCuboids = new HashSet<Cuboid>();
			walls = new HashSet<Cuboid>();
			walkableArea = new HashSet<WalkableArea>();
			paths = new HashSet<Path>();
			population = new Population();
		}
	}

	public struct WalkableArea
	{
		public int yPos;
		public HashSet<Vector3Int> cells;
		public HashSet<Vector3Int> paths;
		public HashSet<Vector3Int> bordersNotGood;
		public HashSet<WalkableArea> neighborsArea;
		public HashSet<Path> neighborsPaths;
	}

	public struct Path
	{
		public HashSet<Vector3Int> cells;
		public int type;
		public HashSet<WalkableArea> neighborsConnected;
		public HashSet<WalkableArea> neighbors;
	}

	public struct Fitness
	{
		public float total;
		public float walls;
		public float walkingAreas;
		public float pathfinding;
		public float difference;
	}

	public class Population
	{
		public int[][][] genes;

		public void Copy(int[][][] original, Vector3Int size)
		{
			genes = new int[size.x][][];

			for (int x = 0; x < size.x; x++)
			{
				int[][] waypointsParamsYZ = new int[size.y][];
				for (int y = 0; y < size.y; y++)
				{
					int[] waypointsParamsZ = new int[size.z];
					for (int z = 0; z < size.z; z++)
					{
						waypointsParamsZ[z] = original[x][y][z];
					}
					waypointsParamsYZ[y] = waypointsParamsZ;
				}
				genes[x] = waypointsParamsYZ;
			}
		}

	}

	//Vector2Int and Vector3Int class so we can use them without using UnityEngine for running the genetic algorithm outside Unity
	public class Vector2Int
	{
		public int x;
		public int y;

		public Vector2Int(int i, int j)
		{
			x = i;
			y = j;
		}
	}

	public class Vector3Int : IEquatable<Vector3Int>
	{
		public int x;
		public int y;
		public int z;

		public Vector3Int(int i, int j, int k)
		{
			x = i;
			y = j;
			z = k;
		}

		public override int GetHashCode()
		{
			return x.GetHashCode() ^ y.GetHashCode() << 2 ^ z.GetHashCode() >> 2;
		}

		public bool Equals(Vector3Int vec)
		{
			return x.Equals(vec.x) && y.Equals(vec.y) && z.Equals(vec.z);
		}
	}

	public class Vector3
	{
		public float x;
		public float y;
		public float z;

		public Vector3(float i, float j, float k)
		{
			x = i;
			y = j;
			z = k;
		}
	}
}