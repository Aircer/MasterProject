using System.Collections.Generic;
using System.Linq;
using System;
using UtilitiesGenetic;
using mVectors;

namespace Genetics
{
    public static class Fitness
    {
		private static int numberBlocks;
		private static float weightFitnessEmptyCuboids;
		private static float weightFitnessWallsCuboids;
		private static float weightFitnessPathfinding;

		public static void InitFitness(Vector3Int size, EvolutionaryAlgoParams algoParams)
		{
			numberBlocks = (size.x - 2) * (size.y - 2) * (size.z - 2);
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
			float fitnessEmptyCuboids;
			float nbCuboidsCorrectSize = 0;
			float nbEmptyCuboids = 0;
			float ratioGoodCuboids;
			float sizeMin = 5;
			float maxSize = 0;
			float nbCuboidsPossible = numberBlocks / sizeMin;
			int nbOut = 0;

			foreach (Cuboid cuboid in phenotype.emptyCuboids)
			{
				if ((cuboid.max.x - cuboid.min.x) * (cuboid.max.y - cuboid.min.y) * (cuboid.max.z - cuboid.min.z) > sizeMin)
					nbCuboidsCorrectSize++;
				else if ((cuboid.max.x - cuboid.min.x) * (cuboid.max.y - cuboid.min.y) * (cuboid.max.z - cuboid.min.z) > maxSize)
				{
					maxSize = (cuboid.max.x - cuboid.min.x) * (cuboid.max.y - cuboid.min.y) * (cuboid.max.z - cuboid.min.z);
					nbOut = cuboid.outCuboids.Count;
				}

				nbEmptyCuboids++;
			}

			if (nbEmptyCuboids > 0)
				ratioGoodCuboids = nbCuboidsCorrectSize / nbCuboidsPossible;
			else
				ratioGoodCuboids = 0;

			fitnessEmptyCuboids = ratioGoodCuboids;

			return fitnessEmptyCuboids;
		}

		public static float GetFitnessWallsCuboids(Phenotype phenotype)
		{
			float fitnessWallsCuboids = 0;
			int nbWalls = phenotype.walls.Count;
			float badWalls = 0;

			foreach (Cuboid wall in phenotype.walls)
			{
				if (wall.width > 1 || wall.height < 2 || wall.length < 2
					||(wall.inCuboids.Count == 0 && wall.outCuboids.Count == 0))
				{
					badWalls++;
				}
			}

			if(nbWalls > 0)
				fitnessWallsCuboids = (nbWalls - badWalls) / nbWalls;

			return fitnessWallsCuboids;
		}

		public static float GetFitnessPathfinding(Phenotype phenotype)
		{
			float fitnessPathfinding = 0;

			
			foreach (WalkableArea path in phenotype.walkableArea)
			{
				if (path.cells.Count < 3)
					fitnessPathfinding = 0; 
			}

			if (phenotype.walkableArea.Count == 0)
				fitnessPathfinding = 0;


			return fitnessPathfinding;
		}
	}
}


