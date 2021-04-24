using System;
using System.Collections.Generic;
using UnityEngine;

namespace MapTileGridCreator.Core
{
	public class GeneticAlgorithm
	{
		public List<DNA> Population { get; private set; }
		public int Generation { get; private set; }
		public float BestFitness { get; private set; }
		public CellInformation[,,] BestGenes { get; private set; }

		public int Elitism;
		public float MutationRate;

		private List<DNA> newPopulation;
		private System.Random random;
		private float fitnessSum;
		private Vector3Int dnaSize;
		private Func<CellInformation> getRandomGeneType;
		private Func<int, float> fitnessFunction;

		public GeneticAlgorithm(int populationSize, Vector3Int dnaSize, System.Random random, Func<CellInformation> getRandomGeneType, Func<int, float> fitnessFunction,
			int elitism, Waypoint[,,] waypoints, float mutationRate = 0.01f)
		{
			Generation = 1;
			Elitism = elitism;
			MutationRate = mutationRate;
			Population = new List<DNA>(populationSize);
			newPopulation = new List<DNA>(populationSize);
			this.random = random;
			this.dnaSize = dnaSize;
			this.getRandomGeneType = getRandomGeneType;
			this.fitnessFunction = fitnessFunction;

			BestGenes = new CellInformation[dnaSize.x, dnaSize.y, dnaSize.z];

			for (int i = 0; i < populationSize; i++)
			{
				Population.Add(new DNA(dnaSize, random, getRandomGeneType, fitnessFunction, waypoints));
			}
		}

		public void NewGeneration(int numNewDNA = 0, bool crossoverNewDNA = false)
		{
			int finalCount = Population.Count + numNewDNA;

			if (finalCount <= 0)
			{
				return;
			}

			if (Population.Count > 0)
			{
				CalculateFitness();
				Population.Sort(CompareDNA);
			}
			newPopulation.Clear();

			for (int i = 0; i < finalCount; i++)
			{
				if (i < Elitism && i < Population.Count)
				{
					newPopulation.Add(Population[i]);
				}
				else if (i < Population.Count || crossoverNewDNA)
				{
					DNA parent1 = ChooseParent();
					DNA parent2 = ChooseParent();

					DNA child = parent1.Crossover(parent2);

					child.Mutate(MutationRate);

					newPopulation.Add(child);
				}
				else
				{
					newPopulation.Add(new DNA(dnaSize, random, getRandomGeneType, fitnessFunction));
				}
			}

			List<DNA> tmpList = Population;
			Population = newPopulation;
			newPopulation = tmpList;

			Generation++;
		}

		private int CompareDNA(DNA a, DNA b)
		{
			if (a.Fitness > b.Fitness)
			{
				return -1;
			}
			else if (a.Fitness < b.Fitness)
			{
				return 1;
			}
			else
			{
				return 0;
			}
		}

		private void CalculateFitness()
		{
			fitnessSum = 0;
			DNA best = Population[0];

			for (int i = 0; i < Population.Count; i++)
			{
				fitnessSum += Population[i].CalculateFitness(i);

				if (Population[i].Fitness > best.Fitness)
				{
					best = Population[i];
				}
			}

			BestFitness = best.Fitness;
			Array.Copy(best.Genes, 0, BestGenes, 0, best.Genes.Length);
		}

		private DNA ChooseParent()
		{
			double randomNumber = random.NextDouble() * fitnessSum;

			for (int i = 0; i < Population.Count; i++)
			{
				if (randomNumber < Population[i].Fitness)
				{
					return Population[i];
				}

				randomNumber -= Population[i].Fitness;
			}

			return Population[0];
		}
	}
}
