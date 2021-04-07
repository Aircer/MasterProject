using UnityEngine;
using UnityEditor;

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

		public CellInformation lastType { get; set; }

		public Vector2 lastRotation { get; set; }
		public CellState state { get; set; }

		public PathfindingState pathFindingType { get; set; }
		private BoxCollider colliderBox;

		private Material startMaterial;
		private Material endMaterial;

		public class DebugPathinding
		{
			public bool inPath;
			public Color colorDot;
			public Vector3Int fromKey;
			public bool pathfindingWaypoint;
		}

		public DebugPathinding DebugPath = new DebugPathinding();

		public void OnEnable()
        {
			startMaterial = Resources.Load("Material/Start") as Material;
			endMaterial = Resources.Load("Material/End") as Material;
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

		public void Painted(CellInformation cellType, GameObject newCellObject, Vector2 rotation)
		{
			SetColliderState(false);
			SetMeshState(true);

			GameObject newChild = PrefabUtility.InstantiatePrefab(newCellObject, parent.transform) as GameObject;
			PrefabUtility.UnpackPrefabInstance(newChild, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
			newChild.transform.parent = this.transform;
			newChild.transform.position = this.transform.position;
			newChild.transform.localEulerAngles = new Vector3(rotation.x, rotation.y, 0);
			lastRotation = rotation;
			lastType = type = cellType;

			state = CellState.Painted;
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

		public void Active(GameObject newCellObject = null, Vector2 rotation = default(Vector2))
		{
			SetColliderState(true);
			SetMeshState(true);

			if (this.transform.childCount == 0 && newCellObject != null)
			{
				GameObject newChild = PrefabUtility.InstantiatePrefab(newCellObject, parent.transform) as GameObject;
				PrefabUtility.UnpackPrefabInstance(newChild, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
				newChild.transform.parent = this.transform;
				newChild.transform.position = this.transform.position;
				newChild.transform.localEulerAngles = new Vector3(rotation.x, rotation.y, 0);
				lastRotation = rotation;
				lastType = type = newChild.transform.GetComponent<CellInformation>();
			}

			state = CellState.Active;
		}

		public void Inactive()
		{
			SetColliderState(false);
			SetMeshState(false);

			foreach (Transform child in this.transform)
			{
				DestroyImmediate(child.gameObject);
			}

			type = null;
			state = CellState.Inactive;
		}

		public void SetColliderState(bool state)
		{
			colliderBox.enabled = state;
		}

		public void SetMeshState(bool state)
		{
			for (int i = 0; i < gameObject.transform.childCount; i++)
			{
				if (type && gameObject.transform.GetChild(i).name == type.name)
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

		public void SetColor(string type)
		{
			switch (type)
			{
				case "start": 
					this.transform.Find("Start_End").GetChild(0).GetComponent<Renderer>().material = startMaterial;
					break;
				case "end":
					this.transform.Find("Start_End").GetChild(0).GetComponent<Renderer>().material = endMaterial;
					break;
				default:
					break;
			}

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

