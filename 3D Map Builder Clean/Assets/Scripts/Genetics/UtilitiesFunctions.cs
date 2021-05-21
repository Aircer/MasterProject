using System;
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
	}

	public struct EvolutionaryAlgoParams
	{
		public float mutationRate;
		public int population;
		public int generations;
		public int elitism;
	}
}

namespace mVectors
{
	public struct Phenotype
	{
		public HashSet<Cuboid> cuboids;
	}

	public struct Cuboid
	{
		public HashSet<Vector3Int> cells;
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