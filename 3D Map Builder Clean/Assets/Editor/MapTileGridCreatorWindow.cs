using System.Collections.Generic;
using System.IO;
using MapTileGridCreator.Core;
using MapTileGridCreator.SerializeSystem;
using MapTileGridCreator.Utilities;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Main window class.
/// </summary>
[CanEditMultipleObjects]
public class MapTileGridCreatorWindow : EditorWindow
{
	#region Variables
	private enum PaintMode
	{
		Single, Erase, Eyedropper
	};

	//Global
	private Vector2 _scroll_position;

	private Grid3D _grid;
	[SerializeField]
	private Vector3Int _size_grid = new Vector3Int(5, 5, 5);

	//Debug Grid
	[SerializeField]
	private bool _debug_grid = true;
	private GameObject _coordinates;

	private Plane[] _planesGrid = new Plane[3];

	//Waypoints
	private WaypointCluster _cluster;

	//Pathfinding
	private string[] _pathfindingTypes = new string[] { "Flood", "A*" };
	private int _pathfindingCurrentType;
	private Vector3Int[] _start_end = new Vector3Int[2];
	private Vector2 _maxJump;

	//Undo
	private GUIContent _undoIcon;
	private bool _noUndo = true;
	private PaintMode _last_mode_paint;
	private List<Vector3Int> _lastIndexToPaint = new List<Vector3Int>();
	private int _last_pallet_index;

	//Paint
	private GUIContent[] _modes_paint;
	private PaintMode _mode_paint;
	private static bool _painting = false;
	private Vector3Int _startingPaintIndex;
	private List<Vector3Int> _indexToPaint = new List<Vector3Int>();
	private GameObject _brush;
	private Vector3Int _oldInputIndex;
	private float _rotationCell = 0;
	private GUIContent _rotationIcon;

	[SerializeField]
	[Min(1)]
	private float _dist_default_interaction = 100.0f;
	[SerializeField]
	private string _path_pallet = "Assets/Cells/Cubes";
	private int _pallet_index;
	private List<GameObject> _pallet = new List<GameObject>();

    #endregion

    [MenuItem("MapTileGridCreator/Open")]
	public static void OpenWindows()
	{
		MapTileGridCreatorWindow window = (MapTileGridCreatorWindow)GetWindow(typeof(MapTileGridCreatorWindow));
		window.Show();
	}

	private void OnEnable()
	{
		_modes_paint = new GUIContent[] {
			new GUIContent(EditorGUIUtility.IconContent("Grid.PaintTool", "Paint one by one the prefab selected")),
			new GUIContent(EditorGUIUtility.IconContent("Grid.EraserTool", "Erase the cell in scene view")),
			new GUIContent(EditorGUIUtility.IconContent("Grid.PickingTool", "Eyedropper to auto select the corresponding prefab in pallete")) };

		_undoIcon = new GUIContent(EditorGUIUtility.IconContent("undoIcon", "Undo last Paint/Erase"));
		_rotationIcon = new GUIContent(EditorGUIUtility.IconContent("d_RotateTool On@2x", "Rotate pallet"));
	}

	private void OnFocus()
	{
		SceneView.duringSceneGui -= OnSceneGUI;
		SceneView.duringSceneGui += OnSceneGUI;

		RefreshPallet();
	}

	private void RefreshPallet()
	{
		_pallet.Clear();
		string[] prefabFiles = Directory.GetFiles(_path_pallet, "*.prefab");
		foreach (string prefabFile in prefabFiles)
		{
			_pallet.Add(AssetDatabase.LoadAssetAtPath(prefabFile, typeof(GameObject)) as GameObject);
		}
	}

	#region SceneManagement

	private void OnSceneGUI(SceneView sceneView)
	{
		if (_grid != null)
		{
			if (_debug_grid)
			{
				//Set the planes of the grid position and normal
				_planesGrid = FuncEditor.SetGridDebugPlanesNormalAndPosition(_planesGrid, _size_grid, SceneView.lastActiveSceneView.rotation.eulerAngles);
				//Draw the drawing plans of the grid 
				FuncEditor.DebugGrid(_grid, DebugsColor.grid_help, _size_grid, _planesGrid);
			}

			PaintEdit();
		}

		HandleUtility.Repaint();
	}

