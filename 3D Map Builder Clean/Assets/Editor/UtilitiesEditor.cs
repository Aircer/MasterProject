using System;
using System.Collections.Generic;
using MapTileGridCreator.Core;
using MapTileGridCreator.CubeImplementation;
using MapTileGridCreator.HexagonalImplementation;
using UnityEditor;
using UnityEngine;
using System.IO;

namespace MapTileGridCreator.Utilities
{
	/// <summary>
	/// Static class containining utilities functions for editor.
	/// </summary>
	public static class FuncEditor
	{
        /// <summary>
        /// IUnstantiate an empty Grid3D.
        /// </summary>
        /// <returns>The grid component associated to the gameobject.</returns>
        public static Grid3D InstantiateGrid3D()
		{
			GameObject obj;
			Grid3D grid;

			obj = new GameObject("CubeGrid");
			grid = obj.AddComponent<CubeGrid>();
			obj.AddComponent<MeshCombiner>();

			grid.Initialize();
			return grid;
		}

		/// <summary>
		/// Refresh a grid. Only work outside playMode.
		/// </summary>
		/// <param name="grid"> The grid to refresh.</param>
		public static void RefreshGrid(Grid3D grid)
		{
			if (!Application.isPlaying)
			{
				grid.Initialize();
			}
		}

		/// <summary>
		/// Debug a square grid. Use this one if the editor performance is limited rather than other grid debug implementation.
		/// </summary>
		/// <param name="grid">The grid to debug.</param>
		/// <param name="color"> The color of the grid.</param>
		/// <param name="size_grid">The size of the grid.</param>
		/// /// <param name="planesGrid">Plans orientation, depends of the camera rotation</param>
		public static void DebugSquareGrid(Grid3D grid, Color color, Vector3Int size_grid, Plane[] planesGrid)
		{
			using (new Handles.DrawingScope(color))
			{
				float flipX = planesGrid[0].normal.x == -1 ? size_grid.x : 0;
				float flipY = planesGrid[1].normal.y == -1 ? size_grid.y : 0;
				float flipZ = planesGrid[2].normal.z == -1 ? size_grid.z : 0;
;
				Handles.zTest = UnityEngine.Rendering.CompareFunction.LessEqual;
				Vector3 pos = grid.transform.position;
				float CaseSize = grid.SizeCell * grid.GapRatio;
				pos.y += - CaseSize / 2.0f;

				for (float x = -1; x < size_grid.x; x++)
				{
					Handles.DrawLine(pos + new Vector3(x + 0.5f, flipY, -0.5f) * CaseSize,
									pos + new Vector3(x + 0.5f, flipY, size_grid.z - 0.5f) * CaseSize);

					Handles.DrawLine(pos + new Vector3(x + 0.5f, size_grid.y, flipZ - 0.5f) * CaseSize,
									pos + new Vector3(x + 0.5f, 0, flipZ - 0.5f) * CaseSize);
				}

				for (float y = -1; y < size_grid.y; y++)
				{ 
					Handles.DrawLine(pos + new Vector3(flipX - 0.5f, y + 1f, -0.5f) * CaseSize,
									pos + new Vector3(flipX - 0.5f, y + 1f, size_grid.z - 0.5f) * CaseSize);

					Handles.DrawLine(pos + new Vector3(-0.5f, y + 1f, flipZ-0.5f) * CaseSize,
									pos + new Vector3(size_grid.x-0.5f, y + 1f, flipZ - 0.5f) * CaseSize);
				}

				for (float z = -1; z < size_grid.z; z++)
				{
					Handles.DrawLine(pos + new Vector3(-0.5f, flipY, z + 0.5f) * CaseSize,
									pos + new Vector3(size_grid.x - 0.5f, flipY, z + 0.5f) * CaseSize);

					Handles.DrawLine(pos + new Vector3(flipX - 0.5f, 0, z + 0.5f) * CaseSize,
									pos + new Vector3(flipX - 0.5f, size_grid.y, z + 0.5f) * CaseSize);
				}
			}
		}

