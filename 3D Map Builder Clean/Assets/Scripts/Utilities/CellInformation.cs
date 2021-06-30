using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UtilitiesGenetic;

public class CellInformation : MonoBehaviour
{
    public TypeParams typeParams;

    public void SetEmpty()
    {
        typeParams = new TypeParams();
        typeParams.SetEmpty();
    }
}
