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
		private Vector3Int _grid_index;

		[SerializeField]
		private bool _colliderState;
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
			_colliderState = false;
		}

		/// <summary>
		/// Get the index of the cell.
		/// </summary>
		/// <returns>The cell's index.</returns>
		public Vector3Int GetIndex()
		{
			return _grid_index;
		}

		public override int GetHashCode()
		{
			return _grid_index.GetHashCode();
		}

		public bool GetColliderState()
		{
			return _colliderState;
		}

		public void SetColliderState(bool state)
		{
			_colliderState = state;

			for (int i = 0; i < gameObject.transform.childCount; i++)
			{
				if (gameObject.transform.GetChild(i).gameObject.activeSelf == true)
				{
					transform.GetChild(i).transform.GetComponent<Collider>().enabled = _colliderState;
				}
			}
		}

		public void SetMeshState(bool state)
		{
			for (int i = 0; i < gameObject.transform.childCount; i++)
			{
				if (gameObject.transform.GetChild(i).gameObject.activeSelf == true)
				{
					transform.GetChild(i).transform.GetComponent<MeshRenderer>().enabled = state;
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

