using System.Collections.Generic;
using System.Linq;
using System;
using UtilitiesGenetic;

namespace Genetics
{
    public static class FitnessComputation
    {
		private static Vector3Int size;
		private static float volumeMax;
		private static float weightFitnessDifference;
		private static float weightFitnessWalkingAreas;
		private static float weightFitnessWallsCuboids;
		private static float weightFitnessPathfinding;
		private static Phenotype initialPhenotype;
		private static int initialHighestWidth;
		private static TypeParams[] typeParams;

		public static void InitFitness(Phenotype initPhen, Vector3Int sizeDNA, EvolutionaryAlgoParams algoParams, TypeParams[] tP)
		{
			initialPhenotype = initPhen;
			initialHighestWidth = GetHighestWidth(initialPhenotype);
			volumeMax = (sizeDNA.x - 2) * (sizeDNA.y - 2) * (sizeDNA.z - 2);
			size = sizeDNA;
			typeParams = tP;
			weightFitnessDifference = algoParams.wDifference;
			weightFitnessWalkingAreas = algoParams.wWalkingAreas;
			weightFitnessWallsCuboids = algoParams.wWallsCuboids;
			weightFitnessPathfinding  = algoParams.wPathfinding;
		}

		public static int GetHighestWidth(Phenotype initPhen)
		{
			int highestWidth = 0;

			foreach(Cuboid wall in initPhen.walls)
			{
				if (wall.width > highestWidth)
					highestWidth = wall.width;
			}

			return highestWidth;
		}

		public static Fitness FitnessFunction(Phenotype phenotype)
		{
			Fitness fitness = new Fitness();
			float fitnessTotal = 0;
			float fitnessDifference = GetFitnessDifference(phenotype);
			float fitnessWalkingAreas = GetFitnessWalkingAreas(phenotype);
			float fitnessWallsCuboids = GetFitnessWallsCuboids(phenotype);
			float fitnessPathfinding = GetFitnessPathfinding(phenotype);

			fitnessTotal = (weightFitnessDifference * fitnessDifference + weightFitnessWalkingAreas * fitnessWalkingAreas 
						+ weightFitnessWallsCuboids * fitnessWallsCuboids + weightFitnessPathfinding * fitnessPathfinding) 
				/ (weightFitnessDifference + weightFitnessWalkingAreas + weightFitnessWallsCuboids + weightFitnessPathfinding);

			fitness.total = fitnessTotal;
			fitness.difference = fitnessDifference;
			fitness.walkingAreas = fitnessWalkingAreas;
			fitness.walls = fitnessWallsCuboids;
			fitness.pathfinding = fitnessPathfinding;

			return fitness;
		}

		public static float GetFitnessEmptyCuboids(Phenotype phenotype)
		{
			float fitnessEmptyCuboids = 0;
			int nbEmptyCuboids = phenotype.emptyCuboids.Count;
			int badEmptyCuboid = 0;
			float sizeMin = 5;
			float ratioGoodEmptyCuboids = 0;

			int currentVolume;
			float totalVolume = 0;
			float totalGoodVolume = 0;
			float ratioGoodVolume = 0;

			foreach (Cuboid cuboid in phenotype.emptyCuboids)
			{
				currentVolume = cuboid.length * cuboid.width * cuboid.height;

				if (currentVolume < sizeMin 
					&& (cuboid.outCuboids.Count + cuboid.inCuboids.Count < 2))
				{
					badEmptyCuboid++;
					totalGoodVolume += currentVolume;
				}

				totalVolume += currentVolume;
			}

			if (nbEmptyCuboids > 0)
				ratioGoodEmptyCuboids = (nbEmptyCuboids - badEmptyCuboid) / nbEmptyCuboids;

			if (totalVolume > 0)
				ratioGoodVolume = (totalVolume - totalGoodVolume) / totalVolume;

			fitnessEmptyCuboids = ratioGoodVolume;

			return fitnessEmptyCuboids;
		}

