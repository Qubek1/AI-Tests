using CI.QuickSave;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class ANN
{
    public VertexLayer[] vertexLayers;
    public WeightsLayer[] weightsLayers;
    public float learningSpeed = 0.05f;
    public ANNLayerCalculation layerCalculations;

    public ANN(params int[] layerSizes)
    {
        vertexLayers = new VertexLayer[layerSizes.Length];
        weightsLayers = new WeightsLayer[layerSizes.Length - 1];
        for (int i = 0; i < layerSizes.Length; i++)
        {
            vertexLayers[i] = new VertexLayer(layerSizes[i]);
            if (i < layerSizes.Length - 1)
            {
                weightsLayers[i] = new WeightsLayer(layerSizes[i], layerSizes[i+1]);
            }
        }
        PopulateRandom();
    }

    public ANN(string fileName)
    {
        Load(fileName);
    }

    public void PopulateRandom()
    {
        for (int i = 1; i < vertexLayers.Length; i++)
        {
            var layer = vertexLayers[i];
            for (int j = 0; j < layer.biases.Length; j++)
            {
                layer.biases[j] = Random.Range(-0.7f, 0.7f);
            }
        }
        foreach (var layer in weightsLayers)
        {
            for (int i = 0; i < layer.inSize; i++)
            {
                for (int j = 0; j < layer.outSize; j++)
                {
                    layer.weights[layer.GetIndex(i, j)] = Random.Range(-0.7f, 0.7f) * Mathf.Sqrt(1f / layer.inSize);
                }
            }
        }
    }

    public float[] Calculate(float[] input)
    {
        vertexLayers[0].values = input;

        for (int layerIndex = 1; layerIndex < vertexLayers.Length; layerIndex++)
        {
            for (int vertexIndex = 0; vertexIndex < vertexLayers[layerIndex].values.Length; vertexIndex++)
            {
                float newValue = vertexLayers[layerIndex].biases[vertexIndex];
                for (int lastLayerVertexIndex = 0; lastLayerVertexIndex < vertexLayers[layerIndex - 1].values.Length; lastLayerVertexIndex++)
                {
                    float lastLayerValue = vertexLayers[layerIndex - 1].values[lastLayerVertexIndex];
                    float weight = weightsLayers[layerIndex - 1].weights[weightsLayers[layerIndex - 1].GetIndex(lastLayerVertexIndex, vertexIndex)];
                    newValue += lastLayerValue * weight;
                }
                vertexLayers[layerIndex].values[vertexIndex] = ActivationFunction(newValue);
            }
        }
        return vertexLayers[vertexLayers.Length - 1].values;
    }

    public float[] CalculateWithDerivatives(float[] input)
    {
        vertexLayers[0].values = input;

        for (int layerIndex = 1; layerIndex < vertexLayers.Length; layerIndex++)
        {
            for (int vertexIndex = 0; vertexIndex < vertexLayers[layerIndex].values.Length; vertexIndex++)
            {
                float newValue = vertexLayers[layerIndex].biases[vertexIndex];
                for (int lastLayerVertexIndex = 0; lastLayerVertexIndex < vertexLayers[layerIndex - 1].values.Length; lastLayerVertexIndex++)
                {
                    float lastLayerValue = vertexLayers[layerIndex - 1].values[lastLayerVertexIndex];
                    float weight = weightsLayers[layerIndex - 1].weights[weightsLayers[layerIndex - 1].GetIndex(lastLayerVertexIndex, vertexIndex)];
                    newValue += lastLayerValue * weight;
                }
                vertexLayers[layerIndex].values[vertexIndex] = ActivationFunction(newValue);
                vertexLayers[layerIndex].valuesDerivatives[vertexIndex] = ActivationFunctionDerivative(newValue);
            }
        }
        return vertexLayers[vertexLayers.Length - 1].values;
    }

    public float[] NewCalculateWithDerivatives(float[] input)
    {
        vertexLayers[0].values = input;

        for (int layerIndex = 1; layerIndex < vertexLayers.Length; layerIndex++)
        {
            layerCalculations.CalculateLayer(
                vertexLayers[layerIndex - 1].values, 
                weightsLayers[layerIndex - 1].weights, 
                vertexLayers[layerIndex].values,
                vertexLayers[layerIndex].biases,
                vertexLayers[layerIndex].valuesDerivatives);
        }
        return vertexLayers[vertexLayers.Length - 1].values;
    }

    public float Train(float[] input, float[] expectedOutput)
    {
        float[][] batchInput = new float[1][];
        batchInput[0] = input;
        float[][] batchOutput = new float[1][];
        batchOutput[0] = expectedOutput;
        return Train(batchInput, batchOutput);
    }

    public float Train(float[][] batchInputs, float[][] batchExpectedOutputs)
    {
        float totalCostSum = 0f;

        foreach (var layer in vertexLayers)
        {
            layer.ClearDerivatives();
        }
        foreach (var layer in weightsLayers)
        {
            layer.ClearDerivatives();
        }

        for (int trainingDataIndex = 0; trainingDataIndex < batchInputs.GetLength(0); trainingDataIndex++)
        {
            float[] input = batchInputs[trainingDataIndex];
            float[] expectedOutput = batchExpectedOutputs[trainingDataIndex];

            float[] calculatedOutput = NewCalculateWithDerivatives(input);
            float[] cost = new float[calculatedOutput.Length];
            float costSum = 0f;

            for (int i = 0; i < calculatedOutput.Length; i++)
            {
                cost[i] = (expectedOutput[i] - calculatedOutput[i]) * (expectedOutput[i] - calculatedOutput[i]);
                costSum += cost[i];
            }
            for (int i = 0; i < calculatedOutput.Length; i++)
            {
                vertexLayers[vertexLayers.Length - 1].valuesDerivatives[i] *=
                    (calculatedOutput[i] - expectedOutput[i]); //* costSum / expectedOutput.Length * learingSpeed;
                //if ((expectedOutput[i] - calculatedOutput[i]) * (expectedOutput[i] - calculatedOutput[i]) > 1)
                //{
                //    Debug.Log(vertexLayers[vertexLayers.Length - 1].valuesDerivatives[i]);
                //}
            }

            // iteration from last layer to the first (bcs its back propagation)
            for (int layerIndex = vertexLayers.Length - 1; layerIndex > 0; layerIndex--)
            {
                ref VertexLayer leftVertices = ref vertexLayers[layerIndex - 1];
                ref VertexLayer rightVertices = ref vertexLayers[layerIndex];
                ref WeightsLayer weightLayer = ref weightsLayers[layerIndex - 1];

                for (int rightVertexIndex = 0; rightVertexIndex < rightVertices.values.Length; rightVertexIndex++)
                {
                    rightVertices.biasesDerivative[rightVertexIndex] += rightVertices.valuesDerivatives[rightVertexIndex];
                }

                for (int leftVertexIndex = 0; leftVertexIndex < leftVertices.values.Length; leftVertexIndex++)
                {
                    float sum = 0;
                    for (int rightVertexIndex = 0; rightVertexIndex < rightVertices.values.Length; rightVertexIndex++)
                    {
                        sum +=
                            weightLayer.weights[weightLayer.GetIndex(leftVertexIndex, rightVertexIndex)] *
                            rightVertices.valuesDerivatives[rightVertexIndex];

                        weightLayer.derivatives[weightLayer.GetIndex(leftVertexIndex, rightVertexIndex)] +=
                            leftVertices.values[leftVertexIndex] *
                            rightVertices.valuesDerivatives[rightVertexIndex];
                    }
                    leftVertices.valuesDerivatives[leftVertexIndex] *= sum;
                }
            }

            totalCostSum += costSum;
            // Debug.Log("traingin completed");
        }

        foreach (var layer in vertexLayers)
        {
            layer.ApplyDerivative(batchInputs.GetLength(0), learningSpeed);
        }
        foreach (var layer in weightsLayers)
        {
            layer.ApplyDerivative(batchInputs.GetLength(0), learningSpeed);
        }

        return totalCostSum;
    }

    public void Save(string fileName)
    {
        QuickSaveWriter writer = QuickSaveWriter.Create(fileName);
        writer.Write("Vertices", vertexLayers);
        writer.Write("Weights", weightsLayers);
        writer.Commit();
    }

    public bool Load(string fileName)
    {
        QuickSaveReader reader = QuickSaveReader.Create(fileName);
        if (reader != null)
        {
            vertexLayers = reader.Read<VertexLayer[]>("Vertices");
            weightsLayers = reader.Read<WeightsLayer[]>("Weights");
            return true;
        }
        else
        {
            Debug.LogWarning("Cant load ann values, files doesnt exists");
            return false;
        }
    }

    private float ActivationFunction(float value)
    {
        return 2 / (1 + Mathf.Exp(-2*value)) - 1;
    }

    private float ActivationFunctionDerivative(float value)
    {
        float fx = ActivationFunction(value);
        return 1 - fx * fx;
    }
}

