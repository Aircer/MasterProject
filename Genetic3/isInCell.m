function [R, I] = isInCell(A, b)

lia = find(strcmp(A, b));

if any(lia)
    R = true;
    I = find(lia, 1, 'first');
else 
    R = false;
    I = -1;
end

% 
% for k = 1:numel(A)
%   if isequal(A{k}, b)
%     R = true;
%     I = k;
%     return;
%   end
end