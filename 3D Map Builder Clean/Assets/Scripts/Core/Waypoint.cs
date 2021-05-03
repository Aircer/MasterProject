using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace MapTileGridCreator.Core
{
	public class Waypoint
	{
		private Vector3Int sizeCluster;
		public WaypointCluster cluster;
		public Vector3Int key { get; private set; }
		public CellInformation type { get; set; }

		public Vector2 rotation { get; set; }
		public bool baseType { get; set; }
		public Vector3Int basePos { get; set; }

		public bool pathfindingWaypoint { get; set; }
		public bool show { get; set; }
		public bool inPath { get; set; }
		public Color colorDot = Color.white;
		public float hCost { get; set; }
		public float gCost { get; set; }
		public Waypoint from { get; set; }
		public Waypoint inPathFrom { get; set; }
		public bool IAChangedMe { get; set; }

		public List<Waypoint> GetNeighbors() 
		{
			List<Waypoint> neighbors = new List<Waypoint>();
			neighbors.AddRange(GetSideNeighborsX());
			neighbors.AddRange(GetSideNeighborsZ());
			if (GetUpNeighbor() != null)
				neighbors.Add(GetUpNeighbor());
			if (GetDownNeighbor() != null)
				neighbors.Add(GetDownNeighbor());
			return neighbors;
		}

		public List<Waypoint> GetSideNeighbors()
		{
			List<Waypoint> neighbors = new List<Waypoint>();
			neighbors.AddRange(GetSideNeighborsX());
			neighbors.AddRange(GetSideNeighborsZ());
			return neighbors;
		}

		public List<Waypoint> GetVerticalAndXNeighbors()
		{
			List<Waypoint> neighbors = new List<Waypoint>();
			neighbors.AddRange(GetSideNeighborsX());
			if (GetUpNeighbor() != null)
				neighbors.Add(GetUpNeighbor());
			if (GetDownNeighbor() != null)
				neighbors.Add(GetDownNeighbor());
			return neighbors;
		}

		public List<Waypoint> GetVerticalAndZNeighbors()
		{
			List<Waypoint> neighbors = new List<Waypoint>();
			neighbors.AddRange(GetSideNeighborsZ());
			if (GetUpNeighbor() != null)
				neighbors.Add(GetUpNeighbor());
			if (GetDownNeighbor() != null)
				neighbors.Add(GetDownNeighbor());
			return neighbors;
		}

		public List<Waypoint> GetSideNeighborsX() 
		{
			List<Waypoint> sideNeighborsX = new List<Waypoint>();

			if (key.x > 0)
				sideNeighborsX.Add(cluster.GetWaypoints()[key.x - 1, key.y, key.z]);
			if (key.x < sizeCluster.x-1)
				sideNeighborsX.Add(cluster.GetWaypoints()[key.x + 1, key.y, key.z]);

			return sideNeighborsX; 
		}

		public List<Waypoint> GetSideNeighborsZ()
		{
			List<Waypoint> sideNeighborsZ = new List<Waypoint>();

			if (key.z > 0)
				sideNeighborsZ.Add(cluster.GetWaypoints()[key.x, key.y, key.z - 1]);
			if (key.z < sizeCluster.z - 1)
				sideNeighborsZ.Add(cluster.GetWaypoints()[key.x, key.y, key.z + 1]);

			return sideNeighborsZ;
		}

		public Waypoint GetUpNeighbor() 
		{
			if (key.y < sizeCluster.y - 1)
				return cluster.GetWaypoints()[key.x, key.y + 1, key.z];
			else
				return null;

		}

		public Waypoint GetDownNeighbor()
		{
			if (key.y > 0)
				return cluster.GetWaypoints()[key.x, key.y - 1, key.z];
			else
				return null;
		}

		public void Initialize(Vector3Int size, Vector3Int newKey, WaypointCluster cluster)
		{
			sizeCluster = size;
			this.cluster = cluster;
			key = newKey;
			type = null;
			baseType = false;
			show = true;
		}

		public void SetType(CellInformation newType)
		{
			if (type == null || type != newType)
			{
				type = newType;

				if (type == null)
				{
					inPath = false;
					baseType = false;
					show = false;
				}
				else
					show = true;
			}
		}

		public void SetBase(bool state)
		{
			baseType = state;
		}

		public void SetBasePos(Vector3Int pos)
		{
			basePos = pos;
		}

		public void SetRotation(Vector3 rot)
        {
			rotation = rot;
        }
	}
}