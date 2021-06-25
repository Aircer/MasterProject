function F = ComputeKLDivergence(C, A_patches, A_patchesValues, B_patches, B_patchesValues)
    
    epsilon = 0.001; 
    u = 1/((C + epsilon)*(1 + epsilon));
    
    D_AB = computeD(epsilon, u, C, A_patches, A_patchesValues, B_patches, B_patchesValues);
    D_BA = computeD(epsilon, u, C, B_patches, B_patchesValues, A_patches, A_patchesValues);

    F = D_AB*0.5 + D_BA*0.5;
end

function D = computeD(epsilon, u, C, A_patches, A_patchesValues, B_patches, B_patchesValues)

    D = 0;
    
    for i=1:C 
        if isempty(A_patches{i})
            return;
        end
        C_x = A_patchesValues(i);
        P_prime = (C_x + epsilon) * u;

        [R, index] = isInCell(B_patches, A_patches{i});
        if R == true
            C_x = B_patchesValues(index);
        else 
            C_x = 0;
        end

        Q_prime = (C_x + epsilon) * u;

        D = D + P_prime * log(P_prime/Q_prime);
    end
end