	/// <summary>
	/// Update the working grid.
	/// </summary>
	/// <param name="selectedGrid"></param>
	private void UpdateGridSelected(Grid3D selectedGrid)
	{
		if (selectedGrid != null)
		{
			if (FuncEditor.IsGameObjectInstancePrefab(selectedGrid.gameObject))
			{
				if (EditorUtility.DisplayDialog("Modify Existing Grid Prefab", "The grid selected is a prefab and cannot be modified unless you unpack it. " +
				"\n Do you want to continue ?", "Yes", "No"))
				{
					PrefabUtility.UnpackPrefabInstance(selectedGrid.gameObject, PrefabUnpackMode.OutermostRoot, InteractionMode.UserAction);
				}
				else
				{
					selectedGrid = null;
				}
			}
		}
		else
		{
			_grid = null;
		}

		if (selectedGrid != null && selectedGrid != _grid)
		{
			_grid = selectedGrid;
		}
	}

	////////////////////////////////////////
	// Paint differents modes

	private void PaintEdit()
	{
		switch (_mode_paint)
		{
			case PaintMode.Single:
				{
					Vector3 input = GetGridPositionInput(0.5f);
					PaintInput(input, _rotationCell);
					SetBrushPosition(input, false, _rotationCell);
				}
				break;

			case PaintMode.Erase:
				{
					Vector3 input = GetGridPositionInput(-0.5f);
					EraseInput(input);
					SetBrushPosition(input, true);
				}
				break;

			case PaintMode.Eyedropper:
				{
					Vector3 input = GetGridPositionInput(-0.1f);
					Cell selected = _grid.TryGetCellByPosition(ref input);
					EyedropperInput(selected);
					if (selected != null)
					{
						SetBrushPosition(selected.transform.position, false);
					}
				}
				break;
		}
	}

	#region Paint/Erase
	private void PaintInput(Vector3 input, float rotation)
	{
		Vector3Int inputIndex = FuncEditor.GetIndexByPosition(_grid, input);

		//If input is outside the Grid do not paint
		if (FuncEditor.InputInGridBoundaries(_grid, inputIndex, _size_grid))
		{
			List<Vector3Int> newIndexToPaint = new List<Vector3Int>();

			if (Event.current.type == EventType.Layout)
			{
				HandleUtility.AddDefaultControl(0);
			}

			//Set starting index of paint 
			if (!_painting && _pallet_index < _pallet.Count && Event.current.type == EventType.MouseDown && Event.current.button == 0)
			{
				_startingPaintIndex = inputIndex;
				_indexToPaint.Clear();
				_painting = true;
			}

			//Get all indexes that will be paint
			if (_painting && _pallet_index < _pallet.Count && Event.current.type == EventType.MouseDrag && Event.current.button == 0)
			{
				newIndexToPaint.Clear();
				for (int i = 0; i <= Mathf.Abs(inputIndex.x - _startingPaintIndex.x); i++)
				{
					for (int j = 0; j <= Mathf.Abs(inputIndex.y - _startingPaintIndex.y); j++)
					{
						for (int k = 0; k <= Mathf.Abs(inputIndex.z - _startingPaintIndex.z); k++)
						{
							Vector3Int index = new Vector3Int(_startingPaintIndex.x + (int)Mathf.Sign(inputIndex.x - _startingPaintIndex.x) * i, _startingPaintIndex.y + (int)Mathf.Sign(inputIndex.y - _startingPaintIndex.y) * j, _startingPaintIndex.z + (int)Mathf.Sign(inputIndex.z - _startingPaintIndex.z) * k);

							Cell cell = FuncEditor.CellAtThisIndex(_grid, index);
							
							//Add the index to the list of indexes to paint only if the cell Collider is not active
							if (!cell.GetColliderState())
								newIndexToPaint.Add(index);
						}
					}
				}

				//Paint all indexes that have not be already painted
				foreach (Vector3Int newIndex in newIndexToPaint)
				{
					bool indexExist = false;
					foreach (Vector3Int index in _indexToPaint)
					{
						if (newIndex == index)
							indexExist = true;
					}

					if (!indexExist)
					{
						_indexToPaint.Add(newIndex);
						ActivatePallet(newIndex, true);
					}
				}

				//Erase indexes no more painted
				foreach (Vector3Int index in _indexToPaint)
				{
					bool indexNoMorePainted = true;
					foreach (Vector3Int newIndex in newIndexToPaint)
					{
						if (newIndex == index)
							indexNoMorePainted = false;
					}

					if (indexNoMorePainted)
					{
						ActivatePallet(index, false);
						newIndexToPaint.Remove(index);
					}
				}

				_indexToPaint = newIndexToPaint;
			}
		}

		if (_painting && Event.current.type == EventType.MouseUp && Event.current.button == 0)
		{
			_painting = false;

			if(!_indexToPaint.Contains(inputIndex))
				_indexToPaint.Add(inputIndex);

			foreach (Vector3Int index in _indexToPaint)
			{
				ActivatePallet(index, false);
				ActivatePallet(index, true);
				SetPathWaypoint(index);
				SetCellColliderState(index, true);
				ShowCell(index, true);
			}

			if (_start_end[1] != Constants.UNDEFINED_POSITION)
				_cluster.FindPath(_start_end[0], _start_end[1], _maxJump);
			if (_start_end[0] != Constants.UNDEFINED_POSITION && _pathfindingCurrentType == 0)
				_cluster.FindPath(_start_end[0], _maxJump);

			_lastIndexToPaint = _indexToPaint;
			_last_mode_paint = _mode_paint;
			_last_pallet_index = _pallet_index;
			_noUndo = false;
		}
	}

