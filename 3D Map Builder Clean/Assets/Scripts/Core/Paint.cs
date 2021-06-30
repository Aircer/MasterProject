using System.Collections;
using System;
using System.Collections.Generic;
using MapTileGridCreator.Core;
using MapTileGridCreator.UtilitiesVisual;
using MapTileGridCreator.UtilitiesMain;
using UnityEditor;
using UnityEngine;
using System.IO;

namespace MapTileGridCreator.Paint
{
	public static class IndexesToPaint
	{
		private static Vector3Int _startingPaintIndex;
		private static Vector3Int size_grid;
		private static HashSet<Vector3Int> _indexToPaint;
		private static bool _painting;

		public static HashSet<Vector3Int> AddInput(Vector3Int _size_grid, Vector3Int input, PaintMode _mode_paint, CellInformation _cellType,
										ref Grid3D grid)
		{
			size_grid = _size_grid;

			if (Event.current.type == EventType.Layout)
			{
				HandleUtility.AddDefaultControl(0);
			}

			HashSet<Vector3Int> newIndexToPaint = new HashSet<Vector3Int>();

			//Set starting index of paint/erase
			if (!_painting && Event.current.type == EventType.MouseDown && Event.current.button == 0)
			{
				_startingPaintIndex = input;
				_indexToPaint = new HashSet<Vector3Int>();
				_painting = true;
			}

			//Get all indexes that will be paint/erase
			if (_painting && (Event.current.type == EventType.MouseDrag || Event.current.type == EventType.MouseDown) && Event.current.button == 0)
			{
				newIndexToPaint = CellsPainted(input, _startingPaintIndex, _mode_paint, grid._cells, _cellType);

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
							grid.PaintCell(grid._cells, newIndex, _cellType);
						}
						else if (_mode_paint == PaintMode.Erase && grid._cells[newIndex.x, newIndex.y, newIndex.z].type != null)
						{
							_indexToPaint.Add(newIndex);
							grid.EraseCell(grid._cells, newIndex);
						}

						FuncVisual.UpdateCellsAroundVisual(grid._cells, newIndex, _cellType);
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
							grid.DesactivateCell(grid._cells, currentIndex);
						}
						if (_mode_paint == PaintMode.Erase)
						{
							grid._cells[currentIndex.x, currentIndex.y, currentIndex.z].Active();
						}

						FuncVisual.UpdateCellsAroundVisual(grid._cells, currentIndex, grid._cells[currentIndex.x, currentIndex.y, currentIndex.z].type);

