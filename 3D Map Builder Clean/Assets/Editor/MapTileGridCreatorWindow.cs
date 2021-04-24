using System.Collections.Generic;
using System.IO;
using MapTileGridCreator.Core;
using MapTileGridCreator.SerializeSystem;
using MapTileGridCreator.Utilities;
using UnityEditor;
using UnityEngine;
using System.Reflection;
using System.Threading;
using System.Diagnostics;

/// <summary>
/// Main window class.
/// </summary>
[CanEditMultipleObjects]
public class MapTileGridCreatorWindow : EditorWindow
{
	#region Variables

	//Global
	private Vector2 _scroll_position;
	private float progressBarTime = 5f;

	private Grid3D _grid;
	private Cell[,,] _cells;
	private List<Grid3D> _suggestionsGrid;
	private List<Cell[,,]> _suggestionCell;
	private List<Camera> _suggestionCamera;

	private Vector3Int _size_grid = new Vector3Int(5, 5, 5);
	private Vector3Int _old_size_grid = new Vector3Int(-1, -1, -1);
	private SuggestionsEditor[] suggWindow;
	private Thread newSuggestionsClustersThread;
	private bool newSuggestionsDone; 

	//Debug Grid
	[SerializeField]
	private bool _debug_grid = true;
	private GameObject _coordinates;

	private Plane[] _planesGrid = new Plane[3];

	//Waypoints
	private WaypointCluster _cluster;
	//Pathfinding
	private PathfindingState _pathfindingState;
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
	private Vector2 _rotationCell;
	private GUIContent _rotationIcon;

	[SerializeField]
	[Min(1)]
	private float _dist_default_interaction = 100.0f;
	[SerializeField]
	private string _path_palletPreview = "Assets/Cells/Pallets";
	private string _path_eraser= "Assets/Cells/Eraser.prefab";
	private string _path_setWaypoint = "Assets/Cells/SetWaypoint.prefab";
	private string _path_camera= "Assets/Cells/Camera.prefab";
	private GameObject _cameraPrefab;
	private int _cellTypes_index;
	private GameObject _erasePrefab;
	private GameObject _setWaypointPrefab;
	private Dictionary<CellInformation, GameObject> _cellPrefabs = new Dictionary<CellInformation, GameObject>();
	private List<CellInformation> _cellTypes = new List<CellInformation>();
	private List<bool> _cellTypesShow = new List<bool>();
	private List<bool> _oldCellTypesShow = new List<bool>();
	public Cell[,,] GetCells()
	{
		return _cells;
	}

	public WaypointCluster GetCluster()
	{
		return _cluster;
	}

	public void SetCluster(WaypointCluster newcluster)
	{
		_cluster = newcluster;
	}

	public List<Grid3D> GetSuggestionGrid()
	{
		return _suggestionsGrid;
	}

	public List<Cell[,,]> GetSuggestionCells()
	{
		return _suggestionCell;
	}

	public Dictionary<CellInformation, GameObject> GetCellPrefabs()
	{
		return _cellPrefabs;
	}
	#endregion

	[MenuItem("3D Map/MapTileGridCreator")]
	public static void OpenWindows()
	{
		MapTileGridCreatorWindow window = (MapTileGridCreatorWindow)GetWindow(typeof(MapTileGridCreatorWindow));
		window.Show();
	}

	[MenuItem("Shortcuts/Rotate Pallet around Y _r")]
	static void RotateRCommand()
	{
		MapTileGridCreatorWindow window = (MapTileGridCreatorWindow)GetWindow(typeof(MapTileGridCreatorWindow));
		window.RotateCell(1);
	}

	[MenuItem("Shortcuts/Rotate Pallet around X _e")]
	static void RotateECommand()
	{
		MapTileGridCreatorWindow window = (MapTileGridCreatorWindow)GetWindow(typeof(MapTileGridCreatorWindow));
		window.RotateCell(0);
	}