public struct VertexLayer
{
    public float[] values;
    public float[] biases;
    public float[] valuesDerivatives;
    public float[] biasesDerivative;
    int size => (values.Length);

    public VertexLayer(int size)
    {
        values = new float[size];
        biases = new float[size];
        valuesDerivatives = new float[size];
        biasesDerivative = new float[size];
    }

    public void ClearDerivatives()
    {
        for (int i = 0; i < size; i++)
        {
            biasesDerivative[i] = 0;
        }
    }

    public void ApplyDerivative(float batchSize, float learningSpeed)
    {
        for (int i = 0; i < size; i++)
        {
            biasesDerivative[i] /= batchSize;
            biases[i] -= biasesDerivative[i] * learningSpeed;
        }
    }
}

public struct WeightsLayer
{
    public float[] weights;
    public float[] derivatives;

    public int inSize;
    public int outSize;

    public WeightsLayer(int inSize, int outSize)
    {
        weights = new float[inSize * outSize];
        derivatives = new float[inSize * outSize];
        this.inSize = inSize;
        this.outSize = outSize;
    }

    public int GetIndex(int inVertex, int outVertex)
    {
        return inVertex + inSize * outVertex;
    }

    public void ClearDerivatives()
    {
        for (int i = 0; i < inSize; i++)
        {
            for (int j = 0; j < outSize; j++)
            {
                derivatives[GetIndex(i, j)] = 0;
            }
        }
    }

    public void ApplyDerivative(float batchSize, float learningSpeed)
    {
        for (int i = 0; i < inSize; i++)
        {
            for (int j = 0; j < outSize; j++)
            {
                derivatives[GetIndex(i, j)] /= batchSize;
                weights[GetIndex(i, j)] -= derivatives[GetIndex(i, j)] * learningSpeed;
            }
        }
    }
}