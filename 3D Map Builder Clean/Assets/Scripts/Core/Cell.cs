using UnityEngine;

namespace MapTileGridCreator.Core
{
	/// <summary>
	/// The cell class handle the bridge between the grid and the gameobject manipulation logic, 
	/// often via GetComponent function for specifics datas during procedural modification for example.
	/// </summary>
	[ExecuteInEditMode]
	[DisallowMultipleComponent]
	public class Cell : MonoBehaviour
	{
		public Grid3D parent { get; set; }
		public Vector3Int index { get; set; }

		public string type { get; set; }

        public CellState state { get; set; }
		public int lastPalletIndex { get; set; }
		private BoxCollider colliderBox;  
		private Material startMaterial;
		private Material endMaterial;

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
			SetState(CellState.Inactive);
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
			transform.rotation = parent.GetDefaultRotation();
			SetState(CellState.Inactive);
			type = "null";
		}

		public void SetState(CellState newState, string cellType = "null", float rotation = 0)
		{
			state = newState;

			switch (newState)
			{
				case CellState.Painted:
					type = cellType;
					this.transform.eulerAngles = new Vector3(0, rotation, 0);
					SetColliderState(false);
					SetMeshState(true);
					break;
				case CellState.Erased:
					SetColliderState(true);
					SetMeshState(false);
					break;
				case CellState.Active:
					type = cellType != "null"?cellType:type;
					this.transform.eulerAngles = rotation != 0? new Vector3(0, rotation, 0): this.transform.eulerAngles;
					SetColliderState(true);
					SetMeshState(true);
					break;
				case CellState.Inactive:
					SetColliderState(false);
					SetMeshState(false);
					break;
			}
		}

		public void Painted(string cellType = "null", float rotation = 0)
		{ 
			type = cellType;
			this.transform.eulerAngles = new Vector3(0, rotation, 0);

			SetColliderState(false);
			SetMeshState(true);
		}

		public void Erased()
		{
			SetColliderState(true);
			SetMeshState(false);
		}

		public void Active(string cellType = "null", float rotation = -1)
		{
			//Change Type and Rotation only if we give Active new values, if not keep the old ones 
			type = cellType != "null" ? cellType : type;
			this.transform.eulerAngles = rotation != -1 ? new Vector3(0, rotation, 0) : this.transform.eulerAngles;

			SetColliderState(true);
			SetMeshState(true);
		}

		public void Inactive()
		{
			SetColliderState(false);
			SetMeshState(false);
		}

		public void ActivatePallet(bool active, int palletIndex=-1, float rotation = 0)
		{
			for (int i = 0; i < this.transform.childCount; i++)
			{
				if (i == palletIndex && active)
				{
					this.transform.GetChild(i).gameObject.SetActive(active);
					type = active ? this.transform.GetChild(i).name: "null";
					lastPalletIndex = palletIndex;
				}
				else
					this.transform.GetChild(i).gameObject.SetActive(false);
			}
			
			this.transform.eulerAngles = new Vector3(0, rotation, 0);
		}

		public bool GetColliderState()
		{
			bool colliderState = false; 

			for (int i = 0; i < gameObject.transform.childCount; i++)
			{
				if (gameObject.transform.GetChild(i).name == type)
				{
					colliderState = transform.GetChild(i).transform.GetComponent<Collider>().enabled;
				}
			}

			return colliderState;
		}

		public void SetColliderState(bool state)
		{
			colliderBox.enabled = state;
		}

		public void SetMeshState(bool state)
		{
			for (int i = 0; i < gameObject.transform.childCount; i++)
			{
				if (gameObject.transform.GetChild(i).name == type)
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

