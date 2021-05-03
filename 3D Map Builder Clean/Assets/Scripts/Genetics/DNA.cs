using System;
using System.Collections.Generic;
using UnityEngine;

namespace MapTileGridCreator.Core
{
	public class DNA
	{
		public WaypointCluster GenesCluster { get; private set; }
		public Waypoint[,,] Genes { get; private set; }
		public float Fitness { get; private set; }

		private System.Random random;
		private Func<CellInformation> getRandomType;
		private Func<int, float> fitnessFunction;
		private Vector3Int sizeDNA;

		public Phenotype phenotype;

		public DNA(Vector3Int size, System.Random random, Func<CellInformation> getRandomType, Func<int, float> fitnessFunction, WaypointCluster cluster = null)
		{
			Genes = new Waypoint[size.x, size.y, size.z];
			this.random = random;
			this.getRandomType = getRandomType;
			this.fitnessFunction = fitnessFunction;
			this.sizeDNA = size;
			this.phenotype = new Phenotype();

			if (cluster != null)
			{
				this.GenesCluster = new WaypointCluster(cluster.GetWaypoints());
			}

			Genes = GenesCluster.GetWaypoints();
			phenotype = IA.GetPhenotype(GenesCluster.minSize, GenesCluster.maxSize, Genes);
		}

		public float CalculateFitness(int index)
		{
			Fitness = fitnessFunction(index);
			return Fitness;
		}

		public void Crossover(DNA otherParent)
		{
			Vector3Int min = new Vector3Int(Mathf.Min(GenesCluster.minSize.x,otherParent.GenesCluster.minSize.x),
										Mathf.Min(GenesCluster.minSize.y, otherParent.GenesCluster.minSize.y),
										Mathf.Min(GenesCluster.minSize.z, otherParent.GenesCluster.minSize.z));

			Vector3Int max = new Vector3Int(Mathf.Max(GenesCluster.maxSize.x, otherParent.GenesCluster.maxSize.x),
							Mathf.Max(GenesCluster.maxSize.y, otherParent.GenesCluster.maxSize.y),
							Mathf.Max(GenesCluster.maxSize.z, otherParent.GenesCluster.maxSize.z));

			for (int x = min.x; x < max.x; x++)
			{
				for (int y = min.y; y < max.y; y++)
				{
					for (int z = min.z; z < max.z; z++)
					{
						Genes[x,y,z].type = otherParent.Genes[x, y, z].type;
						Genes[x, y, z].rotation = otherParent.Genes[x, y, z].rotation;
						Genes[x, y, z].basePos = otherParent.Genes[x, y, z].basePos;
						Genes[x, y, z].baseType = otherParent.Genes[x, y, z].baseType;
					}
				}
			}
		}

		public void Mutate(float mutationRate)
		{
			for (int x = 0; x < mutationRate * sizeDNA.x * sizeDNA.y * sizeDNA.z; x++)
			{
				Vector3Int index = new Vector3Int(random.Next(0, sizeDNA.x), random.Next(0, sizeDNA.y), random.Next(0, sizeDNA.z));
				CellInformation type = getRandomType();
				Vector3 rotation = new Vector3(0, 0, 0);

				if (type != null)
				{
					GenesCluster.SetTypeAndRotationAround(sizeDNA, rotation, type, index);
				}
				else
				{
					GenesCluster.RemoveTypeAround(sizeDNA, index);
				}
			}
		}
	}
}