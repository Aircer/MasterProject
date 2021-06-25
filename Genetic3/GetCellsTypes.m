function cellsTypes = GetCellsTypes(cells, numberTypes, candidatesNumber, numberRuns, generations, population);

cellsTypes = zeros(candidatesNumber, numberRuns, (generations + 1), population, numberTypes);

for i=1:candidatesNumber
    for j = 1:numberRuns  
        for k = 1:generations + 1
            for l = 1:population
                for m = 1:numberTypes
                    cellsTypes(i, j, k, l, m) = sum(cells(i, j, k, l, :) == m);
                end
            end
        end
    end
end

end