		public static float GetFitnessDifference(Phenotype phenotype)
		{
			float fitnessDifference = 0;
			float diff = 0;

			for(int x = 1; x < size.x - 1; x++)
            {
				for (int y = 1; y < size.y - 1; y++)
				{
					for (int z = 1; z < size.z - 1; z++)
					{
						if (phenotype.population.genes[x][y][z] != initialPhenotype.population.genes[x][y][z])
							diff++;
					}
				}
			}


			fitnessDifference = diff / volumeMax;

			return fitnessDifference;
		}

		public static float GetFitnessWallsCuboids(Phenotype phenotype)
		{
			float fitnessWallsCuboids = 0;
			float totalVolume = 0;
			float volumeWRating = 0;
			float rating; 

			foreach (Cuboid wall in phenotype.walls)
			{
				rating = 1f;

				if ((wall.width > FitnessConstants.WALL_WIDTH_MAX && wall.width > initialHighestWidth)
					|| wall.height < FitnessConstants.WALL_HEIGHT_MIN 
					|| wall.length < FitnessConstants.WALL_LENGTH_MIN)
				{
					rating -= 0.33f;
				}

				if (wall.inCuboids.Count == 0 && wall.outCuboids.Count == 0)
				{
					rating -= 0.33f;
				}

				if (wall.bottomEmpty.Count > 0)
				{
					rating -= 0.33f;
				}

				volumeWRating += (wall.width * wall.height * wall.length)* rating;
				totalVolume += (wall.width * wall.height * wall.length);
			}

			if (totalVolume > 0)
				fitnessWallsCuboids = volumeWRating / totalVolume;

			fitnessWallsCuboids = (float)(Math.Pow(2, fitnessWallsCuboids) - 1f);

			return fitnessWallsCuboids;
		}

		public static float GetFitnessWalkingAreas(Phenotype phenotype)
		{
			float fitnessWalkingAreas = 0;
			float totalArea = 0;
			float areaWRating = 0;
			float rating;

			foreach (WalkableArea wa in phenotype.walkableArea)
			{
				rating = 1f;
				if (wa.cells.Count < FitnessConstants.WA_SIZE_MIN)
					rating -= 0.33f;

				if (wa.bordersNotGood.Count > 0)
					rating -= 0.33f;

				if (wa.neighborsArea.Count == 0)
					rating -= 0.33f;

				areaWRating += wa.cells.Count* rating;
				totalArea += wa.cells.Count;
			}

			if (totalArea > 0)
				fitnessWalkingAreas = areaWRating / totalArea;

			fitnessWalkingAreas = (float)(Math.Pow(2, fitnessWalkingAreas) - 1f);

			return fitnessWalkingAreas;
		}

		public static float GetFitnessPathfinding(Phenotype phenotype)
		{
			float fitnessPathfinding = 0.01f;
			float totalVolume = 0;
			float volumeRating = 0;
			float rating;

			foreach (Path path in phenotype.paths)
			{
				rating = 1f;

				if (path.neighborsConnected.Count == 0 || (typeParams[path.type].door && path.cells.Count != 2))
					rating = 0.0f;

				if (PathsHaveCommunWA(phenotype, path) && rating > 0)
					rating -= 0.5f;

				volumeRating += rating;
				totalVolume ++;
			}

			if (totalVolume > 0)
				fitnessPathfinding = volumeRating / totalVolume;

			fitnessPathfinding = (float)(Math.Pow(2, fitnessPathfinding) - 1f);

			return fitnessPathfinding;
		}

		private static bool PathsHaveCommunWA(Phenotype phenotype, Path path)
		{
			int numberCommunWA;
			foreach (Path otherPath in phenotype.paths)
			{
				if (otherPath.cells != path.cells)
				{
					numberCommunWA = 0;

					foreach (WalkableArea otherPathWA in otherPath.neighborsConnected)
					{
						if (path.neighborsConnected.Contains(otherPathWA))
						{
							numberCommunWA++;
						}
					}

					if (numberCommunWA > 1)
						return true;
				}
			}

			return false;
		}
	}
}


