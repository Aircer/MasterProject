function [size, population, generations, numberRuns, candidatesNumber] = GetDataSetUp(IDExperiment)
    pathSetUp = strcat('D:\MasterProject\Genetic3\Data\Experiment_', num2str(IDExperiment), '\ExperimentSetUp.csv');
    [~,~,dataCommun] = xlsread(pathSetUp);
    size = cell2mat(dataCommun(2,1:3));
    population = cell2mat(dataCommun(2,4));
    generations = cell2mat(dataCommun(2,6));
    numberRuns = cell2mat(dataCommun(2,8));
    candidatesNumber = cell2mat(dataCommun(2,14));
end

