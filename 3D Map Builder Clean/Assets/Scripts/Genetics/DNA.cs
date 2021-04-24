using System;
using System.Collections.Generic;
using UnityEngine;

namespace MapTileGridCreator.Core
{
	public class DNA
	{
		public CellInformation[,,] Genes { get; private set; }
		public float Fitness { get; private set; }

		private System.Random random;
		private Func<CellInformation> getRandomGeneType;
		private Func<int, float> fitnessFunction;
		private Vector3Int sizeDNA;

		public DNA(Vector3Int size, System.Random random, Func<CellInformation> getRandomGeneType, Func<int, float> fitnessFunction, Waypoint[,,] waypoints = null)
		{
			Genes = new CellInformation[size.x, size.y, size.z];
			this.random = random;
			this.getRandomGeneType = getRandomGeneType;
			this.fitnessFunction = fitnessFunction;
			this.sizeDNA = size;

			if (waypoints != null)
			{
				for (int i = 0; i < sizeDNA.x; i++)
				{
					for (int j = 0; j < sizeDNA.y; j++)
					{
						for (int k = 0; k < sizeDNA.z; k++)
						{
							Genes[i, j, k] = waypoints[i,j,k].type;
						}
					}
				}
			}
		}

		public float CalculateFitness(int index)
		{
			Fitness = fitnessFunction(index);
			return Fitness;
		}

		public DNA Crossover(DNA otherParent)
		{
			DNA child = new DNA(sizeDNA, random, getRandomGeneType, fitnessFunction);

			for (int i = 0; i < sizeDNA.x; i++)
			{
				for (int j = 0; j < sizeDNA.y; j++)
				{
					for (int k = 0; k < sizeDNA.z; k++)
					{
						child.Genes[i, j, k] = random.NextDouble() < 0.5 ? Genes[i, j, k] : otherParent.Genes[i, j, k];
					}
				}
			}

			return child;
		}

		public void Mutate(float mutationRate)
		{
			for (int i = 0; i < sizeDNA.x; i++)
			{
				for (int j = 0; j < sizeDNA.y; j++)
				{
					for (int k = 0; k < sizeDNA.z; k++)
					{
						if (random.NextDouble() < mutationRate)
						{
							Genes[i, j, k] = getRandomGeneType();
						}
					}
				}
			}
		}
	}
}