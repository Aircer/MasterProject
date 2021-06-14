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

		public static void InitFitness(Phenotype initPhen, Vector3Int sizeDNA, EvolutionaryAlgoParams algoParams)
		{
			initialPhenotype = initPhen;
			volumeMax = (sizeDNA.x - 2) * (sizeDNA.y - 2) * (sizeDNA.z - 2);
			size = sizeDNA;
			weightFitnessDifference = algoParams.wDifference;
			weightFitnessWalkingAreas = algoParams.wWalkingAreas;
			weightFitnessWallsCuboids = algoParams.wWallsCuboids;
			weightFitnessPathfinding  = algoParams.wPathfinding;
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
			int nbWalls = phenotype.walls.Count;
			float badWalls = 0;
			float ratioGoodWalls = 0;
			float totalVolume = 0;
			float totalBadVolume = 0;
			float ratioGoodVolume = 0;

			foreach (Cuboid wall in phenotype.walls)
			{
				if (wall.width > FitnessConstants.WALL_WIDTH_MAX 
					|| wall.height < FitnessConstants.WALL_HEIGHT_MIN 
					|| wall.length < FitnessConstants.WALL_LENGTH_MIN
					|| (wall.inCuboids.Count == 0 && wall.outCuboids.Count == 0)
					|| (wall.bottomEmpty.Count > 0))
				{
					badWalls++;
					totalBadVolume += wall.width * wall.height * wall.length;
				}

				totalVolume += wall.width * wall.height * wall.length;
			}

			if(nbWalls > 0)
				ratioGoodWalls = (nbWalls - badWalls) / nbWalls;

			if (totalVolume > 0)
				ratioGoodVolume = (totalVolume - totalBadVolume) / totalVolume;

			fitnessWallsCuboids = ratioGoodVolume;

			return fitnessWallsCuboids;
		}

		public static float GetFitnessWalkingAreas(Phenotype phenotype)
		{
			float fitnessWalkingAreas = 0;

			float nbWalkableAreas = phenotype.walkableArea.Count; float ratioBadWalkableArea = 0; float badWalkableArea = 0;
			float nbPaths = phenotype.paths.Count; 

			foreach (WalkableArea wa in phenotype.walkableArea)
			{
				if (wa.bordersNotGood.Count > 0 || wa.cells.Count < FitnessConstants.WA_SIZE_MIN || wa.neighborsArea.Count == 0)
					badWalkableArea++;
			}

			if (nbWalkableAreas > 0)
				ratioBadWalkableArea = (nbWalkableAreas - badWalkableArea) / nbWalkableAreas;

			fitnessWalkingAreas = ratioBadWalkableArea;


			return fitnessWalkingAreas;
		}

		public static float GetFitnessPathfinding(Phenotype phenotype)
		{
			float fitnessPathfinding = 0;

			float nbWalkableAreas = phenotype.walkableArea.Count; float ratioBadWalkableArea = 0; float badWalkableArea = 0;
			float nbPaths = phenotype.paths.Count; float ratioBadPath = 0; float badPath = 0;

			foreach (Path path in phenotype.paths)
			{
				if (path.neighborsConnected.Count == 0)
					badPath++;
			}

			if (nbPaths > 0)
				ratioBadPath = (nbPaths - badPath) / nbPaths;

			fitnessPathfinding = ratioBadPath;

			return fitnessPathfinding;
		}
	}
}


