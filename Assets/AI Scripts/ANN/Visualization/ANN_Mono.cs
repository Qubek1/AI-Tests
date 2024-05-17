using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ANN_Mono : MonoBehaviour
{
    public int layersAmount = 3;

    public ANN ann;

    private void Start()
    {
        ann = new ANN(10, 100, 100, 10);
        ann.PopulateRandom();
    }
}
