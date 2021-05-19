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
					WallTransform(cells, waypoints, new Vector3Int(newIndex.x, newIndex.y - 1, newIndex.z));
				if (newIndex.y < cells.GetLength(1) - 1)
					WallTransform(cells, waypoints, new Vector3Int(newIndex.x, newIndex.y + 1, newIndex.z));

				if(type.typeParams.door)
                {
					DoorTransform(cells, waypoints, new Vector3Int(newIndex.x, newIndex.y, newIndex.z));
					if (newIndex.y > 0)
						DoorTransform(cells, waypoints, new Vector3Int(newIndex.x, newIndex.y - 1, newIndex.z));
					if (newIndex.y < cells.GetLength(1) - 1)
						DoorTransform(cells, waypoints, new Vector3Int(newIndex.x, newIndex.y + 1, newIndex.z));
				}
			}
		}

		public static void WallTransform(Cell[,,] cells, Waypoint[,,] waypoints, Vector3Int index)
		{
			if (cells[index.x, index.y, index.z].type && cells[index.x, index.y, index.z].type.typeParams.wall)
			{
				bool[] neighborsWalls = new bool[4];
				bool neighborUpWalls;
				bool neighborDownWalls;

				Vector3 rotation = new Vector3(0, 0, 0);
				string subType = "";

				if (index.x > 0 && waypoints[index.x - 1, index.y, index.z].type != null && waypoints[index.x - 1, index.y, index.z].type.typeParams.wall)
					neighborsWalls[0] = true;
				else
					neighborsWalls[0] = false;

				if (index.x < waypoints.GetLength(0) - 1 && waypoints[index.x + 1, index.y, index.z].type != null && waypoints[index.x + 1, index.y, index.z].type.typeParams.wall)
					neighborsWalls[1] = true;
				else
					neighborsWalls[1] = false;

				if (index.z > 0 && waypoints[index.x, index.y, index.z - 1].type != null && waypoints[index.x, index.y, index.z - 1].type.typeParams.wall)
					neighborsWalls[2] = true;
				else
					neighborsWalls[2] = false;

				if (index.z < waypoints.GetLength(2) - 1 && waypoints[index.x, index.y, index.z + 1].type != null && waypoints[index.x, index.y, index.z + 1].type.typeParams.wall)
					neighborsWalls[3] = true;
				else
					neighborsWalls[3] = false;

				if (index.y > 0 && waypoints[index.x, index.y - 1, index.z].type != null && waypoints[index.x, index.y - 1, index.z].type.typeParams.wall)
					neighborDownWalls = true;
				else
					neighborDownWalls = false;

				if (index.y < waypoints.GetLength(1) - 1 && waypoints[index.x, index.y + 1, index.z].type != null && waypoints[index.x, index.y + 1, index.z].type.typeParams.wall)
					neighborUpWalls = true;
				else
					neighborUpWalls = false;

				if (!neighborsWalls[0] && !neighborsWalls[1] && !neighborsWalls[2] && !neighborsWalls[3])
				{
					subType = "Single";
					rotation = new Vector3(0, 0, 0);
				}

				if ((neighborsWalls[0] || neighborsWalls[1]) && !neighborsWalls[2] && !neighborsWalls[3])
				{
					subType = "DoubleSides";
					rotation = new Vector3(0, 0, 0);
				}

				if ((neighborsWalls[2] || neighborsWalls[3]) && !neighborsWalls[0] && !neighborsWalls[1])
				{
					subType = "DoubleSides";
					rotation = new Vector3(0, 90, 0);
				}
				/*
				if (!neighborsWalls[0] && !neighborsWalls[1] && !neighborsWalls[2] && !neighborsWalls[3] && neighborDownWalls)
				{
					subType = "DoubleSides";
					rotation = cells[index.x, index.y - 1, index.z].rotation;
				}

				if (!neighborsWalls[0] && !neighborsWalls[1] && !neighborsWalls[2] && !neighborsWalls[3] && neighborUpWalls)
				{
					subType = "DoubleSides";
					rotation = cells[index.x, index.y + 1, index.z].rotation;
				}*/

				if (neighborsWalls[1] && neighborsWalls[2] && !neighborsWalls[0] && !neighborsWalls[3])
				{
					subType = "Corner";
					rotation = new Vector3(0, 0, 0);
				}

				if (neighborsWalls[0] && neighborsWalls[2] && !neighborsWalls[1] && !neighborsWalls[3])
				{
					subType = "Corner";
					rotation = new Vector3(0, 90, 0);
				}

				if (neighborsWalls[0] && neighborsWalls[3] && !neighborsWalls[1] && !neighborsWalls[2])
				{
					subType = "Corner";
					rotation = new Vector3(0, 180, 0);
				}

				if (neighborsWalls[1] && neighborsWalls[3] && !neighborsWalls[0] && !neighborsWalls[2])
				{
					subType = "Corner";
					rotation = new Vector3(0, 270, 0);
				}

				if (neighborsWalls[1] && neighborsWalls[2] && !neighborsWalls[0] && neighborsWalls[3])
				{
					subType = "Triple";
					rotation = new Vector3(0, 0, 0);
				}

				if (neighborsWalls[1] && neighborsWalls[2] && neighborsWalls[0] && !neighborsWalls[3])
				{
					subType = "Triple";
					rotation = new Vector3(0, 90, 0);
				}

				if (!neighborsWalls[1] && neighborsWalls[3] && neighborsWalls[0] && neighborsWalls[2])
				{
					subType = "Triple";
					rotation = new Vector3(0, 180, 0);
				}

				if (neighborsWalls[0] && neighborsWalls[3] && neighborsWalls[1] && !neighborsWalls[2])
				{
					subType = "Triple";
					rotation = new Vector3(0, 270, 0);
				}

				if (neighborsWalls[0] && neighborsWalls[3] && neighborsWalls[1] && neighborsWalls[2])
				{
					subType = "Quattro";
					rotation = new Vector3(0, 0, 0);
				}
				/*
				int inc = 0;

				if (neighborsWalls[0])
					inc++;
				if (neighborsWalls[1])
					inc++;
				if (neighborsWalls[2])
					inc++;
				if (neighborsWalls[3])
					inc++;

				if (!neighborDownWalls && !neighborUpWalls && inc > 1)
				{
					subType = "Floor";
					rotation = new Vector3(0, 0, 0);
				}*/

				cells[index.x, index.y, index.z].rotation = rotation;
				cells[index.x, index.y, index.z].TransformVisual(subType, rotation);
			}
		}

		public static void DoorTransform(Cell[,,] cells, Waypoint[,,] waypoints, Vector3Int index)
		{
			if (cells[index.x, index.y, index.z].type && cells[index.x, index.y, index.z].type.typeParams.door)
			{
				bool[] neighborsWalls = new bool[4];
				bool neighborUpDoor;
				bool neighborDownDoor;

				Vector3 rotation = new Vector3(0, 0, 0);
				string subType = "";

				if (index.y > 0 && waypoints[index.x, index.y - 1, index.z].type != null && waypoints[index.x, index.y - 1, index.z].type.typeParams.door)
					neighborDownDoor = true;
				else
					neighborDownDoor = false;

				if (index.y < waypoints.GetLength(1) - 1 && waypoints[index.x, index.y + 1, index.z].type != null && waypoints[index.x, index.y + 1, index.z].type.typeParams.door)
					neighborUpDoor = true;
				else
					neighborUpDoor = false;

				if (!neighborDownDoor)
				{
					subType = "DoorTop";
				}
				else
					subType = "DoorBottom";

				if (neighborUpDoor)
				{
					subType = "Door";
				}

				cells[index.x, index.y, index.z].TransformVisual(subType, cells[index.x, index.y, index.z].rotation);
			}
		}
	}
}

