using System;
using System.Collections.Generic;
using System.Linq;
using UtilitiesGenetic;
using mVectors;

namespace Genetics
{
	public class DNA
	{
		//public WaypointCluster GenesCluster { get; private set; }
		public int[][][] Genes { get; private set; }

		public float Fitness { get; private set; }

		private SharpNeatLib.Maths.FastRandom randomFast;
		private Func<int, float> fitnessFunction;
		private TypeParams[] typeParams;

		public Phenotype phenotype;
		public Vector3Int limitDNA;

		public DNA(Vector3Int size, Func<int, float> fitnessFunction, TypeParams[] typeParams, SharpNeatLib.Maths.FastRandom randomFast, int[][][] waypointParams = null)
		{
			//Genes are bigger than cluster to have empty borders thus it is easier to get neighbors  
			Genes = new int[size.x][][];
			limitDNA = new Vector3Int(size.x - 1, size.y - 1, size.z - 1);
			this.randomFast = randomFast;
			this.fitnessFunction = fitnessFunction;
			this.typeParams = typeParams;

			if (waypointParams != null)
			{
				for (int x = 0; x < size.x; x++)
				{
					int[][] waypointsParamsYZ = new int[size.y][];
					for (int y = 0; y < size.y; y++)
					{
						int[] waypointsParamsZ = new int[size.z];
						for (int z = 0; z < size.z; z++)
						{
							waypointsParamsZ[z] = waypointParams[x][y][z];
						}
						waypointsParamsYZ[y] = waypointsParamsZ;
					}
					Genes[x] = waypointsParamsYZ;
				}
			}
		}

		public List<int> ExistingTypes()
        {
			List<int> differentTypes = new List<int>();

			for (int x = 1; x < limitDNA.x; x++)
			{
				for (int y = 1; y < limitDNA.y; y++)
				{
					for (int z = 1; z < limitDNA.z; z++)
					{
						if(!differentTypes.Contains(Genes[x][y][z]))
							differentTypes.Add(Genes[x][y][z]);
					}
				}
			}

			return differentTypes;
		}

		public float CalculateFitness(int index)
		{
			phenotype = PhenotypeCompute.GetPhenotype(limitDNA.x, limitDNA.y, limitDNA.z, Genes, typeParams);

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
				sizeDNAx = (int)(limitDNA.x * 0.6f);
				sizeDNAz = limitDNA.z;
			}
			else
            {
				sizeDNAx = limitDNA.x;
				sizeDNAz = (int)(limitDNA.z * 0.6f);
			}

			for (int x = 1; x < sizeDNAx; x++)
			{
				for (int y = 1; y < limitDNA.y; y++)
				{
					for (int z = 1; z < sizeDNAz; z++)
					{
						Genes[x][y][z] = parent1.Genes[x][y][z];
					}
				}
			}

			for (int x = sizeDNAx; x < limitDNA.x; x++)
			{
				for (int y = 1; y < limitDNA.y; y++)
				{
					for (int z = sizeDNAz; z < limitDNA.z; z++)
					{
						Genes[x][y][z] = parent2.Genes[x][y][z];
					}
				}
			}
		}

		public void Copy(DNA parent)
		{
			for (int x = 1; x < limitDNA.x; x++)
			{
				for (int y = 1; y < limitDNA.y; y++)
				{
					for (int z = 1; z < limitDNA.z; z++)
					{
						Genes[x][y][z] = parent.Genes[x][y][z];
					}
				}
			}
		}

		public void Mutate(float mutationNumber, List<int> existingTypes)
		{
			for (int x = 0; x < mutationNumber; x++)
			{
				int mutationIndex_x = randomFast.Next(1, limitDNA.x);
				int mutationIndex_y = randomFast.Next(1, limitDNA.y-1);
				int mutationIndex_z = randomFast.Next(1, limitDNA.z);
				int type = existingTypes[randomFast.Next(existingTypes.Count)];

				Vector3Int input = new Vector3Int(mutationIndex_x, mutationIndex_y, mutationIndex_z);
				Genes = Mutations.FillStairXPos(limitDNA, Genes, input, 8, typeParams);

				/*
				if (typeParams[Genes[input.x][input.y][input.z]].floor)
				{ 
					Genes = Mutations.DeleteFloor(limitDNA, Genes, input, typeParams);
				}

				if (typeParams[Genes[input.x][input.y][input.z]].ladder)
				{
					int mutationType = randomFast.Next(2);

					if (mutationType == 0)
						Genes = Mutations.TranslateLadder(limitDNA, Genes, input, typeParams, randomFast);
					if (mutationType == 1)
						Genes = Mutations.DeleteLadder(limitDNA, Genes, input, typeParams);
				}

				if (typeParams[Genes[input.x][input.y][input.z]].door)
				{
					int mutationType = randomFast.Next(2);

					if (mutationType == 0)
						Genes = Mutations.CollapseDoor(limitDNA, Genes, input, typeParams, randomFast);
					if (mutationType == 1)
						Genes = Mutations.TranslateDoor(limitDNA, Genes, input, typeParams, randomFast);
				}

				if (typeParams[Genes[input.x][input.y][input.z]].wall)
				{
					int mutationType = randomFast.Next(5);

					if (mutationType == 0)
						Genes = Mutations.TranslationWall(limitDNA, Genes, input, typeParams, randomFast);
					if (mutationType == 1)
						Genes = Mutations.RotationWall(limitDNA, Genes, input, typeParams, randomFast);
					if (mutationType == 2)
						Genes = Mutations.DeleteWallZ(limitDNA, Genes, input, typeParams);
					if (mutationType == 3)
						Genes = Mutations.DeleteWallX(limitDNA, Genes, input, typeParams);
					if (mutationType == 4)
						Genes = Mutations.CreateDoor(Genes, input, typeParams, randomFast, 3);
				}

				if (Genes[input.x][input.y][input.z] == 0)
				{
					int mutationType = randomFast.Next(100);

					if (mutationType < 20)
						Genes = Mutations.FillWallX(limitDNA, Genes, input, 11, typeParams);
					if (mutationType > 20 && mutationType < 40)
						Genes = Mutations.FillWallZ(limitDNA, Genes, input, 11, typeParams);
					if (mutationType > 40 && mutationType < 90)
						Genes = Mutations.FillFloor(limitDNA, Genes, input, 5, typeParams);
					if (mutationType > 90)
						Genes = Mutations.CreateLadder(limitDNA, Genes, input, 6, typeParams);
				}*/
			}
		}
	}
}