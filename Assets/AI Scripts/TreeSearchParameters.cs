using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI
{
    [Serializable]
    public struct TreeSearchParameters
    {
        public int IterationsCount;
        public float explorationValue;
    }
}