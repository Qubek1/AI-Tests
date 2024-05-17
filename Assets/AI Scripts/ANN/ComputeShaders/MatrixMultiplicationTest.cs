using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatrixMultiplicationTest : MonoBehaviour
{
    public ComputeShader computeShader;

    public RenderTexture renderTexture;

    public float[] oldValues;
    public float[] newValues;
    public float[] weights;

    public float[] CPUvalues;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            newValues = new float[10];
            oldValues = new float[10];
            weights = new float[100];
            for (int i = 0; i < 10; i++)
            {
                //newValues[i] = Random.Range(-1f, 1f);
                oldValues[i] = Random.Range(-1f, 1f);
                for (int j = 0; j < 10; j++)
                {
                    weights[i + j * 10] = Random.Range(-1f, 1f);
                }
            }

            //renderTexture = new RenderTexture(256, 256, 24);
            //renderTexture.enableRandomWrite = true;
            //renderTexture.Create();

            //computeShader.SetTexture(0, "Result", renderTexture);
            //computeShader.Dispatch(0, renderTexture.width / 8, renderTexture.height / 8, 1);

            ComputeBuffer newValuesBuffer = new ComputeBuffer(newValues.Length, sizeof(float));
            newValuesBuffer.SetData(newValues);
            computeShader.SetBuffer(0, "newValues", newValuesBuffer);

            ComputeBuffer oldValuesBuffer = new ComputeBuffer(oldValues.Length, sizeof(float));
            oldValuesBuffer.SetData(oldValues);
            computeShader.SetBuffer(0, "oldValues", oldValuesBuffer);

            ComputeBuffer weightsBuffer = new ComputeBuffer(weights.Length, sizeof(float));
            weightsBuffer.SetData(weights);
            computeShader.SetBuffer(0, "weights", weightsBuffer);

            computeShader.SetInt("oldValuesAmount", oldValues.Length);
            computeShader.SetInt("newValuesAmount", newValues.Length);

            computeShader.Dispatch(0, newValues.Length/10, 1, 1);
            newValuesBuffer.GetData(newValues);

            Debug.Log(newValues[0]);
            Debug.Log(newValues[1]);
            Debug.Log(newValues[2]);

            CPUvalues = new float[newValues.Length];

            for (int x = 0; x < oldValues.Length; x++)
            {
                for (int y = 0; y < newValues.Length; y++)
                {
                    CPUvalues[y] += oldValues[x] * weights[x + oldValues.Length * y];
                }
            }
        }
    }

    public void MultiplyVectorByMatrix(float[] vector, float[] matrix, float[] result)
    {
        ComputeBuffer resultBuffer = new ComputeBuffer(result.Length, sizeof(float));
        resultBuffer.SetData(result);
        computeShader.SetBuffer(0, "newValues", resultBuffer);

        computeShader.SetFloats("oldValues", vector);
        computeShader.SetFloats("weights", matrix);

        computeShader.SetInt("oldValuesAmount", vector.Length);
        computeShader.SetInt("newValuesAmount", result.Length);

        computeShader.Dispatch(0, result.Length / 10, 1, 1);
        resultBuffer.GetData(result);
    }
}