        /// <summary>
        /// Instantiate a cell with a prefab as model.
        /// </summary>
        /// <param name="prefab">The prefab to instantiate as a cell.</param>
        /// <param name="grid">The grid the cell will belongs.</param>
        /// <param name="position"> The position of the cell.</param>
        /// <returns>The cell component associated to the gameobject.</returns>
        public static Cell InstantiateCell(Grid3D grid, Vector3 position)
		{
			Vector3Int index = grid.GetIndexByPosition(ref position);
			return InstantiateCell(grid, index);
		}

		public static Vector3Int GetIndexByPosition(Grid3D grid, Vector3 position)
		{
			return grid.GetIndexByPosition(ref position);
		}

		public static Cell CellAtThisIndex(Grid3D grid, Vector3 index)
		{
			return grid.TryGetCellByPosition(ref index);
		}

		/// <summary>
		/// Instantiate a cell with a prefab as model.
		/// </summary>
		/// <param name="prefab">The prefab to instantiate as a cell.</param>
		/// <param name="grid">The grid the cell will belongs.</param>
		/// <param name="index"> The index of the cell.</param>
		/// <returns>The cell component associated to the gameobject.</returns>
		public static Cell InstantiateCell(Grid3D grid, Vector3Int index)
		{
			//GameObject cellGameObject = PrefabUtility.InstantiatePrefab(pallet, grid.transform) as GameObject;
			GameObject cellGameObject = new GameObject();
			cellGameObject.name = "cell_" + index.x + "_" + index.y + "_" + index.z;
			cellGameObject.transform.parent = grid.transform;
			BoxCollider coll = cellGameObject.AddComponent<BoxCollider>();
			coll.enabled = false;
			Cell cell = cellGameObject.AddComponent<Cell>();
			/*
			foreach (GameObject child in pallet)
			{
				GameObject newChild = PrefabUtility.InstantiatePrefab(child, grid.transform) as GameObject;
				PrefabUtility.UnpackPrefabInstance(newChild, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
				newChild.transform.parent = cellGameObject.transform;
				newChild.SetActive(false);
			}*/

			grid.AddCell(index, cell);
			cell.ResetTransform();

			return cell;
		}

        /// <summary>
        /// Replace a cell with an other cell, and delete the old, component and gameobject.
        /// </summary>
        /// <param name="source">The source gameobject, can be a prefab or an existing gameObject.</param>
        /// <param name="grid">The grid of the old cell.</param>
        /// <param name="old">The old cell to replace.</param>
        /// <returns>The cell component of the new gameobject.</returns>
        public static Cell ReplaceCell(GameObject source, Grid3D grid, Cell old)
		{
			Undo.SetCurrentGroupName("Replace Cell");
			int group = Undo.GetCurrentGroup();

			GameObject prefab = source;
			if (IsGameObjectInstancePrefab(source))
			{
				prefab = GetPrefabFromInstance(source);
			}

			GameObject gameObject = PrefabUtility.InstantiatePrefab(prefab, grid.transform) as GameObject;

			Vector3Int index = old.index;
			gameObject.transform.position = old.transform.position;
			gameObject.name = gameObject.name + "_" + index.x + "_" + index.y + "_" + index.z;

			Cell cell = gameObject.GetComponent<Cell>();
			if (cell == null)
			{
				gameObject.AddComponent<Cell>();
			}
			grid.ReplaceCell(index, cell);
			Undo.RegisterCreatedObjectUndo(cell.gameObject, "Cell replaced");
			Undo.DestroyObjectImmediate(old.gameObject);
			Undo.CollapseUndoOperations(group);
			return cell;
		}

		/// <summary>
		/// Destroy a cell with his gameobject. Do nothing if not instantiated.
		/// </summary>
		/// <param name="cell"></param>
		public static void DestroyCell(Cell cell)
		{
			if (IsGameObjectInstancePrefab(cell.gameObject))
			{
				Undo.DestroyObjectImmediate(cell.gameObject);
			}
		}

		/// <summary>
		/// Stamp cells is to copy and paste a list of cell to an other location.
		/// </summary>
		/// <param name="listCell"> The list of cells to copy.</param>
		/// <param name="grid"> The grid in which place the copy.</param>
		/// <param name="displacement"> The displacement relative to the current position of cells.</param>
		/// <param name="overwrite"> Option if we overwrite an existing cell at destination of copy</param>
		public static void StampCells(List<Cell> listCell, Grid3D grid, Vector3Int destinationIndex, bool overwrite = true)
		{
			Vector3Int displacement = destinationIndex - listCell[0].index;
			foreach (Cell c in listCell)
			{
				Vector3Int index = displacement + c.index;
				Cell cdest = grid.TryGetCellByIndex(ref index);

				GameObject prefabInstance = c.gameObject;
				GameObject prefab = GetPrefabFromInstance(prefabInstance);
				if (cdest == null)
				{
					InstantiateCell(grid, index);
				}
				else if (overwrite)
				{
					ReplaceCell(prefab, grid, cdest);
				}
			}
		}

