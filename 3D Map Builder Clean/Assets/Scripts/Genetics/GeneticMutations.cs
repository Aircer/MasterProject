using System.Collections.Generic;
using System.Linq;
using System;
using UtilitiesGenetic;
using mVectors;

namespace Genetics
{
    public static class Mutations
    {
		public static Vector3Int size;
		public static SharpNeatLib.Maths.FastRandom random;
		public static TypeParams[] typeParams;

		public static void InitMutations(Vector3Int sizeDNA, SharpNeatLib.Maths.FastRandom rand, TypeParams[] tp)
		{
			//sizeDNA is the size of the grid + 2, so we don't have to check the boundaries, then for the mutations the limit is grid + 1
			size = new Vector3Int(sizeDNA.x - 1, sizeDNA.y - 1, sizeDNA.z - 1);
			random = rand;
			typeParams = tp;

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
			
			if (typeParams[Genes[input.x][input.y][input.z]].floor)
			{ 
				Genes = MutationsFloor.DeleteFloor(Genes, input);
			}

			if (typeParams[Genes[input.x][input.y][input.z]].ladder)
			{
				int mutationType = random.Next(2);

				if (mutationType == 0)
					Genes = MutationsLadders.TranslateLadder(Genes, input);
				if (mutationType == 1)
					Genes = MutationsLadders.DeleteLadder(Genes, input);
			}

			if (typeParams[Genes[input.x][input.y][input.z]].door)
			{
				int mutationType = random.Next(2);

				if (mutationType == 0)
					Genes = MutationsDoors.CollapseDoor(Genes, input);
				if (mutationType == 1)
					Genes = MutationsDoors.TranslateDoor(Genes, input);
			}
			
			if (typeParams[Genes[input.x][input.y][input.z]].wall)
			{
				int mutationType = random.Next(1);
				
				if (mutationType == 0)
					Genes = MutationsWalls.TranslationWall(Genes, input);
				if (mutationType == 1)
					Genes = MutationsWalls.RotationWall(Genes, input);
				if (mutationType == 2)
					Genes = MutationsWalls.DeleteWallZ(Genes, input);
				if (mutationType == 3)
					Genes = MutationsWalls.DeleteWallX(Genes, input);
				if (mutationType == 4)
					Genes = MutationsDoors.CreateDoor(Genes, input, 3);
			}

			if (typeParams[Genes[input.x][input.y][input.z]].stair)
			{
				int mutationType = random.Next(2);

				if (mutationType == 0)
					Genes = MutationsStairs.DestroyStair(Genes, input);
				if (mutationType == 1)
					Genes = MutationsStairs.MoveStair(Genes, input, 8);
			}

			if (Genes[input.x][input.y][input.z] == 0)
			{
				int mutationType = random.Next(60);

				if (mutationType < 10)
					Genes = MutationsWalls.FillWallX(Genes, input, 11);
				if (mutationType > 10 && mutationType < 20)
					Genes = MutationsWalls.FillWallZ(Genes, input, 11);
				if (mutationType > 20 && mutationType < 60)
					Genes = MutationsFloor.FillFloor(Genes, input, 5);
				if (mutationType > 60 && mutationType < 80)
					Genes = MutationsLadders.CreateLadder(Genes, input, 6);
				if(mutationType > 80)
					Genes = MutationsStairs.CreateStair(Genes, input, 8);
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