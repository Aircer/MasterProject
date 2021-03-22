using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace MapTileGridCreator.Core
{
	public class Waypoint : MonoBehaviour
	{
		/// Parent graph of the waypoint
		public WaypointCluster parent { get; set; }
		public Vector3Int key { get; set; }
		public CellInformation type { get; set; }
		public Cell cell { get; set; }
		public bool show { get; set; }
		public bool showFlood { get; set; }
		public Color colorDot = Color.white;

		public float hCost { get; set; }
		public float gCost { get; set; }
		public Waypoint from { get; set; }

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
			if (type != newType)
			{ 
				type = newType;
			}
		}

		public void ResetType()
		{
			if (type != null)
			{
				type = null;
			}
		}

		/// Draws the arrow from position "pos" in the direction "dir"
		/// <param name="pos"> Starting position of the arrow</param>
		/// <param name="dir"> Direction of the arrow</param>
		/// <param name="color"> Color of the arrow</param>
		/// <param name="arrowHeadLength"> Length of the arrow head line segments</param>
		/// <param name="arrowHeadAngle"> Angle of opening of the arrow head line segments</param>
		private static void ForGizmo(Vector3 pos, Vector3 direction, Color c, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
		{
			Gizmos.color = c;
			Gizmos.DrawRay(pos, direction-pos);
			Vector3 right = Quaternion.LookRotation(pos-direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * new Vector3(0, 0, 1);
			Vector3 left = Quaternion.LookRotation(pos - direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * new Vector3(0, 0, 1);
			Gizmos.DrawRay(pos, right * arrowHeadLength);
			Gizmos.DrawRay(pos, left * arrowHeadLength);
		}

		/// Draws the white square representing the nodes and an arrow for each outgoing edge
		public virtual void OnDrawGizmos()
		{
			if (show)
			{
				Gizmos.color = colorDot;
				ForGizmo(key, from.key, colorDot);
			}

			if (showFlood)
			{
				Gizmos.color = colorDot;
				Gizmos.DrawSphere(transform.position, 0.2f);
				//GUIStyle style = new GUIStyle();
				//style.normal.textColor = colorDot;
				//Handles.Label(transform.position, gCost.ToString("F1"), style);
			}
		}

		/// Draws the yellow square representing the selected node and a magenta arrow for each outgoing edge
		public virtual void OnDrawGizmosSelected()
		{
			Gizmos.color = Color.yellow;
			Gizmos.DrawCube(transform.position, new Vector3(0.5f, 0.5f, 0.5f));

			for (int i = 0; i < neighbors.Count; ++i)
			{
				Vector3 direction = neighbors[i].key - key;
				Gizmos.color = Color.red;
				Gizmos.DrawRay(this.transform.position, direction);
				//ForGizmo(transform.position + direction.normalized, direction - direction.normalized * 2f, Color.red, 2f);
			}

			for (int i = 0; i < neighbors.Count; ++i)
			{
				Vector3 direction = neighbors[i].key - key;
				Gizmos.color = Color.red;
				Gizmos.DrawRay(this.transform.position, direction);
				//ForGizmo(transform.position + direction.normalized, direction - direction.normalized * 2f, Color.red, 2f);
			}
		}
	}
}