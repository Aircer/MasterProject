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
		}

		/// <summary>
		/// List of ALL waypoints inside the cluster
		/// </summary>
		private Waypoint[,,] waypoints;
		private Dictionary<CellInformation, List<Vector3Int>> waypointsDico;

		public Waypoint[,,] GetWaypoints()
		{
			return waypoints;
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

		public void FindPath(Vector3Int start, Vector3Int end, Vector2 maxJump)
		{
			CleanWaypoints();

			if (start != Constants.UNDEFINED_POSITION && end != Constants.UNDEFINED_POSITION)
				Pathfinding.FindRouteTo3(waypoints[start.x, start.y, start.z], waypoints[end.x, end.y, end.z], maxJump);
		}

		public void FindPath(Vector3Int start, Vector2 maxJump)
		{
			CleanWaypoints();

			Pathfinding.FloodFill(waypoints[start.x, start.y, start.z], maxJump);
		}

		public void CleanWaypoints()
		{
			for(int i =0; i < waypoints.GetLength(0); i++)
			{
				for (int j = 0; j < waypoints.GetLength(1); j++)
				{
					for (int k = 0; k < waypoints.GetLength(2); k++)
					{
						waypoints[i,j,k].inPath = false;
						waypoints[i, j, k].from = null;
						waypoints[i, j, k].gCost = 0;
						waypoints[i, j, k].hCost = 0;
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
	}
}
