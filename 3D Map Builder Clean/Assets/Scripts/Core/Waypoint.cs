using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace MapTileGridCreator.Core
{
	public class Waypoint
	{
		public Vector3Int key { get; set; }
		public CellInformation type { get; set; }
		public bool baseType { get; set; }
		public Vector3Int basePos { get; set; }
		public bool pathfindingWaypoint { get; set; }
		public bool show { get; set; }
		public Vector2 rotation { get; set; }
		public bool inPath { get; set; }
		public Color colorDot = Color.white;
		public float hCost { get; set; }
		public float gCost { get; set; }
		public Waypoint from { get; set; }
		public bool IAChangedMe { get; set; }

		/// The outgoing list of edges
		public List<Waypoint> neighbors = new List<Waypoint>();
		public Waypoint UpNeighbor;
		public Waypoint DownNeighbor;

		/// Links this waypoint (directionally) with the passed waypoint and sets the probabilities of all edges to the same
		/// <param name="node"> Node to be linked to</param>
		public void linkTo(Waypoint waypoint)
		{
			if (waypoint == this)
			{
				Debug.LogError("A waypoint cannot be linked to itself");
				return;
			}
			for (int i = 0; i < neighbors.Count; ++i) if (waypoint == neighbors[i]) return;

			neighbors.Add(waypoint);
			waypoint.neighbors.Add(this);

			if (waypoint.key.y > this.key.y)
			{
				UpNeighbor = waypoint;
				waypoint.DownNeighbor = this;
			}

			if (waypoint.key.y < this.key.y)
			{
				DownNeighbor = waypoint;
				waypoint.UpNeighbor = this;
			}
		}

		/// Removes a link (directionally) between this waypoiny and the passed waypoint and sets the probabilities of all edges to the same
		/// <param name="node"> Node to remove the link from</param>
		public void unlinkFrom(Waypoint waypoint)
		{
			for (int i = 0; i < neighbors.Count; ++i) if (neighbors[i] == waypoint) neighbors.RemoveAt(i);
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
	}
}