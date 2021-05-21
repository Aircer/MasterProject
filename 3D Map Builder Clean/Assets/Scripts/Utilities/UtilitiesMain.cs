using System;
using System.Collections.Generic;
using MapTileGridCreator.Core;
using MapTileGridCreator.UtilitiesVisual;
using MapTileGridCreator.CubeImplementation;
using MapTileGridCreator.HexagonalImplementation;
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections;

namespace MapTileGridCreator.UtilitiesMain
{
	/// <summary>
	/// Static class containining utilities functions for editor.
	/// </summary>
	public static class FuncMain
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
		public static Cell InstantiateCell(Grid3D grid, Vector3Int index, Dictionary<CellInformation, GameObject> pallet)
		{
			//GameObject cellGameObject = PrefabUtility.InstantiatePrefab(pallet, grid.transform) as GameObject;
			GameObject cellGameObject = new GameObject();
			cellGameObject.name = "cell_" + index.x + "_" + index.y + "_" + index.z;
			cellGameObject.transform.parent = grid.transform;
			BoxCollider coll = cellGameObject.AddComponent<BoxCollider>();
			coll.enabled = false;
			Cell cell = cellGameObject.AddComponent<Cell>();
			cell.typeDicoCell = pallet;
			/*foreach (KeyValuePair<CellInformation, GameObject> child in pallet)
			{
				GameObject newChild = PrefabUtility.InstantiatePrefab(child.Value, grid.transform) as GameObject;
				PrefabUtility.UnpackPrefabInstance(newChild, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
				newChild.transform.parent = cellGameObject.transform;
				newChild.transform.localPosition = new Vector3(0, 0, 0);
				newChild.SetActive(false);
				cell.typeDicoCell.Add(child.Key, newChild);
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
		public static void CreateEmptyCells(out Cell[,,] cells, Grid3D grid, Vector3Int size_grid, Dictionary<CellInformation, GameObject> pallet)
		{
			//GameObject pallet = AssetDatabase.LoadAssetAtPath("Assets/Cells/Cell.prefab", typeof(GameObject)) as GameObject;
			Cell[,,] newCells = new Cell[size_grid.x, size_grid.y, size_grid.z];
			int numberCells = size_grid.x * size_grid.y * size_grid.z;
			float progressBarTime = 0f;

			for (int x = 0; x < size_grid.x; x++)
			{
				for (int y = 0; y < size_grid.y; y++)
				{
					for (int z = 0; z < size_grid.z; z++)
					{
						progressBarTime += 1;
						newCells[x, y, z] = InstantiateCell(grid, new Vector3Int(x, y, z), pallet);
						EditorUtility.DisplayProgressBar("Creatings empty cells", "You can go have a coffee...", progressBarTime / numberCells);
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
		public static void CreateCellsAndWaypoints(ref Grid3D grid, ref Cell[,,] cells, ref WaypointCluster cluster, Dictionary<CellInformation, GameObject> pallet, Vector3Int size_grid)
		{
			//Create Grid and all Cells 
			grid = InstantiateGrid3D();
			CreateEmptyCells(out cells, grid, size_grid, pallet);
			grid._cells = cells;
			//Create Cluster and Waypoints
			List<CellInformation> keyList = new List<CellInformation>(pallet.Keys);
			WaypointCluster newCluster = new WaypointCluster(size_grid, keyList);

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
		public static void TransformCellsFromWaypoints(Cell[,,] cells, Waypoint[,,] waypoints)
		{
			int size_x = waypoints.GetLength(0); int size_y = waypoints.GetLength(1); int size_z = waypoints.GetLength(2);

			for (int i = 0; i < size_x; i++)
			{
				for (int j = 0; j < size_y; j++)
				{
					for (int k = 0; k < size_z; k++)
					{
						if (waypoints[i, j, k].type == null && cells[i, j, k].type != null)
						{
							cells[i, j, k].Inactive();
						}

						if (waypoints[i, j, k].type != null && (cells[i, j, k].type == null || waypoints[i, j, k].type != cells[i, j, k].type))
						{
							cells[i, j, k].Inactive();
							cells[i, j, k].Painted(waypoints[i, j, k].type, waypoints[i, j, k].rotation);
							cells[i, j, k].Active();

							if (!waypoints[i, j, k].show)
								cells[i, j, k].Sleep();
						}

						if (waypoints[i, j, k].type != null && !waypoints[i, j, k].show)
						{
							cells[i, j, k].Sleep();
						}
					}
				}
			}

			for (int i = 0; i < size_x; i++)
			{
				for (int j = 0; j < size_y; j++)
				{
					for (int k = 0; k < size_z; k++)
					{
						FuncVisual.UpdateCellsAroundVisual(cells, waypoints, new Vector3Int(i, j, k), waypoints[i,j,k].type);
					}
				}
			}
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
						if(waypoints[i, j, k].inPathFrom != null)
							cells[i, j, k].SetDebug(pathfindingState, waypoints[i, j, k].inPath, waypoints[i, j, k].colorDot, 
													waypoints[i, j, k].pathfindingWaypoint, waypoints[i, j, k].gCost, waypoints[i, j, k].inPathFrom.key);
						else
							cells[i, j, k].SetDebug(pathfindingState, waypoints[i, j, k].inPath, waypoints[i, j, k].colorDot,
													waypoints[i, j, k].pathfindingWaypoint, waypoints[i, j, k].gCost, Vector3Int.down);
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
						cells[i, j, k].ResetDebug();
					}
				}
			}
		}

		/// <summary>
		/// Check if there is enough room to paint the new asset
		///</summary>
		public static bool CanPaintHere(Vector3Int size_grid, Vector3Int index, Vector3Int size, Cell[,,] cells, Vector3 rotation)
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
						if (!InputInGridBoundaries(new Vector3Int(i, j, k), size_grid) || !(cells[i, j, k].state == CellState.Inactive || cells[i, j, k].state == CellState.Painted))
							return false;
					}
				}
			}

			return true;
		}

		public static void SetBounds(ref Vector3Int lowerBound, ref Vector3Int upperBound, Vector3Int index, Vector3Int size, Vector3 rotation)
		{
			Vector3Int newSize = default(Vector3Int);
			newSize.x = (int)Mathf.Cos(rotation.y*Mathf.Deg2Rad) * size.x + (int)Mathf.Sin(rotation.x * Mathf.Deg2Rad) * (int)Mathf.Sin(rotation.y * Mathf.Deg2Rad) * size.y + (int)Mathf.Sin(rotation.y * Mathf.Deg2Rad) * (int)Mathf.Cos(rotation.x * Mathf.Deg2Rad) * size.z;
			newSize.y = (int)Mathf.Cos(rotation.x * Mathf.Deg2Rad) * size.y - (int)Mathf.Sin(rotation.x * Mathf.Deg2Rad) * size.z;
			newSize.z = -(int)Mathf.Sin(rotation.y * Mathf.Deg2Rad) * size.x + (int)Mathf.Cos(rotation.y * Mathf.Deg2Rad) * (int)Mathf.Sin(rotation.x * Mathf.Deg2Rad) * size.y + (int)Mathf.Cos(rotation.y * Mathf.Deg2Rad) * (int)Mathf.Cos(rotation.x * Mathf.Deg2Rad) * size.z;

			lowerBound.x = newSize.x < 0 ? index.x + newSize.x +1: index.x;
			lowerBound.y = newSize.y < 0 ? index.y + newSize.y +1: index.y;
			lowerBound.z = newSize.z < 0 ? index.z + newSize.z +1: index.z;

			upperBound.x = newSize.x > 0 ? index.x + newSize.x -1: index.x;
			upperBound.y = newSize.y > 0 ? index.y + newSize.y -1: index.y;
			upperBound.z = newSize.z > 0 ? index.z + newSize.z -1: index.z;
		}

		public static void SetShowTypeCell(bool show, CellInformation cellInfo, WaypointCluster cluster, Cell[,,] cells)
        {
			Vector3Int size_grid = new Vector3Int(cells.GetLength(0), cells.GetLength(1), cells.GetLength(2));
			for (int i = 0; i < size_grid.x; i++)
			{
				for (int j = 0; j < size_grid.y; j++)
				{
					for (int k = 0; k < size_grid.z; k++)
					{
						if(cluster.GetWaypoints()[i,j,k].type == cellInfo)
                        {
							if (!show)
								cells[i, j, k].Sleep();
							else
								cells[i, j, k].Active();

							cluster.GetWaypoints()[i, j, k].show = show;
						}
					}
				}
			}
		}

		public static void PaintCell(Cell[,,] cells, Vector3Int index, CellInformation type, Vector3 rotation)
		{
			//Active the cell at the index position 
			cells[index.x, index.y, index.z].Painted(type, rotation);
		}

		public static void EraseCell(Cell[,,] cells, WaypointCluster cluster, Vector3Int index, CellInformation type, Vector3 rotation)
		{
			cells[index.x, index.y, index.z].Erased();

			/*
			//All the cells that will be occupied by the activated object in the index cells will be set to Erased state (which activate their collider)
			Vector3Int size_grid = new Vector3Int(cells.GetLength(0), cells.GetLength(1), cells.GetLength(2));
			Vector3Int lowerBound = default(Vector3Int);
			Vector3Int upperBound = default(Vector3Int);

			SetBounds(ref lowerBound, ref upperBound, cluster.GetWaypoints()[index.x, index.y, index.z].basePos, type.typeParams.size, rotation);

			for (int i = lowerBound.x; i <= upperBound.x; i++)
			{
				for (int j = lowerBound.y; j <= upperBound.y; j++)
				{
					for (int k = lowerBound.z; k <= upperBound.z; k++)
					{
						if (InputInGridBoundaries(new Vector3Int(i, j, k), size_grid))
						{
							cells[i, j, k].Erased();
						}
					}
				}
			}*/
		}

