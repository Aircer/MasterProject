using System;
using System.Collections.Generic;
using UtilitiesGenetic;
using CsvHelper;
using System.IO;
using System.Globalization;
using Genetics;
using static Genetic3.Utilities;

namespace Genetic3
{
    public static class Program
    {
		public static string pathExperiment;
		public static string pathCandidates;

		public static int nbRuns;
		public static TypeParams[] typeParams;

		public static Fitness[][][][] fitness;
		public static Population[][][][] populations;

		private static int experimentID;
		private static int numberCandidates;

		private static EvolutionaryAlgoParams algoCommun;
		private static TypeSetUp typeSetUpCommun;
		private static Vector3Int sizeCommun;

		private static string variableObserved;
		private static string[] variableObservedValues;

		static void Main()
        {
			pathExperiment = "D:\\MasterProject\\Genetic3\\Data\\Experiment_";
			typeParams = SetTypesParams();

			SetExperimentParams(ref experimentID, ref nbRuns, ref numberCandidates, ref sizeCommun, ref algoCommun, ref typeSetUpCommun);
			Experiment exp = new Experiment(experimentID, numberCandidates, sizeCommun, algoCommun, typeSetUpCommun);

			SetExperimentObservedVariable(ref exp);

			fitness = new Fitness[exp.numberCandidates][][][];
			populations = new Population[exp.numberCandidates][][][];
			RunExperiment(nbRuns, exp, ref fitness, ref populations);

			WriteDataExperiment(pathExperiment, exp, nbRuns, fitness, populations);
		}

		private static void SetExperimentParams(ref int experimentID, ref int nbRuns, ref int numberCandidates, ref Vector3Int sizeCommun,
										  ref EvolutionaryAlgoParams algoCommun, ref TypeSetUp typeSetUpCommun)
		{
			algoCommun = new EvolutionaryAlgoParams();

			experimentID = 14;
			nbRuns = 20;
			numberCandidates = 3;

			sizeCommun = new Vector3Int(5, 5, 5);

			algoCommun.crossoverType = CrossoverType.Copy;
			algoCommun.mutationType = MutationsType.Normal;

			algoCommun.wWalkingAreas = 1f;
			algoCommun.wWallsCuboids = 1f;
			algoCommun.wPathfinding = 1f;

			typeSetUpCommun = TypeSetUp.Empty;


			algoCommun.population = 50;
			algoCommun.generations = 50;
			algoCommun.mutationRate = 0.005f;
			algoCommun.fitnessStop = 2f;
			algoCommun.wDifference = 0f;
			algoCommun.elitism = 2;
		}

		private static void SetExperimentObservedVariable(ref Experiment exp)
		{
			variableObserved = "Weights";

			exp.algoParams[1].wWallsCuboids = 0;
			exp.algoParams[2].wWalkingAreas = 0;

			variableObservedValues = new string[numberCandidates];
			variableObservedValues[0] = "Normal";
			variableObservedValues[1] = "WeighWallNull";
			variableObservedValues[2] = "WeighWalkingAreaNull";

			exp.SetCandidates();
		}

		private static TypeParams[] SetTypesParams()
		{
			int nbType = 5;
			TypeParams[] newTypesParams = new TypeParams[nbType];

			for (int i = 0; i < nbType; i++)
			{
				newTypesParams[i] = new TypeParams();
				newTypesParams[i].SetEmpty();
			}

			newTypesParams[0].door = true;
			newTypesParams[0].wall = true;

			newTypesParams[1].blockPath = true;
			newTypesParams[1].floor = true;
			newTypesParams[1].ground = true;

			newTypesParams[2].ladder = true;

			newTypesParams[3].blockPath = true;
			newTypesParams[3].ground = true;
			newTypesParams[3].stair = true;

			newTypesParams[4].blockPath = true;
			newTypesParams[4].wall = true;

			return newTypesParams;
		}

		private static void RunExperiment(int nbRuns, Experiment exp, ref Fitness[][][][] fitness, ref Population[][][][] populations)
		{
			for (int i = 0; i < exp.numberCandidates; i++)
			{
				Genetics.Init newInitGenetic = new Genetics.Init();
				newInitGenetic.GetSuggestionsClusters(exp.candidates[i].sizeDNA, typeParams, exp.candidates[i].wayPointsInit, nbRuns, exp.candidates[i].algoParams);

				fitness[i] = newInitGenetic.fitness;
				populations[i] = newInitGenetic.populations;
			}
		}

		private static void WriteDataExperiment(string pathExperiment, Experiment exp, int nbRuns, Fitness[][][][] fitness, Population[][][][] populations)
		{
			System.IO.Directory.CreateDirectory(pathExperiment + exp.experimentID);

			foreach (string pathFile in System.IO.Directory.GetFiles(pathExperiment + exp.experimentID))
			{
				System.IO.File.Delete(pathFile);
			}

			using (var w = new StreamWriter(pathExperiment + exp.experimentID + "\\" + "ExperimentSetUp.csv"))
			{
				var line = string.Format("Size_x; Size_y; Size_z; Population; Elitism; Generations; MutationRate; numberRuns; wDifference; wWalkingAreas; wWallsCuboids; wPathfinding; typeSetUp; numberCandidates; crossoverType; mutationType");
				w.WriteLine(line);

				line = string.Format(sizeCommun.x + ";" + sizeCommun.y + ";" + sizeCommun.z + ";" + 
					algoCommun.population + ";" + algoCommun.elitism + ";" + algoCommun.generations + ";" + algoCommun.mutationRate + ";" + 
					nbRuns + ";" + algoCommun.wDifference + ";" + algoCommun.wWalkingAreas + ";" + algoCommun.wWallsCuboids + ";" +
					algoCommun.wPathfinding + ";" + typeSetUpCommun + ";" + numberCandidates + ";" + algoCommun.crossoverType + ";" + algoCommun.mutationType);
				w.WriteLine(line);

				line = string.Format(variableObserved);
				w.WriteLine(line);

				for (int i = 0; i < numberCandidates; i++)
				{
					w.WriteLine(variableObservedValues[i]);
				}

				w.Flush();
			}

			for (int i = 0; i < exp.numberCandidates; i++)
			{
				string pathCandidate = pathExperiment + exp.experimentID + "\\Candidate" + i;
				Data.Write(exp.candidates[i], pathCandidate, nbRuns, fitness[i], populations[i]);
			}
		}
	}
}
