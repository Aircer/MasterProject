﻿using System;
using System.Collections.Generic;

namespace UtilitiesGenetic
{
	public struct WaypointParams
	{
		public int type;
		public bool baseType;
	}

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
		public float mutationRate;
		public int population;
		public int generations;
		public int elitism;

		public float wEmptyCuboids;
		public float wWallsCuboids;
		public float wPathfinding;
	}
}

namespace mVectors
{
	public struct Phenotype
	{
		public HashSet<Cuboid> emptyCuboids;
		public HashSet<Cuboid> walls;
		public HashSet<WalkableArea> walkableArea;
		public HashSet<Path> paths;
	}

	public struct Cuboid
	{
		public Vector3Int min;
		public Vector3Int max;

		public int width;
		public int length;
		public int height;

		public HashSet<Vector3Int> cellsBorder;
		public HashSet<Vector3Int> cells;

		public HashSet<Cuboid> inCuboids;
		public HashSet<Cuboid> outCuboids;
	}

	public struct WalkableArea
	{
		public int yPos;
		public HashSet<Vector3Int> cells;
		public HashSet<Vector3Int> paths;
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