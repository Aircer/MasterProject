using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace MapTileGridCreator.Core
{
	public class Waypoint : MonoBehaviour
	{
		[SerializeField]
		[HideInInspector]
		/// Parent graph of the waypoint
		protected WaypointCluster parent;

		/// The outgoing list of edges
		public List<Waypoint> outs = new List<Waypoint>();

		[HideInInspector]
		/// Incoming list of edges, hidden in the inspector
		public List<Waypoint> ins = new List<Waypoint>();

		public void setParent(WaypointCluster wc) { parent = wc; }

		public WaypointCluster getParent() { return parent; }

		/// Links this waypoint (directionally) with the passed waypoint and sets the probabilities of all edges to the same
		/// <param name="node"> Node to be linked to</param>
		public void linkTo(Waypoint waypoint)
		{
			if (waypoint == this)
			{
				Debug.LogError("A waypoint cannot be linked to itself");
				return;
			}
			for (int i = 0; i < outs.Count; ++i) if (waypoint == outs[i]) return;
			if (waypoint.ins.Contains(this)) return;
			outs.Add(waypoint);
			waypoint.ins.Add(this);
		}

		/// Removes a link (directionally) between this waypoiny and the passed waypoint and sets the probabilities of all edges to the same
		/// <param name="node"> Node to remove the link from</param>
		public void unlinkFrom(Waypoint waypoint)
		{
			for (int i = 0; i < outs.Count; ++i) if (outs[i] == waypoint) outs.RemoveAt(i);
			waypoint.ins.Remove(this);
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
			Gizmos.DrawRay(pos, direction);
			Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * new Vector3(0, 0, 1);
			Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * new Vector3(0, 0, 1);
			Gizmos.DrawRay(pos + direction, right * arrowHeadLength);
			Gizmos.DrawRay(pos + direction, left * arrowHeadLength);
		}

		/// Draws the white square representing the nodes and an arrow for each outgoing edge
		public virtual void OnDrawGizmos()
		{

			// Draw a yellow sphere at the transform's position
			Gizmos.color = Color.white;
			Gizmos.DrawCube(transform.position, new Vector3(0.5f, 0.5f, 0.5f));
			for (int i = 0; i < outs.Count; ++i)
			{
				Vector3 direction = outs[i].transform.position - transform.position;
				ForGizmo(transform.position + direction.normalized, direction - direction.normalized * 2f, Color.red, 2f);
			}
		}

		/// Draws the yellow square representing the selected node and a magenta arrow for each outgoing edge
		public virtual void OnDrawGizmosSelected()
		{
			Gizmos.color = Color.yellow;
			Gizmos.DrawCube(transform.position, new Vector3(0.5f, 0.5f, 0.5f));
			for (int i = 0; i < outs.Count; ++i)
			{
				Vector3 direction = outs[i].transform.position - transform.position;
				ForGizmo(transform.position + direction.normalized, direction - direction.normalized * 2f, Color.magenta, 2f);
			}
		}


		[ExecuteInEditMode]
		/// Handles the destruction of the waypoint in editor and play modes
		private void OnDestroy()
		{
			if (parent == null) return;
			for (int i = 0; i < outs.Count; ++i) Undo.RegisterCompleteObjectUndo(outs[i], "destroyed");
			for (int i = 0; i < ins.Count; ++i) Undo.RegisterCompleteObjectUndo(ins[i], "destroyed");
			Undo.RegisterCompleteObjectUndo(this.getParent(), "destroyed");
			for (int i = outs.Count - 1; i >= 0; --i) this.unlinkFrom(outs[i]);
			for (int i = ins.Count - 1; i >= 0; --i) ins[i].unlinkFrom(this);
			Undo.RegisterCompleteObjectUndo(this, "destroyed");
			this.getParent().waypoints.Remove(this);
		}

	}
}