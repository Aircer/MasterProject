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
		public CellState state { get; set; }

		private BoxCollider colliderBox;
		public Dictionary<CellInformation, GameObject> typeDicoCell = new Dictionary<CellInformation, GameObject>();
		public Vector3 rotation;

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
			this.transform.localPosition = gridIndex;
		}

		public void Painted(CellInformation cellType)
		{
			if (cellType != null)
			{
				//GameObject newChild = PrefabUtility.InstantiatePrefab(typeDicoCell[cellType], parent.transform) as GameObject;
				//PrefabUtility.UnpackPrefabInstance(newChild, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
				//newChild.transform.parent = this.transform;
				//newChild.transform.localPosition = new Vector3(0, 0, 0);
				//newChild.transform.localEulerAngles = new Vector3(rotation.x, rotation.y, 0);

				typeDicoCell[cellType].SetActive(true);
			}

			type = cellType;

			SetColliderState(false);
			SetMeshState(true);
			state = CellState.Painted;
		}

		public void TransformVisual(string activeElement, Vector3 rotation)
        {
			if(type != null)
            {
				foreach (Transform child in typeDicoCell[type].transform)
				{
					child.gameObject.SetActive(false);
				}

				if (typeDicoCell[type].transform.Find(activeElement))
				{
					typeDicoCell[type].transform.Find(activeElement).gameObject.SetActive(true);
				}

				typeDicoCell[type].transform.localEulerAngles = rotation;
			}
		}

		public void TransformVisualFloor(List<string> activeElements)
		{	
			if (type != null)
			{
				foreach (Transform child in typeDicoCell[type].transform.GetChild(0))
				{
					child.gameObject.SetActive(false);
				}

				typeDicoCell[type].transform.Find("Floor").gameObject.SetActive(true);

				foreach (string elem in activeElements)
				{
					typeDicoCell[type].transform.Find(elem).gameObject.SetActive(true);
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
		}

		public void AWake()
		{
			if(state == CellState.Active)
				SetColliderState(true);
			SetMeshState(true);
		}

		public void Active(CellInformation newType = null, Vector2 rotation = default(Vector2))
		{
			SetColliderState(true);
			SetMeshState(true);

			if (newType != null)
			{
				typeDicoCell[newType].SetActive(true);
				typeDicoCell[newType].transform.localEulerAngles = new Vector3(rotation.x, rotation.y, 0);

				//transform.GetChild(0).gameObject.SetActive(true);
				//transform.GetChild(0).transform.localEulerAngles = new Vector3(rotation.x, rotation.y, 0);
			}

			if (type != null)
			{
				typeDicoCell[type].SetActive(true);
				//transform.GetChild(0).gameObject.SetActive(true);
			}

			state = CellState.Active;
		}

		public void Inactive()
		{
			SetColliderState(false);
			SetMeshState(false);

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
			foreach(KeyValuePair<CellInformation, GameObject> entry in typeDicoCell)
            {
				if (!state || entry.Key != type)
					typeDicoCell[entry.Key].SetActive(false);
				else
					typeDicoCell[entry.Key].SetActive(true);
			}
		}
	}
}

