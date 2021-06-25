function varargout = plotWConfidence(x, y, numberRuns, fstr);

y = squeeze(y);
meanM = repmat(mean(y), 2, 1)';

SEM = repmat(std(y)/sqrt(numberRuns), 2, 1)'; % Standard Error
ts = diag(tinv([0.025  0.975], numberRuns-1));    % T-Score
CI = meanM + SEM*ts;   % Confidence Intervals

px=[x, fliplr(x)]; %,fliplr(x)]
py=[CI(:,1)', fliplr(CI(:,2)')]; %, fliplr(CI(:,1))]'
patch(px,py,1,'FaceColor',fstr,'EdgeColor','none');
hold on
plot(x,mean(y),fstr);
alpha(.2); % make patch transparent
end