		/// <summary>
		/// Ui funtions to draw separator in Editors.
		/// </summary>
		/// <param name="color"> The color of the separation</param>
		/// <param name="thickness">The thickness.</param>
		/// <param name="padding"> The padding.</param>
		public static void DrawUILine(Color color, int thickness = 2, int padding = 10)
		{
			Rect r = EditorGUILayout.GetControlRect(GUILayout.Height(padding + thickness));
			r.height = thickness;
			r.y += padding / 2;
			r.x -= 2;
			r.width += 6;
			EditorGUI.DrawRect(r, color);
		}

		/// <summary>
		/// Test if the given object is an instance of prefab.
		/// </summary>
		public static bool IsGameObjectInstancePrefab(UnityEngine.Object obj)
		{
			PrefabAssetType type = PrefabUtility.GetPrefabAssetType(obj);
			return type == PrefabAssetType.Regular && PrefabUtility.IsPartOfNonAssetPrefabInstance(obj);
		}

		/// <summary>
		/// Test if the given object is an instantiated object in scene view.
		/// </summary>
		public static bool IsGameObjectSceneView(UnityEngine.Object obj)
		{
			PrefabAssetType type = PrefabUtility.GetPrefabAssetType(obj);
			return IsGameObjectInstancePrefab(obj) || type == PrefabAssetType.NotAPrefab;
		}

		/// <summary>
		/// Get the prefab from an instance.
		/// </summary>
		/// <param name="prefabInstance"> The instance of a prefab.</param>
		/// <returns>The prefab, or null if not founded.</returns>
		public static GameObject GetPrefabFromInstance(GameObject prefabInstance)
		{
			if (IsGameObjectInstancePrefab(prefabInstance))
			{
				return PrefabUtility.GetCorrespondingObjectFromSource(prefabInstance);
			}
			return null;
		}

		/// <summary>
		/// Create a new cells
		/// </summary>
		/// /// <param name="progressBarTime"> float use to track time spend creating cells</param>
		/// /// /// <param name="cells"> List of cells created</param>
		/// /// <param name="pallet"> List of all gameobject that can be painted on cells.</param>
		/// /// <param name="grid"> Parent grid of the cell.</param>
		/// /// <param name="size_grid"> Give number of the cells to create.</param>
		public static void CreateEmptyCells(ref float progressBarTime, out Cell[,,] cells, Grid3D grid, Vector3Int size_grid)
		{
			//GameObject pallet = AssetDatabase.LoadAssetAtPath("Assets/Cells/Cell.prefab", typeof(GameObject)) as GameObject;
			Cell[,,] newCells = new Cell[size_grid.x, size_grid.y, size_grid.z];
			int numberCells = size_grid.x * size_grid.y * size_grid.z;

			for (int x = 0; x < size_grid.x; x++)
			{
				for (int y = 0; y < size_grid.y; y++)
				{
					for (int z = 0; z < size_grid.z; z++)
					{
						progressBarTime += 1;
						newCells[x, y, z] = InstantiateCell(grid, new Vector3Int(x, y, z));
						EditorUtility.DisplayProgressBar("Creatings empty cells", "You can go have a coffee...", progressBarTime / (2*numberCells));
					}
				}
			}

			cells = newCells;
		}

		/// <summary>
		/// Check if input is in the grid.
		/// </summary>
		/// /// <returns>Return true if Input is in Boundaries.</returns>
		public static bool InputInGridBoundaries(Vector3 input, Vector3Int size_grid)
		{
			bool inBoundaries = true;

			if (input.x < 0 || input.y < 0 || input.z < 0 || input.x > size_grid.x - 1 || input.y > size_grid.y - 1 || input.z > size_grid.z - 1)
				inBoundaries = false;

			return inBoundaries;
		}


