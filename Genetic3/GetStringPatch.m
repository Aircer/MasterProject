function patch = GetStringPatch(A, patch_size)
    patch = '';
    for x=1:patch_size(1)
        for y=1:patch_size(2)
            for z=1:patch_size(3)
                patch = strcat(patch, num2str(A(x, y, z)));
            end
        end
     end
end

