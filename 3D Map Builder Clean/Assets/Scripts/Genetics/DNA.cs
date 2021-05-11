using System;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using MathNet.Numerics.LinearAlgebra;

namespace MapTileGridCreator.Core
{
	public class DNA
	{
		//public WaypointCluster GenesCluster { get; private set; }
		public WaypointParams[][][] Genes { get; private set; }
		public Matrix<Double>[] typeMatrix;
		public float Fitness { get; private set; }

		private System.Random random;
		private Func<int> getRandomType;
		private Func<int, float> fitnessFunction;
		private TypeParams[] typeParams;

		public Phenotype phenotype;
		public Vector3Int sizeDNA;

		public DNA(Vector3Int size, System.Random random, Func<int> getRandomType, Func<int, float> fitnessFunction,TypeParams[] typeParams, WaypointCluster cluster = null)
		{
			//Genes are bigger than cluster to have empty borders thus it is easier to get neighbors  
			Genes = new WaypointParams[size.x+2][][];
			this.random = random;
			this.getRandomType = getRandomType;
			this.fitnessFunction = fitnessFunction;;
			this.typeParams = typeParams;

			sizeDNA = new Vector3Int(size.x + 2, size.y + 2, size.z + 2);
			phenotype = new Phenotype();
			typeMatrix = new Matrix<Double>[sizeDNA.y];

			if (cluster != null)
			{
				Genes = cluster.GetWaypointsParams();
				phenotype = IA.GetPhenotype(sizeDNA, Genes, typeParams);

				for (int y = 0; y < sizeDNA.y; y++)
				{
					Matrix<Double> subTypeMatrix = Matrix<Double>.Build.Random(sizeDNA.x, sizeDNA.z);
					for (int x = 0; x < sizeDNA.x; x++)
					{
						for (int z = 0; z < sizeDNA.z; z++)
						{
							subTypeMatrix.At(x, z, Genes[x][y][z].type);
						}
					}
					typeMatrix[y] = subTypeMatrix;
				}
			}

			/*Debug.Log("Walls X: " + phenotype.walls_x.Count);
			Debug.Log("Walls Z: " + phenotype.walls_z.Count);
			Debug.Log("Blocks Solo: " + phenotype.blocksSolo.Count);*/
		}

		public float CalculateFitness(int index)
		{
			Fitness = fitnessFunction(index);
			return Fitness;
		}

		public void Crossover(DNA otherParent)
		{
			for (int x = 1; x < sizeDNA.x - 1; x++)
			{
				for (int y = 1; y < sizeDNA.y- 1; y++)
				{
					for (int z = 1; z < sizeDNA.z - 1; z++)
					{
						typeMatrix[y].At(x, z, otherParent.Genes[x][y][z].type);
						Genes[x][y][z].type = otherParent.Genes[x][y][z].type;
						Genes[x][y][z].rotation = otherParent.Genes[x][y][z].rotation;
						Genes[x][y][z].basePos = otherParent.Genes[x][y][z].basePos;
						Genes[x][y][z].baseType = otherParent.Genes[x][y][z].baseType;
					}
				}
			}

			//phenotype = IA.GetPhenotype(Genes, typeParams);
			
			phenotype = new Phenotype();
			phenotype.blocksSolo = new HashSet<Vector3Int>(otherParent.phenotype.blocksSolo); 
			phenotype.walls_x = new HashSet<Wall>();

			foreach(Wall wallX in otherParent.phenotype.walls_x)
            {
				Wall newWall = new Wall();
				newWall.indexes = new HashSet<Vector3Int>(wallX.indexes);
				newWall.position = wallX.position;
				phenotype.walls_x.Add(newWall);
			}

			phenotype.walls_z = new HashSet<Wall>();

			foreach (Wall wallZ in otherParent.phenotype.walls_z)
			{
				Wall newWall = new Wall();
				newWall.indexes = new HashSet<Vector3Int>(wallZ.indexes);
				newWall.position = wallZ.position;
				phenotype.walls_z.Add(newWall);
			}
		}

		public void Mutate(float mutationRate)
		{
			for (int x = 0; x < mutationRate * sizeDNA.x * sizeDNA.y * sizeDNA.z; x++)
			{
				Vector3Int index = new Vector3Int(random.Next(1, sizeDNA.x-1), random.Next(1, sizeDNA.y-1), random.Next(1, sizeDNA.z-1));
				//int type = getRandomType();
				int type = 8; //random.Next(2) == 0 ? 0:8;
				Vector3 rotation = new Vector3(0, 0, 0);

				if (type > 0)
				{
					if (CanAddTypeHere(index, typeParams[type].size, rotation))
					{
						SetTypeAround(rotation, type, index);
						IA.SetWalls(ref phenotype.blocksSolo, ref phenotype.walls_x, ref phenotype.walls_z, Genes, typeParams, index);
					}
				}
				else
				{
					RemoveTypeAround(index);
					IA.UnsetWalls(ref phenotype.blocksSolo, ref phenotype.walls_x, ref phenotype.walls_z, Genes, typeParams, index);
				}
			}
		}

