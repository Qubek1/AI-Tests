using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VertexInfo : MonoBehaviour
{
    public float value;
    public float valueDerivative;
    public float bias;
    public float biasDerivative;
    public List<float> weightsIn;
    public List<float> weightsOut;

    public void UpdateInfo(ANN ann, int layerIndex, int vertexIndex)
    {
        VertexLayer vertexLayer = ann.vertexLayers[layerIndex];
        value = vertexLayer.values[vertexIndex];
        valueDerivative = vertexLayer.valuesDerivatives[vertexIndex];
        bias = vertexLayer.biases[vertexIndex];
        biasDerivative = vertexLayer.biasesDerivative[vertexIndex];

        weightsIn = new List<float>();
        weightsOut = new List<float>();
        if (layerIndex != ann.vertexLayers.Length - 1)
        {
            WeightsLayer rightWeightsLayer = ann.weightsLayers[layerIndex];
            for (int i = 0; i < rightWeightsLayer.weights.GetLength(1); i++)
            {
                weightsOut.Add(rightWeightsLayer.weights[rightWeightsLayer.GetIndex(vertexIndex, i)]);
            }
        }
        if (layerIndex != 0)
        {
            WeightsLayer leftWeightsLayer = ann.weightsLayers[layerIndex - 1];
            for (int i = 0; i < leftWeightsLayer.weights.GetLength(0); i++)
            {
                weightsIn.Add(leftWeightsLayer.weights[leftWeightsLayer.GetIndex(i, vertexIndex)]);
            }
        }
    }
}
