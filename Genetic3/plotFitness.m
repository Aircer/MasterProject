function plotFitness(title, Legend, ylab, numberRuns, candidatesNumber, generations, fit)
    lookGraph = ['b', 'r', 'k', 'm', 'y'];
    x = 1:generations;

    figure('Name',title);

    hold on
    for i=1:candidatesNumber
        plotWConfidence(x, fit, numberRuns, lookGraph(i));
    end
    hold off

    ylim([0 1])
    xlabel('Number Generations') 
    ylabel(ylab) 
    legend(Legend);
    title(title)
    end

