using System.Collections.Generic;
using System.IO;
using MapTileGridCreator.Core;
using MapTileGridCreator.UtilitiesMain;
using EditorMain;
using MapTileGridCreator.Paint;
using UnityEditor;
using UnityEngine;
using System.Reflection;
using System.Threading;
using System.Diagnostics;
using MapTileGridCreator.SerializeSystem;

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
	private List<Grid3D> _suggestionsGrid;

	private Vector3Int _size_grid = new Vector3Int(5, 5, 5);
	public Vector3Int minVal = new Vector3Int(0, 0, 0);
	public Vector3Int maxVal = new Vector3Int(5, 5, 5);
	private SuggestionsEditor[] suggWindow;
	private Thread newSuggestionsClustersThread;
	private bool newSuggestionsDone;

	//Debug Grid
	[SerializeField]
	public Plane[] _planesGrid = new Plane[3];

	//Paint
	private GUIContent[] _modes_paint;
	private PaintMode _mode_paint;
	private static bool _painting = false;
	private Vector3Int _startingPaintIndex;
	private HashSet<Vector3Int> _indexToPaint = new HashSet<Vector3Int>();
	private GameObject _brush;

	[SerializeField]
	[Min(1)]
	private float _dist_default_interaction = 100.0f;
	[SerializeField]
	private string _path_palletPreview = "Assets/Cells/Pallets";
	private string _path_camera = "Assets/Cells/Camera.prefab";
	private GameObject _suggestionsCameraPrefab;
	private int _cellTypes_index;
	private Dictionary<CellInformation, GameObject> _cellPrefabs = new Dictionary<CellInformation, GameObject>();
	private GameObject palletObject;
	private List<CellInformation> _cellTypes = new List<CellInformation>();
	private List<bool> _cellTypesShow = new List<bool>();
	private List<bool> _oldCellTypesShow = new List<bool>();
	private GameObject emptyCellObj;
	private bool autoRefreshSuggestions;

	public Grid3D GetGrid()
	{
		return _grid;
	}

	public List<Grid3D> GetSuggestionGrid()
	{
		return _suggestionsGrid;
	}

	public List<CellInformation> GetCellInfos()
	{
		return _cellTypes;
	}
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
			new GUIContent(EditorGUIUtility.IconContent("Grid.EraserTool", "Erase the cell in scene view"))};

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
		if (emptyCellObj != null)
			DestroyImmediate(emptyCellObj);
		emptyCellObj = new GameObject();
		CellInformation emptyCell = emptyCellObj.AddComponent<CellInformation>();
		emptyCell.SetEmpty();
		_cellTypes.Add(emptyCell);
		_cellTypesShow.Clear();
		_oldCellTypesShow.Clear();

		string[] prefabFiles = Directory.GetFiles(_path_palletPreview, "*.prefab");
		DestroyImmediate(palletObject);
		palletObject = new GameObject();
		palletObject.SetActive(false);

		foreach (string pF in prefabFiles)
		{
			GameObject newObject = AssetDatabase.LoadAssetAtPath(pF, typeof(GameObject)) as GameObject;

			_cellPrefabs[newObject.GetComponent<CellInformation>()] = newObject.gameObject;
			_cellTypes.Add(newObject.GetComponent<CellInformation>());
			_cellTypesShow.Add(true);
			_oldCellTypesShow.Add(true);

			GameObject newChild = PrefabUtility.InstantiatePrefab(newObject) as GameObject;
			PrefabUtility.UnpackPrefabInstance(newChild, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
			newChild.transform.parent = palletObject.transform;
		}

		_suggestionsCameraPrefab = AssetDatabase.LoadAssetAtPath(_path_camera, typeof(GameObject)) as GameObject;
	}

    #region SceneManagement

    private void OnSceneGUI(SceneView sceneView)
	{
		if (_grid != null)
		{
			//Set the planes of the grid position and normal
			_planesGrid = FuncMain.SetGridDebugPlanesNormalAndPosition(_planesGrid, _size_grid, SceneView.lastActiveSceneView.rotation.eulerAngles);
			UpdateCameraSuggestion(SceneView.lastActiveSceneView.rotation.eulerAngles, SceneView.lastActiveSceneView.cameraDistance);
			//Draw the drawing plans of the grid 
			FuncMain.DebugSquareGrid(_grid, _size_grid, _planesGrid, maxVal);
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

		for (int i = 0; i < _suggestionsGrid.Count; i++)
		{
			_suggestionsGrid[i].transform.Find("Camera(Clone)").transform.localEulerAngles = rot;
			_suggestionsGrid[i].transform.Find("Camera(Clone)").GetComponent<Camera>().orthographicSize = zoom * 0.5f;
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
					Vector3Int input = Vector3Int.RoundToInt(GetGridPositionInput(0.5f));

					if (input.x >= 0 && input.y >= 0 && input.z >= 0 && input.x < maxVal.x && input.y < maxVal.y && input.z < maxVal.z)
					{
							_indexToPaint = IndexesToPaint.AddInput(_size_grid, input, _mode_paint, _cellTypes[_cellTypes_index], 
														ref _grid);
					}

					SetBrushPosition(input);

					if (IndexesToPaint.Paint(_size_grid, input, _mode_paint, _cellTypes[_cellTypes_index],
														 ref _grid) && autoRefreshSuggestions)
						StartThreadNewSuggestionsIA();
				}
				break;

			case PaintMode.Erase:
				{
					Vector3Int input = Vector3Int.RoundToInt(GetGridPositionInput(-0.5f));

					if(FuncMain.InputInGridBoundaries(input, _size_grid))
						_indexToPaint = IndexesToPaint.AddInput(_size_grid, input, _mode_paint, _cellTypes[_cellTypes_index],
							ref _grid);

					SetBrushPosition(input);
					if (IndexesToPaint.Paint(_size_grid, input, _mode_paint, _cellTypes[_cellTypes_index],
														 ref _grid) && autoRefreshSuggestions)
						StartThreadNewSuggestionsIA();
				}
				break;
		}
	}

	public void StartThreadNewSuggestionsIA()
	{
		if (newSuggestionsClustersThread != null)
			newSuggestionsClustersThread.Abort();

		suggWindow = (SuggestionsEditor[])Resources.FindObjectsOfTypeAll(typeof(SuggestionsEditor));
		if (suggWindow.Length != 0)
		{
			suggWindow[0].NewSuggestionsInt();
			newSuggestionsClustersThread = new Thread(NewSuggestionsIA);
			newSuggestionsClustersThread.Priority = System.Threading.ThreadPriority.Highest;
			newSuggestionsClustersThread.Start();
			newSuggestionsDone = false;
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
			UnityEngine.Debug.Log("Time To Run IA " + stopWatch.ElapsedMilliseconds + "ms");
			stopWatch.Reset();
		}
	}
	
	#endregion

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

		if (Physics.Raycast(ray, out RaycastHit hit, _dist_default_interaction * 2) && (!isPlaneCollided || (isPlaneCollided && (hit.distance < enter.x || hit.distance < enter.y || hit.distance < enter.z))))
		{
			hitPoint = hit.point;
			if (hit.collider.gameObject.GetComponent<Cell>() != null || hit.collider.GetComponentInParent<Cell>())
			{
				hitPoint = hitPoint + hit.normal * offset_normal_factor;
			}
		}
		else if (isPlaneCollided && (enter.x < _dist_default_interaction || enter.y < _dist_default_interaction || enter.z < _dist_default_interaction))
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
			hitPoint = ray.GetPoint(_dist_default_interaction);
		}

		return hitPoint;
	}

	private void SetBrushPosition(Vector3 position)
	{
		Vector3 pos = _grid.TransformPositionToGridPosition(position);
		if(FuncMain.InputInGridBoundaries(pos, _size_grid) || !_painting)
			_brush.transform.position = pos;
	}

	////////////////////////////////////////

	#region MenusManagement

	private void OnGUI()
	{
		if(_grid == null)
			DrawMainStartingMenu();
		else
			DrawMainMenu();
	}

	private void DrawMainMenu()
	{
		GUILayout.Label("Main Editor", EditorStyles.boldLabel);

		GUILayout.BeginHorizontal();
		if (GUILayout.Button("Change Grid"))
		{
			DestroyImmediate(_grid.gameObject);
			foreach(Grid3D grid in _suggestionsGrid)
            {
				DestroyImmediate(grid.gameObject);
            }
		}

		if (GUILayout.Button("Reset"))
		{
			RefreshPallet();

			CreateBrushAndVisualization();
			_mode_paint = PaintMode.Single;
			ChangeBrushPallet();

			_grid.ResetCells();

			foreach (Grid3D grid in _suggestionsGrid)
			{
				grid.ResetCells();
			}
		}
		GUILayout.EndHorizontal();
		FuncMain.DrawUILine(Color.gray);

		if (GUILayout.Button("Show/Hide Suggestions"))
		{
			suggWindow = (SuggestionsEditor[])Resources.FindObjectsOfTypeAll(typeof(SuggestionsEditor));

			if (suggWindow.Length > 0)
			{
				foreach (SuggestionsEditor suggEditor in suggWindow)
				{
					suggEditor.Close();
				}
			}
			else
			{
				SuggestionsEditor suggWindow = ScriptableObject.CreateInstance<SuggestionsEditor>();
				suggWindow.OpenSuggestions();
			}
		}

		if (GUILayout.Button("Refresh Suggestions"))
		{
			var assembly = Assembly.GetAssembly(typeof(UnityEditor.Editor));
			var type = assembly.GetType("UnityEditor.LogEntries");
			var method = type.GetMethod("Clear");
			method.Invoke(new object(), null);

			if (newSuggestionsClustersThread != null)
				newSuggestionsClustersThread.Abort();

			suggWindow = (SuggestionsEditor[])Resources.FindObjectsOfTypeAll(typeof(SuggestionsEditor));
			if (suggWindow.Length != 0)
			{
				suggWindow[0].NewSuggestionsInt();
				newSuggestionsClustersThread = new Thread(NewSuggestionsIA);
				newSuggestionsClustersThread.Priority = System.Threading.ThreadPriority.Highest;
				newSuggestionsClustersThread.Start();
				newSuggestionsDone = false;
			}
		}

		GUILayout.BeginHorizontal();
		EditorGUIUtility.labelWidth = 30;
		EditorGUILayout.LabelField("Size : " + _size_grid.x + " x " + _size_grid.y + " x " + _size_grid.z);
		EditorGUILayout.LabelField("Number of cells : " + _size_grid.x * _size_grid.y * _size_grid.z);
		GUILayout.EndHorizontal();

		var origFontStyle = EditorStyles.label.fontStyle;
		EditorStyles.label.fontStyle = FontStyle.Bold;

		EditorGUILayout.LabelField("Hide", EditorStyles.boldLabel);
		GUILayout.BeginHorizontal();
		Vector3Int oldMaxVal = maxVal;
		maxVal.x = EditorGUILayout.IntSlider(maxVal.x, 0, _size_grid.x);
		maxVal.y = EditorGUILayout.IntSlider(maxVal.y, 0, _size_grid.y);
		maxVal.z = EditorGUILayout.IntSlider(maxVal.z, 0, _size_grid.z);
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("AutoRefresh Suggestions", EditorStyles.boldLabel);
		autoRefreshSuggestions = EditorGUILayout.Toggle(autoRefreshSuggestions);
		GUILayout.EndHorizontal();

		if (maxVal != oldMaxVal)
		{
			_grid.SetShowLayersCell(maxVal);

			suggWindow = (SuggestionsEditor[])Resources.FindObjectsOfTypeAll(typeof(SuggestionsEditor));

			if (suggWindow.Length != 0)
			{
				for (int i = 0; i < suggWindow[0].numberSuggestions; i++)
				{
					_suggestionsGrid[i].SetShowLayersCell(maxVal);
				}
			}
		}

		FuncMain.DrawUILine(Color.gray);
		DrawEditor();
		FuncMain.DrawUILine(Color.gray);
		if (GUILayout.Button("Save"))
		{
			string fullpath = EditorUtility.SaveFilePanel("File map save", "", _grid.name, "json");
			if (fullpath != "")
			{
				SaveLoadFileSystem.SaveAsyncRawJSON(_grid, fullpath);
			}
		}
	}

	private void DrawMainStartingMenu()
	{
		suggWindow = (SuggestionsEditor[])Resources.FindObjectsOfTypeAll(typeof(SuggestionsEditor));

		foreach(SuggestionsEditor suggEditor in suggWindow)
        {
			suggEditor.Close();
        }

		if (emptyCellObj != null)
			DestroyImmediate(emptyCellObj);

		if (_brush != null)
			DestroyImmediate(_brush);

		autoRefreshSuggestions = true;

		GUILayout.Label("Main Editor", EditorStyles.boldLabel);

		FuncMain.DrawUILine(Color.gray);

		if (GUILayout.Button("New Grid : 5 x 5 x 5 "))
		{
			RefreshPallet();

			CreateBrushAndVisualization();
			_mode_paint = PaintMode.Single;
			_cellTypes_index = 1;
			ChangeBrushPallet();

			SuggestionsEditor suggWindow = ScriptableObject.CreateInstance<SuggestionsEditor>();
			suggWindow.OpenSuggestions();
			_size_grid = new Vector3Int(5, 5, 5);
			CreateGrids();
		}

		if (GUILayout.Button("New Grid : 10 x 6 x 6 "))
		{
			RefreshPallet();

			CreateBrushAndVisualization();
			_mode_paint = PaintMode.Single;
			_cellTypes_index = 1;
			ChangeBrushPallet();

			SuggestionsEditor suggWindow = ScriptableObject.CreateInstance<SuggestionsEditor>();
			suggWindow.OpenSuggestions();
			_size_grid = new Vector3Int(10, 6, 6);
			CreateGrids();
		}

		if (GUILayout.Button("New Grid : 5 x 10 x 5 "))
		{
			RefreshPallet();

			CreateBrushAndVisualization();
			_mode_paint = PaintMode.Single;
			_cellTypes_index = 1;
			ChangeBrushPallet();

			SuggestionsEditor suggWindow = ScriptableObject.CreateInstance<SuggestionsEditor>();
			suggWindow.OpenSuggestions();
			_size_grid = new Vector3Int(5, 10, 5);
			CreateGrids();
		}

		if (GUILayout.Button("New Grid : 7 x 7 x 7 "))
		{
			RefreshPallet();

			CreateBrushAndVisualization();
			_mode_paint = PaintMode.Single;
			_cellTypes_index = 1;
			ChangeBrushPallet();

			SuggestionsEditor suggWindow = ScriptableObject.CreateInstance<SuggestionsEditor>();
			suggWindow.OpenSuggestions();
			_size_grid = new Vector3Int(7, 7, 7);
			CreateGrids();
		}

		if (GUILayout.Button("New Grid : 6 x 8 x 6 "))
		{
			RefreshPallet();

			CreateBrushAndVisualization();
			_mode_paint = PaintMode.Single;
			_cellTypes_index = 1;
			ChangeBrushPallet();

			SuggestionsEditor suggWindow = ScriptableObject.CreateInstance<SuggestionsEditor>();
			suggWindow.OpenSuggestions();
			_size_grid = new Vector3Int(6, 8, 6);
			CreateGrids();
		}

		if (GUILayout.Button("Load"))
		{
			string fullpath = EditorUtility.OpenFilePanel("File map load", "", "json");
			if (fullpath != "")
			{
				RefreshPallet();

				CreateBrushAndVisualization();
				_mode_paint = PaintMode.Single;
				_cellTypes_index = 1;
				ChangeBrushPallet();

				//Destroy then create Grid and Cells with waypoints
				FuncMain.DestroyGrids();

				if (_grid != null)
					DestroyImmediate(_grid.gameObject);
				_grid = SaveLoadFileSystem.LoadRawJSON(fullpath, _cellTypes, _cellPrefabs, palletObject);

				_size_grid = _grid.size;

				SuggestionsEditor suggWindow = ScriptableObject.CreateInstance<SuggestionsEditor>();
				suggWindow.OpenSuggestions();

				CreateSuggestionsGrids();
				maxVal = new Vector3Int(_size_grid.x, _size_grid.y, _size_grid.z);
			}
		}

		Selection.SetActiveObjectWithContext(_grid, null);
		EditorUtility.ClearProgressBar();
	}

    private void CreateGrids()
	{
		//Destroy then create Grid and Cells with waypoints
		FuncMain.DestroyGrids();

		CreateMainGrid();
		CreateSuggestionsGrids();

		maxVal = new Vector3Int(_size_grid.x, _size_grid.y, _size_grid.z);
	}

	private void CreateMainGrid()
	{
		FuncMain.CreateCells(ref _grid, _cellTypes, _cellPrefabs, _size_grid, palletObject);
		_grid.transform.position = new Vector3(0, 0, 0);
	}

	private void CreateSuggestionsGrids()
	{
		suggWindow = (SuggestionsEditor[])Resources.FindObjectsOfTypeAll(typeof(SuggestionsEditor));

		_suggestionsGrid = new List<Grid3D>();

		if (suggWindow.Length != 0)
		{
			for (int i = 0; i < suggWindow[0].numberSuggestions; i++)
			{
				Cell[,,] newCells = new Cell[_size_grid.x, _size_grid.y, _size_grid.z];
				Grid3D newGrid = _grid;
				GameObject newcameraObject = Instantiate<GameObject>(_suggestionsCameraPrefab);
				FuncMain.CreateCells(ref newGrid, _cellTypes, _cellPrefabs, _size_grid, palletObject);
				newGrid.transform.position = new Vector3(1000 * (i + 1), 1000 * (i + 1), 1000 * (i + 1));
				newcameraObject.transform.parent = newGrid.transform;
				newcameraObject.transform.localPosition = new Vector3(_size_grid.x / 2, _size_grid.y / 2, _size_grid.z / 2);

				Camera cam = newcameraObject.GetComponent<Camera>();

				cam.hideFlags = HideFlags.HideAndDontSave;
				cam.farClipPlane = 50;
				cam.nearClipPlane = -50;
				cam.depth = -10f;
				_suggestionsGrid.Add(newGrid);
			}
		}
	}

	private void CreateBrushAndVisualization()
	{
		GameObject newChildVisualization;

		if (GameObject.Find("Visualization"))
			DestroyImmediate(GameObject.Find("Visualization"));

		newChildVisualization = PrefabUtility.InstantiatePrefab(Resources.Load("Visualization")) as GameObject;
		PrefabUtility.UnpackPrefabInstance(newChildVisualization, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
		_brush = GameObject.Find("Brush");

		foreach (Transform child in palletObject.transform)
		{
			GameObject newObject = Instantiate(child.gameObject);
			newObject.transform.name = newObject.transform.name.Replace("(Clone)", "").Trim();
			newObject.transform.parent = _brush.transform;
			newObject.transform.localPosition = new Vector3(0, 0, 0);
		}
	}

	private void DrawEditor()
	{
		DrawBrushPanel();
		FuncMain.DrawUILine(Color.gray);
		DrawPanelPallet();
	}

	private void DrawBrushPanel()
	{
		
		GUI.enabled = true;

		PaintMode old_mode_paint = _mode_paint;
		_mode_paint = (PaintMode)GUILayout.Toolbar((int)_mode_paint, _modes_paint);

		if (old_mode_paint != _mode_paint) { ChangeBrushPallet(); };
	}

	private void DrawPanelPallet()
	{
		if (_cellPrefabs.Count == 0)
		{
			EditorGUILayout.HelpBox("No prefab founded for pallet.", MessageType.Warning);
		}
		else
		{			
			List<Texture> palletIcons = new List<Texture>();

			_scroll_position = GUILayout.BeginScrollView(_scroll_position);

			foreach (KeyValuePair<CellInformation, GameObject> prefab in _cellPrefabs)
			{
				palletIcons.Add(AssetPreview.GetAssetPreview(prefab.Value));
			}

			for(int i=0; i<palletIcons.Count; i += 2)
            {
				GUILayout.BeginHorizontal();
				if (GUILayout.Button(palletIcons[i])) { _cellTypes_index = i + 1; _cellTypesShow[i] = true; _mode_paint = PaintMode.Single; ChangeBrushPallet(); };
				if(i+1 < palletIcons.Count)
					if (GUILayout.Button(palletIcons[i+1])) { _cellTypes_index = i + 2; _cellTypesShow[i+1] = true; _mode_paint = PaintMode.Single; ChangeBrushPallet(); };
				GUILayout.EndHorizontal();
			}
			GUILayout.EndScrollView();
		}
	}

	private void ChangeBrushPallet()
    {
		foreach (Transform child in _brush.transform)
		{
			child.gameObject.SetActive(false);
		}

		if (_mode_paint == PaintMode.Single)
			_brush.transform.Find(_cellPrefabs[_cellTypes[_cellTypes_index]].name).gameObject.SetActive(true);
		else if (_mode_paint == PaintMode.Erase)
			_brush.transform.Find("Erase").gameObject.SetActive(true);
		else if(_mode_paint == PaintMode.SetPathfindingWaypoint)
			_brush.transform.Find("PathfindingWaypoint").gameObject.SetActive(true);
	}
	#endregion
}
