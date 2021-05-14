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
			int sizeDNAx_Minus2 = sizeDNAx_wtBorder - 1;
			int sizeDNAz_Minus2 = sizeDNAz_wtBorder - 1;

			for (int x = 1; x < sizeDNAx_Minus2; x += 2)
			{
				for (int y = 1; y < sizeDNAy_wtBorder; y++)
				{
					for (int z = 1; z < sizeDNAz_Minus2; z += 2)
					{
						Genes[x][y][z].type = parent1.Genes[x][y][z].type;
						Genes[x][y][z].rotation = parent1.Genes[x][y][z].rotation;

						Genes[x+1][y][z+1].type = parent2.Genes[x+1][y][z+1].type;
						Genes[x+1][y][z+1].rotation = parent2.Genes[x+1][y][z+1].rotation;
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
				//int type = existingTypes[randomFast.Next(existingTypes.Count)];
				int mutationType = randomFast.Next(2);

				if(mutationType == 0)
					Genes[mutationIndex_x][mutationIndex_y][mutationIndex_z].type = 0;
				else
					Genes[mutationIndex_x][mutationIndex_y][mutationIndex_z].type = 9;

				/*
				if (mutationType == 0)
					Extend(mutationIndex_x, mutationIndex_y, mutationIndex_z);
				else if(mutationType == 1)
					Swap(mutationIndex_x, mutationIndex_y, mutationIndex_z);
				else if (mutationType == 2)
					Erase(mutationIndex_x, mutationIndex_y, mutationIndex_z);*/
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