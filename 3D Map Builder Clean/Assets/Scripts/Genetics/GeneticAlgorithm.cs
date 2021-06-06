using System;
using System.Collections.Generic;
using UtilitiesGenetic;
using mVectors;

namespace Genetics
{
	public class GeneticAlgorithm
	{
		public DNA[] oldPopulation { get; private set; }
		public DNA[] newPopulation { get; private set; }

		private int generation;
		private int elitism;
		private int populationSize;
		private float mutationNumber;

		private float fitnessSum;
		private SharpNeatLib.Maths.FastRandom randomFast;
		private List<int> existingTypes;

		public GeneticAlgorithm(EvolutionaryAlgoParams algoParams, Vector3Int dnaSize, 
			int[][][] waypointParams, TypeParams[] typeParams, SharpNeatLib.Maths.FastRandom randomFast)
		{
			generation = 1;
			elitism = algoParams.elitism;
			populationSize = algoParams.population;
			mutationNumber = algoParams.mutationRate* dnaSize.x* dnaSize.y* dnaSize.z;

			oldPopulation = new DNA[populationSize];
			newPopulation = new DNA[populationSize];

			this.randomFast = randomFast;

			for (int i = 0; i < populationSize; i++)
			{
				DNA newPop = new DNA(dnaSize, typeParams, randomFast, waypointParams);
				oldPopulation[i] = newPop;
				DNA newPop2 = new DNA(dnaSize, typeParams, randomFast, waypointParams);
				newPopulation[i] = newPop2;
			}

			existingTypes = oldPopulation[0].ExistingTypes();
		}

		public void NewGeneration()
		{
			ClassifyPopulation();
			
			if (generation == 1)
			{
				UnityEngine.Debug.Log(oldPopulation[0].phenotype.walls.Count);

				foreach (Cuboid wall in oldPopulation[0].phenotype.walls)
				{
					UnityEngine.Debug.Log("Size Wall: "  + (wall.max.x - wall.min.x) + " " + (wall.max.y - wall.min.y) + " " + (wall.max.z - wall.min.z));
				}
			}

			for (int i = 0; i < populationSize; i++)
			{
				if (i < elitism)
				{
					newPopulation[i].Copy(oldPopulation[i]);
				}
				else 
				{
					DNA parent1 = ChooseParent();
					DNA parent2 = ChooseParent();
					newPopulation[i].Crossover(parent1, parent2);

					newPopulation[i].Copy(parent1);
					newPopulation[i].Mutate(mutationNumber, existingTypes);
				}
			}
			
			DNA[] tmpArray = oldPopulation;
			oldPopulation = newPopulation;
			newPopulation = tmpArray;

			generation++;
		}

		public void ClassifyPopulation()
		{
			if (populationSize > 0)
			{
				CalculateFitness();

				DNA temp;
				int populationSizeMinus = populationSize - 1;

				for (int i = 0; i < populationSizeMinus; i++)
				{
					for (int j = i + 1; j < populationSize; j++)
					{
						if (oldPopulation[i].fitness < oldPopulation[j].fitness)
						{

							temp = oldPopulation[i];
							oldPopulation[i] = oldPopulation[j];
							oldPopulation[j] = temp;
						}
					}
				}
			}
		}

		private void CalculateFitness()
		{
			fitnessSum = 0;
			for (int i = 0; i < populationSize; i++)
			{
				fitnessSum += oldPopulation[i].CalculateFitness();
			}
		}

		private DNA ChooseParent()
		{
			double randomNumber = randomFast.NextDouble() * fitnessSum;

			for (int i = 0; i < populationSize; i++)
			{
				if (randomNumber < oldPopulation[i].fitness)
				{
					return oldPopulation[i];
				}

				randomNumber -= oldPopulation[i].fitness;
			}

			return oldPopulation[0];
		}
	}
}
