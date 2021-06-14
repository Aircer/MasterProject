﻿using UnityEngine;
using System.Collections.Generic;
using MapTileGridCreator.UtilitiesMain;
using UnityEditor;

namespace MapTileGridCreator.Core
{
	public class WaypointCluster 
	{
		public WaypointCluster(Vector3Int size, List<CellInformation> cellInfos)
		{
			CreateWaypoints(size);
			pathfindingWaypoints = new List<Waypoint>();
			this.cellInfos = cellInfos;
			this.size = size;
		}

		public WaypointCluster(Vector3Int sizeDNA, int[][][] newWaypointsParams, CellInformation[] cellsInfos)
		{
			pathfindingWaypoints = new List<Waypoint>();
			waypoints = new Waypoint[sizeDNA.x-2, sizeDNA.y-2, sizeDNA.z-2];
			this.size = new Vector3Int(sizeDNA.x - 2, sizeDNA.y - 2, sizeDNA.z - 2);
			this.cellInfos = new List<CellInformation>();

			for (int i = 0; i < cellsInfos.Length; i++)
			{
				this.cellInfos.Add(cellsInfos[i]);
			}

			for (int x = 1; x < sizeDNA.x-1; x++)
			{
				for (int y = 1; y < sizeDNA.y-1; y++)
				{
					for (int z = 1; z < sizeDNA.z-1; z++)
					{
						Waypoint newWaypoint = new Waypoint();
						newWaypoint.Initialize(sizeDNA, new Vector3Int(x-1, y-1, z-1), this);
						if (newWaypointsParams[x][y][z] > 0)
							newWaypoint.type = cellInfos[newWaypointsParams[x][y][z] - 1];
						else
							newWaypoint.type = null;

						waypoints[x-1, y-1, z-1] = newWaypoint;
					}
				}
			}
		}

		/// <summary>
		/// List of ALL waypoints inside the cluster
		/// </summary>
		private Waypoint[,,] waypoints;
		private List<Waypoint> pathfindingWaypoints;
		public Vector3Int size;
		public List<CellInformation> cellInfos;

		public Waypoint[,,] GetWaypoints()
		{
			return waypoints;
		}

		public int[][][] GetWaypointsParams()
		{
			Vector3Int size = new Vector3Int(waypoints.GetLength(0), waypoints.GetLength(1), waypoints.GetLength(2));
			int[][][] waypointsParamsXYZ = new int[size.x + 2][][];

			for (int x = 0; x < size.x+2; x++)
			{
				int[][] waypointsParamsYZ = new int[size.y + 2][];
				for (int y = 0; y < size.y+2; y++)
				{
					int[] waypointsParamsZ = new int[size.z + 2];
					for (int z = 0; z < size.z+2; z++)
					{
						//Genes are bigger than cluster to have empty borders thus it is easier to get neighbors  
						if (x == 0 || y == 0 || z == 0 || x == size.x + 1 || y == size.y + 1 || z == size.z + 1)
						{
							waypointsParamsZ[z] = 0;
						}
						else
						{
							waypointsParamsZ[z] = cellInfos.IndexOf(waypoints[x - 1, y - 1, z - 1].type)+1;
						}
					}
					waypointsParamsYZ[y] = waypointsParamsZ;
				}
				waypointsParamsXYZ[x] = waypointsParamsYZ;
			}

			return waypointsParamsXYZ;
		}

		public void SetPathfinding(Vector3Int index, PathfindingState state, Vector2 maxjump)
		{
			CleanInPathWaypoints();
			if (state == PathfindingState.A_Star)
			{
				if (pathfindingWaypoints != null && pathfindingWaypoints.Contains(waypoints[index.x, index.y, index.z]))
				{
					pathfindingWaypoints.Remove(waypoints[index.x, index.y, index.z]);
					waypoints[index.x, index.y, index.z].pathfindingWaypoint = false;
				}
				else
				{
					pathfindingWaypoints.Add(waypoints[index.x, index.y, index.z]);
					waypoints[index.x, index.y, index.z].pathfindingWaypoint = true;
				}

				for (int i = 0; i < pathfindingWaypoints.Count - 1; i++)
				{
					FindPath(pathfindingWaypoints[i], pathfindingWaypoints[i + 1], maxjump);
				}

			}
			else if (state == PathfindingState.Floodfill)
			{
				foreach (Waypoint wp in pathfindingWaypoints)
				{
					wp.pathfindingWaypoint = false;
				}
				pathfindingWaypoints.Clear();
				pathfindingWaypoints.Add(waypoints[index.x, index.y, index.z]);
				waypoints[index.x, index.y, index.z].pathfindingWaypoint = true;

				FindPath(waypoints[index.x, index.y, index.z], maxjump);
			}
		}

