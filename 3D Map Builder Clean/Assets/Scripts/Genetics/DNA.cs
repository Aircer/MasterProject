using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace MapTileGridCreator.Core
{
	public class DNA
	{
		//public WaypointCluster GenesCluster { get; private set; }
		public WaypointParams[][][] Genes { get; private set; }

		public float Fitness { get; private set; }

		private SharpNeatLib.Maths.FastRandom randomFast;
		private System.Random randomSystem;

		private Func<int, float> fitnessFunction;
		private TypeParams[] typeParams;

		public Phenotype phenotype;
		public int sizeDNAx_wtBorder; public int sizeDNAy_wtBorder; public int sizeDNAz_wtBorder;
		private Vector3Int sizeDNA_wtBorder;

		public DNA(Vector3Int size, System.Random randomSystem, SharpNeatLib.Maths.FastRandom randomFast, Func<int, float> fitnessFunction,
			TypeParams[] typeParams, WaypointCluster cluster = null)
		{
			//Genes are bigger than cluster to have empty borders thus it is easier to get neighbors  
			Genes = new WaypointParams[size.x][][];
			this.randomSystem = randomSystem;
			this.randomFast = randomFast;
			this.fitnessFunction = fitnessFunction;;
			this.typeParams = typeParams;

			sizeDNAx_wtBorder = size.x-1;
			sizeDNAy_wtBorder = size.y-1;
			sizeDNAz_wtBorder = size.z-1;
			sizeDNA_wtBorder = new Vector3Int(sizeDNAx_wtBorder, sizeDNAy_wtBorder, sizeDNAz_wtBorder);

			if (cluster != null)
			{
				Genes = cluster.GetWaypointsParams();
			}
		}

		public List<int> ExistingTypes()
        {
			List<int> differentTypes = new List<int>();

			for (int x = 1; x < sizeDNAx_wtBorder; x++)
			{
				for (int y = 1; y < sizeDNAy_wtBorder; y++)
				{
					for (int z = 1; z < sizeDNAz_wtBorder; z++)
					{
						if(!differentTypes.Contains(Genes[x][y][z].type))
							differentTypes.Add(Genes[x][y][z].type);
					}
				}
			}

			return differentTypes;
		}

		public float CalculateFitness(int index)
		{
			phenotype = IA.GetPhenotype(sizeDNAx_wtBorder, sizeDNAy_wtBorder, sizeDNAz_wtBorder, Genes, typeParams);
			Fitness = fitnessFunction(index);
			return Fitness;
		}

		public void Crossover(DNA parent1, DNA parent2)
		{
			int XorY = randomFast.Next(2);
			int sizeDNAx;
			int sizeDNAz;

			if (XorY == 0)
			{
				sizeDNAx = (int)(sizeDNAx_wtBorder*0.6f);
				sizeDNAz = sizeDNAz_wtBorder;
			}
			else
            {
				sizeDNAx = sizeDNAx_wtBorder;
				sizeDNAz = (int)(sizeDNAz_wtBorder * 0.6f);
			}

			for (int x = 1; x < sizeDNAx; x++)
			{
				for (int y = 1; y < sizeDNAy_wtBorder; y++)
				{
					for (int z = 1; z < sizeDNAz; z++)
					{
						Genes[x][y][z].type = parent1.Genes[x][y][z].type;
					}
				}
			}

			for (int x = sizeDNAx; x < sizeDNAx_wtBorder; x++)
			{
				for (int y = 1; y < sizeDNAy_wtBorder; y++)
				{
					for (int z = sizeDNAz; z < sizeDNAz_wtBorder; z++)
					{
						Genes[x][y][z].type = parent2.Genes[x][y][z].type;
					}
				}
			}
		}

		public void Copy(DNA parent)
		{
			for (int x = 1; x < sizeDNAx_wtBorder; x++)
			{
				for (int y = 1; y < sizeDNAy_wtBorder; y++)
				{
					for (int z = 1; z < sizeDNAz_wtBorder; z++)
					{
						Genes[x][y][z].type = parent.Genes[x][y][z].type;
						Genes[x][y][z].rotation = parent.Genes[x][y][z].rotation;
					}
				}
			}
		}

		public void Mutate(float mutationNumber, List<int> existingTypes)
		{
			for (int x = 0; x < mutationNumber; x++)
			{
				int mutationIndex_x = randomFast.Next(1, sizeDNAx_wtBorder);
				int mutationIndex_y = randomFast.Next(1, sizeDNAy_wtBorder);
				int mutationIndex_z = randomFast.Next(1, sizeDNAz_wtBorder);
				int type = existingTypes[randomFast.Next(existingTypes.Count)];
				int mutationType = randomFast.Next(9);

				Vector3Int input = new Vector3Int(mutationIndex_x, mutationIndex_y, mutationIndex_z);
				
				if(mutationType < 2)
					Genes = IA.MutationWall(sizeDNA_wtBorder, Genes, input, typeParams, randomFast);
				if (mutationType == 1)
					Genes = IA.DeleteWallZ(sizeDNA_wtBorder, Genes, input, typeParams);
				if (mutationType == 2)
					Genes = IA.DeleteWallX(sizeDNA_wtBorder, Genes, input, typeParams);
				if (mutationType == 3)
					Genes = IA.FillWallX(sizeDNA_wtBorder, Genes, input, 1, typeParams);
				if (mutationType == 4)
					Genes = IA.FillWallZ(sizeDNA_wtBorder, Genes, input, 1, typeParams);
				if(mutationType == 5)
					Genes = IA.FillFloor(sizeDNA_wtBorder, Genes, new Vector3Int(mutationIndex_x, 1, mutationIndex_z), 4, typeParams);
				if (mutationType == 6)
					Genes = IA.TranslateDoor(sizeDNA_wtBorder, Genes, input, typeParams, randomFast);
				if (mutationType == 7)
					Genes = IA.CreateDoor(sizeDNA_wtBorder, Genes, input, typeParams, randomFast, 3);
				if (mutationType == 8)
					Genes = IA.CollapseDoor(sizeDNA_wtBorder, Genes, input, typeParams, randomFast);
			}
		}

		//Fill the cell with the same type as a neighbor 
		public void Extend(int x, int y, int z)
		{
			int[] typesAround = new int[typeParams.Length];
			int newType = 0;
			for (int i = -1; i < 2; i++)
			{
				for (int j = -1; j < 2; j++)
				{
					for (int k = -1; k < 2; k++)
					{
						if (Genes[x + i][y + j][z + k].type > 0)
						{
							typesAround[Genes[x + i][y + j][z + k].type]++;
							if (typesAround[Genes[x + i][y + j][z + k].type] > typesAround[newType])
								newType = Genes[x + i][y + j][z + k].type;
						}
					}
				}
			}

			if(typesAround[newType] > 0 && newType != Genes[x][y][z].type)
				Genes[x][y][z].type = newType;
		}

		//Swap the cell with a random other cell
		public void Swap(int x, int y, int z)
		{
			int swap_x = randomFast.Next(1, sizeDNAx_wtBorder);
			int swap_y = randomFast.Next(1, sizeDNAy_wtBorder);
			int swap_z = randomFast.Next(1, sizeDNAz_wtBorder);

			int type = Genes[x][y][z].type;
			Genes[x][y][z].type = Genes[swap_x][swap_y][swap_z].type;
			Genes[swap_x][swap_y][swap_z].type = type;
		}

		//Erase the cell
		public void Erase(int x, int y, int z)
		{
			Genes[x][y][z].type = 0;
		}
	}
}