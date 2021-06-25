function fitness = GetFitness(IDExperiment, numberSubFitness, candidatesNumber, numberRuns, generations, population)
fitness = zeros(candidatesNumber, numberRuns, generations + 1, population, numberSubFitness + 1);
path = strcat('D:\MasterProject\Genetic3\Data\Experiment_', num2str(IDExperiment));
for i=1:candidatesNumber
    
    pathFitness = strcat(path, '\Candidate', num2str(i-1), '\Fitness_');
    fitRun = zeros((generations + 1)*population, numberSubFitness + 1);

    % figure
    % hold on 
    for j = 1:numberRuns  
%         [~,~,data] = xlsread(strcat(pathFitness, num2str(j - 1), '.csv'));
%         fitRun(:, :) = cell2mat(data);

        a = readtable(strcat(pathFitness, num2str(j - 1), '.csv'),'Format','%s%s%s%s%s');
        fitRun(:, :) = str2double(table2array(a));
        
        for k = 1:generations + 1
            fitness(i, j, k, :, :) = fitRun((k-1)*population + 1:k*population, :);
        end
    end
end
end

