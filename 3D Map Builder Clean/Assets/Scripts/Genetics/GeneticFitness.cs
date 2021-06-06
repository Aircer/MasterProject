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
		private static int fitnessType;

		public static void InitFitness(Vector3Int size, int fT)
		{
			numberBlocks = (size.x - 2) * (size.y - 2) * (size.z - 2);
			fitnessType = fT;
		}

		public static float FitnessFunction(Phenotype phenotype)
		{
			float finalScore = 0; float nbCuboidsCorrectSize = 0; float nbCuboids = 0;
			float ratioGoodCuboids = 0; float sizeMin = 0; float nbCuboidsPossible;
			sizeMin = 5;

			nbCuboidsPossible = numberBlocks / sizeMin;

			foreach (Cuboid cuboid in phenotype.emptyCuboids)
			{
				if ((cuboid.max.x - cuboid.min.x) * (cuboid.max.y - cuboid.min.y) * (cuboid.max.z - cuboid.min.z) > sizeMin)
					nbCuboidsCorrectSize++;
				nbCuboids++;
			}

			if (nbCuboids > 0)
				ratioGoodCuboids = nbCuboidsCorrectSize / nbCuboidsPossible;
			else
				ratioGoodCuboids = 0;

			finalScore = ratioGoodCuboids; //Mathf.Pow(2, ratioGoodCuboids) - 1;

			/*
			foreach (WalkableArea path in phenotype.walkableArea)
			{
				if (path.cells.Count < 3)
					finalScore = 0; 
			}

			if (phenotype.walkableArea.Count == 0)
				finalScore = 0;*/


			float maxSize = 0;
			int nbOut = 0;
			foreach (Cuboid cuboid in phenotype.emptyCuboids)
			{
				if ((cuboid.max.x - cuboid.min.x) * (cuboid.max.y - cuboid.min.y) * (cuboid.max.z - cuboid.min.z) > maxSize)
				{
					maxSize = (cuboid.max.x - cuboid.min.x) * (cuboid.max.y - cuboid.min.y) * (cuboid.max.z - cuboid.min.z);
					nbOut = cuboid.outCuboids.Count;
				}
			}

			if (maxSize > 0)
				finalScore = nbOut / maxSize;
			else
				finalScore = 0;

			finalScore = phenotype.emptyCuboids.Count;

			foreach (Cuboid wall in phenotype.walls)
			{
				if ((wall.max.x - wall.min.x) > 1 && (wall.max.z - wall.min.z) > 1)
				{
					finalScore = 0;
				}
			}

			return finalScore;
		}
	}
}