	private void EraseInput(Vector3 input)
	{
		Vector3Int inputIndex = FuncEditor.GetIndexByPosition(_grid, input);
		//Painting out of bounds
		if (FuncEditor.InputInGridBoundaries(_grid, inputIndex, _size_grid))
		{
			List<Vector3Int> newIndexToPaint = new List<Vector3Int>();

			if (Event.current.type == EventType.Layout)
			{
				HandleUtility.AddDefaultControl(0);
			}

			//Set starting index of paint 
			if (!_painting && Event.current.type == EventType.MouseDown && Event.current.button == 0)
			{
				_startingPaintIndex = inputIndex;
				_indexToPaint.Clear();
				_painting = true;
			}

			//Get all indexes that will be paint
			if (_painting && Event.current.type == EventType.MouseDrag && Event.current.button == 0)
			{
				newIndexToPaint.Clear();
				for (int i = 0; i <= Mathf.Abs(inputIndex.x - _startingPaintIndex.x); i++)
				{
					for (int j = 0; j <= Mathf.Abs(inputIndex.y - _startingPaintIndex.y); j++)
					{
						for (int k = 0; k <= Mathf.Abs(inputIndex.z - _startingPaintIndex.z); k++)
						{
							Vector3Int index = new Vector3Int(_startingPaintIndex.x + (int)Mathf.Sign(inputIndex.x - _startingPaintIndex.x) * i, _startingPaintIndex.y + (int)Mathf.Sign(inputIndex.y - _startingPaintIndex.y) * j, _startingPaintIndex.z + (int)Mathf.Sign(inputIndex.z - _startingPaintIndex.z) * k);

							Cell cell = FuncEditor.CellAtThisIndex(_grid, index);

							if (cell.GetColliderState())
								newIndexToPaint.Add(index);
						}
					}
				}

				//Paint all indexes that have not be already painted
				foreach (Vector3Int newIndex in newIndexToPaint)
				{
					bool indexExist = false;
					foreach (Vector3Int index in _indexToPaint)
					{
						if (newIndex == index)
							indexExist = true;
					}

					if (!indexExist)
					{
						_indexToPaint.Add(newIndex);
						ShowCell(newIndex, false);
					}
				}

				//Erase indexes no more painted
				foreach (Vector3Int index in _indexToPaint)
				{
					bool indexNoMorePainted = true;
					foreach (Vector3Int newIndex in newIndexToPaint)
					{
						if (newIndex == index)
							indexNoMorePainted = false;
					}

					if (indexNoMorePainted)
					{
						newIndexToPaint.Remove(index);
						ShowCell(index, true);
					}
				}
				_indexToPaint = newIndexToPaint;
			}
		}
		if (_painting && Event.current.type == EventType.MouseUp && Event.current.button == 0)
		{
			_painting = false;
			_indexToPaint.Add(inputIndex);

			foreach (Vector3Int point in _indexToPaint)
			{
				RemovePathWaypoint(point);
				ActivatePallet(point, false);
			}

			if (_start_end[1] != Constants.UNDEFINED_POSITION)
				_cluster.FindPath(_start_end[0], _start_end[1], _maxJump);
			if (_start_end[0] != Constants.UNDEFINED_POSITION && _pathfindingCurrentType == 0)
				_cluster.FindPath(_start_end[0], _maxJump);

			_lastIndexToPaint = _indexToPaint;
			_last_mode_paint = _mode_paint;
			_noUndo = false;
		}
	}

