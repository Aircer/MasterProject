using System;
using System.Collections.Generic;
using UtilitiesGenetic;

namespace Genetics
{
	public class GeneticAlgorithm
	{
		public DNA[] oldPopulation { get; private set; }
		public DNA[] newPopulation { get; private set; }
		public Fitness[][] fitnessPopulation { get; private set; }
		public Population[][] populations { get; private set; }

		public int generation;
		private int elitism;
		private int populationSize;
		private float mutationNumber;
		private Vector3Int sizeDNA;
		private float fitnessSum;
		private SharpNeatLib.Maths.FastRandom randomFast;
		private List<int> existingTypes;
		private CrossoverType crossoverType;
		public GeneticAlgorithm(EvolutionaryAlgoParams algoParams, Vector3Int dnaSize, 
			int[][][] waypointParams, TypeParams[] typeParams, SharpNeatLib.Maths.FastRandom randomFast)
		{
			sizeDNA = dnaSize;
			generation = 1;
			elitism = algoParams.elitism;
			populationSize = algoParams.population;
			mutationNumber = algoParams.mutationRate* dnaSize.x* dnaSize.y* dnaSize.z;
			crossoverType = algoParams.crossoverType;

			oldPopulation = new DNA[populationSize];
			newPopulation = new DNA[populationSize];

			this.randomFast = randomFast;

			fitnessPopulation = new Fitness[algoParams.generations + 1][];
			populations = new Population[algoParams.generations + 1][];

			for (int i = 0; i < populationSize; i++)
			{
				DNA newPop = new DNA(dnaSize, typeParams, randomFast, waypointParams);
				oldPopulation[i] = newPop;
				DNA newPop2 = new DNA(dnaSize, typeParams, randomFast, waypointParams);
				newPopulation[i] = newPop2;
			}

			FitnessComputation.InitFitness(PhenotypeCompute.GetPhenotype(oldPopulation[0].Genes), dnaSize, algoParams, typeParams);
			existingTypes = oldPopulation[0].ExistingTypes();

			populations[0] = new Population[populationSize];
			for (int i = 0; i < populationSize; i++)
			{
				populations[0][i] = new Population();
				populations[0][i].Copy(newPopulation[0].Genes, sizeDNA);
			}

			fitnessPopulation[0] = ClassifyPopulation();

			/*
			foreach (Cuboid wall in oldPopulation[0].phenotype.walls)
			{
				UnityEngine.Debug.Log(wall.length + " x " + wall.height + " x " + wall.width);
			}*/
		}

		public void NewGeneration()
		{
			for (int i = 0; i < populationSize; i++)
			{
				if (i < elitism)
				{
					newPopulation[i].Copy(oldPopulation[i]);
				}
				else
				{
					if (crossoverType == CrossoverType.Copy)
					{
						DNA parent = ChooseParent();
						newPopulation[i].Copy(parent);
					}

					if (crossoverType == CrossoverType.Swap)
					{
						DNA parent1 = ChooseParent();
						DNA parent2 = ChooseParent();
						newPopulation[i].Crossover(parent1, parent2);
					}

					newPopulation[i].Mutate(mutationNumber);
				}
			}

			DNA[] tmpArray = oldPopulation;
			oldPopulation = newPopulation;
			newPopulation = tmpArray;

			fitnessPopulation[generation] = ClassifyPopulation();

			populations[generation] = new Population[populationSize];
			for (int i = 0; i < populationSize; i++)
			{
				populations[generation][i] = new Population();
				populations[generation][i].Copy(oldPopulation[i].Genes, sizeDNA);
			}

			generation++;
		}

		public Fitness[] ClassifyPopulation()
		{
			if (populationSize > 0)
			{
				Fitness[] fitnessPop = CalculateFitness();

				DNA temp;

				for (int i = 0; i < populationSize - 1; i++)
				{
					for (int j = i + 1; j < populationSize; j++)
					{
						if (oldPopulation[i].fitness.total < oldPopulation[j].fitness.total)
						{

							temp = oldPopulation[i];
							oldPopulation[i] = oldPopulation[j];
							oldPopulation[j] = temp;
						}
					}
				}
				return fitnessPop;
			}

			return null;
		}

		private Fitness[] CalculateFitness()
		{
			Fitness[] fitnessPop = new Fitness[populationSize];

			fitnessSum = 0;
			for (int i = 0; i < populationSize; i++)
			{
				fitnessPop[i] = oldPopulation[i].CalculateFitness();
				fitnessSum += fitnessPop[i].total;
			}

			return fitnessPop;
		}

		private DNA ChooseParent()
		{
			double randomNumber = randomFast.NextDouble() * fitnessSum;

			for (int i = 0; i < populationSize; i++)
			{
				if (randomNumber < oldPopulation[i].fitness.total)
				{
					return oldPopulation[i];
				}

				randomNumber -= oldPopulation[i].fitness.total;
			}

			return oldPopulation[0];
		}
	}
}