	private void OnEnable()
	{
		_modes_paint = new GUIContent[] {
			new GUIContent(EditorGUIUtility.IconContent("Grid.PaintTool", "Paint one by one the prefab selected")),
			new GUIContent(EditorGUIUtility.IconContent("Grid.EraserTool", "Erase the cell in scene view")),
			new GUIContent(EditorGUIUtility.IconContent("Grid.PickingTool", "Create waypoint for Pathfinding")),
			new GUIContent(EditorGUIUtility.IconContent("d_ToolHandleCenter@2x", "Eyedropper to auto select the corresponding prefab in pallete")) };

		_undoIcon = new GUIContent(EditorGUIUtility.IconContent("undoIcon", "Undo last Paint/Erase"));
		_rotationIcon = new GUIContent(EditorGUIUtility.IconContent("d_RotateTool On@2x", "Rotate pallet"));

		RefreshPallet();
	}

	private void OnFocus()
	{
		SceneView.duringSceneGui -= OnSceneGUI;
		SceneView.duringSceneGui += OnSceneGUI;
	}

	public void RefreshPallet()
	{
		_cellPrefabs.Clear();
		_cellTypes.Clear();
		_cellTypesShow.Clear();
		_oldCellTypesShow.Clear();

		string[] prefabFiles = Directory.GetFiles(_path_palletPreview, "*.prefab");
		foreach (string prefabFile in prefabFiles)
		{
			GameObject newPrefab = AssetDatabase.LoadAssetAtPath(prefabFile, typeof(GameObject)) as GameObject;
			_cellPrefabs[newPrefab.GetComponent<CellInformation>()] = newPrefab;
			_cellTypes.Add(newPrefab.GetComponent<CellInformation>());
			_cellTypesShow.Add(true);
			_oldCellTypesShow.Add(true);
		}

		_erasePrefab = AssetDatabase.LoadAssetAtPath(_path_eraser, typeof(GameObject)) as GameObject;
		_setWaypointPrefab = AssetDatabase.LoadAssetAtPath(_path_setWaypoint, typeof(GameObject)) as GameObject;
		_cameraPrefab = AssetDatabase.LoadAssetAtPath(_path_camera, typeof(GameObject)) as GameObject;
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
				UpdateCameraSuggestion(SceneView.lastActiveSceneView.rotation.eulerAngles, SceneView.lastActiveSceneView.cameraDistance);
				//Draw the drawing plans of the grid 
				FuncEditor.DebugSquareGrid(_grid, DebugsColor.grid_help, _size_grid, _planesGrid);
			}
			PaintEdit();
		}

		if (newSuggestionsClustersThread != null && !newSuggestionsClustersThread.IsAlive && !newSuggestionsDone)
		{
			newSuggestionsDone = true;
			suggWindow = (SuggestionsEditor[])Resources.FindObjectsOfTypeAll(typeof(SuggestionsEditor));
			if (suggWindow.Length != 0)
			{
				suggWindow[0].NewSuggestionsCells();
			}
		}

		HandleUtility.Repaint();
	}

	private void UpdateCameraSuggestion(Vector3 rot, float zoom)
    {
		for(int i =0; i < _suggestionsGrid.Count; i++)
		{
			_suggestionsGrid[i].transform.Find("Camera(Clone)").transform.localEulerAngles = rot;
			_suggestionsGrid[i].transform.Find("Camera(Clone)").GetComponent<Camera>().orthographicSize = zoom*0.5f;
		}
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
					ChangeBrushPallet();
					Vector3Int input = Vector3Int.RoundToInt(GetGridPositionInput(0.5f));
					if(FuncEditor.CanPaintHere(_size_grid , input, _cellTypes[_cellTypes_index].size, _cells, _rotationCell))
                    {
						if (_cellTypes[_cellTypes_index].size.x == 1 && _cellTypes[_cellTypes_index].size.y == 1 && _cellTypes[_cellTypes_index].size.z == 1)
							AddInput(input);
						else
							AddInputSolo(input);
					}

					SetBrushPosition(input);
					Paint();
				}
				break;

			case PaintMode.Erase:
				{
					ChangeBrushPallet();
					Vector3Int input = Vector3Int.RoundToInt(GetGridPositionInput(-0.5f));
					if(FuncEditor.InputInGridBoundaries(input, _size_grid))
						AddInput(input);
					SetBrushPosition(input);
					Paint();
				}
				break;

			case PaintMode.SetPathfindingWaypoint:
				{
					ChangeBrushPallet();
					Vector3Int input = Vector3Int.RoundToInt(GetGridPositionInput(0.5f));
					if (FuncEditor.InputInGridBoundaries(input, _size_grid))
						SetWaypoint(input);
					SetBrushPosition(input);
				}
				break;

			case PaintMode.Eyedropper:
				{
					Vector3 input = GetGridPositionInput(-0.1f);
					Cell selected = _grid.TryGetCellByPosition(ref input);
					EyedropperInput(selected);
					if (selected != null)
					{
						SetBrushPosition(selected.transform.position);
					}
				}
				break;
		}
	}

	private void RotateCell(int axe)
    {
		if (axe == 0)
			_rotationCell.x = _rotationCell.x == 270 ? 0 : _rotationCell.x + 90;
		if (axe == 1)
			_rotationCell.y = _rotationCell.y == 270 ? 0 : _rotationCell.y + 90;
		
	}

	#region Paint/Erase
	private void AddInput(Vector3Int input)
	{
		if (Event.current.type == EventType.Layout)
		{
			HandleUtility.AddDefaultControl(0);
		}

		List<Vector3Int> newIndexToPaint = new List<Vector3Int>();

		//Set starting index of paint/erase
		if (!_painting && Event.current.type == EventType.MouseDown && Event.current.button == 0)
		{
			_startingPaintIndex = input;
			_indexToPaint.Clear();
			_painting = true;
		}

		//Get all indexes that will be paint/erase
		if (_painting && (Event.current.type == EventType.MouseDrag || Event.current.type == EventType.MouseDown) && Event.current.button == 0)
		{
			newIndexToPaint.Clear();
			Vector3Int index;

			for (int i = 0; i <= Mathf.Abs(input.x - _startingPaintIndex.x); i++)
			{
				for (int j = 0; j <= Mathf.Abs(input.y - _startingPaintIndex.y); j++)
				{
					for (int k = 0; k <= Mathf.Abs(input.z - _startingPaintIndex.z); k++)
					{
						index = new Vector3Int(_startingPaintIndex.x + (int)Mathf.Sign(input.x - _startingPaintIndex.x) * i, _startingPaintIndex.y + (int)Mathf.Sign(input.y - _startingPaintIndex.y) * j, _startingPaintIndex.z + (int)Mathf.Sign(input.z - _startingPaintIndex.z) * k);
						//Add the index to the list of indexes to paint/erase

						if (((_mode_paint == PaintMode.Single && (_cells[index.x, index.y, index.z].state == CellState.Inactive || _cells[index.x, index.y, index.z].state == CellState.Painted)) || (_mode_paint == PaintMode.Erase && (_cells[index.x, index.y, index.z].state == CellState.Active || _cells[index.x, index.y, index.z].state == CellState.Erased))))
						{
							newIndexToPaint.Add(index);
						}
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
					if (_mode_paint == PaintMode.Single)
					{
						_indexToPaint.Add(newIndex);
						_cells[newIndex.x, newIndex.y, newIndex.z].Painted(_cellTypes[_cellTypes_index], _rotationCell);

						FuncEditor.SetType(_size_grid, _rotationCell, _cluster, _cells, _cellTypes[_cellTypes_index], newIndex);
						_cluster.SetRotation(_rotationCell, newIndex);

						if (_cellTypes[_cellTypes_index].wall)
						{
							_cells[newIndex.x, newIndex.y, newIndex.z].WallTransform(_cluster.GetWaypoints());
							if(newIndex.x > 0)
								_cells[newIndex.x-1, newIndex.y, newIndex.z].WallTransform(_cluster.GetWaypoints());
							if (newIndex.z > 0)
								_cells[newIndex.x, newIndex.y, newIndex.z-1].WallTransform(_cluster.GetWaypoints());
							if (newIndex.x < _size_grid.x-1)
								_cells[newIndex.x + 1, newIndex.y, newIndex.z].WallTransform(_cluster.GetWaypoints());
							if (newIndex.z < _size_grid.z - 1)
								_cells[newIndex.x, newIndex.y, newIndex.z+1].WallTransform(_cluster.GetWaypoints());
							if (newIndex.y > 0)
								_cells[newIndex.x, newIndex.y - 1, newIndex.z].WallTransform(_cluster.GetWaypoints());
						}
					}
					else if (_mode_paint == PaintMode.Erase && _cluster.GetWaypoints()[newIndex.x, newIndex.y, newIndex.z].type != null)
					{
						_indexToPaint.Add(newIndex);

						Vector3Int basePosition = _cluster.GetWaypoints()[newIndex.x, newIndex.y, newIndex.z].basePos;
						_cells[basePosition.x, basePosition.y, basePosition.z].Erased();
					}
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
                    {
						FuncEditor.RemoveType(_size_grid, _rotationCell, _cluster, _cells, _cellTypes[_cellTypes_index], currentIndex);
						_cells[currentIndex.x, currentIndex.y, currentIndex.z].Inactive();
					}
					if (_mode_paint == PaintMode.Erase)
						_cells[currentIndex.x, currentIndex.y, currentIndex.z].Active();

					if (_cells[currentIndex.x, currentIndex.y, currentIndex.z].lastType.wall)
					{
						_cells[currentIndex.x, currentIndex.y, currentIndex.z].WallTransform(_cluster.GetWaypoints());
						if (currentIndex.x > 0)
							_cells[currentIndex.x - 1, currentIndex.y, currentIndex.z].WallTransform(_cluster.GetWaypoints());
						if (currentIndex.z > 0)
							_cells[currentIndex.x, currentIndex.y, currentIndex.z - 1].WallTransform(_cluster.GetWaypoints());
						if (currentIndex.x < _size_grid.x - 1)
							_cells[currentIndex.x + 1, currentIndex.y, currentIndex.z].WallTransform(_cluster.GetWaypoints());
						if (currentIndex.z < _size_grid.z - 1)
							_cells[currentIndex.x, currentIndex.y, currentIndex.z + 1].WallTransform(_cluster.GetWaypoints());
						if (currentIndex.y > 0)
							_cells[currentIndex.x, currentIndex.y - 1, currentIndex.z].WallTransform(_cluster.GetWaypoints());
					}

					newIndexToPaint.Remove(currentIndex);
				}
			}

			_indexToPaint = newIndexToPaint;
		}
	}
	private void AddInputSolo(Vector3Int input)
	{
		//Set starting index of paint/erase
		if (!_painting && Event.current.type == EventType.MouseDown && Event.current.button == 0)
		{
			_indexToPaint.Clear();
			_painting = true;
			_indexToPaint.Add(input);
			_cells[input.x, input.y, input.z].Painted(_cellTypes[_cellTypes_index], _rotationCell);
			FuncEditor.SetType(_size_grid, _rotationCell, _cluster, _cells, _cellTypes[_cellTypes_index], input);
			_cluster.SetRotation(_rotationCell, input);
		}
	}

	private void SetWaypoint(Vector3Int input)
	{
		//Set waypoint
		if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
		{
			_cluster.SetPathfinding(input, _pathfindingState, _maxJump);
			FuncEditor.ShowPathfinding(_cells, _cluster.GetWaypoints(), _pathfindingState);
		}
	}

	public void Paint()
	{
		if (Event.current.type == EventType.Layout)
		{
			HandleUtility.AddDefaultControl(0);
		}

		if (!_painting && Event.current.type == EventType.MouseUp && Event.current.button == 0)
			UnityEngine.Debug.Log("Can't paint here!");

		if (_painting && Event.current.type == EventType.MouseUp && Event.current.button == 0)
		{
			foreach (Vector3Int index in _indexToPaint)
			{
				if (_mode_paint == PaintMode.Single)
				{
					_cells[index.x, index.y, index.z].Active();
				}

				if (_mode_paint == PaintMode.Erase) 
				{
					FuncEditor.RemoveType(_size_grid, _rotationCell, _cluster, _cells, _cellTypes[_cellTypes_index], index);

					if (_cells[index.x, index.y, index.z].lastType.wall)
					{
						_cells[index.x, index.y, index.z].WallTransform(_cluster.GetWaypoints());
						if (index.x > 0)
							_cells[index.x-1, index.y, index.z].WallTransform(_cluster.GetWaypoints());
						if (index.z > 0)
							_cells[index.x, index.y, index.z-1].WallTransform(_cluster.GetWaypoints());
						if (index.x < _size_grid.x - 1)
							_cells[index.x+1, index.y, index.z].WallTransform(_cluster.GetWaypoints());
						if (index.z < _size_grid.z - 1)
							_cells[index.x, index.y, index.z+1].WallTransform(_cluster.GetWaypoints());
						if (index.y > 0)
							_cells[index.x, index.y-1, index.z].WallTransform(_cluster.GetWaypoints());
					}

					_cells[index.x, index.y, index.z].Inactive();
				}
			}

			_undo = MyUndo.UpdateUndo(_undo, _indexToPaint, _mode_paint, _cellTypes_index);
			_painting = false;

			suggWindow = (SuggestionsEditor[])Resources.FindObjectsOfTypeAll(typeof(SuggestionsEditor));
			if (suggWindow.Length != 0)
            {
				suggWindow[0].NewSuggestionsClusters();
				newSuggestionsClustersThread = new Thread(NewSuggestionsIA);
				newSuggestionsClustersThread.Start();
				newSuggestionsDone = false;
			}
			//ClearLog();
		}
	}
	public void NewSuggestionsIA()
    {
		if (suggWindow.Length != 0)
		{
			Stopwatch stopWatch;
			stopWatch = new Stopwatch();
			stopWatch.Start();
			suggWindow[0].NewSuggestionsIA();
			stopWatch.Stop();
			//UnityEngine.Debug.Log("Time To Run IA " + stopWatch.ElapsedMilliseconds + "ms");
			stopWatch.Reset();
		}
	}
	public void ClearLog()
	{
		var assembly = Assembly.GetAssembly(typeof(UnityEditor.Editor));
		var type = assembly.GetType("UnityEditor.LogEntries");
		var method = type.GetMethod("Clear");
		method.Invoke(new object(), null);
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

			/*int newIndex = _cellPrefabs.FindIndex(x => x.Equals(prefab));
			if (newIndex >= 0)
			{
				_mode_paint = (int)PaintMode.Single;
				_cellTypes_index = newIndex;
				Debug.Log("Prefab " + prefab.name + " selected.");
			}
			else
			{
				Debug.LogError("Prefab is not from the pallet");
			}*/
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

	private void SetBrushPosition(Vector3 position)
	{
		Vector3 pos = _grid.TransformPositionToGridPosition(position);
		if(FuncEditor.InputInGridBoundaries(pos, _size_grid) || !_painting)
			_brush.transform.position = pos;

		_brush.transform.localEulerAngles = new Vector3(_rotationCell.x, _rotationCell.y, 0); 
	}

	#endregion

	////////////////////////////////////////

	#region MenusManagement

	private void OnGUI()
	{
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

			PathfindingState oldPathfindingType = _pathfindingState;
			string[] stringPath = System.Enum.GetNames(typeof(PathfindingState));
			_pathfindingState = (PathfindingState)EditorGUILayout.Popup("Pathfinding", (int)_pathfindingState, stringPath);

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
			if (_size_grid != _old_size_grid || _cells == null)
			{
				RefreshPallet();

				//Create Visualization object (Coordinates, Brush and ToolManager) if it doesn't exist
				if (!GameObject.Find("Visualization"))
					PrefabUtility.InstantiatePrefab(Resources.Load("Visualization"));

				_brush = GameObject.Find("Brush");
				_coordinates = GameObject.Find("Coordinates");

				//Destroy then create Grid and Cells with waypoints
				FuncEditor.DestroyGrids();

				FuncEditor.CreateCellsAndWaypoints(ref _grid, ref _cells, ref _cluster, _cellPrefabs, _size_grid);
				_grid.transform.position = new Vector3(0, 0, 0);
				suggWindow = (SuggestionsEditor[])Resources.FindObjectsOfTypeAll(typeof(SuggestionsEditor));

				_suggestionCell = new List<Cell[,,]>();
				_suggestionsGrid = new List<Grid3D>();
				_suggestionCamera = new List<Camera>();

				if (suggWindow.Length != 0)
				{
					for (int i = 0; i < suggWindow[0].numberSuggestions; i++)
					{
						WaypointCluster newCluster = _cluster;
						Cell[,,] newCells = _cells;
						Grid3D newGrid = _grid;
						GameObject newcameraObject = Instantiate<GameObject>(_cameraPrefab);
						FuncEditor.CreateCellsAndWaypoints(ref newGrid, ref newCells, ref newCluster, _cellPrefabs, _size_grid);
						_suggestionCell.Add(newCells);
						newGrid.transform.position = new Vector3(1000 * (i + 1), 1000 * (i + 1), 1000 * (i + 1));
						newcameraObject.transform.parent = newGrid.transform;
						newcameraObject.transform.localPosition = new Vector3(_size_grid.x / 2, _size_grid.y / 2, _size_grid.z / 2);
						//newcameraObject.GetComponent<Camera>().hideFlags = HideFlags.HideAndDontSave;
						newcameraObject.GetComponent<Camera>().farClipPlane = 50;
						newcameraObject.GetComponent<Camera>().nearClipPlane = -50;
						_suggestionsGrid.Add(newGrid);
					}
				}

				_old_size_grid = _size_grid;
			}
			else
			{
				FuncEditor.ResetCells(ref _cells, _old_size_grid);
				FuncEditor.ResetWaypoints(ref _cluster, _old_size_grid);

				if (suggWindow.Length != 0)
				{
					for (int i = 0; i < suggWindow[0].numberSuggestions; i++)
					{
						FuncEditor.TransformCellsFromWaypoints(_suggestionCell[i], _cluster.GetWaypoints());
					}
				}
			}
		}
		//Reset position for pathfinding
		_start_end[0] = Constants.UNDEFINED_POSITION;
		_start_end[1] = Constants.UNDEFINED_POSITION;

		_undo.noUndo = true;
		EditorUtility.ClearProgressBar();
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
							_cells[index.x, index.y, index.z].Inactive();
							FuncEditor.RemoveType(_size_grid, _rotationCell, _cluster, _cells, _cellTypes[_cellTypes_index], index);
						}
						_undo.noUndo = true;
					}
					break;

				case PaintMode.Erase:
					{
						foreach (Vector3Int index in _undo.lastIndexToPaint)
						{
							if (_cells[index.x, index.y, index.z].lastType != null)
							{
								_cells[index.x, index.y, index.z].Painted(_cells[index.x, index.y, index.z].lastType, _cells[index.x, index.y, index.z].lastRotation);
								_cells[index.x, index.y, index.z].Active();
								FuncEditor.SetType(_size_grid, _cells[index.x, index.y, index.z].lastRotation, _cluster, _cells, _cells[index.x, index.y, index.z].lastType, index);
								_cluster.SetRotation(_cells[index.x, index.y, index.z].lastRotation, index);
							}
						}
						_undo.noUndo = true;
					}
					break;

				case PaintMode.SetPathfindingWaypoint:
					{
						
					}
					break;

				case PaintMode.Eyedropper:
					{
						Vector3 input = GetGridPositionInput(-0.1f);
						Cell selected = _grid.TryGetCellByPosition(ref input);
						EyedropperInput(selected);
						if (selected != null)
						{
							SetBrushPosition(selected.transform.position);
						}
					}
					break;
			}
		}
		GUI.enabled = true;

		/*
		if (GUILayout.Button(_rotationIcon))
		{
			_rotationCell.x = _rotationCell.x == 270 ? 0 : _rotationCell.x + 90;
		}

		if (GUILayout.Button(_rotationIcon))
		{
			_rotationCell.y = _rotationCell.y == 270 ? 0 : _rotationCell.y + 90;
		}*/

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
			/*List<GUIContent> palletIcons = new List<GUIContent>();

			foreach (KeyValuePair<CellInformation, GameObject> prefab in _cellPrefabs)
			{
				Texture2D texture = AssetPreview.GetAssetPreview(prefab.Value);
				GUILayout.Button(texture);
				_debug_grid = EditorGUILayout.Toggle("Debug grid", _debug_grid);
				GUIContent preview = new GUIContent(texture, prefab.Value.name);
				palletIcons.Add(preview);
			}

			int oldCellType = _cellTypes_index;
			_scroll_position = GUILayout.BeginScrollView(_scroll_position);
			_cellTypes_index = GUILayout.SelectionGrid(_cellTypes_index, palletIcons.ToArray(), 2);
			GUILayout.EndScrollView();
			if (oldCellType != _cellTypes_index) ChangeBrushPallet();
			*/
			
			List<Texture> palletIcons = new List<Texture>();
			int oldCellType = _cellTypes_index;

			_scroll_position = GUILayout.BeginScrollView(_scroll_position);

			foreach (KeyValuePair<CellInformation, GameObject> prefab in _cellPrefabs)
			{
				palletIcons.Add(AssetPreview.GetAssetPreview(prefab.Value));
			}

			for(int i=0; i<palletIcons.Count; i += 2)
            {
				GUILayout.BeginHorizontal();
				if (GUILayout.Button(palletIcons[i])) { _cellTypes_index = i; _cellTypesShow[i] = true; _mode_paint = PaintMode.Single; };
				if(i+1 < palletIcons.Count)
					if (GUILayout.Button(palletIcons[i+1])) { _cellTypes_index = i + 1; _cellTypesShow[i+1] = true; _mode_paint = PaintMode.Single; };
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
				_cellTypesShow[i] =  EditorGUILayout.Toggle("Show/Hide", _cellTypesShow[i]);
				if (i + 1 < palletIcons.Count)
					_cellTypesShow[i+1] = EditorGUILayout.Toggle("Show/Hide", _cellTypesShow[i + 1]);
				GUILayout.EndHorizontal();
			}
			GUILayout.EndScrollView();

			for (int i = 0; i < palletIcons.Count; i++)
			{
				if (_oldCellTypesShow[i] != _cellTypesShow[i])
                {
					FuncEditor.SetShowTypeCell(_cellTypesShow[i], _cellTypes[i], _cluster, _cells);
					_oldCellTypesShow[i] = _cellTypesShow[i];
				}
			}

			if (oldCellType != _cellTypes_index) { ChangeBrushPallet(); };
		}
	}

	private void ChangeBrushPallet()
    {
		foreach (Transform child in _brush.transform)
		{
			DestroyImmediate(child.gameObject);
		}
		GameObject newBrushPallet;
		if (_mode_paint == PaintMode.Single)
			newBrushPallet = Instantiate<GameObject>(_cellPrefabs[_cellTypes[_cellTypes_index]]);
		else if (_mode_paint == PaintMode.Erase)
			newBrushPallet = Instantiate<GameObject>(_erasePrefab);
		else if(_mode_paint == PaintMode.SetPathfindingWaypoint)
			newBrushPallet = Instantiate<GameObject>(_setWaypointPrefab);
		else
			newBrushPallet = new GameObject();

		newBrushPallet.transform.parent = _brush.transform;
		newBrushPallet.transform.localPosition = new Vector3(0, 0, 0);
		newBrushPallet.transform.localEulerAngles = new Vector3(0, 0, 0);
	}
	#endregion
}
