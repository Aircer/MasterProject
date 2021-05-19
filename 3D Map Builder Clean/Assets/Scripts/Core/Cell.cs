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
		public class DebugPathinding
		{
			public bool inPath;
			public Color colorDot;
			public Vector3Int fromKey;
			public bool pathfindingWaypoint;
			public float cost;
		}

		public Grid3D parent { get; set; }
		public Vector3Int index { get; set; }
		public CellInformation type { get; set; }
		public CellInformation lastType { get; set; }
		public Vector2 lastRotation { get; set; }
		public CellState state { get; set; }
		public PathfindingState pathFindingType { get; set; }

		private BoxCollider colliderBox;
		private DebugPathinding DebugPath = new DebugPathinding();

		public bool baseType;
		public Dictionary<CellInformation, GameObject> typeDicoCell = new Dictionary<CellInformation, GameObject>();
		public Vector3 rotation;

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
			if (cellType != null && transform.childCount == 0)
			{
				GameObject newChild = PrefabUtility.InstantiatePrefab(typeDicoCell[cellType], parent.transform) as GameObject;
				PrefabUtility.UnpackPrefabInstance(newChild, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
				newChild.transform.parent = this.transform;
				newChild.transform.localPosition = new Vector3(0, 0, 0);
				newChild.transform.localEulerAngles = new Vector3(rotation.x, rotation.y, 0);

				//typeDicoCell[cellType].SetActive(true);
				//typeDicoCell[cellType].transform.localEulerAngles = new Vector3(rotation.x, rotation.y, 0);
			}

			lastRotation = rotation;
			lastType = type = cellType;
			baseType = true;

			SetColliderState(false);
			SetMeshState(true);
			state = CellState.Painted;
		}

		public void TransformVisual(string activeElement, Vector3 rotation)
        {
			if(type != null)
            {
				/*foreach (Transform child in typeDicoCell[type].transform)
				{
					child.gameObject.SetActive(false);
				}
				typeDicoCell[type].transform.Find(activeElement).gameObject.SetActive(true);
				typeDicoCell[type].transform.localEulerAngles = rotation;*/

				if(transform.GetChild(0).transform.Find(activeElement))
                {
					foreach (Transform child in transform.GetChild(0))
					{
						child.gameObject.SetActive(false);
					}
					transform.GetChild(0).transform.Find(activeElement).gameObject.SetActive(true);
				}

				transform.GetChild(0).transform.localEulerAngles = rotation;
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
				//typeDicoCell[newType].SetActive(true);
				//typeDicoCell[newType].transform.localEulerAngles = new Vector3(rotation.x, rotation.y, 0);

				transform.GetChild(0).gameObject.SetActive(true);
				transform.GetChild(0).transform.localEulerAngles = new Vector3(rotation.x, rotation.y, 0);

				lastRotation = rotation;
				lastType = type = newType;
			}

			if (type != null)
			{
				//typeDicoCell[type].SetActive(true);
				transform.GetChild(0).gameObject.SetActive(true);
			}

			state = CellState.Active;
		}

		public void Inactive()
		{
			SetColliderState(false);
			SetMeshState(false);

			if (type != null)
			{
				//typeDicoCell[type].SetActive(false);
				DestroyImmediate(transform.GetChild(0).gameObject);
			}

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
			/*for (int i = 0; i < gameObject.transform.childCount; i++)
			{
				if (type && gameObject.transform.GetChild(i).name == type.name || !state)
				{
					gameObject.transform.GetChild(i).transform.gameObject.SetActive(state);
				}
			}*/
			if(transform.childCount > 0)
				transform.GetChild(0).transform.gameObject.SetActive(state);
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

		public void SetDebug(PathfindingState pathfindingState, bool inPath, Color colorDot, bool pathfindingWaypoint, float gCost, Vector3Int fromKey)
		{
			pathFindingType = pathfindingState;
			DebugPath.inPath = inPath;
			DebugPath.colorDot = colorDot;
			DebugPath.pathfindingWaypoint = pathfindingWaypoint;
			DebugPath.cost = gCost;
			if(fromKey != Vector3Int.down)
				DebugPath.fromKey = fromKey;
		}

		public void ResetDebug()
		{
			DebugPath.inPath = false;
			DebugPath.pathfindingWaypoint = false;
			DebugPath.cost = 0;
			DebugPath.fromKey = Vector3Int.down;
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

