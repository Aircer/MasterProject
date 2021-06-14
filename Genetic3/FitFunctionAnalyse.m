clear all;
clc;
IDExperiment = 1;
path = 'D:\MasterProject\Genetic3\Data\';
pathSetUp = strcat(path, 'Experiment_', num2str(IDExperiment), '\ExperimentSetUp.csv');
pathFitness = strcat(path, 'Experiment_', num2str(IDExperiment), '\Fitness_');

[~,~,X] = xlsread(pathSetUp);
dataSetUp = cell2mat(X(2,:));

size = dataSetUp(1:3);
population = dataSetUp(4);
elitism = dataSetUp(5);
generations = dataSetUp(6);
mutationsRate = dataSetUp(7);
numberRuns = dataSetUp(8); 
fitnessWeights = dataSetUp(9:12); 

fitRun = zeros(generations*population, 5);
fitGen = zeros(population, 5);
fit = zeros(numberRuns, generations, population, 5);
bestFit = zeros(numberRuns, generations, 5);
meanFit = zeros(numberRuns, generations, 5);

% figure
% hold on 
for i = 1:numberRuns  
    [~,~,data] = xlsread(strcat(pathFitness, num2str(i - 1), '.csv'));
    fitRun(:, :) = cell2mat(data);
    
    for j = 1:generations  
        fit(i, j, :, :) = fitRun((j-1)*population + 1:j*population, :);
        
        fitGen(:, :) = fit(i, j, :, :);
        [M,I] = max(fitGen(:, 1));
        bestFit(i, j, :) = fitGen(I, :);
        meanFit(i, j, :) = mean(fitGen);
    end
end


x = 1:generations;
plotshaded(x,bestFit(:, :, 3),'b');
plotshaded(x,meanFit(:, :, 3),'r');

xlabel('Number Generations') 
ylabel('Fitness') 
legend('','best','', 'mean');
