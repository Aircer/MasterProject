using System.Collections.Generic;

using MapTileGridCreator.UtilitiesMain;
using MapTileGridCreator.Utilities;

using UnityEngine;
using MapTileGridCreator.Core;

namespace MapTileGridCreator.SerializeSystem
{
	/// <summary>
	/// Class use to transfer grid data mapping. Use only inside the SerializeSystem.
	/// </summary>
	[System.Serializable]
	internal class Grid3DDTO
	{
		[SerializeField]
		public string _name;

		[SerializeField]
		public Vector3Int size;

		[SerializeField]
		public int[] _cellsValues;

		[SerializeField]
		public List<CellInformation> cellInfos;

		public Grid3DDTO(Grid3D grid)
		{
			size = grid.size;
			_name = grid.name;
			_cellsValues = ConvertJaggedArrayTo1DArray(size, grid.ConvertCellsToInt());
		}

		public Grid3D ToGrid3D(List<CellInformation> cellInfos, Dictionary<CellInformation, GameObject> pallet, GameObject palletObject)
		{
			Grid3D grid = FuncMain.InstantiateGrid3D(size, cellInfos, pallet, palletObject);
			grid.name = _name;

			grid.ConvertIntToCells(Convert1DArrayToJaggedArray(size, _cellsValues));

			return grid;
		}

		private int[] ConvertJaggedArrayTo1DArray(Vector3Int size, int[][][] jaggedArray)
		{
			int[] newArray = new int[(size.x + 2) * (size.y + 2) * (size.z + 2)];
			int index = 0;

			for (int x = 0; x < size.x + 2; x++)
			{
				for (int y = 0; y < size.y + 2; y++)
				{
					for (int z = 0; z < size.z + 2; z++)
					{
						newArray[index] = jaggedArray[x][y][z];
						index++;
					}
				}
			}

			return newArray;
		}

		public int[][][] Convert1DArrayToJaggedArray(Vector3Int size, int[] oneDArray)
		{
			int[][][] genesXYZ = new int[size.x + 2][][];
			int index = 0;

			for (int x = 0; x < size.x + 2; x++)
			{
				int[][] genesYZ = new int[size.y + 2][];
				for (int y = 0; y < size.y + 2; y++)
				{
					int[] genesZ = new int[size.z + 2];
					for (int z = 0; z < size.z + 2; z++)
					{
						genesZ[z] = oneDArray[index];
						index++;
					}
					genesYZ[y] = genesZ;
				}
				genesXYZ[x] = genesYZ;
			}

			return genesXYZ;
		}
	}
}