						newIndexToPaint.Remove(currentIndex);
					}
				}

				_indexToPaint = newIndexToPaint;
			}

			return _indexToPaint;
		}

		private static HashSet<Vector3Int> CellsPainted(Vector3Int input, Vector3Int _startingPaintIndex, PaintMode _mode_paint, Cell[,,] _cells, CellInformation _cellType)
		{
			if(_mode_paint == PaintMode.Erase)
				return CellsErase(input, _startingPaintIndex, _mode_paint, _cells);

			if (_cellType.typeParams.door)
				return CellsDoor(input, _startingPaintIndex, _mode_paint, _cells);
			else if (_cellType.typeParams.floor)
				return CellsFloor(input, _startingPaintIndex, _mode_paint, _cells);
			else if (_cellType.typeParams.stair)
				return CellsStair(input, _startingPaintIndex, _mode_paint, _cells);
			else
				return CellsWall(input, _startingPaintIndex, _mode_paint, _cells);
		}

		private static HashSet<Vector3Int> CellsFloor(Vector3Int input, Vector3Int _startingPaintIndex, PaintMode _mode_paint, Cell[,,] _cells)
		{
			HashSet<Vector3Int> indexesToPaint = new HashSet<Vector3Int>();
			Vector3Int min = input;
			Vector3Int max = _startingPaintIndex;

			if (input.x > _startingPaintIndex.x)
			{
				max.x = input.x;
				min.x = _startingPaintIndex.x;
			}
			
			if (input.z > _startingPaintIndex.z)
			{
				max.z = input.z;
				min.z = _startingPaintIndex.z;
			}

			int y = _startingPaintIndex.y;

			for (int x = min.x; x <= max.x; x++)
			{
				for (int z = min.z; z <= max.z; z++)
				{
					if (((_mode_paint == PaintMode.Single && (_cells[x, y, z].state == CellState.Inactive ||
						_cells[x, y, z].state == CellState.Painted)) || (_mode_paint == PaintMode.Erase &&
						(_cells[x, y, z].state == CellState.Active || _cells[x, y, z].state == CellState.Erased))))
					{
						indexesToPaint.Add(new Vector3Int(x, y, z));
					}
				}
			}

			return indexesToPaint;
		}

		private static HashSet<Vector3Int> CellsWall(Vector3Int input, Vector3Int _startingPaintIndex, PaintMode _mode_paint, Cell[,,] _cells)
		{
			HashSet<Vector3Int> indexesToPaint = new HashSet<Vector3Int>();
			Vector3Int min = input;
			Vector3Int max = _startingPaintIndex;

			if (input.x > _startingPaintIndex.x)
			{
				max.x = input.x;
				min.x = _startingPaintIndex.x;
			}

			if (input.y > _startingPaintIndex.y)
			{
				max.y = input.y;
				min.y = _startingPaintIndex.y;
			}

			if (input.z > _startingPaintIndex.z)
			{
				max.z = input.z;
				min.z = _startingPaintIndex.z;
			}

			for (int x = min.x; x <= max.x; x++)
			{
				for (int y = min.y; y <= max.y; y++)
				{
					for (int z = min.z; z <= max.z; z++)
					{
						if (((_mode_paint == PaintMode.Single && (_cells[x, y, z].state == CellState.Inactive ||
							_cells[x, y, z].state == CellState.Painted)) || (_mode_paint == PaintMode.Erase &&
							(_cells[x, y, z].state == CellState.Active || _cells[x, y, z].state == CellState.Erased))))
						{
							indexesToPaint.Add(new Vector3Int(x, y, z));
						}
					}
				}
			}

			return indexesToPaint;
		}

		private static HashSet<Vector3Int> CellsErase(Vector3Int input, Vector3Int _startingPaintIndex, PaintMode _mode_paint, Cell[,,] _cells)
		{
			HashSet<Vector3Int> indexesToPaint = new HashSet<Vector3Int>();
			Vector3Int min = input;
			Vector3Int max = _startingPaintIndex;

			if (input.x > _startingPaintIndex.x)
			{
				max.x = input.x;
				min.x = _startingPaintIndex.x;
			}

			if (input.y > _startingPaintIndex.y)
			{
				max.y = input.y;
				min.y = _startingPaintIndex.y;
			}

			if (input.z > _startingPaintIndex.z)
			{
				max.z = input.z;
				min.z = _startingPaintIndex.z;
			}

			for (int x = min.x; x <= max.x; x++)
			{
				for (int y = min.y; y <= max.y; y++)
				{
					for (int z = min.z; z <= max.z; z++)
					{
						if (((_mode_paint == PaintMode.Single && (_cells[x, y, z].state == CellState.Inactive ||
							_cells[x, y, z].state == CellState.Painted)) || (_mode_paint == PaintMode.Erase &&
							(_cells[x, y, z].state == CellState.Active || _cells[x, y, z].state == CellState.Erased))))
						{
							if(_cells[x, y, z].type != null && _cells[x, y, z].type.typeParams.door)
                            {
								if (y > 0 && _cells[x, y - 1, z].type != null && _cells[x, y - 1, z].type.typeParams.door)
									indexesToPaint.Add(new Vector3Int(x, y - 1, z));
								if (y < size_grid.y - 1 && _cells[x, y + 1, z].type != null && _cells[x, y + 1, z].type.typeParams.door)
									indexesToPaint.Add(new Vector3Int(x, y + 1, z));
							}

							indexesToPaint.Add(new Vector3Int(x, y, z));
						}
					}
				}
			}

			return indexesToPaint;
		}

		private static HashSet<Vector3Int> CellsDoor(Vector3Int input, Vector3Int _startingPaintIndex, PaintMode _mode_paint, Cell[,,] _cells)
		{
			Debug.Log("ERASE DOOR");
			HashSet<Vector3Int> indexesToPaint = new HashSet<Vector3Int>();

			if (_startingPaintIndex.y + 1 >= size_grid.y)
				return indexesToPaint;

			Vector3Int min = input;
			Vector3Int max = _startingPaintIndex;

			if (input.x > _startingPaintIndex.x)
			{
				max.x = input.x;
				min.x = _startingPaintIndex.x;
			}

			max.y = _startingPaintIndex.y + 1;
			min.y = _startingPaintIndex.y;

			if (input.z > _startingPaintIndex.z)
			{
				max.z = input.z;
				min.z = _startingPaintIndex.z;
			}

			for (int x = min.x; x <= max.x; x++)
			{
				for (int y = min.y; y <= max.y; y++)
				{
					for (int z = min.z; z <= max.z; z++)
					{
						if (((_mode_paint == PaintMode.Single && (_cells[x, y, z].state == CellState.Inactive ||
							_cells[x, y, z].state == CellState.Painted)) || (_mode_paint == PaintMode.Erase &&
							(_cells[x, y, z].state == CellState.Active || _cells[x, y, z].state == CellState.Erased))))
						{
							indexesToPaint.Add(new Vector3Int(x, y, z));
						}
					}
				}
			}

			return indexesToPaint;
		}

		private static HashSet<Vector3Int> CellsStair(Vector3Int input, Vector3Int _startingPaintIndex, PaintMode _mode_paint, Cell[,,] _cells)
		{
			HashSet<Vector3Int> indexesToPaint = new HashSet<Vector3Int>();
			int direction = 1;

			if (_startingPaintIndex.y + 1 >= size_grid.y)
				return indexesToPaint;

			Vector3Int start = _startingPaintIndex;
			Vector3Int end = input;

			if (input.y > _startingPaintIndex.y)
			{
				end.y = input.y;
				start.y = _startingPaintIndex.y;
			}


			if (Math.Abs(input.x - _startingPaintIndex.x) > Math.Abs(input.z - _startingPaintIndex.z))
			{
				if (start.x > end.x)
				{
					direction = -1; 
				}

				int x = start.x;
				int y = start.y;
				int z = _startingPaintIndex.z;

				while (y <= end.y && (x <= end.x || direction == -1) && (x >= end.x || direction == 1))
				{
					if (((_mode_paint == PaintMode.Single && (_cells[x, y, z].state == CellState.Inactive ||
						_cells[x, y, z].state == CellState.Painted)) || (_mode_paint == PaintMode.Erase &&
						(_cells[x, y, z].state == CellState.Active || _cells[x, y, z].state == CellState.Erased))))
					{
						indexesToPaint.Add(new Vector3Int(x, y, z));
					}

					x +=  direction;
					y++;
				}
			}
			else
			{
				if (start.z > end.z)
				{
					direction = -1;
				}

				int x = _startingPaintIndex.x;
				int y = start.y;
				int z = start.z;

				while (y <= end.y && (z <= end.z || direction == -1) && (z >= end.z || direction == 1))
				{
					if (((_mode_paint == PaintMode.Single && (_cells[x, y, z].state == CellState.Inactive ||
						_cells[x, y, z].state == CellState.Painted)) || (_mode_paint == PaintMode.Erase &&
						(_cells[x, y, z].state == CellState.Active || _cells[x, y, z].state == CellState.Erased))))
					{
						indexesToPaint.Add(new Vector3Int(x, y, z));
					}

					y++;
					z += direction;
				}
			}

			return indexesToPaint;
		}

		public static bool Paint(Vector3Int _size_grid, Vector3Int input, PaintMode _mode_paint, CellInformation _cellType,
										 ref Grid3D grid)
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
						grid._cells[index.x, index.y, index.z].Active();
					}

					if (_mode_paint == PaintMode.Erase)
					{
						grid.DesactivateCell(grid._cells, index);
						FuncVisual.UpdateCellsAroundVisual(grid._cells, index, grid._cells[index.x, index.y, index.z].type);
					}
				}

				_painting = false;

				return true;
			}

			return false;
		}
	}
}