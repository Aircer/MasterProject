clear all;
clc;
path = 'D:\MasterProject\Genetic3\Data\DataFitData_';
numberRuns = 20;

size = zeros(numberRuns,3);
population = zeros(numberRuns);
elitism = zeros(numberRuns);
mutationsRate = zeros(numberRuns);
generations = zeros(numberRuns);
fit = zeros(numberRuns, 500, 500);
bestFit = zeros(numberRuns, 500);
meanFit = zeros(numberRuns, 500);

% figure
% hold on 
for i = 1:numberRuns  
    
    [~,~,data] = xlsread(strcat(path, num2str(i - 1), '.csv'));
    
    size(i, :) = cell2mat(data(2,1:3));
    elitism(i) = cell2mat(data(2,5));
    mutationsRate(i) = cell2mat(data(2,7));
    population(i) = cell2mat(data(2,4));
    generations(i) = cell2mat(data(2,6));
    
    fit(i, 1: population(i), 1:generations(i)) = cell2mat(data(3:end,:))';
    bestFit(i, 1:generations(i)) = max(fit(i, 1: population(i), 1:generations(i)), [], 2);
    meanFit(i, 1:generations(i)) = mean(fit(i, 1: population(i), 1:generations(i)),2);
    
%     x = 0:generations(i) - 1;
%     plot(x,bestFit(i, 1:generations(i)))
%     plot(x,meanFit(i, 1:generations(i)))

end

x = 0:generations(1) - 1;
plotshaded(x,bestFit(:, 1:generations(1)),'b');
plotshaded(x,meanFit(:, 1:generations(1)),'r');

xlabel('Number Generations') 
ylabel('Fitness') 
legend('','best','', 'mean');
