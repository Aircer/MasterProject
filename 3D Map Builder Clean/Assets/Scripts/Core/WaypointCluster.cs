using UnityEngine;
using System.Collections.Generic;
using MapTileGridCreator.UtilitiesMain;
using UnityEditor;

namespace MapTileGridCreator.Core
{
	public class WaypointCluster 
	{
		public WaypointCluster(Vector3Int size)
		{
			CreateWaypoints(size);
			pathfindingWaypoints = new List<Waypoint>();
		}

		public WaypointCluster(CellInformation[,,] newCellInfos)
		{
			CreateWaypoints(new Vector3Int(newCellInfos.GetLength(0), newCellInfos.GetLength(1), newCellInfos.GetLength(2)));
			pathfindingWaypoints = new List<Waypoint>();
			waypointsDico = new Dictionary<CellInformation, List<Vector3Int>>();

			for (int x = 0; x < waypoints.GetLength(0); x++)
			{
				for (int y = 0; y < waypoints.GetLength(1); y++)
				{
					for (int z = 0; z < waypoints.GetLength(2); z++)
					{
						waypoints[x, y, z].type = newCellInfos[x, y, z];
						waypoints[x, y, z].baseType = true;
						waypoints[x, y, z].show = true;

						if (waypoints[x, y, z].type != null)
                        {
							if (waypointsDico.ContainsKey(waypoints[x, y, z].type))
							{
								waypointsDico[waypoints[x, y, z].type].Add(new Vector3Int(x, y, z));
							}
							else
							{
								List<Vector3Int> newList = new List<Vector3Int>();
								newList.Add(new Vector3Int(x, y, z));
								waypointsDico.Add(waypoints[x, y, z].type, newList);
							}
						}
					}
				}
			}
		}

		/// <summary>
		/// List of ALL waypoints inside the cluster
		/// </summary>
		private Waypoint[,,] waypoints;
		private Dictionary<CellInformation, List<Vector3Int>> waypointsDico;
		private List<Waypoint> pathfindingWaypoints;

		public Waypoint[,,] GetWaypoints()
		{
			return waypoints;
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

		public Dictionary<CellInformation, List<Vector3Int>> GetWaypointsDico()
		{
			return waypointsDico;
		}

		private void CreateWaypoints(Vector3Int sg)
		{
			waypoints = new Waypoint[sg.x, sg.y, sg.z];
			waypointsDico = new Dictionary<CellInformation, List<Vector3Int>>();

			//Create all the vertices
			for (int x = 0; x < sg.x; x++)
			{
				for (int y = 0; y < sg.y; y++)
				{
					for (int z = 0; z < sg.z; z++)
					{
						Waypoint newWaypoint = new Waypoint();
						newWaypoint.key = new Vector3Int(x, y, z);
						newWaypoint.type = null;
						newWaypoint.baseType = false;
						waypoints[x, y, z] = newWaypoint;
						ConnectWaypoint(x, y, z, sg);
					}
				}
			}
		}

		private void ConnectWaypoint(int x, int y, int z, Vector3Int sg)
		{
			if (x - 1 > -1)
				waypoints[x, y, z].linkTo(waypoints[x - 1, y, z]);
			if (x - 1 > -1 && z - 1 > -1)
				waypoints[x, y, z].linkTo(waypoints[x - 1, y, z - 1]);
			if (z - 1 > -1)
				waypoints[x, y, z].linkTo(waypoints[x, y, z - 1]);
			if (y - 1 > -1)
				waypoints[x, y, z].linkTo(waypoints[x, y - 1, z]);
			if(x - 1 > -1 && z + 1 < sg.z)
			waypoints[x, y, z].linkTo(waypoints[x - 1, y, z + 1]);
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

		private void SetType(CellInformation type, int x, int y, int z)
		{ 
			if (type != null)
			{
				if (waypointsDico.ContainsKey(type))
				{
					waypointsDico[type].Add(new Vector3Int(x, y, z));
				}
				else
				{
					List<Vector3Int> newList = new List<Vector3Int>();
					newList.Add(new Vector3Int(x, y, z));
					waypointsDico.Add(type, newList);
				}
			}
			else
			{ 
				if(waypoints[x, y, z].type != null)
                {
					waypointsDico[waypoints[x, y, z].type].Remove(new Vector3Int(x, y, z));
					
					if(waypointsDico[waypoints[x, y, z].type].Count == 0)
                    {
						waypointsDico.Remove(waypoints[x, y, z].type);
                    }
				}
			}

			waypoints[x, y, z].SetType(type);
		}

		private void SetBase(int x, int y, int z)
        {
			waypoints[x, y, z].baseType = true;
		}

		private void ResetBase(int x, int y, int z)
		{
			waypoints[x, y, z].baseType = false;
		}

		private void SetTypeAround(Vector3Int size_grid, Vector3 rotation, CellInformation type, Vector3Int index)
		{
			Vector3Int lowerBound = default;
			Vector3Int upperBound = default;
			FuncMain.SetBounds(ref lowerBound, ref upperBound, index, type.size, rotation);

			for (int i = lowerBound.x; i <= upperBound.x; i++)
			{
				for (int j = lowerBound.y; j <= upperBound.y; j++)
				{
					for (int k = lowerBound.z; k <= upperBound.z; k++)
					{
						if (FuncMain.InputInGridBoundaries(new Vector3Int(i, j, k), size_grid))
                        {
							SetType(type, i, j, k);
							waypoints[i, j, k].basePos = index;
						}
					}
				}
			}
			
			SetBase(index.x, index.y, index.z);
		}

		private void SetRotation(Vector2 rotation, Vector3Int index)
		{
			waypoints[index.x, index.y, index.z].rotation = rotation;
		}

		public void RemoveTypeAround(Vector3Int size_grid, Vector3 rotation, CellInformation type, Vector3Int index)
		{
			Vector3Int lowerBound = default(Vector3Int);
			Vector3Int upperBound = default(Vector3Int);
			Vector3Int basePos = waypoints[index.x, index.y, index.z].basePos;
			Waypoint baseWaypoint = waypoints[basePos.x, basePos.y, basePos.z];

			if (baseWaypoint != null && baseWaypoint.type != null)
			{
				FuncMain.SetBounds(ref lowerBound, ref upperBound, basePos, baseWaypoint.type.size, baseWaypoint.rotation);

				for (int i = lowerBound.x; i <= upperBound.x; i++)
				{
					for (int j = lowerBound.y; j <= upperBound.y; j++)
					{
						for (int k = lowerBound.z; k <= upperBound.z; k++)
						{
							if (FuncMain.InputInGridBoundaries(new Vector3Int(i, j, k), size_grid))
							{
								SetType(null, i, j, k);
							}
						}
					}
				}

				ResetBase(basePos.x, basePos.y, basePos.z);
			}
		}

		public void SetTypeAndRotationAround(Vector3Int size_grid, Vector3 rotation, CellInformation type, Vector3Int index)
		{
			SetTypeAround(size_grid, rotation, type, index);
			SetRotation(rotation, index);
		}
	}
}