		/// <summary>
		/// Set Planes for debug position and normal in function of the camera Rotation
		/// </summary>
		/// /// <returns>Return planes updated
		public static Plane[] SetGridDebugPlanesNormalAndPosition(Plane[] planesGrid, Vector3Int size_grid, Vector3 rot)
		{
			bool[] planesGridInverted = new bool[3];

			//Invert planes if the camera is rotated
			planesGridInverted[0] = rot.y > 180 && rot.y < 360 ? false : true;
			planesGridInverted[1] = rot.x > 180 && rot.x < 360 ? true : false;
			planesGridInverted[2] = rot.y > 90 && rot.y < 270 ? false : true;

			//Update grid debug position
			planesGrid[0].SetNormalAndPosition(new Vector3(planesGridInverted[0] ? -1.0f : 1.0f, 0.0f, 0.0f), new Vector3(planesGridInverted[0] ? size_grid.x - 1 : 0, 0, 0));
			planesGrid[1].SetNormalAndPosition(new Vector3(0.0f, planesGridInverted[1] ? -1.0f : 1.0f, 0.0f), new Vector3(0, planesGridInverted[1] ? size_grid.y - 1 : 0, 0));
			planesGrid[2].SetNormalAndPosition(new Vector3(0.0f, 0.0f, planesGridInverted[2] ? -1.0f : 1.0f), new Vector3(0, 0, planesGridInverted[2] ? size_grid.z - 1 : 0));

			return planesGrid;
		}

		/// <summary>
		/// Create cells and waypoints
		/// </summary>
		public static void CreateCellsAndWaypoints(ref Grid3D grid, ref Cell[,,] cells, ref WaypointCluster cluster, ref float progressBarTime, Dictionary<CellInformation, GameObject> pallet, Vector3Int size_grid)
		{
			//Create Grid and all Cells 
			grid = InstantiateGrid3D();
			CreateEmptyCells(ref progressBarTime, out cells, grid, size_grid);
			//Create Cluster and Waypoints
			WaypointCluster newCluster = new WaypointCluster(size_grid);

			cluster = newCluster;
		}

		/// <summary>
		/// Destroy all grids (objects with tag "Grid")
		/// </summary>
		public static void DestroyGrids()
		{
			//Destroy all existing grids 
			GameObject[] allObjects = GameObject.FindGameObjectsWithTag("Grid");
			foreach (GameObject obj in allObjects)
			{
				UnityEngine.Object.DestroyImmediate(obj);
			}
		}

		/// <summary>
		/// Create cells and waypoints
		/// </summary>
		/// /// <param name="cells"> Cells to transform</param>
		/// /// <param name="waypoints"> Waypoints used to get info for transform Cells</param>
		public static Cell[,,] TransformCellsFromWaypoints(Cell[,,] cells, Waypoint[,,] waypoints, Dictionary<CellInformation, GameObject> cellPrefabs)
		{
			for (int i = 0; i < waypoints.GetLength(0); i++)
			{
				for (int j = 0; j < waypoints.GetLength(1); j++)
				{
					for (int k = 0; k < waypoints.GetLength(2); k++)
					{
						if (waypoints[i, j, k].type == null && cells[i, j, k].type != null)
							cells[i, j, k].Inactive();

						if (waypoints[i, j, k].type != null && waypoints[i, j, k].baseType && cells[i, j, k].type != waypoints[i, j, k].type)
						{
							cells[i, j, k].Active(cellPrefabs[waypoints[i, j, k].type], waypoints[i, j, k].rotation);

							if (!waypoints[i, j, k].show)
								cells[i, j, k].Sleep();
						}

						if (waypoints[i, j, k].type != null && !waypoints[i, j, k].baseType && cells[i, j, k].type != waypoints[i, j, k].type)
						{
							cells[i, j, k].Erased();
						}
					}
				}
			}

			return cells;
		}

