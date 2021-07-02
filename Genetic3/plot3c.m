function plot3c(sizeCandidate, cells, Title)
% Plot the points
hold on
for x=1:sizeCandidate(1)
    for y=1:sizeCandidate(2)
        for z=1:sizeCandidate(3)
            if cells(x, y, z) > 0
                if cells(x, y, z)== 1;col='yellow';end 
                if cells(x, y, z)== 2;col=[0.6350 0.0780 0.1840];end 
                if cells(x, y, z)== 3;col='red';end 
                if cells(x, y, z)== 4;col='magenta';end 
                if cells(x, y, z)== 5;col=[0.66 0.66 0.66];end 
                plotCube(x, y, z, col)
            end
        end
    end
end
hold off
xlim([1 sizeCandidate(1) + 1]);
ylim([1 sizeCandidate(3) + 1]);
zlim([1 sizeCandidate(2) + 1]);
title(Title);