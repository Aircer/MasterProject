using System.Collections.Generic;

using MapTileGridCreator.Core;
using MapTileGridCreator.UtilitiesMain;

using UnityEngine;

namespace MapTileGridCreator.TransformationsBank
{

	[CreateAssetMenu(fileName = "modif_HeightRandom", menuName = "MapTileGridCreator/Modifiers/HeightRandom")]
	public class ModifierHeightRandom : Modifier
	{
		#region Inspector
#pragma warning disable 0649

		[Header("ModifierHeightRandom")]

		//TODO Distribution curve
		[SerializeField]
		[Min(0)]
		private int Min_Random;

		[SerializeField]
		[Min(0)]
		private int Max_Random;

#pragma warning restore 0649
		#endregion

		public override Queue<Vector3Int> Modify(Grid3D grid, Vector3Int index)
		{
			Cell root;
			if ((root = grid.TryGetCellByIndex(ref index)) == null)
			{
				return null;
			}

			Queue<Vector3Int> newIndexes = new Queue<Vector3Int>();
			List<Cell> neighb = grid.GetNeighboursCell(ref index);
			foreach (Cell cell in neighb)
			{
				newIndexes.Enqueue(cell.index);
			}

			//Modif
			Vector3Int upIndex = root.index + grid.GetConnexAxes()[1];
			if (!grid.HaveCell(ref upIndex))
			{
				int height = Random.Range(Min_Random, Max_Random);
				for (int i = 0; i < height; i++)
				{
					if (!grid.HaveCell(ref upIndex))
					{
						FuncMain.InstantiateCell(grid, upIndex);
					}
					upIndex += grid.GetConnexAxes()[1];
				}
			}
			return newIndexes;
		}
	}
}
