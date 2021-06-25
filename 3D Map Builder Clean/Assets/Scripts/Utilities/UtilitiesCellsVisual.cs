using MapTileGridCreator.Core;
using System.Collections.Generic;
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

				for (int i = -1; i < 2; i++)
				{
					for (int k = -1; k < 2; k++)
					{
						//FloorTransform(cells, waypoints, new Vector3Int(newIndex.x + i, newIndex.y - 1, newIndex.z + k));
						LadderTransform(cells, waypoints, new Vector3Int(newIndex.x + i, newIndex.y, newIndex.z + k));
					}
				}
				
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

			if (type != null && type.typeParams.stair)
			{
				for (int i = -1; i < 2; i++)
				{
					for (int j = -1; j < 2; j++)
					{
						for (int k = -1; k < 2; k++)
						{
							StairTransform(cells, waypoints, new Vector3Int(newIndex.x + i, newIndex.y + j, newIndex.z + k));
						}
					}
				}
			}
			
			if (type != null && type.typeParams.floor)
			{
				//FloorTransform(cells, waypoints, new Vector3Int(newIndex.x, newIndex.y, newIndex.z));

				if (CellIsWall(newIndex.x, newIndex.y - 1, newIndex.z, cells, waypoints))
					WallTransform(cells, waypoints, new Vector3Int(newIndex.x, newIndex.y - 1, newIndex.z));
			}

			if (type != null && type.typeParams.ladder)
			{
				LadderTransform(cells, waypoints, new Vector3Int(newIndex.x, newIndex.y, newIndex.z));
			}
		}

		public static void WallTransform(Cell[,,] cells, Waypoint[,,] waypoints, Vector3Int index)
		{
			if (cells[index.x, index.y, index.z].type && cells[index.x, index.y, index.z].type.typeParams.wall
				&& !cells[index.x, index.y, index.z].type.typeParams.door)
			{
				bool[] neighborsWalls = new bool[4];

				Vector3 rotation = new Vector3(0, 0, 0);
				string subType = "Single";
				/*
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
				}

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
				
				if (CellIsFloor(index.x, index.y + 1, index.z, cells, waypoints))
				{
					subType += "Up";
				}

				cells[index.x, index.y, index.z].rotation = rotation;
				cells[index.x, index.y, index.z].TransformVisual(subType, rotation);
			}
		}

		public static void FloorTransform(Cell[,,] cells, Waypoint[,,] waypoints, Vector3Int index)
		{
			if (index.x >= 0 && index.y >= 0 && index.z >= 0
				&& index.x < cells.GetLength(0) && index.y < cells.GetLength(1) && index.z < cells.GetLength(2)
				&& cells[index.x, index.y, index.z].type && cells[index.x, index.y, index.z].type.typeParams.floor)
			{
				List<string> activeElements = new List<string>();

				if (CellIsWall(index.x - 1, index.y + 1, index.z, cells, waypoints) || CellIsWall(index.x - 1, index.y, index.z, cells, waypoints))
					activeElements.Add("x_minus");
				if(CellIsWall(index.x + 1, index.y + 1, index.z, cells, waypoints) || CellIsWall(index.x + 1, index.y, index.z, cells, waypoints))
					activeElements.Add("x_plus");
				if(CellIsWall(index.x, index.y + 1, index.z - 1, cells, waypoints) || CellIsWall(index.x, index.y, index.z - 1, cells, waypoints))
					activeElements.Add("z_minus");
				if(CellIsWall(index.x, index.y + 1, index.z + 1, cells, waypoints) || CellIsWall(index.x, index.y, index.z + 1, cells, waypoints))
					activeElements.Add("z_plus");

				cells[index.x, index.y, index.z].TransformVisualFloor(activeElements);
			}
		}

		public static void LadderTransform(Cell[,,] cells, Waypoint[,,] waypoints, Vector3Int index)
		{
			if (CellIsLadder(index.x, index.y, index.z, cells, waypoints))
			{
				string subType = "LadderSide";

				int rotation = 0;
				int ytemp = index.y; int yMin = index.y; int yMax = index.y;
				int Xneg = 0;
				int Xpos = 0;
				int Zneg = 0;
				int Zpos = 0;

				while (CellIsLadder(index.x, ytemp, index.z, cells, waypoints))
				{
					if (!CellIsWall(index.x - 1, ytemp, index.z, cells, waypoints) || !CellIsWall(index.x + 1, ytemp, index.z, cells, waypoints))
					{
						if (CellIsWall(index.x - 1, index.y, index.z, cells, waypoints))
							Xneg++;

						if (CellIsWall(index.x + 1, index.y, index.z, cells, waypoints))
							Xpos++;
					}

					if (!CellIsWall(index.x, ytemp, index.z - 1, cells, waypoints) || !CellIsWall(index.x, ytemp, index.z + 1, cells, waypoints))
					{
						if (CellIsWall(index.x, index.y, index.z - 1, cells, waypoints))
							Zneg++;

						if (CellIsWall(index.x, index.y, index.z + 1, cells, waypoints))
							Zpos++;
					}
					yMax = ytemp;
					ytemp++;
				}

				ytemp = index.y - 1;

				while (CellIsLadder(index.x, ytemp, index.z, cells, waypoints))
				{
					if (!CellIsWall(index.x - 1, ytemp, index.z, cells, waypoints) || !CellIsWall(index.x + 1, ytemp, index.z, cells, waypoints))
					{
						if (CellIsWall(index.x - 1, index.y, index.z, cells, waypoints))
							Xneg++;

						if (CellIsWall(index.x + 1, index.y, index.z, cells, waypoints))
							Xpos++;
					}

					if (!CellIsWall(index.x, ytemp, index.z - 1, cells, waypoints) || !CellIsWall(index.x, ytemp, index.z + 1, cells, waypoints))
					{
						if (CellIsWall(index.x, index.y, index.z - 1, cells, waypoints))
							Zneg++;

						if (CellIsWall(index.x, index.y, index.z + 1, cells, waypoints))
							Zpos++;
					}

					yMin = ytemp;
					ytemp--;
				}

				if (Xpos > Xneg && Xpos > Zneg && Xpos > Zpos)
					rotation = 90;
				if (Xneg > Xpos && Xneg > Zneg && Xneg > Zpos)
					rotation = -90;
				if (Zneg > Xneg && Zneg > Xpos && Zneg > Zpos)
					rotation = 180;
				if (Zpos > Zneg && Zpos > Xneg && Zpos > Xpos)
					rotation = 0;

				for (int i = yMin; i < yMax + 1; i++)
				{
					cells[index.x, i, index.z].TransformVisual(subType, new Vector3(0, rotation, 0));
				}
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
				string subType = "Door";

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

				if (!neighborsWalls[0] && !neighborsWalls[1] && !neighborsWalls[2] && !neighborsWalls[3])
				{
					rotation = new Vector3(0, 0, 0);
				}

				if ((neighborsWalls[2] || neighborsWalls[3]) && !neighborsWalls[0] && !neighborsWalls[1])
				{
					rotation = new Vector3(0, 90, 0);
				}

				if (neighborsWalls[1] && neighborsWalls[2] && !neighborsWalls[0] && !neighborsWalls[3])
				{
					rotation = new Vector3(0, 0, 0);
				}

				if (neighborsWalls[0] && neighborsWalls[2] && !neighborsWalls[1] && !neighborsWalls[3])
				{
					rotation = new Vector3(0, 90, 0);
				}

				if (neighborsWalls[0] && neighborsWalls[3] && !neighborsWalls[1] && !neighborsWalls[2])
				{
					rotation = new Vector3(0, 180, 0);
				}

				if (neighborsWalls[1] && neighborsWalls[3] && !neighborsWalls[0] && !neighborsWalls[2])
				{
					rotation = new Vector3(0, 270, 0);
				}

				if (neighborsWalls[1] && neighborsWalls[2] && !neighborsWalls[0] && neighborsWalls[3])
				{
					rotation = new Vector3(0, 0, 0);
				}

				if (neighborsWalls[1] && neighborsWalls[2] && neighborsWalls[0] && !neighborsWalls[3])
				{
					rotation = new Vector3(0, 90, 0);
				}

				if (!neighborsWalls[1] && neighborsWalls[3] && neighborsWalls[0] && neighborsWalls[2])
				{
					rotation = new Vector3(0, 180, 0);
				}

				if (neighborsWalls[0] && neighborsWalls[3] && neighborsWalls[1] && !neighborsWalls[2])
				{
					rotation = new Vector3(0, 270, 0);
				}

				if (neighborsWalls[0] && neighborsWalls[3] && neighborsWalls[1] && neighborsWalls[2])
				{
					rotation = new Vector3(0, 0, 0);
				}

				cells[index.x, index.y, index.z].TransformVisual(subType, rotation);
			}
		}

		public static void StairTransform(Cell[,,] cells, Waypoint[,,] waypoints, Vector3Int index)
		{
			if (index.x >= 0 && index.y >= 0 && index.z >= 0
				&& index.x < cells.GetLength(0) && index.y < cells.GetLength(1) && index.z < cells.GetLength(2)
				&& cells[index.x, index.y, index.z].type && cells[index.x, index.y, index.z].type.typeParams.stair)
			{
				bool[,,] neighborsStairs = new bool[3,3,3];

				Vector3 rotation = new Vector3(0, 0, 0);
				string subType = "";

				for (int i = -1; i < 2; i++)
				{
					for (int j = -1; j < 2; j++)
					{
						for (int k = -1; k < 2; k++)
						{
							if (index.x + i >= 0 && index.x + i < cells.GetLength(0)
							&& index.y + j >= 0 && index.y + j < cells.GetLength(1)
							&& index.z + k >= 0 && index.z + k < cells.GetLength(2))
								neighborsStairs[i + 1, j + 1, k + 1] = CellIsStair(index.x + i, index.y + j, index.z + k, cells, waypoints);
							else
								neighborsStairs[i + 1, j + 1, k + 1] = false;
						}
					}
				}

				subType = "StairsClassic";
				int X_pos = 0; int X_neg = 0;
				int Z_pos = 0; int Z_neg = 0;

				if (neighborsStairs[2, 2, 1])
				{
					X_neg -= 2;
					X_pos += 2;
				}
				if (neighborsStairs[2, 1, 1])
				{
					X_neg--;
					X_pos++;
				}
				if (neighborsStairs[0, 0, 1])
				{
					X_neg -= 2;
					X_pos += 2;
				}

				if (neighborsStairs[0, 2, 1])
				{
					X_neg += 2;
					X_pos -= 2;
				}
				if (neighborsStairs[0, 1, 1])
				{
					X_neg++;
					X_pos--;
				}
				if (neighborsStairs[2, 0, 1])
				{
					X_neg += 2;
					X_pos -= 2;
				}


				if (neighborsStairs[1, 2, 2])
				{
					Z_pos += 2;
					Z_neg -= 2;
				}
				if (neighborsStairs[1, 1, 2])
				{
					Z_pos++;
					Z_neg--;
				}
				if (neighborsStairs[1, 0, 0])
				{
					Z_pos += 2;
					Z_neg -= 2;
				}

				if (neighborsStairs[1, 2, 0])
				{
					Z_pos -= 2;
					Z_neg += 2;
				}
				if (neighborsStairs[1, 1, 0])
				{
					Z_pos--;
					Z_neg++;
				}
				if (neighborsStairs[1, 0, 2])
				{
					Z_pos -= 2;
					Z_neg += 2;
				}

				if (X_pos > X_neg)
					rotation = new Vector3Int(0, 270, 0);
				else
					rotation = new Vector3Int(0, 90, 0);

				if ((Z_pos > X_pos && Z_pos > X_neg) || (Z_neg > X_pos && Z_neg > X_neg))
				{
					if (Z_pos > Z_neg)
						rotation = new Vector3Int(0, 180, 0);
					else
						rotation = new Vector3Int(0, 0, 0);
				}

				/*
				if (!(X_neg > 2 || X_pos > 2 || Z_neg > 2 || Z_pos > 2)
			     || (!neighborsStairs[3, 3, 2] && !neighborsStairs[1, 3, 2] && !neighborsStairs[2, 3, 3] && !neighborsStairs[2, 3, 1])
				 || ())
				{
					if ((neighborsStairs[1, 2, 2] && neighborsStairs[0, 1, 2] && !neighborsStairs[3, 1, 2])
					|| (neighborsStairs[3, 2, 2]  && neighborsStairs[4, 1, 2] && !neighborsStairs[1, 1, 2])
					|| (neighborsStairs[2, 2, 1]  && neighborsStairs[2, 1, 0] && !neighborsStairs[2, 1, 3])
					|| (neighborsStairs[2, 2, 3]  && neighborsStairs[2, 1, 4] && !neighborsStairs[2, 1, 1]))
						subType = "StairsBlock";
				}*/
				/*
				if ((neighborsStairs[1, 2, 2] || neighborsStairs[3, 2, 2] || neighborsStairs[2, 2, 1] || neighborsStairs[2, 2, 3])
				 && (neighborsStairs[1, 3, 2] || neighborsStairs[3, 3, 2] || neighborsStairs[2, 3, 1] || neighborsStairs[2, 3, 3]))
					subType = "StairsBlock";

				if (!(X_neg > 2 || X_pos > 2 || Z_neg > 2 || Z_pos > 2)
				&& (neighborsStairs[1, 2, 2] || neighborsStairs[3, 2, 2] || neighborsStairs[2, 2, 1] || neighborsStairs[2, 2, 3])
				&& (!neighborsStairs[1, 3, 2] && !neighborsStairs[3, 3, 2] && !neighborsStairs[2, 3, 1] && !neighborsStairs[2, 3, 3])
				&& (neighborsStairs[0, 1, 2] || neighborsStairs[4, 1, 2] || neighborsStairs[2, 1, 0] || neighborsStairs[2, 1, 4]))
					subType = "StairsBlock";*/
				/*
				if (!(X_neg >= 3 || X_pos >= 3 || Z_neg >= 3 || Z_pos >= 3)
				&&((neighborsStairs[0, 0, 1] || neighborsStairs[2, 0, 1]) && (neighborsStairs[1, 1, 0] || neighborsStairs[1, 1, 2])
				||((neighborsStairs[1, 0, 0] || neighborsStairs[1, 0, 2]) && (neighborsStairs[0, 1, 1] || neighborsStairs[2, 1, 1]))))
					subType = "StairsDown";

				if (!(X_neg >= 3 || X_pos >= 3 || Z_neg >= 3 || Z_pos >= 3)
				&& ((neighborsStairs[0, 2, 1] || neighborsStairs[2, 2, 1]) && (neighborsStairs[1, 1, 0] || neighborsStairs[1, 1, 2])
				|| ((neighborsStairs[1, 2, 0] || neighborsStairs[1, 2, 2]) && (neighborsStairs[0, 1, 1] || neighborsStairs[2, 1, 1]))))
					subType = "StairsUp";

				
				int nbNeighbors = 0;

				if (neighborsStairs[1, 1, 0])
						nbNeighbors++;
				if (neighborsStairs[1, 1, 2])
					nbNeighbors++;
				if (neighborsStairs[0, 1, 1])
					nbNeighbors++;
				if (neighborsStairs[2, 1, 1])
					nbNeighbors++;

				
				if (!(X_neg > 1 || X_pos > 1 || Z_neg > 1 || Z_pos > 1) && nbNeighbors > 1)
					subType = "StairsUp";*/

				cells[index.x, index.y, index.z].rotation = rotation;
				cells[index.x, index.y, index.z].TransformVisual(subType, rotation);
			}
		}

		public static bool CellIsStair(int x, int y, int z, Cell[,,] cells, Waypoint[,,] waypoints)
		{
			if (x >= 0 && y >= 0 && z >= 0 
				&& x < waypoints.GetLength(0) && y < waypoints.GetLength(1) && z < waypoints.GetLength(2)
				&& waypoints[x, y, z].type != null && waypoints[x, y, z].type.typeParams.stair)
				return true;
			else
				return false;
		}

		public static bool CellIsWall(int x, int y, int z, Cell[,,] cells, Waypoint[,,] waypoints)
		{
			if (x >= 0 && y >= 0 && z >= 0
				&& x < waypoints.GetLength(0) && y < waypoints.GetLength(1) && z < waypoints.GetLength(2)
				&& waypoints[x, y, z].type != null && waypoints[x, y, z].type.typeParams.wall)
				return true;
			else
				return false;
		}

		public static bool CellIsFloor(int x, int y, int z, Cell[,,] cells, Waypoint[,,] waypoints)
		{
			if (x >= 0 && y >= 0 && z >= 0
				&& x < waypoints.GetLength(0) && y < waypoints.GetLength(1) && z < waypoints.GetLength(2)
				&& waypoints[x, y, z].type != null && waypoints[x, y, z].type.typeParams.floor)
				return true;
			else
				return false;
		}

		public static bool CellIsLadder(int x, int y, int z, Cell[,,] cells, Waypoint[,,] waypoints)
		{
			if (x >= 0 && y >= 0 && z >= 0
				&& x < waypoints.GetLength(0) && y < waypoints.GetLength(1) && z < waypoints.GetLength(2)
				&& waypoints[x, y, z].type != null && waypoints[x, y, z].type.typeParams.ladder)
				return true;
			else
				return false;
		}
	}
}

