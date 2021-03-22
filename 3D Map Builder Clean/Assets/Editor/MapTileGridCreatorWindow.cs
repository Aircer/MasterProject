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

	//Global
	private Vector2 _scroll_position;

	private Grid3D _grid;
	public Grid3D _suggestionsGrid { get; set; }
	public Dictionary<Vector3Int, Cell> _suggestionCell { get; set; }
	public WaypointCluster _suggestionCluster { get; set; }
	[SerializeField]
	private Vector3Int _size_grid = new Vector3Int(5, 5, 5);
	public Dictionary<Vector3Int, Cell> _cells { get; set; }

	//Debug Grid
	[SerializeField]
	private bool _debug_grid = true;
	private GameObject _coordinates;

	private Plane[] _planesGrid = new Plane[3];

	//Waypoints
	public WaypointCluster _cluster { get; set; }

	//Pathfinding
	private string[] _pathfindingTypes = new string[] { "Flood", "A*" };
	private int _pathfindingCurrentType;
	private Vector3Int[] _start_end = new Vector3Int[2];
	private Vector2 _maxJump;

	//Undo
	private GUIContent _undoIcon;
	private MyUndo _undo = new MyUndo();

	//Paint
	private GUIContent[] _modes_paint;
	private PaintMode _mode_paint;
	private static bool _painting = false;
	private Vector3Int _startingPaintIndex;
	private List<Vector3Int> _indexToPaint = new List<Vector3Int>();
	private GameObject _brush;
	private float _rotationCell = 0;
	private GUIContent _rotationIcon;

	[SerializeField]
	[Min(1)]
	private float _dist_default_interaction = 100.0f;
	[SerializeField]
	private string _path_palletPreview = "Assets/Cells/Cubes";
	private string _path_pallet = "Assets/Cells/Pallet";
	private int _cellTypes_index;
	private List<GameObject> _cellPrefabs = new List<GameObject>();
	private GameObject _cellPrefab;
	private List<CellInformation> _cellTypes = new List<CellInformation>();

	#endregion

	[MenuItem("3D Map/MapTileGridCreator")]
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
		_cellPrefabs.Clear();
		string[] prefabFiles = Directory.GetFiles(_path_palletPreview, "*.prefab");
		foreach (string prefabFile in prefabFiles)
		{
			GameObject newPrefab = AssetDatabase.LoadAssetAtPath(prefabFile, typeof(GameObject)) as GameObject;
			_cellPrefabs.Add(newPrefab);
			_cellTypes.Add(newPrefab.GetComponent<CellInformation>());
		}

		string[] prefabPalletFile = Directory.GetFiles(_path_pallet, "*.prefab");
		_cellPrefab = AssetDatabase.LoadAssetAtPath(prefabPalletFile[0], typeof(GameObject)) as GameObject;
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
					PaintInput(input, _rotationCell);
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

		//If input is outside the Grid do not paint/erase
		if (FuncEditor.InputInGridBoundaries(_grid, inputIndex, _size_grid))
		{
			List<Vector3Int> newIndexToPaint = new List<Vector3Int>();

			if (Event.current.type == EventType.Layout)
			{
				HandleUtility.AddDefaultControl(0);
			}

			//Set starting index of paint/erase
			if (!_painting && Event.current.type == EventType.MouseDown && Event.current.button == 0)
			{
				_startingPaintIndex = inputIndex;
				_indexToPaint.Clear();
				_painting = true;
			}

			//Get all indexes that will be paint/erase
			if (_painting && Event.current.type == EventType.MouseDrag && Event.current.button == 0)
			{
				newIndexToPaint.Clear();
				Vector3Int index;
				for (int i = 0; i <= Mathf.Abs(inputIndex.x - _startingPaintIndex.x); i++)
				{
					for (int j = 0; j <= Mathf.Abs(inputIndex.y - _startingPaintIndex.y); j++)
					{
						for (int k = 0; k <= Mathf.Abs(inputIndex.z - _startingPaintIndex.z); k++)
						{
							index = new Vector3Int(_startingPaintIndex.x + (int)Mathf.Sign(inputIndex.x - _startingPaintIndex.x) * i, _startingPaintIndex.y + (int)Mathf.Sign(inputIndex.y - _startingPaintIndex.y) * j, _startingPaintIndex.z + (int)Mathf.Sign(inputIndex.z - _startingPaintIndex.z) * k);
							//Add the index to the list of indexes to paint/erase
							if (_cells.ContainsKey(index) && ((_mode_paint == PaintMode.Single && (_cells[index].state == CellState.Inactive || _cells[index].state == CellState.Painted)) || (_mode_paint == PaintMode.Erase && (_cells[index].state == CellState.Active || _cells[index].state == CellState.Erased))))
								newIndexToPaint.Add(index);
						}
					}
				}

				//Paint/erase all indexes that have not be already painted/erased
				foreach (Vector3Int newIndex in newIndexToPaint)
				{
					bool indexExist = false;
					foreach (Vector3Int currentIndex in _indexToPaint)
					{
						if (newIndex == currentIndex)
							indexExist = true;
					}

					if (!indexExist)
					{
						_indexToPaint.Add(newIndex);
						if (_mode_paint == PaintMode.Single)
							_cells[newIndex].Painted(_cellTypes[_cellTypes_index], rotation);
						if (_mode_paint == PaintMode.Erase) 
							_cells[newIndex].Erased();
					}
				}

				//Remove indexes no more painted/erased
				foreach (Vector3Int currentIndex in _indexToPaint)
				{
					bool indexNoMorePainted = true;
					foreach (Vector3Int newIndex in newIndexToPaint)
					{
						if (newIndex == currentIndex)
							indexNoMorePainted = false;
					}

					if (indexNoMorePainted)
					{
						if (_mode_paint == PaintMode.Single)
							_cells[currentIndex].Inactive();
						if (_mode_paint == PaintMode.Erase)
							_cells[currentIndex].Active();
						newIndexToPaint.Remove(currentIndex);
					}
				}

				_indexToPaint = newIndexToPaint;
			}
		}

		if (_painting && Event.current.type == EventType.MouseUp && Event.current.button == 0)
		{
			_painting = false;

			if (_cells.ContainsKey(inputIndex) && !_indexToPaint.Contains(inputIndex))
			{
				_indexToPaint.Add(inputIndex);
				if (_mode_paint == PaintMode.Single)
					_cells[inputIndex].Painted(_cellTypes[_cellTypes_index], rotation);
			}

			foreach (Vector3Int index in _indexToPaint)
			{
				if (_mode_paint == PaintMode.Single)
				{
					_cells[index].Active();
					SetPathWaypoint(index);
                }

				if (_mode_paint == PaintMode.Erase) 
				{
					_cells[index].Inactive();
					RemovePathWaypoint(index);
				}
			}

			if (_start_end[1] != Constants.UNDEFINED_POSITION)
				_cluster.FindPath(_start_end[0], _start_end[1], _maxJump);
			if (_start_end[0] != Constants.UNDEFINED_POSITION && _pathfindingCurrentType == 0)
				_cluster.FindPath(_start_end[0], _maxJump);

			_undo = MyUndo.UpdateUndo(_undo, _indexToPaint, _mode_paint, _cellTypes_index);
		}
	}

	private void SetPathWaypoint(Vector3Int index)
	{
		Cell cell = FuncEditor.CellAtThisIndex(_grid, index);
		if (cell && cell.type)
		{
			if (cell.type.ID == "Start_End")
			{
				if (_start_end[0] == Constants.UNDEFINED_POSITION || _pathfindingCurrentType == 0)
                {
					if (_cells.ContainsKey(_start_end[0]))
						_cells[_start_end[0]].Inactive();
					if (_cells.ContainsKey(_start_end[1]))
						_cells[_start_end[1]].Inactive();

					_start_end[0] = index;
					_cells[_start_end[0]].Active();
					_cells[_start_end[0]].SetColor("start");
				}
				else
				{
					if (_start_end[1] != index && _start_end[1] != Constants.UNDEFINED_POSITION)
					{
						_cells[_start_end[1]].Inactive();
					}

					_start_end[1] = index;
					_cells[_start_end[1]].SetColor("end");
				}
			}
		}
	}

	private void RemovePathWaypoint(Vector3Int index)
	{
		if (_cells[index].type.ID == "Start_End")
		{
			if (_start_end[0] == index)
			{
				_start_end[0] = _start_end[1];
				_start_end[1] = Constants.UNDEFINED_POSITION;
				if(_cells.ContainsKey(_start_end[0]))
					_cells[_start_end[0]].SetColor("start");
			}
			else
			{
				_start_end[1] = Constants.UNDEFINED_POSITION;
			}
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

			int newIndex = _cellPrefabs.FindIndex(x => x.Equals(prefab));
			if (newIndex >= 0)
			{
				_mode_paint = (int)PaintMode.Single;
				_cellTypes_index = newIndex;
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
		int pallet = _cellTypes_index;

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

			//Destroy then create Grid and Cells with waypoints
			FuncEditor.DestroyGrids();

			Grid3D newGrid;
			Dictionary<Vector3Int, Cell> newCells;
			WaypointCluster newCluster;

			FuncEditor.CreateCells(out newGrid, out newCells, out newCluster, _cellPrefabs, _size_grid);

			_grid = newGrid;
			_cells = newCells;
			_cluster = newCluster;

			FuncEditor.CreateCells(out newGrid, out newCells, out newCluster, _cellPrefabs, _size_grid);
			newGrid.transform.position = new Vector3(1000, 1000, 1000);

			_suggestionsGrid = newGrid;
			_suggestionCell = newCells;
			_suggestionCluster = newCluster;

			//Reset position for pathfinding
			_start_end[0] = Constants.UNDEFINED_POSITION;
			_start_end[1] = Constants.UNDEFINED_POSITION;

			_undo.noUndo = true;
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
		GUI.enabled = !_undo.noUndo;
		if (GUILayout.Button(_undoIcon))
		{
			switch (_undo.last_mode_paint)
			{
				case PaintMode.Single:
					{
						foreach (Vector3Int index in _undo.lastIndexToPaint)
						{
							_cells[index].Inactive();
						}
						_undo.noUndo = true;
					}
					break;

				case PaintMode.Erase:
					{
						foreach (Vector3Int index in _undo.lastIndexToPaint)
						{
							_cells[index].Active();
							//SetPathWaypoint(index);
						}
						_undo.noUndo = true;
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
		if (_cellPrefabs.Count == 0)
		{
			EditorGUILayout.HelpBox("No prefab founded for pallet.", MessageType.Warning);
		}
		else
		{
			List<GUIContent> palletIcons = new List<GUIContent>();
			foreach (GameObject prefab in _cellPrefabs)
			{
				Texture2D texture = AssetPreview.GetAssetPreview(prefab);
				GUIContent preview = new GUIContent(texture, prefab.name);
				palletIcons.Add(preview);
			}
			_scroll_position = GUILayout.BeginScrollView(_scroll_position);
			_cellTypes_index = GUILayout.SelectionGrid(_cellTypes_index, palletIcons.ToArray(), 2);
			GUILayout.EndScrollView();
		}
	}

	#endregion
}