		private void SetTypeAround(Vector3 rotation, int type, Vector3Int index)
		{
			if (typeParams[type].size.x != 1 && typeParams[type].size.y != 1 && typeParams[type].size.z != 1)
			{
				Vector3Int lowerBound = default;
				Vector3Int upperBound = default;
				SetBounds(ref lowerBound, ref upperBound, index, typeParams[type].size, rotation);

				for (int i = lowerBound.x; i <= upperBound.x; i++)
				{
					for (int j = lowerBound.y; j <= upperBound.y; j++)
					{
						for (int k = lowerBound.z; k <= upperBound.z; k++)
						{
							Genes[i][j][k].type = type;
							Genes[i][j][k].rotation = new Vector3(0, 0, 0);
							Genes[i][j][k].basePos = index;
							Genes[i][j][k].baseType = false;
						}
					}
				}
				Genes[index.x][index.y][index.z].baseType = true;
			}
			else
			{
				Genes[index.x][index.y][index.z].type = type;
				Genes[index.x][index.y][index.z].rotation = new Vector3(0, 0, 0);
				Genes[index.x][index.y][index.z].basePos = index;
				Genes[index.x][index.y][index.z].baseType = true;
			}
		
		}

		private void RemoveTypeAround(Vector3Int index)
		{
			Vector3Int lowerBound = default(Vector3Int);
			Vector3Int upperBound = default(Vector3Int);
			Vector3Int basePos = Genes[index.x][index.y][index.z].basePos;
			WaypointParams baseWaypoint = Genes[basePos.x][basePos.y][basePos.z];

			if (baseWaypoint.type > 0)
			{
				SetBounds(ref lowerBound, ref upperBound, basePos, typeParams[baseWaypoint.type].size, baseWaypoint.rotation);

				for (int i = lowerBound.x; i <= upperBound.x; i++)
				{
					for (int j = lowerBound.y; j <= upperBound.y; j++)
					{
						for (int k = lowerBound.z; k <= upperBound.z; k++)
						{
							if (InputInGridBoundaries(i,j,k))
							{
								Genes[i][j][k].type = 0;
								Genes[i][j][k].rotation = new Vector3(0, 0, 0);
								Genes[i][j][k].basePos = index;
								Genes[i][j][k].baseType = false;
							}
						}
					}
				}
			}
		}

		private bool CanAddTypeHere(Vector3Int index, Vector3Int size, Vector3 rotation)
		{
			if (size.x == 1 && size.y == 1 && size.z == 1)
				if (Genes[index.x][index.y][index.z].type != 0)
					return false;
				else
					return true;

			Vector3Int lowerBound = default(Vector3Int);
			Vector3Int upperBound = default(Vector3Int);
			SetBounds(ref lowerBound, ref upperBound, index, size, rotation);

			for (int i = lowerBound.x; i <= upperBound.x; i++)
			{
				for (int j = lowerBound.y; j <= upperBound.y; j++)
				{
					for (int k = lowerBound.z; k <= upperBound.z; k++)
					{
						if (!InputInGridBoundaries(i,j,k) || Genes[i][j][k].type > 0)
							return false;
					}
				}
			}

			return true;
		}

		private bool InputInGridBoundaries(int i, int j, int k)
		{
			bool inBoundaries = true;

			if (i < 0 || j < 0 || k < 0 || i > sizeDNA.x - 1 || j > sizeDNA.y - 1 || k > sizeDNA.z - 1)
				inBoundaries = false;

			return inBoundaries;
		}

		private void SetBounds(ref Vector3Int lowerBound, ref Vector3Int upperBound, Vector3Int index, Vector3Int size, Vector3 rotation)
		{
			Vector3Int newSize = default(Vector3Int);
			newSize.x = (int)Mathf.Cos(rotation.y * Mathf.Deg2Rad) * size.x + (int)Mathf.Sin(rotation.x * Mathf.Deg2Rad) * (int)Mathf.Sin(rotation.y * Mathf.Deg2Rad) * size.y + (int)Mathf.Sin(rotation.y * Mathf.Deg2Rad) * (int)Mathf.Cos(rotation.x * Mathf.Deg2Rad) * size.z;
			newSize.y = (int)Mathf.Cos(rotation.x * Mathf.Deg2Rad) * size.y - (int)Mathf.Sin(rotation.x * Mathf.Deg2Rad) * size.z;
			newSize.z = -(int)Mathf.Sin(rotation.y * Mathf.Deg2Rad) * size.x + (int)Mathf.Cos(rotation.y * Mathf.Deg2Rad) * (int)Mathf.Sin(rotation.x * Mathf.Deg2Rad) * size.y + (int)Mathf.Cos(rotation.y * Mathf.Deg2Rad) * (int)Mathf.Cos(rotation.x * Mathf.Deg2Rad) * size.z;

			lowerBound.x = newSize.x < 0 ? index.x + newSize.x + 1 : index.x;
			lowerBound.y = newSize.y < 0 ? index.y + newSize.y + 1 : index.y;
			lowerBound.z = newSize.z < 0 ? index.z + newSize.z + 1 : index.z;

			upperBound.x = newSize.x > 0 ? index.x + newSize.x - 1 : index.x;
			upperBound.y = newSize.y > 0 ? index.y + newSize.y - 1 : index.y;
			upperBound.z = newSize.z > 0 ? index.z + newSize.z - 1 : index.z;
		}
	}
}