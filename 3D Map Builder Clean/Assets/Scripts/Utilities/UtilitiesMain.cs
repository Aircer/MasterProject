using System.Collections.Generic;
using MapTileGridCreator.Core;
using MapTileGridCreator.UtilitiesVisual;
using UnityEditor;
using UnityEngine;

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
		public static Grid3D InstantiateGrid3D(Vector3Int size, List<CellInformation> cellInfos, Dictionary<CellInformation, GameObject> pallet, GameObject palletObject)
		{
			GameObject obj;
			Grid3D grid;

			obj = new GameObject("CubeGrid");
			grid = obj.AddComponent<Grid3D>();
			grid.Initialize(size, cellInfos, pallet, palletObject);
			return grid;
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
				pos.y += -0.5f;

				for (float x = -1; x < size_grid.x; x++)
				{
					Handles.DrawLine(pos + new Vector3(x + 0.5f, flipY, -0.5f),
									pos + new Vector3(x + 0.5f, flipY, size_grid.z - 0.5f));

					Handles.DrawLine(pos + new Vector3(x + 0.5f, size_grid.y, flipZ - 0.5f),
									pos + new Vector3(x + 0.5f, 0, flipZ - 0.5f));
				}

				for (float y = -1; y < size_grid.y; y++)
				{
					Handles.DrawLine(pos + new Vector3(flipX - 0.5f, y + 1f, -0.5f),
									pos + new Vector3(flipX - 0.5f, y + 1f, size_grid.z - 0.5f));

					Handles.DrawLine(pos + new Vector3(-0.5f, y + 1f, flipZ - 0.5f),
									pos + new Vector3(size_grid.x - 0.5f, y + 1f, flipZ - 0.5f));
				}

				for (float z = -1; z < size_grid.z; z++)
				{
					Handles.DrawLine(pos + new Vector3(-0.5f, flipY, z + 0.5f),
									pos + new Vector3(size_grid.x - 0.5f, flipY, z + 0.5f));

					Handles.DrawLine(pos + new Vector3(flipX - 0.5f, 0, z + 0.5f),
									pos + new Vector3(flipX - 0.5f, size_grid.y, z + 0.5f));
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
		/// Create cells and waypoints
		/// </summary>
		public static void CreateCells(ref Grid3D grid, List<CellInformation> cellInfos, Dictionary<CellInformation, GameObject> pallet, Vector3Int size_grid, GameObject palletObject)
		{
			//Create Grid and all Cells 
			grid = InstantiateGrid3D(size_grid, cellInfos, pallet, palletObject);
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

		public static void SetShowLayersCell(Vector3Int minVal, Vector3Int maxVal, Cell[,,] cells)
		{
			Vector3Int size_grid = new Vector3Int(cells.GetLength(0), cells.GetLength(1), cells.GetLength(2));

			for (int i = 0; i < size_grid.x; i++)
			{
				for (int j = 0; j < size_grid.y; j++)
				{
					for (int k = 0; k < size_grid.z; k++)
					{
						if (i > maxVal.x - 1 || i < minVal.x - 1 ||
							j > maxVal.y - 1 || j < minVal.y - 1 ||
							k > maxVal.z - 1 || k < minVal.z - 1)
						{
							cells[i, j, k].Sleep();
							//cluster.GetWaypoints()[i, j, k].show = false;
						}
						else
						{
							cells[i, j, k].AWake();
							//cluster.GetWaypoints()[i, j, k].show = true;
						}
					}
				}
			}
		}
	}
}

