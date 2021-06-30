using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using MapTileGridCreator.UtilitiesVisual;
using MapTileGridCreator.UtilitiesMain;

namespace MapTileGridCreator.Core
{
	[DisallowMultipleComponent]
	[ExecuteInEditMode]
	public class Grid3D : MonoBehaviour
	{
		public Dictionary<Vector3Int, Cell> _map;
		public Cell[,,] _cells;
		public int[,,] _cells_values;
		public Vector3Int size;
		public List<CellInformation> cellInfos;

		public void Initialize(Vector3Int size, List<CellInformation> cellInfos, Dictionary<CellInformation, GameObject> pallet, GameObject palletObject)
		{
			this.size = size;
			this.cellInfos = cellInfos;
			this.transform.gameObject.tag = "Grid";
			_map = new Dictionary<Vector3Int, Cell>();
			CreateEmptyCells(pallet, palletObject);
		}

		/// <summary>
		/// Create a new cells
		/// </summary>
		/// /// <param name="progressBarTime"> float use to track time spend creating cells</param>
		/// /// /// <param name="cells"> List of cells created</param>
		/// /// <param name="pallet"> List of all gameobject that can be painted on cells.</param>
		/// /// <param name="grid"> Parent grid of the cell.</param>
		/// /// <param name="size_grid"> Give number of the cells to create.</param>
		public void CreateEmptyCells(Dictionary<CellInformation, GameObject> pallet, GameObject palletObject)
		{
			_cells = new Cell[size.x, size.y, size.z];
			int numberCells = size.x * size.y * size.z;
			float progressBarTime = 0f;

			for (int x = 0; x < size.x; x++)
			{
				for (int y = 0; y < size.y; y++)
				{
					for (int z = 0; z < size.z; z++)
					{
						progressBarTime += 1;
						_cells[x, y, z] = InstantiateCell(new Vector3Int(x, y, z), pallet, palletObject);
						EditorUtility.DisplayProgressBar("Creatings empty cells", "You can go have a coffee...", progressBarTime / numberCells);
					}
				}
			}
		}

		/// <summary>
		/// Instantiate a cell with a prefab as model.
		/// </summary>
		/// <param name="prefab">The prefab to instantiate as a cell.</param>
		/// <param name="grid">The grid the cell will belongs.</param>
		/// <param name="index"> The index of the cell.</param>
		/// <returns>The cell component associated to the gameobject.</returns>
		public Cell InstantiateCell(Vector3Int index, Dictionary<CellInformation, GameObject> pallet, GameObject palletObject)
		{
			//GameObject cellGameObject = PrefabUtility.InstantiatePrefab(pallet, grid.transform) as GameObject;
			GameObject cellGameObject = new GameObject();
			cellGameObject.name = "cell_" + index.x + "_" + index.y + "_" + index.z;
			cellGameObject.transform.parent = this.transform;
			BoxCollider coll = cellGameObject.AddComponent<BoxCollider>();
			coll.enabled = false;
			Cell cell = cellGameObject.AddComponent<Cell>();
			cell.type = null;
			GameObject newChild = GameObject.Instantiate(palletObject);
			newChild.transform.parent = cellGameObject.transform;
			newChild.SetActive(true);
			foreach (Transform child in newChild.transform)
			{
				foreach (KeyValuePair<CellInformation, GameObject> keyValuePair in pallet)
				{
					if (keyValuePair.Key.name == child.GetComponent<CellInformation>().name)
					{
						cell.typeDicoCell.Add(keyValuePair.Key, child.gameObject);
						child.gameObject.SetActive(false);
					}
				}
			}

			this.AddCell(index, cell);
			return cell;
		}

		public int[][][] ConvertCellsToInt()
		{
			int[][][] genesXYZ = new int[size.x + 2][][];

			for (int x = 0; x < size.x + 2; x++)
			{
				int[][] genesYZ = new int[size.y + 2][];
				for (int y = 0; y < size.y + 2; y++)
				{
					int[] genesZ = new int[size.z + 2];
					for (int z = 0; z < size.z + 2; z++)
					{
						//Genes are bigger than cluster to have empty borders thus it is easier to get neighbors  
						if (x == 0 || y == 0 || z == 0 || x == size.x + 1 || y == size.y + 1 || z == size.z + 1)
						{
							genesZ[z] = 0;
						}
						else
						{
							if(_cells[x - 1, y - 1, z - 1].type != null)
								genesZ[z] = cellInfos.IndexOf(_cells[x - 1, y - 1, z - 1].type);
							else
								genesZ[z] = 0;
						}
					}
					genesYZ[y] = genesZ;
				}
				genesXYZ[x] = genesYZ;
			}

			return genesXYZ;
		}

