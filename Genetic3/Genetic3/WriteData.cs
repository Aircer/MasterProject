using System;
using System.Collections.Generic;
using UtilitiesGenetic;
using CsvHelper;
using System.IO;
using System.Globalization;
using Genetics;

namespace Genetic3
{
	public static class Data
	{

		public static void Write(Candidate candidate, string path, int nbRuns, Fitness[][][] fitness, Population[][][] populations)
		{
			System.IO.Directory.CreateDirectory(path);

			foreach(string pathFile in System.IO.Directory.GetFiles(path))
			{
				System.IO.File.Delete(pathFile);
			}

			using (var w = new StreamWriter(path + "\\" + "ExperimentSetUp.csv"))
			{
				var line = string.Format("Size_x; Size_y; Size_z; Population; Elitism; Generations; MutationRate; numberRuns; wDifference; wWalkingAreas; wWallsCuboids; wPathfinding; typeSetUp; crossoverType; mutationType");
				w.WriteLine(line);
				line = string.Format(candidate.sizeGrid.x + ";" + candidate.sizeGrid.y + ";" + candidate.sizeGrid.z + ";" + candidate.algoParams.population + ";"
					+ candidate.algoParams.elitism + ";" + candidate.algoParams.generations + ";" + candidate.algoParams.mutationRate + ";" + nbRuns + ";"
					+ candidate.algoParams.wDifference + ";" + candidate.algoParams.wWalkingAreas + ";" + candidate.algoParams.wWallsCuboids + ";" + candidate.algoParams.wPathfinding + ";" + candidate.typeSetUp + ";"
					+ candidate.algoParams.crossoverType + ";" + candidate.algoParams.mutationType);
				w.WriteLine(line);

				w.Flush();
			}

			for (int i = 0; i < nbRuns; i++)
			{
				using (var w = new StreamWriter(path + "\\" + "Fitness" + "_" + i + ".csv"))
				{
					for (int j = 0; j < candidate.algoParams.generations + 1; j++)
					{
						for (int k = 0; k < candidate.algoParams.population; k++)
						{
							WriteFitness(w, i, j, k, fitness);
						}
					}
				}
			}

			for (int i = 0; i < nbRuns; i++)
			{
				using (var w = new StreamWriter(path + "\\" + "Cells" + "_" + i + ".csv"))
				{
					for (int j = 0; j < candidate.algoParams.generations + 1; j++)
					{
						for (int k = 0; k < candidate.algoParams.population; k++)
						{
							WritePop(w, i, j, k, candidate, populations);
						}
					}
				}
			}
		}

		public static void WritePop(StreamWriter w, int i, int j, int k, Candidate candidate, Population[][][] populations)
		{
			for (int x = 1; x < candidate.sizeDNA.x - 1; x++)
			{
				for (int y = 1; y < candidate.sizeDNA.y - 1; y++)
				{
					for (int z = 1; z < candidate.sizeDNA.z - 1; z++)
					{
						w.Write(populations[i][j][k].genes[x][y][z]);
						w.Write(";");
					}
				}
			}
			w.WriteLine();
		}

		public static void WriteFitness(StreamWriter w, int i, int j, int k, Fitness[][][] fitness)
		{
			w.Write(fitness[i][j][k].total);
			w.Write(";");
			w.Write(fitness[i][j][k].difference);
			w.Write(";");
			w.Write(fitness[i][j][k].walkingAreas);
			w.Write(";");
			w.Write(fitness[i][j][k].walls);
			w.Write(";");
			w.Write(fitness[i][j][k].pathfinding);
			w.Write(";");

			w.WriteLine();
		}
	}
}
