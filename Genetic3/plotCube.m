function plotCube(x, y, z, C)

xc=x+0.5; yc=y+0.5; zc=z+0.5;    % coordinated of the center
L=0.95;                 % cube size (length of an edge)
alpha=0.9;           % transparency (max=1=opaque)

X = [0 0 0 0 0 1; 1 0 1 1 1 1; 1 0 1 1 1 1; 0 0 0 0 0 1];
Y = [0 0 0 0 1 0; 0 1 0 0 1 1; 0 1 1 1 1 1; 0 0 1 1 1 0];
Z = [0 0 1 0 0 0; 0 0 1 0 0 0; 1 1 1 0 1 1; 1 1 1 0 1 1];

X = L*(X-0.5) + xc;
Y = L*(Y-0.5) + yc;
Z = L*(Z-0.5) + zc; 

fill3(X,Y,Z,C,'FaceAlpha',alpha);    % draw cube

axis equal
end

