%% GET DATA FITNESS
clear all;
clc;
warning('off');

IDExperiment = 13;

[size, sizeCandidate, population, generations, numberRuns, candidatesNumber] = GetDataSetUp(IDExperiment);

Legend = GetLegend(candidatesNumber, IDExperiment);
numberSubFitness = 4;
fitness = GetFitness(IDExperiment, numberSubFitness, candidatesNumber, numberRuns, generations, population);
cells = GetCells(IDExperiment, candidatesNumber, numberRuns, generations, population, size, sizeCandidate);

disp('GET DATA FITNESS done')
%% GET MEAN AND MAX

totalFitness = fitness(:, :, :, :, 1);
meanFitness = zeros(candidatesNumber, numberRuns, generations + 1, numberSubFitness + 1);
maxFitness = zeros(candidatesNumber, numberRuns, generations + 1, numberSubFitness + 1);

[M,I] = max(totalFitness,[], 4);
meanFitness(:, :, :, :) = mean(fitness, 4);

for i=1:candidatesNumber
    for j = 1:numberRuns  
        for k = 1:generations + 1
            maxFitness(i, j, k, :) = fitness(i, j, k, I(i,j,k), :);
        end
    end
end

disp('GET MEAN AND MAX done')

%% GET CELLS TYPES
numberTypes = 5;
cellsTypes = GetCellsTypes(cells, numberTypes, candidatesNumber, numberRuns, generations, population);
maxCellsTypes = zeros(candidatesNumber, numberRuns, generations + 1, numberTypes);
for i=1:candidatesNumber
    for j = 1:numberRuns  
        for k = 1:generations + 1
            maxCellsTypes(i, j, k, :) = cellsTypes(i, j, k, I(i,j,k), :)/(sizeCandidate(i, 1) *sizeCandidate(i, 2) *sizeCandidate(i, 3));
        end
    end
end

disp('GET CELLS TYPES done')
%% PLOT TOTAL MAX FITNESS
lookGraph = ['b', 'r', 'k', 'y', 'm'];
x = 0:generations;

figure('Name','Max TotalFitness');

hold on
for i=1:candidatesNumber
    plotWConfidence(x, maxFitness(i, :, :, 1), numberRuns, lookGraph(i));
end
hold off

ylim([0 1])
xlabel('Number Generations') 
ylabel('Max TotalFitness') 
legend(Legend);
title('Max TotalFitness')

%% PLOT TOTAL MEAN FITNESS
lookGraph = ['b', 'r', 'k', 'y', 'm'];
x = 0:generations;

figure('Name','Mean TotalFitness');

hold on
for i=1:candidatesNumber
    plotWConfidence(x, meanFitness(i, :, :, 1), numberRuns, lookGraph(i));
end
hold off

ylim([0 1])
xlabel('Number Generations') 
ylabel('Mean TotalFitness') 
legend(Legend);
title('Mean TotalFitness')
%% PLOT ALL MAX FITNESS
lookGraph = ['b', 'r', 'k', 'y', 'm'];
x = 0:generations;

figure('Name','Sub Max Fitness');

subplot(2,2,1)
hold on
for i=1:candidatesNumber
    plotWConfidence(x, maxFitness(i, :, :, 2), numberRuns, lookGraph(i));
end
hold off
ylim([0 1])
xlabel('Number Generations') 
ylabel('Max Fitness') 
title('Max Fitness Difference')
legend(Legend);

subplot(2,2,2)
hold on
for i=1:candidatesNumber
    plotWConfidence(x, maxFitness(i, :, :, 3), numberRuns, lookGraph(i));
end
hold off
ylim([0 1])
xlabel('Number Generations') 
ylabel('Max Fitness') 
title('Max Fitness Walking Area')
legend(Legend);

subplot(2,2,3)
hold on
for i=1:candidatesNumber
    plotWConfidence(x, maxFitness(i, :, :, 4), numberRuns, lookGraph(i));
end
hold off
ylim([0 1])
xlabel('Number Generations') 
ylabel('Max Fitness') 
title('Max Fitness Walls')
legend(Legend);

subplot(2,2,4)
hold on
for i=1:candidatesNumber
    plotWConfidence(x, maxFitness(i, :, :, 5), numberRuns, lookGraph(i));
end
hold off
ylim([0 1])
xlabel('Number Generations') 
ylabel('Max Fitness') 
title('Max Fitness Paths')
legend(Legend);

%% PLOT ALL MEAN FITNESS
lookGraph = ['b', 'r', 'k', 'y', 'm'];
x = 0:generations;

figure('Name','Sub Mean Fitness');

subplot(2,2,1)
hold on
for i=1:candidatesNumber
    plotWConfidence(x, meanFitness(i, :, :, 2), numberRuns, lookGraph(i));
end
hold off
ylim([0 1])
xlabel('Number Generations') 
ylabel('Mean Fitness') 
title('Mean Fitness Difference')
legend(Legend);

subplot(2,2,2)
hold on
for i=1:candidatesNumber
    plotWConfidence(x, meanFitness(i, :, :, 3), numberRuns, lookGraph(i));
end
hold off
ylim([0 1])
xlabel('Number Generations') 
ylabel('Mean Fitness') 
title('Mean Fitness Walking Area')
legend(Legend);

subplot(2,2,3)
hold on
for i=1:candidatesNumber
    plotWConfidence(x, meanFitness(i, :, :, 4), numberRuns, lookGraph(i));
end
hold off
ylim([0 1])
xlabel('Number Generations') 
ylabel('Mean Fitness') 
title('Mean Fitness Walls')
legend(Legend);

subplot(2,2,4)
hold on
for i=1:candidatesNumber
    plotWConfidence(x, meanFitness(i, :, :, 5), numberRuns, lookGraph(i));
end
hold off
ylim([0 1])
xlabel('Number Generations') 
ylabel('Mean Fitness') 
title('Mean Fitness Paths')
legend(Legend);
%% PLOT ALL MAX FITNESS
lookGraph = ['b', 'r', 'k', 'y', 'm'];
x = 0:generations;

figure('Name','Sub Max Fitness');

subplot(2,2,1)
for i=1:candidatesNumber
    plotWConfidence(x, maxCellsTypes(i, :, :, 2), numberRuns, lookGraph(i));
end
xlabel('Number Generations') 
ylabel('Percentage Floor Cells') 
title('Percentage Floor Cells')
ylim([0 0.5])
legend(Legend);

subplot(2,2,2)
for i=1:candidatesNumber
    plotWConfidence(x, maxCellsTypes(i, :, :, 5), numberRuns, lookGraph(i));
end

xlabel('Number Generations') 
ylabel('Percentage Walls Cells') 
title('Percentage Walls Cells')
ylim([0 0.5])
legend(Legend);

subplot(2,2,3)
for i=1:candidatesNumber
    plotWConfidence(x, maxCellsTypes(i, :, :, 1) + maxCellsTypes(i, :, :, 3)...
                    + maxCellsTypes(i, :, :, 4), numberRuns, lookGraph(i));
end
xlabel('Number Generations') 
ylabel('Percentage Paths Cells') 
title('Percentage Paths Cells')
ylim([0 0.1])
legend(Legend);