	private void ActivatePallet(Vector3Int input, bool active)
	{
		Cell cell = FuncEditor.CellAtThisIndex(_grid, input);
		if(cell)
			cell.ActivatePallet(active, _pallet_index, _rotationCell);
	}

	private void SetCellColliderState(Vector3Int input, bool active)
	{
		Cell cell = FuncEditor.CellAtThisIndex(_grid, input);
		if (cell)
			cell.SetColliderState(active);
	}

	private void ShowCell(Vector3Int input, bool show)
	{
		Cell cell = FuncEditor.CellAtThisIndex(_grid, input);
		if (cell)
			cell.SetMeshState(show);
	}

	private void SetPathWaypoint(Vector3Int index)
	{
		Cell cell = FuncEditor.CellAtThisIndex(_grid, index);
		if (cell)
		{
			if (cell.GetTypeCell() == "Start_End")
			{
				if (_start_end[0] == Constants.UNDEFINED_POSITION || _pathfindingCurrentType == 0)
                {
					ActivatePallet(_start_end[0], false);
					ActivatePallet(_start_end[1], false);
					_start_end[0] = index;
					ActivatePallet(_start_end[0], true);
					cell.SetColor("start");
				}
				else
				{
					if (_start_end[1] != index && _start_end[1] != Constants.UNDEFINED_POSITION)
					{
						ActivatePallet(_start_end[1], false);
					}

					_start_end[1] = index;
					cell.SetColor("end");
				}
			}

			if (cell.GetTypeCell() == "Default" || cell.GetTypeCell() == "Grass" || cell.GetTypeCell() == "Water")
			{
				_cluster.PathBlocked(index, true);
				_cluster.Ground(index, true);
			}

			if (cell.GetTypeCell() == "Ladders" || cell.GetTypeCell() == "Stairs")
			{
				_cluster.Ground(index, true);
			}
		}
	}

	private void RemovePathWaypoint(Vector3Int index)
	{
		Cell cell = FuncEditor.CellAtThisIndex(_grid, index);
		if (cell.GetTypeCell() == "Start_End")
		{
			if (_start_end[0] == index)
			{
				_start_end[0] = _start_end[1];
				_start_end[1] = Constants.UNDEFINED_POSITION;
				Cell cell2 = FuncEditor.CellAtThisIndex(_grid, _start_end[0]);
				cell2.SetColor("start");
			}
			else
			{
				_start_end[1] = Constants.UNDEFINED_POSITION;
			}
		}
		if (cell.GetTypeCell() == "Default" || cell.GetTypeCell() == "Grass" || cell.GetTypeCell() == "Water")
		{
			_cluster.PathBlocked(index, false);
			_cluster.Ground(index, false);
		}

		if (cell.GetTypeCell() == "Ladders" || cell.GetTypeCell() == "Stairs")
		{
			_cluster.Ground(index, false);
		}
	}
	#endregion

	private void EyedropperInput(Cell selectedCell)
	{
		if (Event.current.type == EventType.Layout)
		{
			HandleUtility.AddDefaultControl(0); // Consume the event
		}

		//Select prefab from pallett and got to paint
		if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
		{
			GameObject prefabInstance = selectedCell.gameObject;
			GameObject prefab = FuncEditor.GetPrefabFromInstance(prefabInstance);

			int newIndex = _pallet.FindIndex(x => x.Equals(prefab));
			if (newIndex >= 0)
			{
				_mode_paint = (int)PaintMode.Single;
				_pallet_index = newIndex;
				Debug.Log("Prefab " + prefab.name + " selected.");
			}
			else
			{
				Debug.LogError("Prefab is not from the pallet");
			}
		}
		//Cancel
		else if (Event.current.type == EventType.MouseDown && Event.current.button == 1)
		{
			_mode_paint = PaintMode.Single;
		}
	}


	////////////////////////////////////////
	// Utilities scene view

