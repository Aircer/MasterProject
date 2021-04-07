using UnityEngine;
using System.Collections.Generic;
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
					Debug.Log("PATh");
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
		public void ResetPathfinding()
		{
			pathfindingWaypoints.Clear();
			Debug.Log("HERE");
		}

		public Dictionary<CellInformation, List<Vector3Int>> GetWaypointsDico()
		{
			return waypointsDico;
		}

		public void CreateWaypoints(Vector3Int sg)
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
						//waypointsDico[null].Add(new Vector3Int(x, y, z));
						ConnectWaypoint(x, y, z);
					}
				}
			}
		}

		private void ConnectWaypoint(int x, int y, int z)
		{
			if (x - 1 > -1)
				waypoints[x, y, z].linkTo(waypoints[x - 1, y, z]);
			if (x - 1 > -1 && z - 1 > -1)
				waypoints[x, y, z].linkTo(waypoints[x - 1, y, z - 1]);
			if (y - 1 > -1)
				waypoints[x, y, z].linkTo(waypoints[x, y - 1, z]);
			if (z - 1 > -1)
				waypoints[x, y, z].linkTo(waypoints[x, y, z - 1]);
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

		public void CleanWaypoints()
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

		public void SetType(CellInformation type, int x, int y, int z)
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
				}
			}

			waypoints[x, y, z].SetType(type);
		}

		public void SetRotation(Vector2 rotation, int x, int y, int z)
		{
			waypoints[x, y, z].rotation = rotation;
		}

		public void SetBase(int x, int y, int z)
        {
			waypoints[x, y, z].baseType = true;
		}

		public void ResetBase(int x, int y, int z)
		{
			waypoints[x, y, z].baseType = false;
		}

		public void SetTypeAround(Vector3Int size_grid, Vector3 rotation, CellInformation type, Vector3Int index)
		{
			Vector3Int lowerBound = default(Vector3Int);
			Vector3Int upperBound = default(Vector3Int);
			SetBounds(ref lowerBound, ref upperBound, index, type.size, rotation);

			for (int i = lowerBound.x; i <= upperBound.x; i++)
			{
				for (int j = lowerBound.y; j <= upperBound.y; j++)
				{
					for (int k = lowerBound.z; k <= upperBound.z; k++)
					{
						if (InputInGridBoundaries(new Vector3Int(i, j, k), size_grid))
                        {
							SetType(type, i, j, k);
							waypoints[i, j, k].basePos = index;
						}
					}
				}
			}
			
			SetBase(index.x, index.y, index.z);
		}

		/// <summary>
		/// Check if input is in the grid.
		/// </summary>
		/// /// <returns>Return true if Input is in Boundaries.</returns>
		public static bool InputInGridBoundaries(Vector3 input, Vector3Int size_grid)
		{
			bool inBoundaries = true;

			if (input.x < 0 || input.y < 0 || input.z < 0 || input.x > size_grid.x - 1 || input.y > size_grid.y - 1 || input.z > size_grid.z - 1)
				inBoundaries = false;

			return inBoundaries;
		}

		public static void SetBounds(ref Vector3Int lowerBound, ref Vector3Int upperBound, Vector3Int index, Vector3Int size, Vector3 rotation)
		{
			Vector3Int newSize = default(Vector3Int);
			newSize.x = (int)Mathf.Cos(rotation.y * Mathf.Deg2Rad) * size.x + (int)Mathf.Sin(rotation.x * Mathf.Deg2Rad) * (int)Mathf.Sin(rotation.y * Mathf.Deg2Rad) * size.y + (int)Mathf.Sin(rotation.y * Mathf.Deg2Rad) * (int)Mathf.Cos(rotation.x * Mathf.Deg2Rad) * size.z;
			//newSize.x = newSize.x == 0 ? 1 : newSize.x;
			newSize.y = (int)Mathf.Cos(rotation.x * Mathf.Deg2Rad) * size.y - (int)Mathf.Sin(rotation.x * Mathf.Deg2Rad) * size.z;
			//newSize.y = newSize.y == 0 ? 1 : newSize.y;
			newSize.z = -(int)Mathf.Sin(rotation.y * Mathf.Deg2Rad) * size.x + (int)Mathf.Cos(rotation.y * Mathf.Deg2Rad) * (int)Mathf.Sin(rotation.x * Mathf.Deg2Rad) * size.y + (int)Mathf.Cos(rotation.y * Mathf.Deg2Rad) * (int)Mathf.Cos(rotation.x * Mathf.Deg2Rad) * size.z;
			//newSize.z = newSize.z == 0 ? 1 : newSize.z;

			lowerBound.x = newSize.x < 0 ? index.x + newSize.x + 1 : index.x;
			//lowerBound.x = lowerBound.x == 0 ? 1 : lowerBound.x;
			lowerBound.y = newSize.y < 0 ? index.y + newSize.y + 1 : index.y;
			//lowerBound.y = lowerBound.y == 0 ? 1 : lowerBound.y;
			lowerBound.z = newSize.z < 0 ? index.z + newSize.z + 1 : index.z;
			//lowerBound.z = lowerBound.z == 0 ? 1 : lowerBound.z;

			upperBound.x = newSize.x > 0 ? index.x + newSize.x - 1 : index.x;
			//upperBound.x = upperBound.x == 0 ? 1 : upperBound.x;
			upperBound.y = newSize.y > 0 ? index.y + newSize.y - 1 : index.y;
			//upperBound.y = upperBound.y == 0 ? 1 : upperBound.y;
			upperBound.z = newSize.z > 0 ? index.z + newSize.z - 1 : index.z;
			//upperBound.z = upperBound.z == 0 ? 1 : upperBound.z;

			//Debug.Log("size: " + newSize + "UPP: " + upperBound + " LOW: " + lowerBound);
		}
	}
}
