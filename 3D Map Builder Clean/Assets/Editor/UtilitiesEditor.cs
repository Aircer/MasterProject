using System;
using System.Collections.Generic;
using MapTileGridCreator.Core;
using MapTileGridCreator.CubeImplementation;
using MapTileGridCreator.HexagonalImplementation;
using UnityEditor;
using UnityEngine;

namespace MapTileGridCreator.Utilities
{
	/// <summary>
	/// Static class containining utilities functions for editor.
	/// </summary>
	public static class FuncEditor
	{
		public enum PaintMode
		{
			Single, Erase, Eyedropper
		};

		/// <summary>
		/// IUnstantiate an empty Grid3D.
		/// </summary>
		/// <param name="typegrid">The type of the grid.</param>
		/// <returns>The grid component associated to the gameobject.</returns>
		public static Grid3D InstantiateGrid3D(TypeGrid3D typegrid)
		{
			GameObject obj;
			Grid3D grid;

			switch (typegrid)
			{
				case TypeGrid3D.Cube:
					{
						obj = new GameObject("CubeGrid");
						grid = obj.AddComponent<CubeGrid>();
						GameObject cells = new GameObject();
						cells.name = "Cells";
						cells.transform.parent = obj.transform;
					}
					break;
				case TypeGrid3D.Hexagonal:
					{
						obj = new GameObject("HexagonalGrid");
						grid = obj.AddComponent<HexagonalGrid>();
					}

					break;
				default:
					throw new ArgumentException("No type implemented " + typegrid.ToString() + " inherit Grid3D");
			}

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
		/// Debug a plane grid for helping to visualize the cells.
		/// </summary>
		/// <param name="grid">The grid to debug.</param>
		/// <param name="color"> The color of the grid.</param>
		/// <param name="offset_grid_y"> The offset of y position. </param>
		/// <param name="size_grid">The size of the grid.</param>
		public static void DebugGrid(Grid3D grid, Color color, Vector3Int size_grid, Plane[] planesGrid)
		{
			if (grid.GetTypeGrid() == TypeGrid3D.Hexagonal)
			{
				DebugHexagonGrid(grid, color, size_grid);
			}
			else
			{
				DebugSquareGrid(grid, color, size_grid, planesGrid);
			}
		}

		/// <summary>
		/// Debug a square grid. Use this one if the editor performance is limited rather than other grid debug implementation.
		/// </summary>
		/// <param name="grid">The grid to debug.</param>
		/// <param name="color"> The color of the grid.</param>
		/// <param name="offset_grid_y"> The offset of y position. </param>
		/// <param name="size_grid">The size of the grid.</param>
		private static void DebugSquareGrid(Grid3D grid, Color color, Vector3Int size_grid, Plane[] planesGrid)
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
		/// Debug a hexagon grid.
		/// </summary>
		/// <param name="grid">The grid to debug.</param>
		/// <param name="color"> The color of the grid.</param>
		/// <param name="offset_grid_y"> The offset of y position. </param>
		/// <param name="size_grid">The size of the grid.</param>
		private static void DebugHexagonGrid(Grid3D grid, Color color, Vector3Int size_grid)
		{
			using (new Handles.DrawingScope(color))
			{
				Handles.zTest = UnityEngine.Rendering.CompareFunction.LessEqual;
				Vector3 pos = grid.transform.position;
				float CaseSize = grid.SizeCell * grid.GapRatio;
				pos.y += - CaseSize / 2.0f;

				List<Vector3> axes = grid.GetAxes();
				float angle_xz = Vector3.Angle(axes[2], axes[0]) / 2;

				//Form 
				Vector3[] form = new Vector3[7];
				int p = 0;
				for (int i = 0; i < axes.Count * 3; i += 3)
				{
					if (axes[i % axes.Count].y == 0)
					{
						form[p] = pos + Quaternion.AngleAxis(angle_xz, axes[1]) * (axes[i % axes.Count] * CaseSize / 2.0f);
						p++;
					}
				}
				form[p] = form[0];

				//Grid
				for (int z = -size_grid.z; z <= size_grid.z; z++)
				{
					for (int x = -size_grid.x; x <= size_grid.x; x++)
					{
						Vector3 cellpos = (axes[0] * x + axes[2] * z) * CaseSize;

						Vector3[] points = new Vector3[7];
						for (int i = 0; i < 7; i++)
						{
							points[i] = cellpos + form[i];
						}
						Handles.DrawPolyLine(points);
					}
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
		public static Cell InstantiateCell(GameObject prefab, Grid3D grid, Vector3 position)
		{
			Vector3Int index = grid.GetIndexByPosition(ref position);
			return InstantiateCell(prefab, grid, index);
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
		public static Cell InstantiateCell(List<GameObject> pallet, Grid3D grid, Transform cells, Vector3Int index)
		{
			//GameObject gameObject = PrefabUtility.InstantiatePrefab(prefab, cells.transform) as GameObject;
			GameObject cellGameObject = new GameObject();
			cellGameObject.name = "cell_" + index.x + "_" + index.y + "_" + index.z;
			cellGameObject.transform.parent = cells;
			BoxCollider coll = cellGameObject.AddComponent<BoxCollider>();
			coll.enabled = false;
			Cell cell= cellGameObject.AddComponent<Cell>();

			foreach (GameObject child in pallet)
			{
				GameObject newChild = PrefabUtility.InstantiatePrefab(child, cells.transform) as GameObject;
				newChild.transform.parent = cellGameObject.transform;
				newChild.SetActive(false);
			}
			/*PrefabUtility.UnpackPrefabInstance(gameObject, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
			Cell cell = gameObject.GetComponent<Cell>();
			if (cell == null)
			{
				cell = gameObject.AddComponent<Cell>();
			}*/
			grid.AddCell(index, cell);
			cell.ResetTransform();

			return cell;
		}

		public static Cell ActivatePallet(int palletIndex, Grid3D grid, Vector3Int index, bool active, float rotation = 0)
		{
			Cell cell = CellAtThisIndex(grid, index);

			for (int i = 0; i < cell.transform.childCount; i++)
			{
				if (i == palletIndex)
					cell.transform.GetChild(i).gameObject.SetActive(active);
				else
					cell.transform.GetChild(i).gameObject.SetActive(false);
			}
			cell.transform.eulerAngles = new Vector3(0, rotation, 0);

			return cell;
		}

		public static Cell SwapCell(Grid3D oldGrid, Grid3D newGrid, Vector3Int index)
		{
			Cell cell = CellAtThisIndex(oldGrid, index);
			
			if (cell != null)
			{
				newGrid.AddCell(index, cell);
				cell.transform.SetParent(newGrid.transform);
				oldGrid.DeleteCell(cell);
				cell.ResetTransform();
			}

			//Undo.SetTransformParent(cell.gameObject.transform, newGrid.transform, "Cell swap");
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
					InstantiateCell(prefab, grid, index);
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

		public static Dictionary<Vector3Int, Cell> CreateEmptyCells(List<GameObject> pallet, Grid3D grid, Vector3Int size_grid)
		{
			//GameObject pallet = AssetDatabase.LoadAssetAtPath("Assets/Cells/Cell.prefab", typeof(GameObject)) as GameObject;
			Transform cellsParent = grid.transform.Find("Cells");
			Dictionary<Vector3Int, Cell> cells = new Dictionary<Vector3Int, Cell>();

			for (int x = 0; x < size_grid.x; x++)
			{
				for (int y = 0; y < size_grid.y; y++)
				{
					for (int z = 0; z < size_grid.z; z++)
					{
						cells.Add(new Vector3Int(x, y, z), InstantiateCell(pallet, grid, cellsParent, new Vector3Int(x, y, z)));
					}
				}
			}

			return cells;
		}

		public static bool InputInGridBoundaries(Grid3D grid, Vector3 input, Vector3Int size_grid)
		{
			TypeGrid3D typegrid = grid.GetTypeGrid();
			bool inBoundaries = true;

			switch (typegrid)
			{
				case TypeGrid3D.Cube:
					{
						if (input.x < 0 || input.y < 0 || input.z < 0 || input.x > size_grid.x - 1 || input.y > size_grid.y - 1 || input.z > size_grid.z - 1)
							inBoundaries = false;
					}
					break;
				case TypeGrid3D.Hexagonal:
					{

					}

					break;
				default:
					throw new ArgumentException("No type implemented " + typegrid.ToString() + " inherit Grid3D");
			}

			return inBoundaries;
		}

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

		public static WaypointCluster CreateClusterAndWaypoints(Vector3Int size_grid)
		{
			GameObject clusterGameObject = new GameObject();
			WaypointCluster cluster;
			GameObject[] allObjects = GameObject.FindGameObjectsWithTag("Cluster");
			foreach (GameObject obj in allObjects)
			{
				Editor.DestroyImmediate(obj);
			}

			//Editor.Instantiate(clusterGameObject);
			clusterGameObject.tag = "Cluster";
			clusterGameObject.name = "WaypointsCluster";
			cluster= clusterGameObject.AddComponent<WaypointCluster>();
			cluster.CreateWaypoints(size_grid);

			return cluster;
		}

		public static void CreateGridAndCells(out Grid3D grid,  out Dictionary<Vector3Int, Cell> cells, List<GameObject> pallet, Vector3Int size_grid)
		{
			//Destroy all existing grids 
			GameObject[] allObjects = GameObject.FindGameObjectsWithTag("Grid");
			foreach (GameObject obj in allObjects)
			{
                UnityEngine.Object.DestroyImmediate(obj);
			}

			//Create Grid and all Cells 
			grid = InstantiateGrid3D(TypeGrid3D.Cube);
			cells = CreateEmptyCells(pallet, grid, size_grid);
		}
	}
}

