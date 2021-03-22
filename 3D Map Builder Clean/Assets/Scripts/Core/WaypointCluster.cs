using UnityEngine;
using System.Collections.Generic;
using UnityEditor;

namespace MapTileGridCreator.Core
{
	public class WaypointCluster
	{
		/// <summary>
		/// List of ALL waypoints inside the cluster
		/// </summary>
		public Dictionary<Vector3Int, Waypoint> waypoints = new Dictionary<Vector3Int, Waypoint>();

		private Vector3 size_grid;

		public void CreateWaypoints(Dictionary<Vector3Int, Cell> cells, Vector3 sg)
		{
			waypoints.Clear();
			size_grid = sg; 

			//Create all the vertices
			for (int x = 0; x < size_grid.x; x++)
			{
				for (int y = 0; y < size_grid.y; y++)
				{
					for (int z = 0; z < size_grid.z; z++)
					{
						cells[new Vector3Int(x, y, z)].waypoint = CreateWaypoint(cells[new Vector3Int(x, y, z)]);
					}
				}
			}

			for (int x = 0; x < size_grid.x; x++)
			{
				for (int y = 0; y < size_grid.y; y++)
				{
					for (int z = 0; z < size_grid.z; z++)
					{
						ConnectWaypoint(x, y, z);
					}
				}
			}
		}

		private void ConnectWaypoint(int x, int y, int z)
		{
			if (x - 1 > -1)
				waypoints[new Vector3Int(x, y, z)].linkTo(waypoints[new Vector3Int(x - 1, y, z)]);
			if (x - 1 > -1 && z - 1 > -1)
				waypoints[new Vector3Int(x, y, z)].linkTo(waypoints[new Vector3Int(x - 1, y, z - 1)]);
			if (x - 1 > -1 && z + 1 < size_grid.z)
				waypoints[new Vector3Int(x, y, z)].linkTo(waypoints[new Vector3Int(x - 1, y, z + 1)]);
			if (x + 1 < size_grid.x)
				waypoints[new Vector3Int(x, y, z)].linkTo(waypoints[new Vector3Int(x + 1, y, z)]);
			if (x + 1 < size_grid.x && z - 1 > -1)
				waypoints[new Vector3Int(x, y, z)].linkTo(waypoints[new Vector3Int(x + 1, y, z - 1)]);
			if (x + 1 < size_grid.x && z + 1 < size_grid.z)
				waypoints[new Vector3Int(x, y, z)].linkTo(waypoints[new Vector3Int(x + 1, y, z + 1)]);
			if (y - 1 > -1)
				waypoints[new Vector3Int(x, y, z)].linkTo(waypoints[new Vector3Int(x, y - 1, z)]);
			if (y + 1 < size_grid.y)
				waypoints[new Vector3Int(x, y, z)].linkTo(waypoints[new Vector3Int(x, y + 1, z)]);
			if (z - 1 > -1)
				waypoints[new Vector3Int(x, y, z)].linkTo(waypoints[new Vector3Int(x, y, z - 1)]);
			if (z + 1 < size_grid.z)
				waypoints[new Vector3Int(x, y, z)].linkTo(waypoints[new Vector3Int(x, y, z + 1)]);
		}

		public Waypoint CreateWaypoint(Cell currentCell)
		{
			currentCell.gameObject.AddComponent<Waypoint>();

			Waypoint newWaypoint = currentCell.GetComponent<Waypoint>();

			waypoints.Add(currentCell.index, newWaypoint);
			newWaypoint.parent = this;
			newWaypoint.key = currentCell.index;
			newWaypoint.cell = currentCell;
			return newWaypoint;
		}

		public void FindPath(Vector3Int start, Vector3Int end, Vector2 maxJump)
		{
			CleanWaypoints();

			if (waypoints.ContainsKey(start) && waypoints.ContainsKey(end))
				Pathfinding.FindRouteTo3(waypoints[start], waypoints[end], maxJump);
		}

		public void FindPath(Vector3Int start, Vector2 maxJump)
		{
			CleanWaypoints();

			Pathfinding.FloodFill(waypoints[start], maxJump);
		}

		public void CleanWaypoints()
		{
			foreach (var item in waypoints)
			{
				item.Value.show = false;
				item.Value.showFlood = false;
				item.Value.gCost = 0;
				item.Value.hCost = 0;
			}
		}
	}
}