		private void CreateWaypoints(Vector3Int sg)
		{
			waypoints = new Waypoint[sg.x, sg.y, sg.z];

			//Create all the vertices
			for (int x = 0; x < sg.x; x++)
			{
				for (int y = 0; y < sg.y; y++)
				{
					for (int z = 0; z < sg.z; z++)
					{
						Waypoint newWaypoint = new Waypoint();
						newWaypoint.Initialize(sg, new Vector3Int(x, y, z), this);
						waypoints[x, y, z] = newWaypoint;
					}
				}
			}
		}

		public void FindPath(Waypoint start, Waypoint end, Vector2 maxJump)
		{
			CleanWaypoints();
			Pathfinding.FindRouteTo3(start, end, maxJump);
		}

		public void FindPath(Waypoint start, Vector2 maxJump)
		{
			CleanWaypoints();
			Pathfinding.FloodFill(start, maxJump);
		}

		private void CleanWaypoints()
		{
			for(int i =0; i < waypoints.GetLength(0); i++)
			{
				for (int j = 0; j < waypoints.GetLength(1); j++)
				{
					for (int k = 0; k < waypoints.GetLength(2); k++)
					{
						waypoints[i, j, k].from = null;
						waypoints[i, j, k].gCost = 0;
						waypoints[i, j, k].hCost = 0;
					}
				}
			}
		}

		public void CleanInPathWaypoints()
		{
			for (int i = 0; i < waypoints.GetLength(0); i++)
			{
				for (int j = 0; j < waypoints.GetLength(1); j++)
				{
					for (int k = 0; k < waypoints.GetLength(2); k++)
					{
						waypoints[i,j,k].inPath = false;
					}
				}
			}
		}

		private void SetBase(Vector3Int index, CellInformation type, Vector3 rotation)
		{ 
			if (type != null)
			{
				waypoints[index.x, index.y, index.z].SetBase(true);
			}
		}

		private void SetTypeAround(Vector3Int size_grid, Vector3 rotation, CellInformation type, Vector3Int index)
		{
			if (type != null)
			{
				waypoints[index.x, index.y, index.z].SetType(type);
				waypoints[index.x, index.y, index.z].SetBasePos(index);
				waypoints[index.x, index.y, index.z].SetBase(true);
				SetBase(index, type, rotation);
				//Debug.Log(minSize + " -- " + maxSize);
			}
		}

		public CellInformation RemoveTypeAround(Vector3Int size_grid, Vector3Int index)
		{
			CellInformation type = waypoints[index.x, index.y, index.z].type;
			waypoints[index.x, index.y, index.z].SetType(null);
			/*
			Vector3Int lowerBound = default(Vector3Int);
			Vector3Int upperBound = default(Vector3Int);
			Vector3Int basePos = waypoints[index.x, index.y, index.z].basePos;
			Waypoint baseWaypoint = waypoints[basePos.x, basePos.y, basePos.z];
			CellInformation type = baseWaypoint.type;
			if (baseWaypoint != null && baseWaypoint.type != null)
			{
				FuncMain.SetBounds(ref lowerBound, ref upperBound, basePos, baseWaypoint.type.typeParams.size, baseWaypoint.rotation);

				for (int i = lowerBound.x; i <= upperBound.x; i++)
				{
					for (int j = lowerBound.y; j <= upperBound.y; j++)
					{
						for (int k = lowerBound.z; k <= upperBound.z; k++)
						{
							if (FuncMain.InputInGridBoundaries(new Vector3Int(i, j, k), size_grid))
							{
								waypoints[i, j, k].SetType(null);
								//UnsetSize(i, j, k);
							}
						}
					}
				}

				waypoints[index.x, index.y, index.z].SetBase(false);
				//Debug.Log(minSize + " -- " + maxSize);
			}*/

			return type;
		}

