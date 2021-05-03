using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MapTileGridCreator.Core
{
	public class GeneticAlgorithm
	{
		public DNA[] oldPopulation { get; private set; }
		public DNA[] newPopulation { get; private set; }
		public int Generation { get; private set; }
		public float BestFitness { get; private set; }
		public Waypoint[,,] BestGenes { get; private set; }

		public int Elitism;
		public float MutationRate;

		private System.Random random;
		private float fitnessSum;
		private Vector3Int dnaSize;
		private Func<CellInformation> getRandomType;
		private Func<int, float> fitnessFunction;

		public GeneticAlgorithm(int populationSize, Vector3Int dnaSize, System.Random random, Func<CellInformation> getRandomType, Func<int, float> fitnessFunction,
			int elitism, WaypointCluster cluster, float mutationRate = 0.01f)
		{
			Generation = 1;
			Elitism = elitism;
			MutationRate = mutationRate;
			oldPopulation = new DNA[populationSize];
			newPopulation = new DNA[populationSize];
			this.random = random;
			this.dnaSize = dnaSize;
			this.getRandomType = getRandomType;
			this.fitnessFunction = fitnessFunction;

			BestGenes = new Waypoint[dnaSize.x, dnaSize.y, dnaSize.z];

			for (int i = 0; i < populationSize; i++)
			{
				DNA newPop = new DNA(dnaSize, random, getRandomType, fitnessFunction, cluster);
				oldPopulation[i] = newPop;
				DNA newPop2 = new DNA(dnaSize, random, getRandomType, fitnessFunction, cluster);
				newPopulation[i] = newPop2;
			}

			foreach (Wall wall in newPopulation[0].phenotype.walls_x)
			{
				UnityEngine.Debug.Log("WallX position : " + wall.position + " size : " + wall.indexes.Count);
			}

			foreach (Wall wall in newPopulation[0].phenotype.walls_z)
			{
				UnityEngine.Debug.Log("WallZ position : " + wall.position + " size : " + wall.indexes.Count);
			}
		}

		public void NewGeneration(int numNewDNA = 0, bool crossoverNewDNA = false)
		{
			int finalCount = oldPopulation.Length + numNewDNA;

			if (finalCount <= 0)
			{
				return;
			}

			ClassifyPopulation();

			for (int i = 0; i < finalCount; i++)
			{
				if (i < Elitism && i < oldPopulation.Length)
				{
					newPopulation[i].Crossover(oldPopulation[i]);
				}
				else if (i < oldPopulation.Length || crossoverNewDNA)
				{
					DNA parent1 = ChooseParent();
					newPopulation[i].Crossover(parent1);
					newPopulation[i].Mutate(MutationRate);
				}
			}
			
			DNA[] tmpArray = oldPopulation;
			oldPopulation = newPopulation;
			newPopulation = tmpArray;

			Generation++;
		}

		public void ClassifyPopulation()
		{
			if (oldPopulation.Length > 0)
			{
				CalculateFitness();

				DNA temp;

				for (int i = 0; i < oldPopulation.Length - 1; i++)
				{
					for (int j = i + 1; j < oldPopulation.Length; j++)
					{
						if (oldPopulation[i].Fitness < oldPopulation[j].Fitness)
						{

							temp = oldPopulation[i];
							oldPopulation[i] = oldPopulation[j];
							oldPopulation[j] = temp;

							temp = newPopulation[i];
							newPopulation[i] = newPopulation[j];
							newPopulation[j] = temp;
						}
					}
				}
			}
		}

		private void CalculateFitness()
		{
			fitnessSum = 0;
			DNA best = oldPopulation[0];

			for (int i = 0; i < oldPopulation.Length; i++)
			{
				fitnessSum += oldPopulation[i].CalculateFitness(i);

				if (oldPopulation[i].Fitness > best.Fitness)
				{
					best = oldPopulation[i];
				}
			}

			BestFitness = best.Fitness;
			BestGenes = best.Genes;
		}

		private DNA ChooseParent()
		{
			double randomNumber = random.NextDouble() * fitnessSum;

			for (int i = 0; i < oldPopulation.Length; i++)
			{
				if (randomNumber < oldPopulation[i].Fitness)
				{
					return oldPopulation[i];
				}

				randomNumber -= oldPopulation[i].Fitness;
			}

			return oldPopulation[0];
		}
	}
}
