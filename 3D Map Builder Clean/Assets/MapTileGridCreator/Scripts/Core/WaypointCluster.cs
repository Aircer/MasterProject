using UnityEngine;
using System.Collections.Generic;
using UnityEditor;

namespace MapTileGridCreator.Core
{
	public class WaypointCluster: MonoBehaviour
	{
		/// <summary>
		/// List of ALL waypoints inside the cluster
		/// </summary>
		public Dictionary<Vector3Int, Waypoint> waypoints = new Dictionary<Vector3Int, Waypoint>();
		/// <summary>
		/// Numeric id assigned to the waypoint
		/// </summary>
		private uint currentID = 0;

		private Vector3 size_grid;

		public void CreateWaypoints(Vector3 sg)
		{
			currentID = 0;
			waypoints.Clear();
			size_grid = sg; 

			int childs = transform.childCount;
			for (int i = childs - 1; i > -1; i--)
			{
				GameObject.DestroyImmediate(transform.GetChild(i).gameObject);
			}

			//Create all the vertices
			for (int x = 0; x < size_grid.x; x++)
			{
				for (int y = 0; y < size_grid.y; y++)
				{
					for (int z = 0; z < size_grid.z; z++)
					{
						CreateWaypoint(new Vector3Int(x, y, z));
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

		public Waypoint CreateWaypoint(Vector3Int key)
		{
			GameObject waypointAux = Resources.Load("Waypoint") as GameObject;
			GameObject waypointInstance = PrefabUtility.InstantiatePrefab(waypointAux) as GameObject;
			waypointInstance.transform.position = key;
			waypointInstance.transform.parent = this.transform;
			waypointInstance.name = currentID.ToString();
			++currentID;
			waypoints.Add(key, waypointInstance.GetComponent<Waypoint>());
			waypointInstance.GetComponent<Waypoint>().SetParent(this);
			waypointInstance.GetComponent<Waypoint>().SetKey(key); 
			return waypointInstance.GetComponent<Waypoint>();
		}

		public void FindPath(Vector3Int start, Vector3Int end)
		{
			List<Waypoint> points = new List<Waypoint>();
			points.AddRange(waypoints.Values);
			List<Waypoint> way;

			foreach (Waypoint point in points)
			{
				point.SetShow(false);
			}

			way = Pathfinding.FindRouteTo2(waypoints[start], waypoints[end]);

			if (way == null)
				Debug.Log("Path blocked");
			else 
			{
				foreach (Waypoint point in way)
				{
					point.SetShow(true);
				}
			}
		}

		public void PathBlocked(Vector3Int index, bool removeWaypoint)
		{
			if (removeWaypoint)
				waypoints[index].IsBlocking();
			else
			{
				ConnectWaypoint(index.x, index.y, index.z);
			}
		}

	}
}
