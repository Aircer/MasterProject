using MapTileGridCreator.Core;
using UnityEngine;

namespace MapTileGridCreator.UtilitiesVisual
{
	/// <summary>
	/// Static class containining utilities functions for editor.
	/// </summary>
	public static class FuncVisual
	{
		public static void UpdateCellsAroundVisual(Cell[,,] cells, Waypoint[,,] waypoints, Vector3Int newIndex, CellInformation type)
		{
			if (type != null && type.typeParams.wall)
			{
				WallTransform(cells, waypoints, new Vector3Int(newIndex.x, newIndex.y, newIndex.z));
				if (newIndex.x > 0)
					WallTransform(cells, waypoints, new Vector3Int(newIndex.x - 1, newIndex.y, newIndex.z));
				if (newIndex.z > 0)
					WallTransform(cells, waypoints, new Vector3Int(newIndex.x, newIndex.y, newIndex.z-1));
				if (newIndex.x < cells.GetLength(0) - 1)
					WallTransform(cells, waypoints, new Vector3Int(newIndex.x + 1, newIndex.y, newIndex.z));
				if (newIndex.z < cells.GetLength(2) - 1)
					WallTransform(cells, waypoints, new Vector3Int(newIndex.x, newIndex.y, newIndex.z+1));
				if (newIndex.y > 0)
					WallTransform(cells, waypoints, new Vector3Int(newIndex.x, newIndex.y-1, newIndex.z));
			}
		}

		public static void WallTransform(Cell[,,] cells, Waypoint[,,] waypoints, Vector3Int index)
		{
			if (cells[index.x, index.y, index.z].type && cells[index.x, index.y, index.z].type.typeParams.wall)
			{
				bool[] neighbordsWalls = new bool[4];
				Vector3 rotation = new Vector3(0, 0, 0);
				string subType = "";

				if (index.x > 0 && waypoints[index.x - 1, index.y, index.z].type != null && waypoints[index.x - 1, index.y, index.z].type.typeParams.wall)
					neighbordsWalls[0] = true;
				else
					neighbordsWalls[0] = false;

				if (index.x < waypoints.GetLength(0) - 1 && waypoints[index.x + 1, index.y, index.z].type != null && waypoints[index.x + 1, index.y, index.z].type.typeParams.wall)
					neighbordsWalls[1] = true;
				else
					neighbordsWalls[1] = false;

				if (index.z > 0 && waypoints[index.x, index.y, index.z - 1].type != null && waypoints[index.x, index.y, index.z - 1].type.typeParams.wall)
					neighbordsWalls[2] = true;
				else
					neighbordsWalls[2] = false;

				if (index.z < waypoints.GetLength(2) - 1 && waypoints[index.x, index.y, index.z + 1].type != null && waypoints[index.x, index.y, index.z + 1].type.typeParams.wall)
					neighbordsWalls[3] = true;
				else
					neighbordsWalls[3] = false;

				if (!neighbordsWalls[0] && !neighbordsWalls[1] && !neighbordsWalls[2] && !neighbordsWalls[3])
				{
					subType = "Single";
					rotation = new Vector3(0, 0, 0);
				}

				if ((neighbordsWalls[0] || neighbordsWalls[1]) && !neighbordsWalls[2] && !neighbordsWalls[3])
				{
					subType = "DoubleSides";
					rotation = new Vector3(0, 0, 0);
				}

				if ((neighbordsWalls[2] || neighbordsWalls[3]) && !neighbordsWalls[0] && !neighbordsWalls[1])
				{
					subType = "DoubleSides";
					rotation = new Vector3(0, 90, 0);
				}

				if (neighbordsWalls[1] && neighbordsWalls[2] && !neighbordsWalls[0] && !neighbordsWalls[3])
				{
					subType = "Corner";
					rotation = new Vector3(0, 0, 0);
				}

				if (neighbordsWalls[0] && neighbordsWalls[2] && !neighbordsWalls[1] && !neighbordsWalls[3])
				{
					subType = "Corner";
					rotation = new Vector3(0, 90, 0);
				}

				if (neighbordsWalls[0] && neighbordsWalls[3] && !neighbordsWalls[1] && !neighbordsWalls[2])
				{
					subType = "Corner";
					rotation = new Vector3(0, 180, 0);
				}

				if (neighbordsWalls[1] && neighbordsWalls[3] && !neighbordsWalls[0] && !neighbordsWalls[2])
				{
					subType = "Corner";
					rotation = new Vector3(0, 270, 0);
				}

				if (neighbordsWalls[1] && neighbordsWalls[2] && !neighbordsWalls[0] && neighbordsWalls[3])
				{
					subType = "Triple";
					rotation = new Vector3(0, 0, 0);
				}

				if (neighbordsWalls[1] && neighbordsWalls[2] && neighbordsWalls[0] && !neighbordsWalls[3])
				{
					subType = "Triple";
					rotation = new Vector3(0, 90, 0);
				}

				if (!neighbordsWalls[1] && neighbordsWalls[3] && neighbordsWalls[0] && neighbordsWalls[2])
				{
					subType = "Triple";
					rotation = new Vector3(0, 180, 0);
				}

				if (neighbordsWalls[0] && neighbordsWalls[3] && neighbordsWalls[1] && !neighbordsWalls[2])
				{
					subType = "Triple";
					rotation = new Vector3(0, 270, 0);
				}

				if (neighbordsWalls[0] && neighbordsWalls[3] && neighbordsWalls[1] && neighbordsWalls[2])
				{
					subType = "Quattro";
					rotation = new Vector3(0, 0, 0);
				}

				cells[index.x, index.y, index.z].TransformVisual(subType, rotation);
			}
		}
	}
}

