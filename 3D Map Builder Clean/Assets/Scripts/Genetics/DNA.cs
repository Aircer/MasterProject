using System;
using System.Collections.Generic;
using System.Linq;
using UtilitiesGenetic;

namespace Genetics
{
	public class DNA
	{
		//public WaypointCluster GenesCluster { get; private set; }
		public int[][][] Genes { get; set; }

		public Fitness fitness { get; private set; }

		private SharpNeatLib.Maths.FastRandom randomFast;
		private TypeParams[] typeParams;

		public Phenotype phenotype;
		public Vector3Int limitDNA;

		public DNA(Vector3Int size, TypeParams[] typeParams, SharpNeatLib.Maths.FastRandom randomFast, int[][][] waypointParams = null)
		{
			//Genes are bigger than cluster to have empty borders thus it is easier to get neighbors  
			Genes = new int[size.x][][];
			limitDNA = new Vector3Int(size.x - 1, size.y - 1, size.z - 1);
			this.randomFast = randomFast;
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

		public Fitness CalculateFitness(FitnessComputation fitness, PhenotypeCompute phenoCompute)
		{
			phenotype = phenoCompute.GetPhenotype(Genes);

			this.fitness = fitness.FitnessFunction(phenotype);
			return this.fitness;
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

		public void Mutate(float mutationNumber, Mutations mutation)
		{
			for (int x = 0; x < mutationNumber; x++)
			{
				Genes = mutation.Mutate(Genes);
			}
		}
	}
}