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

		#region Inspector
#pragma warning disable 0649

		[SerializeField]
		private string _type;

		[SerializeField]
		private Vector3Int _grid_index;
#pragma warning restore 0649
		#endregion

		private Grid3D _parent;

		/// <summary>
		/// Init the cell position, rotation and the index of the cell.
		/// Use only in inspector or grid, otherwise it can cause undesirable behaviour.
		/// </summary>
		/// <param name="gridIndex">The index of the cell set.</param>
		/// <param name="parent">The Grid3d it belongs.</param>
		public void Initialize(Vector3Int gridIndex, Grid3D parent)
		{
			_parent = parent;
			_grid_index = gridIndex;
		}

		/// <summary>
		/// Reset the cell transform. The position, scale, and rotation will be resetted.
		/// The cell must be initialised before.
		/// </summary>
		/// <param name="grid">The parent grid.</param>
		/// <param name="cell">The cell to initialize transform.</param>
		public void ResetTransform()
		{
			transform.localPosition = _parent.GetLocalPositionCell(ref _grid_index);
			transform.localScale = Vector3.one * _parent.SizeCell;
			transform.rotation = _parent.GetDefaultRotation();
			ActivatePallet(false);
			_type = "null";
		}

		/// <summary>
		/// Get the index of the cell.
		/// </summary>
		/// <returns>The cell's index.</returns>
		/// 

		public string GetTypeCell()
		{
			return _type;
		}

		public Vector3Int GetIndex()
		{
			return _grid_index;
		}

		public override int GetHashCode()
		{
			return _grid_index.GetHashCode();
		}

		public void ActivatePallet(bool active, int palletIndex=0, float rotation = 0)
		{
			if (!active)
			{
				SetColliderState(false);
				SetMeshState(true);
			}

			for (int i = 0; i < this.transform.childCount; i++)
			{
				if (i == palletIndex && active)
				{
					this.transform.GetChild(i).gameObject.SetActive(active);
					_type = active ? this.transform.GetChild(i).name: "null";
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
				if (gameObject.transform.GetChild(i).gameObject.activeSelf == true)
				{
					colliderState = transform.GetChild(i).transform.GetComponent<Collider>().enabled;
				}
			}

			return colliderState;
		}

		public void SetColliderState(bool state)
		{
			for (int i = 0; i < gameObject.transform.childCount; i++)
			{
				if (gameObject.transform.GetChild(i).gameObject.activeSelf == true)
				{
					transform.GetChild(i).transform.GetComponent<Collider>().enabled = state;
				}
			}
		}

		public void SetMeshState(bool state)
		{
			for (int i = 0; i < gameObject.transform.childCount; i++)
			{
				if (gameObject.transform.GetChild(i).gameObject.activeSelf == true)
				{
					if (transform.GetChild(i).transform.GetChild(0))
						transform.GetChild(i).transform.GetChild(0).gameObject.SetActive(state);
				}
			}
		}

		public override bool Equals(object obj)
		{
			return obj is Cell c && c._grid_index.Equals(_grid_index);
		}

		/// <summary>
		/// Get the grid parent. 
		/// Use the transform hierarchy to get the parent.
		/// </summary>
		/// <returns>The grid parent if the parent transform exist and have Grid3D component, otherwise null.</returns>
		public Grid3D GetGridParent()
		{
			Transform parent = transform.parent;
			if (_parent == null && parent != null)
			{
				_parent = parent.GetComponent<Grid3D>();
			}
			return _parent;
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

