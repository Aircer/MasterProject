using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace MapTileGridCreator.Core
{
	/// <summary>
	/// The cell class handle the bridge between the grid and the gameobject manipulation logic, 
	/// often via GetComponent function for specifics datas during procedural modification for example.
	/// </summary>
	[ExecuteInEditMode]
	[DisallowMultipleComponent]
#pragma warning disable CS0659 // Le type se substitue à Object.Equals(object o) mais pas à Object.GetHashCode()
	public class Cell : MonoBehaviour
#pragma warning restore CS0659 // Le type se substitue à Object.Equals(object o) mais pas à Object.GetHashCode()
	{
		public Grid3D parent { get; set; }
		public Vector3Int index { get; set; }
		public CellInformation type { get; set; }
		public bool baseType;
		public Dictionary<CellInformation, GameObject> typeDicoCell = new Dictionary<CellInformation, GameObject>();
		public CellInformation lastType { get; set; }

		public Vector2 lastRotation { get; set; }
		public CellState state { get; set; }

		public PathfindingState pathFindingType { get; set; }
		private BoxCollider colliderBox;

		public class DebugPathinding
		{
			public bool inPath;
			public Color colorDot;
			public Vector3Int fromKey;
			public bool pathfindingWaypoint;
			public float cost;
		}

		public DebugPathinding DebugPath = new DebugPathinding();

		public void OnEnable()
        {

		}

        /// <summary>
        /// Init the cell position, rotation and the index of the cell.
        /// Use only in inspector or grid, otherwise it can cause undesirable behaviour.
        /// </summary>
        /// <param name="gridIndex">The index of the cell set.</param>
        /// <param name="parent">The Grid3d it belongs.</param>
        public void Initialize(Vector3Int gridIndex, Grid3D grid)
		{
			parent = grid;
			index = gridIndex;
			colliderBox = this.transform.gameObject.GetComponent<BoxCollider>();
		}

		/// <summary>
		/// Reset the cell transform. The position, scale, and rotation will be resetted.
		/// The cell must be initialised before.
		/// </summary>
		/// <param name="grid">The parent grid.</param>
		/// <param name="cell">The cell to initialize transform.</param>
		public void ResetTransform()
		{
			transform.localPosition = parent.GetLocalPositionCell(index);
			transform.localScale = Vector3.one * parent.SizeCell;
		}

		public void Painted(CellInformation cellType, Vector2 rotation)
		{
			/*
			GameObject newChild = PrefabUtility.InstantiatePrefab(newCellObject, parent.transform) as GameObject;
			PrefabUtility.UnpackPrefabInstance(newChild, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
			newChild.transform.parent = this.transform;
			newChild.transform.position = this.transform.position;
			newChild.transform.localEulerAngles = new Vector3(rotation.x, rotation.y, 0);
			lastRotation = rotation;
			lastType = type = cellType;
			*/

			if (cellType != null)
			{
				typeDicoCell[cellType].SetActive(true);
				typeDicoCell[cellType].transform.localEulerAngles = new Vector3(rotation.x, rotation.y, 0);
				lastRotation = rotation;
				lastType = type = cellType;
				baseType = true;
			}

			SetColliderState(false);
			SetMeshState(true);
			state = CellState.Painted;
		}

		public void WallTransform(Waypoint[,,] waypoints)
        {
			if (type != null && type.wall)
			{
				typeDicoCell[type].transform.Find("Single").gameObject.SetActive(false);
				typeDicoCell[type].transform.Find("DoubleSides").gameObject.SetActive(false);
				typeDicoCell[type].transform.Find("Triple").gameObject.SetActive(false);
				typeDicoCell[type].transform.Find("Corner").gameObject.SetActive(false);
				typeDicoCell[type].transform.Find("Quattro").gameObject.SetActive(false);

				bool[] neighbordsWalls = new bool[4];

				if (index.x > 0 && waypoints[index.x - 1, index.y, index.z].type != null && waypoints[index.x - 1, index.y, index.z].type.wall)
					neighbordsWalls[0] = true;
				else
					neighbordsWalls[0] = false;

				if (index.x < waypoints.GetLength(0)-1 && waypoints[index.x + 1, index.y, index.z].type != null && waypoints[index.x + 1, index.y, index.z].type.wall)
					neighbordsWalls[1] = true;
				else
					neighbordsWalls[1] = false;

				if (index.z > 0 && waypoints[index.x, index.y, index.z-1].type != null && waypoints[index.x, index.y, index.z-1].type.wall)
					neighbordsWalls[2] = true;
				else
					neighbordsWalls[2] = false;

				if (index.z < waypoints.GetLength(2)-1 && waypoints[index.x, index.y, index.z+1].type != null && waypoints[index.x, index.y, index.z+1].type.wall)
					neighbordsWalls[3] = true;
				else
					neighbordsWalls[3] = false;

				if (!neighbordsWalls[0] && !neighbordsWalls[1] && !neighbordsWalls[2] && !neighbordsWalls[3])
				{
					typeDicoCell[type].transform.Find("Single").gameObject.SetActive(true);
					typeDicoCell[type].transform.localEulerAngles = new Vector3(0, 0, 0);
				}

				if ((neighbordsWalls[0] || neighbordsWalls[1]) && !neighbordsWalls[2] && !neighbordsWalls[3])
				{
					typeDicoCell[type].transform.Find("DoubleSides").gameObject.SetActive(true);
					typeDicoCell[type].transform.localEulerAngles = new Vector3(0, 0, 0);
				}

				if ((neighbordsWalls[2] || neighbordsWalls[3]) && !neighbordsWalls[0] && !neighbordsWalls[1])
				{
					typeDicoCell[type].transform.Find("DoubleSides").gameObject.SetActive(true);
					typeDicoCell[type].transform.localEulerAngles = new Vector3(0, 90, 0);
				}

				if (neighbordsWalls[1] && neighbordsWalls[2] && !neighbordsWalls[0] && !neighbordsWalls[3])
				{
					typeDicoCell[type].transform.Find("Corner").gameObject.SetActive(true);
					typeDicoCell[type].transform.localEulerAngles = new Vector3(0, 0, 0);
				}

				if (neighbordsWalls[0] && neighbordsWalls[2] && !neighbordsWalls[1] && !neighbordsWalls[3])
				{
					typeDicoCell[type].transform.Find("Corner").gameObject.SetActive(true);
					typeDicoCell[type].transform.localEulerAngles = new Vector3(0, 90, 0);
				}

				if (neighbordsWalls[0] && neighbordsWalls[3] && !neighbordsWalls[1] && !neighbordsWalls[2])
				{
					typeDicoCell[type].transform.Find("Corner").gameObject.SetActive(true);
					typeDicoCell[type].transform.localEulerAngles = new Vector3(0, 180, 0);
				}

				if (neighbordsWalls[1] && neighbordsWalls[3] && !neighbordsWalls[0] && !neighbordsWalls[2])
				{
					typeDicoCell[type].transform.Find("Corner").gameObject.SetActive(true);
					typeDicoCell[type].transform.localEulerAngles = new Vector3(0, 270, 0);
				}

				if (neighbordsWalls[1] && neighbordsWalls[2] && !neighbordsWalls[0] && neighbordsWalls[3])
				{
					typeDicoCell[type].transform.Find("Triple").gameObject.SetActive(true);
					typeDicoCell[type].transform.localEulerAngles = new Vector3(0, 0, 0);
				}

				if (neighbordsWalls[1] && neighbordsWalls[2] && neighbordsWalls[0] && !neighbordsWalls[3])
				{
					typeDicoCell[type].transform.Find("Triple").gameObject.SetActive(true);
					typeDicoCell[type].transform.localEulerAngles = new Vector3(0, 90, 0);
				}

				if (!neighbordsWalls[1] && neighbordsWalls[3] && neighbordsWalls[0] && neighbordsWalls[2])
				{
					typeDicoCell[type].transform.Find("Triple").gameObject.SetActive(true);
					typeDicoCell[type].transform.localEulerAngles = new Vector3(0, 180, 0);
				}

				if (neighbordsWalls[0] && neighbordsWalls[3] && neighbordsWalls[1] && !neighbordsWalls[2])
				{
					typeDicoCell[type].transform.Find("Triple").gameObject.SetActive(true);
					typeDicoCell[type].transform.localEulerAngles = new Vector3(0, 270, 0);
				}

				if (neighbordsWalls[0] && neighbordsWalls[3] && neighbordsWalls[1] && neighbordsWalls[2])
				{
					typeDicoCell[type].transform.Find("Quattro").gameObject.SetActive(true);
					typeDicoCell[type].transform.localEulerAngles = new Vector3(0, 0, 0);
				}
			}
        }

		public void Erased()
		{
			SetColliderState(true);
			SetMeshState(false);
			state = CellState.Erased;
		}

		public void Sleep()
		{
			SetColliderState(false);
			SetMeshState(false);
			state = CellState.Sleep;
		}

		public void Active(CellInformation newType = null, Vector2 rotation = default(Vector2))
		{
			SetColliderState(true);
			SetMeshState(true);

			if (newType != null)
			{
				typeDicoCell[newType].SetActive(true);
				typeDicoCell[newType].transform.localEulerAngles = new Vector3(rotation.x, rotation.y, 0);
				lastRotation = rotation;
				lastType = type = newType;
			}

			if(type != null)
				typeDicoCell[type].SetActive(true);
			state = CellState.Active;
		}

		public void Inactive()
		{
			SetColliderState(false);
			SetMeshState(false);

			if(type != null)
				typeDicoCell[type].SetActive(false);

			type = null;
			state = CellState.Inactive;
		}

		public void SetColliderState(bool state)
		{
			if(colliderBox != null)
				colliderBox.enabled = state;
		}

		public void SetMeshState(bool state)
		{
			for (int i = 0; i < gameObject.transform.childCount; i++)
			{
				if (type && gameObject.transform.GetChild(i).name == type.name || !state)
				{
					gameObject.transform.GetChild(i).transform.gameObject.SetActive(state);
				}
			}
		}

		public override bool Equals(object obj)
		{
			return obj is Cell c && c.index.Equals(index);
		}

		/// <summary>
		/// Get the grid parent. 
		/// Use the transform hierarchy to get the parent.
		/// </summary>
		/// <returns>The grid parent if the parent transform exist and have Grid3D component, otherwise null.</returns>
		public Grid3D GetGridParent()
		{
			Transform gridParent = transform.parent;
			if (parent == null && gridParent != null)
			{
				parent = gridParent.GetComponent<Grid3D>();
			}
			return parent;
		}

		/// Draws the white square representing the nodes and an arrow for each outgoing edge
		public virtual void OnDrawGizmos()
		{
			if (DebugPath.pathfindingWaypoint)
			{
				Gizmos.color = Color.red;
				Gizmos.DrawSphere(index, 0.3f);
			}
			if (DebugPath.inPath)
			{
				if (pathFindingType == PathfindingState.A_Star)
				{
					Gizmos.color = DebugPath.colorDot;
					Gizmos.DrawRay(index, DebugPath.fromKey - index);
					Vector3 right = Quaternion.LookRotation(index - DebugPath.fromKey) * Quaternion.Euler(0, 160f, 0) * new Vector3(0, 0, 1);
					Vector3 left = Quaternion.LookRotation(index - DebugPath.fromKey) * Quaternion.Euler(0, 160f, 0) * new Vector3(0, 0, 1);
					Gizmos.DrawRay(index, right * 0.25f);
					Gizmos.DrawRay(index, left * 0.25f);
				}

				if (pathFindingType == PathfindingState.Floodfill)
				{
					Gizmos.color = DebugPath.colorDot;
					Gizmos.DrawSphere(index, 0.2f);
					Handles.Label(new Vector3(transform.position.x, transform.position.y + 0.7f, transform.position.z), DebugPath.cost.ToString("0.00"));
				}
			}
		}

        /// <summary>
        /// When disable, unregister the cell in the parent grid.
        /// </summary>
        private void OnDisable()
		{
			if (GetGridParent() != null)
			{
				GetGridParent().DeleteCell(this);
			}
		}
	}
}