	/// <summary>
	/// Get the position of the pointer on the grid
	/// </summary>
	/// <param name="offset_normal_factor"> Offset use to return the upper or lower cell</param>
	private Vector3 GetGridPositionInput(float offset_normal_factor)
	{
		Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
		Vector3 hitPoint;
		Vector3 enter = new Vector3(0.0f, 0.0f, 0.0f);

		_planesGrid[0].Raycast(ray, out enter.x);
		_planesGrid[1].Raycast(ray, out enter.y);
		_planesGrid[2].Raycast(ray, out enter.z);

		bool isPlaneCollided = enter != Vector3.zero;

		if (Physics.Raycast(ray, out RaycastHit hit, _dist_default_interaction * (_grid.SizeCell + 1)) && (!isPlaneCollided || (isPlaneCollided && (hit.distance < enter.x || hit.distance < enter.y || hit.distance < enter.z))))
		{
			hitPoint = hit.point;
			if (hit.collider.gameObject.GetComponent<Cell>() != null || hit.collider.GetComponentInParent<Cell>())
			{
				hitPoint = hitPoint + hit.normal * _grid.SizeCell * offset_normal_factor;
			}
		}
		else if (isPlaneCollided && (enter.x < _dist_default_interaction * _grid.SizeCell || enter.y < _dist_default_interaction * _grid.SizeCell || enter.z < _dist_default_interaction * _grid.SizeCell))
		{
			if(enter.y > enter.x && enter.z > enter.x)
				hitPoint = ray.GetPoint(enter.x);
			else if(enter.z > enter.y)
				hitPoint = ray.GetPoint(enter.y);
			else
				hitPoint = ray.GetPoint(enter.z);
		}
		else
		{
			hitPoint = ray.GetPoint(_dist_default_interaction * _grid.SizeCell);
		}

		return hitPoint;
	}

	private void SetBrushPosition(Vector3 position, bool erase = false, float rotation = 0)
	{
		int pallet = _pallet_index;

		if (erase)
			pallet = _brush.transform.childCount-1;

		for (int i = 0; i < _brush.transform.childCount; i++)
		{
			if (i == pallet)
				_brush.transform.GetChild(i).gameObject.SetActive(true);
			else
				_brush.transform.GetChild(i).gameObject.SetActive(false);
		}

		Vector3 pos = _grid.TransformPositionToGridPosition(position);
		if(FuncEditor.InputInGridBoundaries(_grid, pos, _size_grid) || !_painting)
			_brush.transform.position = pos;

		_brush.transform.eulerAngles = new Vector3(0, rotation, 0); 
	}

	#endregion

	////////////////////////////////////////

	#region MenusManagement

	private void OnGUI()
	{
		if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.R)
			_rotationCell = _rotationCell == 270 ? 0 : _rotationCell + 90;

