function [R, I] = isInCellString(A, b)
R = false;
I = -1;
for k = 1:numel(A)
  if isequal(A{k}, b)
    R = true;
    I = k;
    return;
  end
end