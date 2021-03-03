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
		public List<Waypoint> waypoints = new List<Waypoint>();
		/// <summary>
		/// Numeric id assigned to the waypoint
		/// </summary>
		private uint currentID = 0;

		/// <summary>
		/// Creates a new waypoint
		/// </summary>
		/// <param name="point"> The point where the waypoint will be created</param>
		/// <returns>Returns the created waypoint</returns>
		/// 

		public float[,] Positions = new float[,] {
			 {-10,-10,0},
			 {0,0,0},
			 {20,0,0}};

		public void CreateWaypoints(Vector3 size_grid)
		{
			currentID = 0;
			waypoints.Clear();

			//Create all the vertices
			for (int x = 0; x < size_grid.x; x++)
			{
				for (int y = 0; y < size_grid.y; y++)
				{
					for (int z = 0; z < size_grid.z; z++)
					{
						CreateWaypoint(x, y, z);
					}
				}
			}
		}

		public Waypoint CreateWaypoint(float posX, float posY, float posZ)
		{
			GameObject waypointAux = Resources.Load("Waypoint") as GameObject;
			GameObject waypointInstance = PrefabUtility.InstantiatePrefab(waypointAux) as GameObject;
			waypointInstance.transform.position = new Vector3(posX, posY, posZ);
			waypointInstance.transform.parent = this.transform;
			waypointInstance.name = currentID.ToString();
			++currentID;
			waypoints.Add(waypointInstance.GetComponent<Waypoint>());
			waypointInstance.GetComponent<Waypoint>().setParent(this);
			return waypointInstance.GetComponent<Waypoint>();
		}

	}
}
