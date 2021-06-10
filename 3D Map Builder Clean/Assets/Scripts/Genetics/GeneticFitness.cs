using System.Collections.Generic;
using System.Linq;
using System;
using UtilitiesGenetic;
using mVectors;

namespace Genetics
{
    public static class Fitness
    {
		private static float volumeMax;
		private static float weightFitnessEmptyCuboids;
		private static float weightFitnessWallsCuboids;
		private static float weightFitnessPathfinding;

		public static void InitFitness(Vector3Int size, EvolutionaryAlgoParams algoParams)
		{
			volumeMax = (size.x - 2) * (size.y - 2) * (size.z - 2);
			weightFitnessEmptyCuboids = algoParams.wEmptyCuboids;
			weightFitnessWallsCuboids = algoParams.wWallsCuboids;
			weightFitnessPathfinding  = algoParams.wPathfinding;
		}

		public static float FitnessFunction(Phenotype phenotype)
		{
			float fitnessTotal = 0;
			float fitnessEmptyCuboids = GetFitnessEmptyCuboids(phenotype);
			float fitnessWallsCuboids = GetFitnessWallsCuboids(phenotype);
			float fitnessPathfinding = GetFitnessPathfinding(phenotype);

			fitnessTotal = (weightFitnessEmptyCuboids * fitnessEmptyCuboids + weightFitnessWallsCuboids * fitnessWallsCuboids 
																			+ weightFitnessPathfinding * fitnessPathfinding) 
							/ (weightFitnessEmptyCuboids + weightFitnessWallsCuboids + weightFitnessPathfinding);

			return fitnessTotal;
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
				if (wall.width > 1 || wall.height < 2 || wall.length < 2
					||(wall.inCuboids.Count == 0 && wall.outCuboids.Count == 0))
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

		public static float GetFitnessPathfinding(Phenotype phenotype)
		{
			float fitnessPathfinding = 0;

			float nbWalkableAreas = phenotype.walkableArea.Count; float ratioBadWalkableArea = 0; float badWalkableArea = 0;
			float nbPaths = phenotype.paths.Count; float ratioBadPath = 0; float badPath = 0;

			foreach (WalkableArea wa in phenotype.walkableArea)
			{
				if (wa.bordersNotGood.Count > 0 || wa.cells.Count < 3 || wa.neighborsArea.Count == 0)
					badWalkableArea++; 
			}

			if(nbWalkableAreas > 0)
				ratioBadWalkableArea = (nbWalkableAreas - badWalkableArea) / nbWalkableAreas;

			foreach (Path path in phenotype.paths)
			{
				if (path.neighborsConnected.Count == 0)
					badPath++;
			}

			if (nbPaths > 0)
				ratioBadPath = (nbPaths - badPath) / nbPaths;

			fitnessPathfinding = (0.5f*ratioBadWalkableArea + ratioBadPath) / 1.5f;


			return fitnessPathfinding;
		}
	}
}


