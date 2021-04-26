using MapTileGridCreator.Core;
using MapTileGridCreator.UtilitiesMain;

using UnityEditor;

using UnityEngine;

namespace MapTileGridCreator.CustomInpectors
{
	[CustomEditor(typeof(Grid3D), true)]
	public class GridInspector : Editor
	{
		private Grid3D _grid;

		public void OnEnable()
		{
			_grid = (Grid3D)target;
			FuncMain.RefreshGrid(_grid);
		}

		public void OnDisable()
		{
			_grid = null;
		}

		public override void OnInspectorGUI()
		{
			if (_grid.transform.hasChanged)
			{
				_grid.transform.localScale = Vector3.one;
				_grid.transform.localRotation = Quaternion.identity;
			}

			GUI.enabled = !Application.isPlaying;
			DrawDefaultInspector();
			if (GUILayout.Button("Refresh"))
			{
				FuncMain.RefreshGrid(_grid);
			}
			if (GUILayout.Button("Reset childs"))
			{
				FuncMain.RefreshGrid(_grid);
				foreach (Cell c in _grid.GetComponentsInChildren<Cell>())
				{
					c.ResetTransform();
				}
			}
			GUI.enabled = true;
			if (GUILayout.Button("Show tools window"))
			{
				MapTileGridCreatorWindow.OpenWindows();
			}
		}
	}
}