		/// <summary>
		/// Show Pathfinding
		/// </summary>
		/// /// <param name="cells"> Cells of the grid</param>
		/// /// <param name="waypoints"> Waypoints used to get info for pathfinding</param>
		/// ///  <param name="pathfindingState"> Pathfinfing type (A* or FloodFill)</param>
		public static void ShowPathfinding(Cell[,,] cells, Waypoint[,,] waypoints, PathfindingState pathfindingState)
		{
			for (int i = 0; i < waypoints.GetLength(0); i++)
			{
				for (int j = 0; j < waypoints.GetLength(1); j++)
				{
					for (int k = 0; k < waypoints.GetLength(2); k++)
					{
						cells[i, j, k].pathFindingType = pathfindingState;
						cells[i, j, k].DebugPath.inPath = waypoints[i, j, k].inPath;
						cells[i, j, k].DebugPath.colorDot = waypoints[i, j, k].colorDot;
						cells[i, j, k].DebugPath.pathfindingWaypoint = waypoints[i, j, k].pathfindingWaypoint;
						cells[i, j, k].DebugPath.cost = waypoints[i, j, k].gCost;
						if (waypoints[i, j, k].inPathFrom != null)
                        {
							cells[i, j, k].DebugPath.fromKey = waypoints[i, j, k].inPathFrom.key;
						}
					}
				}
			}
		}

		/// <summary>
		/// Reset Cells
		/// </summary>
		/// /// <param name="cells"> Cells to reset</param>
		public static void ResetCells(ref Cell[,,] cells, Vector3Int size_grid)
		{
			for (int i = 0; i < size_grid.x; i++)
			{
				for (int j = 0; j < size_grid.y; j++)
				{
					for (int k = 0; k < size_grid.z; k++)
					{
						cells[i, j, k].Inactive();
						cells[i, j, k].DebugPath.inPath = false;
						cells[i, j, k].DebugPath.pathfindingWaypoint = false;
					}
				}
			}
		}

		/// <summary>
		/// Reset Waypoints
		/// </summary>
		/// /// <param name="cluster"> Cluster to reset</param>
		public static void ResetWaypoints(ref WaypointCluster cluster, Vector3Int size_grid)
		{
			for (int i = 0; i < size_grid.x; i++)
			{
				for (int j = 0; j < size_grid.y; j++)
				{
					for (int k = 0; k < size_grid.z; k++)
					{
						cluster.SetType(null, i, j, k);
					}
				}
			}

			cluster.ResetPathfinding();
		}

		/// <summary>
		/// Check if there is enough room to paint the new asset
		///</summary>
		public static bool CanPaintHere(Vector3Int size_grid, Waypoint[,,] waypoints, Vector3Int index, Vector3Int size, Vector3 rotation)
		{
		
			Vector3Int lowerBound = default(Vector3Int);
			Vector3Int upperBound = default(Vector3Int);
			SetBounds(ref lowerBound, ref upperBound, index, size, rotation);

			for (int i = lowerBound.x; i <= upperBound.x; i++)
			{
				for (int j = lowerBound.y; j <= upperBound.y; j++)
				{
					for (int k = lowerBound.z; k <= upperBound.z; k++)
					{
						if (!InputInGridBoundaries(new Vector3Int(i, j, k), size_grid) || (waypoints[i, j, k].type != null && waypoints[i, j, k].type.blockPath))
							return false;
					}
				}
			}

			return true;
		}
		public static void SetType(Vector3Int size_grid, Vector3 rotation, WaypointCluster cluster, Cell[,,] cells, CellInformation type, Vector3Int index)
        {
			Vector3Int lowerBound = default(Vector3Int);
			Vector3Int upperBound = default(Vector3Int);
			SetBounds(ref lowerBound, ref upperBound, index, type.size, rotation);

			for (int i = lowerBound.x; i <= upperBound.x; i++)
			{
				for (int j = lowerBound.y; j <= upperBound.y; j++)
				{
					for (int k = lowerBound.z; k <= upperBound.z; k++)
					{
						if(InputInGridBoundaries(new Vector3Int(i,j,k), size_grid))
                        {
							cluster.SetType(type, i, j, k);
							cluster.GetWaypoints()[i, j, k].basePos = index;
							if(index.x != i || index.y != j || index.z != k)
								cells[i, j, k].Erased();
						}
					}
				}
			}
			cluster.SetBase(index.x, index.y, index.z);
		}

