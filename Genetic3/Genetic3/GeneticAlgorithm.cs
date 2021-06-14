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
			fitnessPopulation = new Fitness[algoParams.generations][];
			populations = new Population[algoParams.generations][];

			FitnessComputation.InitFitness(PhenotypeCompute.GetPhenotype(oldPopulation[0].Genes), dnaSize, algoParams);
		}

		public void NewGeneration()
		{
			fitnessPopulation[generation - 1] = ClassifyPopulation();

			populations[generation - 1] = new Population[populationSize];

			for (int i = 0; i < populationSize; i++)
			{
				if (i < elitism)
				{
					newPopulation[i].Copy(oldPopulation[i]);
				}
				else
				{
					DNA parent1 = ChooseParent();

					newPopulation[i].Copy(parent1);
					newPopulation[i].Mutate(mutationNumber);
				}

				populations[generation - 1][i] = oldPopulation[i].phenotype.population;
			}

			DNA[] tmpArray = oldPopulation;
			oldPopulation = newPopulation;
			newPopulation = tmpArray;

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