		public static void DesactivateCell(Cell[,,] cells, WaypointCluster cluster, Vector3Int index, CellInformation type, Vector3 rotation)
		{
			cells[index.x, index.y, index.z].Inactive();

			/*
			Vector3Int size_grid = new Vector3Int(cells.GetLength(0), cells.GetLength(1), cells.GetLength(2));
			Vector3Int lowerBound = default(Vector3Int);
			Vector3Int upperBound = default(Vector3Int);
			Vector3Int basePos = cluster.GetWaypoints()[index.x, index.y, index.z].basePos;
			Waypoint baseWaypoint = cluster.GetWaypoints()[basePos.x, basePos.y, basePos.z];

			if (baseWaypoint != null && baseWaypoint.type != null)
			{
				SetBounds(ref lowerBound, ref upperBound, basePos, baseWaypoint.type.typeParams.size, baseWaypoint.rotation);

				for (int i = lowerBound.x; i <= upperBound.x; i++)
				{
					for (int j = lowerBound.y; j <= upperBound.y; j++)
					{
						for (int k = lowerBound.z; k <= upperBound.z; k++)
						{
							if (InputInGridBoundaries(new Vector3Int(i, j, k), size_grid))
							{
								cells[i, j, k].Inactive();
							}
						}
					}
				}
			}*/
		}