		/// <summary>
		/// Create cells and waypoints
		/// </summary>
		/// /// <param name="cells"> Cells to transform</param>
		/// /// <param name="waypoints"> Waypoints used to get info for transform Cells</param>
		public void ConvertIntToCells(int[][][] genes, Vector3Int minVal, Vector3Int maxVal)
		{
			for (int i = 0; i < size.x; i++)
			{
				for (int j = 0; j < size.y; j++)
				{
					for (int k = 0; k < size.z; k++)
					{
						if (genes[i+1][j+1][k+1] == 0 && _cells[i, j, k].type != null)
						{
							_cells[i, j, k].Inactive();
						}

						if (genes[i + 1][j + 1][k + 1] != 0 && (_cells[i, j, k].type == null || cellInfos[genes[i + 1][j + 1][k + 1]].name != _cells[i, j, k].type.name))
						{
							_cells[i, j, k].Inactive();
							_cells[i, j, k].Painted(cellInfos[genes[i + 1][j + 1][k + 1]]);
							_cells[i, j, k].Active();

							/*
							if (!waypoints[i, j, k].show)
								cells[i, j, k].Sleep();*/
						}
						/*
						if (genes[i][j][k] != 0 && !waypoints[i, j, k].show)
						{
							cells[i, j, k].Sleep();
						}*/
					}
				}
			}

			for (int i = 0; i < size.x; i++)
			{
				for (int j = 0; j < size.y; j++)
				{
					for (int k = 0; k < size.z; k++)
					{
						FuncVisual.UpdateCellsAroundVisual(_cells, new Vector3Int(i, j, k), cellInfos[genes[i + 1][j + 1][k + 1]]);
					}
				}
			}

			FuncMain.SetShowLayersCell(minVal, maxVal, _cells);
		}

		/// <summary>
		/// Get a cell by index. If it is not registered to the grid, return null.
		/// </summary>
		/// <param name="index">The index's cell to get.</param>
		public Cell TryGetCellByIndex(ref Vector3Int index)
		{
			_map.TryGetValue(index, out Cell cell);
			return cell;
		}

		/// <summary>
		/// Convert a position to an index grid.
		/// </summary>
		/// <param name="position"> The position given in world coordinates.</param>
		/// <returns>The index corresponding.</returns>
		public Vector3Int GetIndexByPosition(ref Vector3 position)
		{
			Vector3Int indexCanon = Vector3Int.RoundToInt(position);
			Vector3Int index = Vector3Int.RoundToInt(GetMatrixGridToLocalPosition().inverse.MultiplyPoint3x4(indexCanon));
			return index;
		}

		/// <summary>
		/// Get a cell by position in world coordinates. If it is not registered to the grid, return null.
		/// </summary>
		/// <param name="position">The position supposed of the cell./param>
		public Cell TryGetCellByPosition(ref Vector3 position)
		{
			Vector3Int index = GetIndexByPosition(ref position);
			return TryGetCellByIndex(ref index);
		}

		/// <summary>
		/// Get the local position of the cell.
		/// </summary>
		/// <param name="index">The index supposed of the cell.</param>
		public Vector3 GetLocalPositionCell(Vector3Int index)
		{
			return GetMatrixGridToLocalPosition().MultiplyPoint3x4(index);
		}

		/// <summary>
		/// Get the position of a cell.
		/// </summary>
		/// <param name="index">The index cell</param>
		public Vector3 GetPositionCell(UnityEngine.Vector3Int index)
		{
			return GetLocalPositionCell(index);
		}

		/// <summary>
		/// Convert a postion to a grid position in world coordinates.
		/// </summary>
		/// <param name="position">The position to convert.</param>
		public Vector3 TransformPositionToGridPosition(Vector3 position)
		{
			Vector3Int index = GetIndexByPosition(ref position);
			return GetPositionCell(index);
		}

		/// <summary>
		/// Add a cell at a given index to the grid and initialize it.
		/// </summary>
		/// <param name="index">The index of the cell.</param>
		/// <param name="cell">The cell data to initialize and register.</param>
		public void AddCell(Vector3Int index, Cell cell)
		{
			cell.Initialize(index, this);
			_map.Add(index, cell);
		}

		/// <summary>
		/// Get the matrix to pass from grid coordinates to local position.
		/// </summary>
		/// <returns>A Matrix4x4.</returns>
		protected Matrix4x4 GetMatrixGridToLocalPosition()
		{
			//Transpose for reading
			return new Matrix4x4(
				new Vector4(1, 0, 0, 0),
				new Vector4(1, 0, -1, 0),
				new Vector4(0, 1, 1, 0),
				new Vector4(0, 0, 0, 1));
		}

		public void PaintCell(Cell[,,] cells, Vector3Int index, CellInformation type)
		{
			//Active the cell at the index position 
			cells[index.x, index.y, index.z].Painted(type);
		}

		public void EraseCell(Cell[,,] cells, Vector3Int index)
		{
			cells[index.x, index.y, index.z].Erased();
		}

		public void DesactivateCell(Cell[,,] cells, Vector3Int index)
		{
			cells[index.x, index.y, index.z].Inactive();
		}
	}
}

