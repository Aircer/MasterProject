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
		public static void UpdateCellsAroundVisual(Cell[,,] cells, Vector3Int newIndex, CellInformation type)
		{
			if (type != null && type.typeParams.wall)
			{
				FloorTransform(cells, new Vector3Int(newIndex.x, newIndex.y - 1, newIndex.z));
				FloorTransform(cells, new Vector3Int(newIndex.x, newIndex.y + 1, newIndex.z));
				WallTransform(cells, new Vector3Int(newIndex.x, newIndex.y, newIndex.z));

				for (int i = -1; i < 2; i++)
				{
					for (int k = -1; k < 2; k++)
					{
						LadderTransform(cells, new Vector3Int(newIndex.x + i, newIndex.y, newIndex.z + k));
					}
				}
				
				if (newIndex.x > 0)
					WallTransform(cells, new Vector3Int(newIndex.x - 1, newIndex.y, newIndex.z));
				if (newIndex.z > 0)
					WallTransform(cells, new Vector3Int(newIndex.x, newIndex.y, newIndex.z-1));
				if (newIndex.x < cells.GetLength(0) - 1)
					WallTransform(cells, new Vector3Int(newIndex.x + 1, newIndex.y, newIndex.z));
				if (newIndex.z < cells.GetLength(2) - 1)
					WallTransform(cells, new Vector3Int(newIndex.x, newIndex.y, newIndex.z+1));
				if (newIndex.y > 0)
					WallTransform(cells, new Vector3Int(newIndex.x, newIndex.y - 1, newIndex.z));
				if (newIndex.y < cells.GetLength(1) - 1)
					WallTransform(cells, new Vector3Int(newIndex.x, newIndex.y + 1, newIndex.z));

				if(type.typeParams.door)
                {
					DoorTransform(cells, new Vector3Int(newIndex.x, newIndex.y, newIndex.z));
					if (newIndex.y > 0)
						DoorTransform(cells, new Vector3Int(newIndex.x, newIndex.y - 1, newIndex.z));
					if (newIndex.y < cells.GetLength(1) - 1)
						DoorTransform(cells, new Vector3Int(newIndex.x, newIndex.y + 1, newIndex.z));
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
							StairTransform(cells, new Vector3Int(newIndex.x + i, newIndex.y + j, newIndex.z + k));
						}
					}
				}
			}
			
			if (type != null && type.typeParams.floor)
			{
				FloorTransform(cells, new Vector3Int(newIndex.x, newIndex.y, newIndex.z));

				if (CellIsWall(newIndex.x, newIndex.y - 1, newIndex.z, cells))
					WallTransform(cells, new Vector3Int(newIndex.x, newIndex.y - 1, newIndex.z));
			}

			if (type != null && type.typeParams.ladder)
			{
				LadderTransform(cells, new Vector3Int(newIndex.x, newIndex.y, newIndex.z));
			}
		}

		public static void WallTransform(Cell[,,] cells, Vector3Int index)
		{
			if (cells[index.x, index.y, index.z].type && cells[index.x, index.y, index.z].type.typeParams.wall
				&& !cells[index.x, index.y, index.z].type.typeParams.door)
			{
				bool[] neighborsWalls = new bool[4];

				Vector3 rotation = new Vector3(0, 0, 0);
				string subType = "Single";

				if (CellIsFloor(index.x, index.y + 1, index.z, cells))
				{
					subType += "Up";
				}

				cells[index.x, index.y, index.z].rotation = rotation;
				cells[index.x, index.y, index.z].TransformVisual(subType, rotation);
			}
		}

		public static void FloorTransform(Cell[,,] cells, Vector3Int index)
		{
			if (index.x >= 0 && index.y >= 0 && index.z >= 0
				&& index.x < cells.GetLength(0) && index.y < cells.GetLength(1) && index.z < cells.GetLength(2)
				&& cells[index.x, index.y, index.z].type && cells[index.x, index.y, index.z].type.typeParams.floor)
			{
				string subType = "Floor";

				if (CellIsDoor(index.x, index.y - 1, index.z, cells))
				{
					subType += "Under";
				}

				cells[index.x, index.y, index.z].TransformVisual(subType, new Vector3Int(0,0,0));
			}
		}

		public static void LadderTransform(Cell[,,] cells, Vector3Int index)
		{
			if (CellIsLadder(index.x, index.y, index.z, cells))
			{
				string subType = "LadderSide";

				int rotation = 0;
				int ytemp = index.y; int yMin = index.y; int yMax = index.y;
				int Xneg = 0;
				int Xpos = 0;
				int Zneg = 0;
				int Zpos = 0;

				while (CellIsLadder(index.x, ytemp, index.z, cells))
				{
					if (!CellIsWall(index.x - 1, ytemp, index.z, cells) || !CellIsWall(index.x + 1, ytemp, index.z, cells))
					{
						if (CellIsWall(index.x - 1, index.y, index.z, cells))
							Xneg++;

						if (CellIsWall(index.x + 1, index.y, index.z, cells))
							Xpos++;
					}

					if (!CellIsWall(index.x, ytemp, index.z - 1, cells) || !CellIsWall(index.x, ytemp, index.z + 1, cells))
					{
						if (CellIsWall(index.x, index.y, index.z - 1, cells))
							Zneg++;

						if (CellIsWall(index.x, index.y, index.z + 1, cells))
							Zpos++;
					}
					yMax = ytemp;
					ytemp++;
				}

				ytemp = index.y - 1;

				while (CellIsLadder(index.x, ytemp, index.z, cells))
				{
					if (!CellIsWall(index.x - 1, ytemp, index.z, cells) || !CellIsWall(index.x + 1, ytemp, index.z, cells))
					{
						if (CellIsWall(index.x - 1, index.y, index.z, cells))
							Xneg++;

						if (CellIsWall(index.x + 1, index.y, index.z, cells))
							Xpos++;
					}

					if (!CellIsWall(index.x, ytemp, index.z - 1, cells) || !CellIsWall(index.x, ytemp, index.z + 1, cells))
					{
						if (CellIsWall(index.x, index.y, index.z - 1, cells))
							Zneg++;

						if (CellIsWall(index.x, index.y, index.z + 1, cells))
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

		public static void DoorTransform(Cell[,,] cells, Vector3Int index)
		{
			if (cells[index.x, index.y, index.z].type && cells[index.x, index.y, index.z].type.typeParams.door)
			{
				bool[] neighborsWalls = new bool[4];
				bool neighborUpDoor;
				bool neighborDownDoor;

				Vector3 rotation = new Vector3(0, 0, 0);
				string subType = "Door";

				if (index.y > 0 && cells[index.x, index.y - 1, index.z].type != null && cells[index.x, index.y - 1, index.z].type.typeParams.door)
					neighborDownDoor = true;
				else
					neighborDownDoor = false;

				if (index.y < cells.GetLength(1) - 1 && cells[index.x, index.y + 1, index.z].type != null && cells[index.x, index.y + 1, index.z].type.typeParams.door)
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

				if (index.x > 0 && cells[index.x - 1, index.y, index.z].type != null && cells[index.x - 1, index.y, index.z].type.typeParams.wall)
					neighborsWalls[0] = true;
				else
					neighborsWalls[0] = false;

				if (index.x < cells.GetLength(0) - 1 && cells[index.x + 1, index.y, index.z].type != null && cells[index.x + 1, index.y, index.z].type.typeParams.wall)
					neighborsWalls[1] = true;
				else
					neighborsWalls[1] = false;

				if (index.z > 0 && cells[index.x, index.y, index.z - 1].type != null && cells[index.x, index.y, index.z - 1].type.typeParams.wall)
					neighborsWalls[2] = true;
				else
					neighborsWalls[2] = false;

				if (index.z < cells.GetLength(2) - 1 && cells[index.x, index.y, index.z + 1].type != null && cells[index.x, index.y, index.z + 1].type.typeParams.wall)
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

		public static void StairTransform(Cell[,,] cells, Vector3Int index)
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
								neighborsStairs[i + 1, j + 1, k + 1] = CellIsStair(index.x + i, index.y + j, index.z + k, cells);
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

				cells[index.x, index.y, index.z].rotation = rotation;
				cells[index.x, index.y, index.z].TransformVisual(subType, rotation);
			}
		}

		public static bool CellIsStair(int x, int y, int z, Cell[,,] cells)
		{
			if (x >= 0 && y >= 0 && z >= 0 
				&& x < cells.GetLength(0) && y < cells.GetLength(1) && z < cells.GetLength(2)
				&& cells[x, y, z].type != null && cells[x, y, z].type.typeParams.stair)
				return true;
			else
				return false;
		}

		public static bool CellIsWall(int x, int y, int z, Cell[,,] cells)
		{
			if (x >= 0 && y >= 0 && z >= 0
				&& x < cells.GetLength(0) && y < cells.GetLength(1) && z < cells.GetLength(2)
				&& cells[x, y, z].type != null && cells[x, y, z].type.typeParams.wall)
				return true;
			else
				return false;
		}

		public static bool CellIsDoor(int x, int y, int z, Cell[,,] cells)
		{
			if (x >= 0 && y >= 0 && z >= 0
				&& x < cells.GetLength(0) && y < cells.GetLength(1) && z < cells.GetLength(2)
				&& cells[x, y, z].type != null && cells[x, y, z].type.typeParams.door)
				return true;
			else
				return false;
		}

		public static bool CellIsFloor(int x, int y, int z, Cell[,,] cells)
		{
			if (x >= 0 && y >= 0 && z >= 0
				&& x < cells.GetLength(0) && y < cells.GetLength(1) && z < cells.GetLength(2)
				&& cells[x, y, z].type != null && cells[x, y, z].type.typeParams.floor)
				return true;
			else
				return false;
		}

		public static bool CellIsLadder(int x, int y, int z, Cell[,,] cells)
		{
			if (x >= 0 && y >= 0 && z >= 0
				&& x < cells.GetLength(0) && y < cells.GetLength(1) && z < cells.GetLength(2)
				&& cells[x, y, z].type != null && cells[x, y, z].type.typeParams.ladder)
				return true;
			else
				return false;
		}
	}
}

