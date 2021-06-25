function [R, I] = patchInArray(C, A, b)
R = false;
I = -1;
for k = 1:1
  if isequal(A(k, :, :, :), b)
    R = true;
    I = k;
    return;
  end
end