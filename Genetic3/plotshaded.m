function varargout = plotshaded(x,y,fstr);

maxY = max(y, [], 1);
meanY = mean(y, 1);
minY = min(y, [], 1);

px=[x,fliplr(x)];
py=[maxY, fliplr(minY)];
patch(px,py,1,'FaceColor',fstr,'EdgeColor','none');
hold on
plot(x,meanY,fstr);
alpha(.2); % make patch transparent
end