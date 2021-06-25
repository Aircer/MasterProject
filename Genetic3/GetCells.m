function cells = GetCells(IDExperiment, candidatesNumber, numberRuns, generations, population, size, sizeCandidate)
cells = zeros(candidatesNumber, numberRuns, (generations + 1), population, size(1)*size(2)*size(3));
path = strcat('D:\MasterProject\Genetic3\Data\Experiment_', num2str(IDExperiment));
for i=1:candidatesNumber
    
    pathCells = strcat(path, '\Candidate', num2str(i-1), '\Cells_');
    
    cellsRun = zeros((generations + 1)*population, sizeCandidate(i, 1)*sizeCandidate(i, 2)*sizeCandidate(i, 3));
        
    % figure
    % hold on 
    for j = 1:numberRuns  
%         [~,~,data] = xlsread(strcat(pathCells, num2str(j - 1), '.csv'));
        a = readtable(strcat(pathCells, num2str(j - 1), '.csv'));
        cellsRun(:, :) = table2array(a);

        for k = 1:generations+1
            cells(i, j, k, :, 1:sizeCandidate(i,1)*sizeCandidate(i,2)*sizeCandidate(i,3)) = cellsRun((k-1)*population + 1:k*population, :); 
        end
    end
end
end