		public void SetTypeAndRotationAround(Vector3Int size_grid, Vector3 rotation, CellInformation type, Vector3Int index)
		{
			SetTypeAround(size_grid, rotation, type, index);
			waypoints[index.x, index.y, index.z].SetRotation(rotation);
		}

		/*
		public void SetSize(int x, int y, int z)
        {
			if (x < minSize.x)
				minSize.x = x;
			if (y < minSize.y)
				minSize.y = y;
			if (z < minSize.z)
				minSize.z = z;

			if (x+1 > maxSize.x)
				maxSize.x = x+1;
			if (y+1 > maxSize.y)
				maxSize.y = y+1;
			if (z+1 > maxSize.z)
				maxSize.z = z+1;
		}*/
		/*
		public void UnsetSize(int x, int y, int z)
		{
			if (x + 1 < maxSize.x)
            {
				int i = x;
				int j = minSize.y;
				int k = minSize.z;
				bool newLimit = true;

				while (i < maxSize.x && newLimit)
				{
					j = minSize.y;
					while (j < maxSize.y && newLimit)
					{
						k = minSize.z;
						while (k < maxSize.z && newLimit)
						{
							if (waypoints[i, j, k].type != null) newLimit = false;
							k++;
						}
						j++;
					}
					i++;
				}

				if (newLimit)
					maxSize.x = x+1;
			}

			if (y + 1 < maxSize.y)
			{
				int i = minSize.x;
				int j = y;
				int k = minSize.z;
				bool newLimit = true;

				while (j < maxSize.y && newLimit)
				{
					i = minSize.x;
					while (i < maxSize.x && newLimit)
					{
						k = minSize.z;
						while (k < maxSize.z && newLimit)
						{
							if (waypoints[i, j, k].type != null) newLimit = false;
							k++;
						}
						i++;
					}
					j++;
				}

				if (newLimit)
					maxSize.y = y + 1;
			}

			if (z + 1 < maxSize.z)
			{
				int i = minSize.x;
				int j = minSize.y;
				int k = z;
				bool newLimit = true;

				while (k < maxSize.z && newLimit)
				{
					i = minSize.x;
					while (i < maxSize.x && newLimit)
					{
						j = minSize.y;
						while (j < maxSize.y && newLimit)
						{
							if (waypoints[i, j, k].type != null) newLimit = false;
							j++;
						}
						i++;
					}
					k++;
				}

				if (newLimit)
					maxSize.z = z + 1;
			}

			if (x > minSize.x)
			{
				int i = maxSize.x;
				int j = maxSize.y;
				int k = maxSize.z;
				bool newLimit = true;

				while (x-1 < i && newLimit)
				{
					j = maxSize.y;
					while (minSize.y > j && newLimit)
					{
						k = maxSize.z;
						while (minSize.z > k && newLimit)
						{
							if (waypoints[i, j, k].type != null) newLimit = false;
							k--;
						}
						j--;
					}
					i--;
				}

				if (newLimit)
					minSize.x = x;
			}

			if (y > minSize.y)
			{
				int i = maxSize.x;
				int j = maxSize.y;
				int k = maxSize.z;
				bool newLimit = true;

				while (y-1 < j && newLimit)
				{
					i = maxSize.x;
					while (minSize.x > i && newLimit)
					{
						k = maxSize.z;
						while (minSize.z > k && newLimit)
						{
							if (waypoints[i, j, k].type != null) newLimit = false;
							k--;
						}
						i--;
					}
					j--;
				}

				if (newLimit)
					minSize.y = y;
			}

			if (z > minSize.z)
			{
				int i = maxSize.x;
				int j = maxSize.y;
				int k = maxSize.z;
				bool newLimit = true;

				while (z-1 < k && newLimit)
				{
					i = maxSize.x;
					while (minSize.x > i && newLimit)
					{
						j = maxSize.y;
						while (minSize.y > j && newLimit)
						{
							if (waypoints[i, j, k].type != null) newLimit = false;
							j--;
						}
						i--;
					}
					k--;
				}

				if (newLimit)
					minSize.z = z;
			}
		}*/
	}
}
