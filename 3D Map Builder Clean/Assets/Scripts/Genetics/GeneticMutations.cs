using System.Collections.Generic;
using System.Linq;
using System;
using UtilitiesGenetic;

namespace Genetics
{
    public class Mutations
    {
		public Vector3Int size;
		public SharpNeatLib.Maths.FastRandom random;
		public TypeParams[] typeParams;
		public MutationsType mutationType;

		private MutationsStairs mutationsStairs;
		private MutationsDoors mutationsDoors;
		private MutationsLadders mutationsLadders;
		private MutationsWalls mutationsWalls;
		private MutationsFloor mutationsFloor;

		public void InitMutations(Vector3Int sizeDNA, SharpNeatLib.Maths.FastRandom rand, TypeParams[] tp, MutationsType mutType)
		{
			//sizeDNA is the size of the grid + 2, so we don't have to check the boundaries, then for the mutations the limit is grid + 1
			size = new Vector3Int(sizeDNA.x - 1, sizeDNA.y - 1, sizeDNA.z - 1);
			random = rand;
			typeParams = tp;
			mutationType = mutType;


			mutationsStairs = new MutationsStairs();
			mutationsStairs.InitMutations(size, random, typeParams);
			mutationsWalls = new MutationsWalls();
			mutationsWalls.InitMutations(size, random, typeParams);
			mutationsFloor = new MutationsFloor();
			mutationsFloor.InitMutations(size, random, typeParams);
			mutationsLadders = new MutationsLadders();
			mutationsLadders.InitMutations(size, random, typeParams);
			mutationsDoors = new MutationsDoors();
			mutationsDoors.InitMutations(size, random, typeParams);
		}

		public int[][][] Mutate(int[][][] Genes)
        {
			int mutationIndex_x = random.Next(1, size.x);
			int mutationIndex_y = random.Next(1, size.y - 1);
			int mutationIndex_z = random.Next(1, size.z);

			Vector3Int input = new Vector3Int(mutationIndex_x, mutationIndex_y, mutationIndex_z);

			if (typeParams[Genes[input.x][input.y][input.z]].floor 
				&& mutationType != MutationsType.NoFloor && mutationType != MutationsType.OnlyTransformations
				&& mutationType != MutationsType.NoCreateDeleteFloorAndWalls)
			{
				Genes = mutationsFloor.DeleteFloor(Genes, input);
			}

			if (typeParams[Genes[input.x][input.y][input.z]].ladder && mutationType != MutationsType.NoPathsUp)
			{
				int val = random.Next(2);
				if (mutationType == MutationsType.OnlyTransformations) val = 0;
				if (mutationType == MutationsType.NoTransformations) val = 1;

				if (val == 0)
					Genes = mutationsLadders.TranslateLadder(Genes, input);
				if (val == 1)
					Genes = mutationsLadders.DeleteLadder(Genes, input);
			}

			if (typeParams[Genes[input.x][input.y][input.z]].door && mutationType != MutationsType.NoDoors)
			{
				int val = random.Next(2);
				if (mutationType == MutationsType.OnlyTransformations) val = 0;
				if (mutationType == MutationsType.NoTransformations) val = 1;

				if (val == 0)
					Genes = mutationsDoors.TranslateDoor(Genes, input);
				if (val == 1)
					Genes = mutationsDoors.CollapseDoor(Genes, input);
			}
			
			if (typeParams[Genes[input.x][input.y][input.z]].wall && mutationType != MutationsType.NoWalls)
			{
				int val = random.Next(5);
				if (mutationType == MutationsType.OnlyTransformations) val = random.Next(0, 2);
				if (mutationType == MutationsType.NoTransformations) val = random.Next(2, 5);
				if (mutationType == MutationsType.NoCreateDeleteFloorAndWalls) val = random.Next(0, 4);

				if (val == 0)
					Genes = mutationsWalls.TranslationWall(Genes, input);
				if (val == 1)
					Genes = mutationsWalls.RotationWall(Genes, input);
				if (val == 2 && mutationType != MutationsType.NoDoors)
					Genes = mutationsDoors.CreateDoor(Genes, input, 1);
				if (val == 3)
					Genes = mutationsWalls.DeleteWallZ(Genes, input);
				if (val == 4)
					Genes = mutationsWalls.DeleteWallX(Genes, input);
			}

			if (typeParams[Genes[input.x][input.y][input.z]].stair && mutationType != MutationsType.NoPathsUp)
			{
				int val = random.Next(2);
				if (mutationType == MutationsType.OnlyTransformations) val = 0;
				if (mutationType == MutationsType.NoTransformations) val = 1;

				if (val == 0)
					Genes = mutationsStairs.MoveStair(Genes, input, 4);
				if (val == 1)
					Genes = mutationsStairs.DestroyStair(Genes, input);
			}

			if (Genes[input.x][input.y][input.z] == 0 
				&& mutationType != MutationsType.OnlyTransformations)
			{
				int val = random.Next(100);
				if(mutationType == MutationsType.NoCreateDeleteFloorAndWalls) val = random.Next(60, 100);
				if (val < 5 && mutationType != MutationsType.NoWalls)
					Genes = mutationsWalls.FillWallX(Genes, input, 5);
				if (val > 5 && val < 10 && mutationType != MutationsType.NoWalls)
					Genes = mutationsWalls.FillWallZ(Genes, input, 5);
				if (val > 10 && val < 60 && mutationType != MutationsType.NoFloor)
					Genes = mutationsFloor.FillFloor(Genes, input, 2);
				if (val > 60 && val < 65 && mutationType != MutationsType.NoPathsUp)
					Genes = mutationsLadders.CreateLadder(Genes, input, 3);
				if (val > 65 && val < 70 && mutationType != MutationsType.NoPathsUp)
					Genes = mutationsStairs.CreateStair(Genes, new Vector3Int(input.x, input.y, input.z), 4);
			}

			return Genes;
		}

		public bool CellIsStruct(int x, int y, int z, int[][][] Genes)
        {
            if (typeParams[Genes[x][y][z]].floor || typeParams[Genes[x][y][z]].wall)
                return true;
            else
                return false;
        }

    }
}