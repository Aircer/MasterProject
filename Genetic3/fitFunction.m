function [] = fitFunction()
clear all;
clc;
[~,~,data] = xlsread('D:\MasterProject\Genetic3\Data\DataFitData.csv');
size = cell2mat(data(2,1:3));
population = cell2mat(data(2,4));
elitism = cell2mat(data(2,5));
generations = cell2mat(data(2,6));
mutationsRate = cell2mat(data(2,7));

fit = cell2mat(data(3:end,:));
bestFit = fit(:,1);
meanFit = mean(fit,2);

figure
x = 0:generations;
plot(x,bestFit)

hold on 
plot(x,meanFit)
hold off
end