		DrawMainMenu();
	}

	private void DrawMainMenu()
	{
		GUILayout.Label("Map Tile Grid Creator Settings", EditorStyles.boldLabel);

		DrawNewGridPanel();
		FuncEditor.DrawUILine(Color.gray);
		UpdateGridSelected(_grid);

		if (_grid != null)
		{
			EditorGUILayout.LabelField("Number of cells : " + _grid.GetNumberOfCells());
		}

		GUILayout.BeginHorizontal();

		if (GUILayout.Button("Load"))
		{
			string fullpath = EditorUtility.OpenFilePanel("File map load", "", "json");
			if (fullpath != "")
			{
				_grid = SaveLoadFileSystem.LoadRawJSON(fullpath);
			}

			Selection.SetActiveObjectWithContext(_grid, null);
		}

		if (_grid != null)
		{
			if (GUILayout.Button("Save"))
			{
				string fullpath = EditorUtility.SaveFilePanel("File map save", "", _grid.name, "json");
				if (fullpath != "")
				{
					SaveLoadFileSystem.SaveAsyncRawJSON(_grid, fullpath);
				}
			}
		}
		GUILayout.EndHorizontal();

		if (_grid != null)
		{
			var origFontStyle = EditorStyles.label.fontStyle;
			EditorStyles.label.fontStyle = FontStyle.Bold;

			int oldPathfindingType = _pathfindingCurrentType;
			_pathfindingCurrentType = EditorGUILayout.Popup("Pathfinding", _pathfindingCurrentType, _pathfindingTypes);

			if (oldPathfindingType != _pathfindingCurrentType)
			{
                if (_pathfindingCurrentType == 0 && _start_end[0] != Constants.UNDEFINED_POSITION)
					_cluster.FindPath(_start_end[0], _maxJump);
				if (_pathfindingCurrentType == 1 && _start_end[1] != Constants.UNDEFINED_POSITION)
					_cluster.FindPath(_start_end[0], _start_end[1], _maxJump);
			}

			_debug_grid = EditorGUILayout.Toggle("Debug grid", _debug_grid);

			if (_debug_grid)
			{
				EditorGUILayout.LabelField("Size grid", EditorStyles.boldLabel);
				EditorStyles.label.fontStyle = origFontStyle;
				_size_grid = EditorGUILayout.Vector3IntField("", _size_grid);
			}

			EditorGUILayout.LabelField("Max Jump", EditorStyles.boldLabel);
			_maxJump.x = EditorGUILayout.FloatField("Horizontal", _maxJump.x);
			_maxJump.y = EditorGUILayout.FloatField("Vertical", _maxJump.y);

			_coordinates.SetActive(_debug_grid);

			FuncEditor.DrawUILine(Color.gray);
			DrawEditor();
		}
	}

	private void DrawNewGridPanel()
	{
		FuncEditor.DrawUILine(Color.gray);


		if (GUILayout.Button("New"))
		{
			//Create Visualization object (Coordinates, Brush and ToolManager) if it doesn't exist
			if (!GameObject.Find("Visualization"))
				PrefabUtility.InstantiatePrefab(Resources.Load("Visualization"));

			_brush = GameObject.Find("Brush");
			_coordinates = GameObject.Find("Coordinates");

			//Destroy then create Cluster and Waypoints (contain waypoints) 
			_cluster = FuncEditor.CreateClusterAndWaypoints(_size_grid);

			//Destroy then create Grid and Cells (contain waypoints) 
			_grid = FuncEditor.CreateGridAndCells(_pallet, _size_grid);

			//Reset position for pathfinding
			_start_end[0] = Constants.UNDEFINED_POSITION;
			_start_end[1] = Constants.UNDEFINED_POSITION;
		}
	}

	private void DrawEditor()
	{
		DrawBrushPanel();
		FuncEditor.DrawUILine(Color.gray);
		DrawPanelPallet();
	}

	private void DrawBrushPanel()
	{
		GUILayout.BeginHorizontal();
		GUI.enabled = !_noUndo;
		if (GUILayout.Button(_undoIcon))
		{
			switch (_last_mode_paint)
			{
				case PaintMode.Single:
					{
						foreach (Vector3Int index in _lastIndexToPaint)
						{
							FuncEditor.ActivatePallet(_last_pallet_index, _grid, index, false);
						}
						_noUndo = true;
					}
					break;

				case PaintMode.Erase:
					{
						foreach (Vector3Int index in _lastIndexToPaint)
						{
							Cell cell = FuncEditor.CellAtThisIndex(_grid, index);
							FuncEditor.ActivatePallet(cell.lastPalletIndex, _grid, index, true);
							SetPathWaypoint(index);
							SetCellColliderState(index, true);
							ShowCell(index, true);
						}
						_noUndo = true;
					}
					break;

				case PaintMode.Eyedropper:
					{
						Vector3 input = GetGridPositionInput(-0.1f);
						Cell selected = _grid.TryGetCellByPosition(ref input);
						EyedropperInput(selected);
						if (selected != null)
						{
							SetBrushPosition(selected.transform.position, false);
						}
					}
					break;
			}
		}
		GUI.enabled = true;

		if (GUILayout.Button(_rotationIcon))
		{
			_rotationCell = _rotationCell == 270 ? 0 : _rotationCell + 90;
		}

		GUILayout.EndHorizontal();

		_mode_paint = (PaintMode)GUILayout.Toolbar((int)_mode_paint, _modes_paint);
	}

	private void DrawPanelPallet()
	{
		if (_pallet.Count == 0)
		{
			EditorGUILayout.HelpBox("No prefab founded for pallet.", MessageType.Warning);
		}
		else
		{
			List<GUIContent> palletIcons = new List<GUIContent>();
			foreach (GameObject prefab in _pallet)
			{
				Texture2D texture = AssetPreview.GetAssetPreview(prefab);
				GUIContent preview = new GUIContent(texture, prefab.name);
				palletIcons.Add(preview);
			}
			_scroll_position = GUILayout.BeginScrollView(_scroll_position);
			_pallet_index = GUILayout.SelectionGrid(_pallet_index, palletIcons.ToArray(), 2);
			GUILayout.EndScrollView();
		}
	}

	#endregion
}
