using System.Collections;
using System.Collections.Generic;
using UtilitiesGenetic;

namespace Genetics
{
	public class Cuboid
	{
		public Vector3Int min;
		public Vector3Int max;

		public int width;
		public int length;
		public int height;

		public HashSet<Vector3Int> cellsBorder;
		public HashSet<Vector3Int> cells;
		public HashSet<Vector3Int> bottomEmpty;

		public HashSet<Cuboid> inCuboids;
		public HashSet<Cuboid> outCuboids;
	}
}