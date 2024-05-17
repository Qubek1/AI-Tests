using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ANNLayerCalculation : MonoBehaviour
{
    public ComputeShader computeShader;
    public ComputeShader layerBatchCalculationCS;
    public ComputeShader backpropagationBatchCalculationCS;

    private void Start()
    {
        //ANN ann = new ANN(5, 100, 100, 1);
        //float[] batchInputs = new float[5] { 1, 2, 3, 4, 5};
        //CalculateLayer(batchInputs, ann.weightsLayers[0].weights, ann.vertexLayers[1].values, ann.vertexLayers[1].biases, ann.vertexLayers[1].valuesDerivatives);
        //Debug.Log(ann.vertexLayers[1].values[0]);
    }

    public float[] Convert2DArrayTo1DArray(float[][] array)
    {
        float[] newArray = new float[array.GetLength(0) * array.GetLength(1)];
        for (int i = 0; i < array.GetLength(0); i++)
        {
            for (int j = 0; j < array.GetLength(1); j++)
            {
                newArray[j + i * array.GetLength(1)] = array[i][j];
            }
        }
        return newArray;
    }

    public void CalculateLayer(float[] inValues, float[] weights, float[] outValues, float[] biases, float[] derivatives)
    {
        ComputeBuffer outValuesBuffer = new ComputeBuffer(outValues.Length, sizeof(float));
        outValuesBuffer.SetData(outValues);
        computeShader.SetBuffer(0, "outValues", outValuesBuffer);

        ComputeBuffer inValuesBuffer = new ComputeBuffer(inValues.Length, sizeof(float));
        inValuesBuffer.SetData(inValues);
        computeShader.SetBuffer(0, "inValues", inValuesBuffer);

        ComputeBuffer weightsBuffer = new ComputeBuffer(weights.Length, sizeof(float));
        weightsBuffer.SetData(weights);
        computeShader.SetBuffer(0, "weights", weightsBuffer);

        ComputeBuffer biasesBuffer = new ComputeBuffer(biases.Length, sizeof(float));
        biasesBuffer.SetData(biases);
        computeShader.SetBuffer(0, "biases", biasesBuffer);

        ComputeBuffer derivativesBuffer = new ComputeBuffer(derivatives.Length, sizeof(float));
        derivativesBuffer.SetData(derivatives);
        computeShader.SetBuffer(0, "derivatives", derivativesBuffer);

        //computeShader.SetFloats("oldValues", oldValues);
        //computeShader.SetFloats("weights", weights);

        computeShader.SetInt("inValuesAmount", inValues.Length);
        computeShader.SetInt("outValuesAmount", outValues.Length);

        computeShader.Dispatch(0, outValues.Length / 10, 1, 1);
        outValuesBuffer.GetData(outValues);
        derivativesBuffer.GetData(derivatives);

        outValuesBuffer.Release();
        inValuesBuffer.Release();
        weightsBuffer.Release();
        biasesBuffer.Release();
        derivativesBuffer.Release();
    }

    public void CalculateDataBatchLayer(float[] inValuesMatrix, float[] outValuesMatrix, float[] derivativesMatrix, float[] biases, float[] weights, int inValuesAmount, int outValuesAmount, int batchSize)
    {
        ComputeBuffer inValuesMatrixBuffer = new ComputeBuffer(inValuesAmount * batchSize, sizeof(float));
        inValuesMatrixBuffer.SetData(inValuesMatrix);
        computeShader.SetBuffer(0, "inValuesMatrix", inValuesMatrixBuffer);

        ComputeBuffer outValuesMatrixBuffer = new ComputeBuffer(outValuesAmount * batchSize, sizeof(float));
        outValuesMatrixBuffer.SetData(outValuesMatrix);
        computeShader.SetBuffer(0, "outValuesMatrix", outValuesMatrixBuffer);

        ComputeBuffer derivativesMatrixBuffer = new ComputeBuffer(derivativesMatrix.Length, sizeof(float));
        derivativesMatrixBuffer.SetData(derivativesMatrix);
        computeShader.SetBuffer(0, "derivativesMatrix", derivativesMatrixBuffer);

        ComputeBuffer weightsBuffer = new ComputeBuffer(weights.Length, sizeof(float));
        weightsBuffer.SetData(weights);
        computeShader.SetBuffer(0, "weights", weightsBuffer);

        ComputeBuffer biasesBuffer = new ComputeBuffer(biases.Length, sizeof(float));
        biasesBuffer.SetData(biases);
        computeShader.SetBuffer(0, "biases", biasesBuffer);

        computeShader.SetInt("inValuesAmount", inValuesAmount);
        computeShader.SetInt("outValuesAmount", outValuesAmount);
        computeShader.SetInt("batchSize", batchSize);

        computeShader.Dispatch(0, outValuesAmount / 10, batchSize / 10, 1);
        outValuesMatrixBuffer.SetData(outValuesMatrix);
        derivativesMatrixBuffer.SetData(derivativesMatrix);

        inValuesMatrixBuffer.Release();
        outValuesMatrixBuffer.Release();
        derivativesMatrixBuffer.Release();
        weightsBuffer.Release();
        biasesBuffer.Release();
    }
}