		public static IEnumerator CoroutineTransformCellsFromWaypoints(Cell[,,] cells, Waypoint[,,] waypoints)
		{
			//TransformCellsFromWaypoints(cells, waypoints);

			for (int i = 0; i < waypoints.GetLength(0); i++)
			{
				for (int j = 0; j < waypoints.GetLength(1); j++)
				{
					for (int k = 0; k < waypoints.GetLength(2); k++)
					{
						if (waypoints[i, j, k].type == null && cells[i, j, k].type != null)
						{
							cells[i, j, k].Inactive();
						}

						if (waypoints[i, j, k].type != null)
						{
							cells[i, j, k].Inactive();
							cells[i, j, k].Painted(waypoints[i, j, k].type, waypoints[i, j, k].rotation);
							cells[i, j, k].Active();

							FuncVisual.UpdateCellsAroundVisual(cells, waypoints, new Vector3Int(i, j, k), waypoints[i, j, k].type);

							if (!waypoints[i, j, k].show)
								cells[i, j, k].Sleep();
						}

						if (waypoints[i, j, k].type != null && !waypoints[i, j, k].show)
						{
							cells[i, j, k].Sleep();
						}
					}
					yield return null;
				}
			}

			for (int i = 0; i < waypoints.GetLength(0); i++)
			{
				for (int j = 0; j < waypoints.GetLength(1); j++)
				{
					for (int k = 0; k < waypoints.GetLength(2); k++)
					{
						FuncVisual.UpdateCellsAroundVisual(cells, waypoints, new Vector3Int(i, j, k), waypoints[i, j, k].type);
					}
					yield return null;
				}
			}
		}

		public static class EditorCoroutines
		{

			public class Coroutine
			{
				public IEnumerator enumerator;
				public System.Action<bool> OnUpdate;
				public List<IEnumerator> history = new List<IEnumerator>();
			}

			static readonly List<Coroutine> coroutines = new List<Coroutine>();

			public static void Execute(IEnumerator enumerator, System.Action<bool> OnUpdate = null)
			{
				if (coroutines.Count == 0)
				{
					EditorApplication.update += Update;
				}
				var coroutine = new Coroutine { enumerator = enumerator, OnUpdate = OnUpdate };
				coroutines.Add(coroutine);
			}

			static void Update()
			{
				for (int i = 0; i < coroutines.Count; i++)
				{
					var coroutine = coroutines[i];
					bool done = !coroutine.enumerator.MoveNext();
					if (done)
					{
						if (coroutine.history.Count == 0)
						{
							coroutines.RemoveAt(i);
							i--;
						}
						else
						{
							done = false;
							coroutine.enumerator = coroutine.history[coroutine.history.Count - 1];
							coroutine.history.RemoveAt(coroutine.history.Count - 1);
						}
					}
					else
					{
						if (coroutine.enumerator.Current is IEnumerator)
						{
							coroutine.history.Add(coroutine.enumerator);
							coroutine.enumerator = (IEnumerator)coroutine.enumerator.Current;
						}
					}
					if (coroutine.OnUpdate != null) coroutine.OnUpdate(done);
				}
				if (coroutines.Count == 0) EditorApplication.update -= Update;
			}

			internal static void StopAll()
			{
				coroutines.Clear();
				EditorApplication.update -= Update;
			}

		}
	}
}