		public static void RemoveType(Vector3Int size_grid, Vector3 rotation, WaypointCluster cluster, Cell[,,] cells, CellInformation type, Vector3Int index)
		{
			Vector3Int lowerBound = default(Vector3Int);
			Vector3Int upperBound = default(Vector3Int);
			Vector3Int basePos = cluster.GetWaypoints()[index.x, index.y, index.z].basePos;
			Waypoint baseWaypoint = cluster.GetWaypoints()[basePos.x, basePos.y, basePos.z];

			SetBounds(ref lowerBound, ref upperBound, basePos, baseWaypoint.type.size, baseWaypoint.rotation);

			for (int i = lowerBound.x; i <= upperBound.x; i++)
			{
				for (int j = lowerBound.y; j <= upperBound.y; j++)
				{
					for (int k = lowerBound.z; k <= upperBound.z; k++)
					{
						if (InputInGridBoundaries(new Vector3Int(i, j, k), size_grid))
						{
							cluster.SetType(null, i, j, k);
							cells[i, j, k].Inactive();
						}
					}
				}
			}
			cluster.ResetBase(basePos.x, basePos.y, basePos.z);
		}

		public static void SetBounds(ref Vector3Int lowerBound, ref Vector3Int upperBound, Vector3Int index, Vector3Int size, Vector3 rotation)
		{
			Vector3Int newSize = default(Vector3Int);
			newSize.x = (int)Mathf.Cos(rotation.y*Mathf.Deg2Rad) * size.x + (int)Mathf.Sin(rotation.x * Mathf.Deg2Rad) * (int)Mathf.Sin(rotation.y * Mathf.Deg2Rad) * size.y + (int)Mathf.Sin(rotation.y * Mathf.Deg2Rad) * (int)Mathf.Cos(rotation.x * Mathf.Deg2Rad) * size.z;
			//newSize.x = newSize.x == 0 ? 1 : newSize.x;
			newSize.y = (int)Mathf.Cos(rotation.x * Mathf.Deg2Rad) * size.y - (int)Mathf.Sin(rotation.x * Mathf.Deg2Rad) * size.z;
			//newSize.y = newSize.y == 0 ? 1 : newSize.y;
			newSize.z = -(int)Mathf.Sin(rotation.y * Mathf.Deg2Rad) * size.x + (int)Mathf.Cos(rotation.y * Mathf.Deg2Rad) * (int)Mathf.Sin(rotation.x * Mathf.Deg2Rad) * size.y + (int)Mathf.Cos(rotation.y * Mathf.Deg2Rad) * (int)Mathf.Cos(rotation.x * Mathf.Deg2Rad) * size.z;
			//newSize.z = newSize.z == 0 ? 1 : newSize.z;

			lowerBound.x = newSize.x < 0 ? index.x + newSize.x +1: index.x;
			//lowerBound.x = lowerBound.x == 0 ? 1 : lowerBound.x;
			lowerBound.y = newSize.y < 0 ? index.y + newSize.y +1: index.y;
			//lowerBound.y = lowerBound.y == 0 ? 1 : lowerBound.y;
			lowerBound.z = newSize.z < 0 ? index.z + newSize.z +1: index.z;
			//lowerBound.z = lowerBound.z == 0 ? 1 : lowerBound.z;

			upperBound.x = newSize.x > 0 ? index.x + newSize.x -1: index.x;
			//upperBound.x = upperBound.x == 0 ? 1 : upperBound.x;
			upperBound.y = newSize.y > 0 ? index.y + newSize.y -1: index.y;
			//upperBound.y = upperBound.y == 0 ? 1 : upperBound.y;
			upperBound.z = newSize.z > 0 ? index.z + newSize.z -1: index.z;
			//upperBound.z = upperBound.z == 0 ? 1 : upperBound.z;

			//Debug.Log("size: " + newSize + "UPP: " + upperBound + " LOW: " + lowerBound);
		}

		public static void SetShowTypeCell(bool show, CellInformation cellInfo, WaypointCluster cluster, Cell[,,] cells)
        {
			if(cluster.GetWaypointsDico().ContainsKey(cellInfo))
            {
				List<Vector3Int> index = cluster.GetWaypointsDico()[cellInfo];

				for (int j = 0; j < index.Count; j++)
				{
					if (!show)
						cells[index[j].x, index[j].y, index[j].z].Sleep();
					else
						cells[index[j].x, index[j].y, index[j].z].Active();

					cluster.GetWaypoints()[index[j].x, index[j].y, index[j].z].show = show;
				}
			}
		}
	}
}

