function Legend = GetLegend(candidatesNumber, IDExperiment)
pathSetUp = strcat('D:\MasterProject\Genetic3\Data\Experiment_', num2str(IDExperiment), '\ExperimentSetUp.csv');
[~,~,dataCommun] = xlsread(pathSetUp);
for i=1:candidatesNumber
    variableName = cell2mat(dataCommun(3,1));
    if strcmp(variableName,'size') 
        varObsText(i) = strcat(variableName, '__', string(dataCommun(3 + i,1)), 'x' , string(dataCommun(3 + i,2)),  'x' , string(dataCommun(3 + i,3)));
    elseif isnumeric(dataCommun(3 + i,1))
        varObsText(i) = strcat(variableName, '__', num2str(dataCommun(3 + i,1)));
    else
       varObsText(i) = strcat(variableName, '__', string(dataCommun(3 + i,1)));
    end
end

Legend=cell(2*candidatesNumber,1);%  two positions 
for i=1: candidatesNumber*2
    if mod(i,2) == 0
        Legend{i}= varObsText(i/2);
    else
       Legend{i}= ''; 
    end
end

end

