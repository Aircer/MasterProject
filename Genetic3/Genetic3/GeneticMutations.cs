using System.Collections.Generic;
using System.Linq;
using System;
using UtilitiesGenetic;

namespace Genetics
{
    public static class Mutations
    {
		public static Vector3Int size;
		public static SharpNeatLib.Maths.FastRandom random;
		public static TypeParams[] typeParams;
		public static MutationsType mutationType;

		public static void InitMutations(Vector3Int sizeDNA, SharpNeatLib.Maths.FastRandom rand, TypeParams[] tp, MutationsType mutType)
		{
			//sizeDNA is the size of the grid + 2, so we don't have to check the boundaries, then for the mutations the limit is grid + 1
			size = new Vector3Int(sizeDNA.x - 1, sizeDNA.y - 1, sizeDNA.z - 1);
			random = rand;
			typeParams = tp;
			mutationType = mutType;

			MutationsStairs.InitMutations(size, random, typeParams);
			MutationsWalls.InitMutations(size, random, typeParams);
			MutationsFloor.InitMutations(size, random, typeParams);
			MutationsLadders.InitMutations(size, random, typeParams);
			MutationsDoors.InitMutations(size, random, typeParams);

			PhenotypeCompute.InitMutations(size, random, typeParams);
		}

		public static int[][][] Mutate(int[][][] Genes)
        {
			int mutationIndex_x = random.Next(1, size.x);
			int mutationIndex_y = random.Next(1, size.y - 1);
			int mutationIndex_z = random.Next(1, size.z);

			Vector3Int input = new Vector3Int(mutationIndex_x, mutationIndex_y, mutationIndex_z);

			if (typeParams[Genes[input.x][input.y][input.z]].floor && mutationType != MutationsType.NoFloor)
			{
				Genes = MutationsFloor.DeleteFloor(Genes, input);
			}

			if (typeParams[Genes[input.x][input.y][input.z]].ladder && mutationType != MutationsType.NoPathsUp)
			{
				int val = mutationType == MutationsType.NoTransformations ? 1: random.Next(2);

				if (val == 0)
					Genes = MutationsLadders.TranslateLadder(Genes, input);
				if (val == 1)
					Genes = MutationsLadders.DeleteLadder(Genes, input);
			}

			if (typeParams[Genes[input.x][input.y][input.z]].door && mutationType != MutationsType.NoDoors)
			{
				int val = mutationType == MutationsType.NoTransformations ? 1 : random.Next(2);

				if (val == 0)
					Genes = MutationsDoors.TranslateDoor(Genes, input);
				if (val == 1)
					Genes = MutationsDoors.CollapseDoor(Genes, input);
			}
			
			if (typeParams[Genes[input.x][input.y][input.z]].wall && mutationType != MutationsType.NoWalls)
			{
				int val = mutationType == MutationsType.NoTransformations ? random.Next(2, 5) : random.Next(5);

				if (val == 0)
					Genes = MutationsWalls.TranslationWall(Genes, input);
				if (val == 1)
					Genes = MutationsWalls.RotationWall(Genes, input);
				if (val == 2 && mutationType != MutationsType.NoDoors)
					Genes = MutationsDoors.CreateDoor(Genes, input, 1);
				if (val == 3)
					Genes = MutationsWalls.DeleteWallZ(Genes, input);
				if (val == 4)
					Genes = MutationsWalls.DeleteWallX(Genes, input);
			}

			if (typeParams[Genes[input.x][input.y][input.z]].stair && mutationType != MutationsType.NoPathsUp)
			{
				int val = mutationType == MutationsType.NoTransformations ? 1 : random.Next(2);

				if (val == 0)
					Genes = MutationsStairs.MoveStair(Genes, input, 4);
				if (val == 1)
					Genes = MutationsStairs.DestroyStair(Genes, input);
			}

			if (Genes[input.x][input.y][input.z] == 0)
			{
				int val = random.Next(100);

				if (val < 5 && mutationType != MutationsType.NoWalls)
					Genes = MutationsWalls.FillWallX(Genes, input, 5);
				if (val > 5 && val < 10 && mutationType != MutationsType.NoWalls)
					Genes = MutationsWalls.FillWallZ(Genes, input, 5);
				if (val > 10 && val < 60 && mutationType != MutationsType.NoFloor)
					Genes = MutationsFloor.FillFloor(Genes, input, 2);
				if (val > 60 && val < 65 && mutationType != MutationsType.NoPathsUp)
					Genes = MutationsLadders.CreateLadder(Genes, input, 3);
				if (val > 65 && val < 70 && mutationType != MutationsType.NoPathsUp)
					Genes = MutationsStairs.CreateStair(Genes, new Vector3Int(input.x, input.y, input.z), 4);
			}

			return Genes;
		}

		public static bool CellIsStruct(int x, int y, int z, int[][][] Genes)
        {
            if (typeParams[Genes[x][y][z]].floor || typeParams[Genes[x][y][z]].wall)
                return true;
            else
                return false;
        }

    }
}