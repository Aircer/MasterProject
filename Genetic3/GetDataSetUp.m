function [size, sizeCandidate, population, generations, numberRuns, candidatesNumber] = GetDataSetUp(IDExperiment)
    pathSetUp = strcat('D:\MasterProject\Genetic3\Data\Experiment_', num2str(IDExperiment), '\ExperimentSetUp.csv');
    [~,~,dataCommun] = xlsread(pathSetUp);
    size = cell2mat(dataCommun(2,1:3));
    population = cell2mat(dataCommun(2,4));
    generations = cell2mat(dataCommun(2,6));
    numberRuns = cell2mat(dataCommun(2,8));
    candidatesNumber = cell2mat(dataCommun(2,14));
    
    sizeCandidate = zeros(candidatesNumber, 3);
    path = strcat('D:\MasterProject\Genetic3\Data\Experiment_', num2str(IDExperiment));
    for i=1:candidatesNumber
        pathSetUpCandidate = strcat(path, '\Candidate', num2str(i-1), '\ExperimentSetUp.csv');
        [~,~,dataCommun] = xlsread(pathSetUpCandidate);
        sizeCandidate(i, :) = cell2mat(dataCommun(2,1:3));
    end
end

