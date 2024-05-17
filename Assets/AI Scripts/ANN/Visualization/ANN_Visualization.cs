using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ANN_Visualization : MonoBehaviour
{
    public string fileName;
    public int[] layerSizes;
    public float[] input;
    public float[] expectedOutput;
    public float learningSpeed;
    public int batchSize = 20;
    public int maxIterations = 100000;

    public Gradient gradient;
    public GameObject vertexPrefab;
    public GameObject layerPrefab;
    public Transform layersContainer;
    private List<List<GameObject>> vertices = new List<List<GameObject>>();

    public ANN ann;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            ann.learningSpeed = learningSpeed;
            ann.Train(input, expectedOutput);
            //ann.CalculateWithDerivatives(input);
            UpdateVisualization();
        }
        if (Input.GetKeyDown(KeyCode.W))
        {
            float costSum = 0;
            ann.learningSpeed = learningSpeed;
            float[][] batchInput = new float[100000][];
            float[][] batchOutput = new float[100000][];
            for (int i = 0; i < 100000; i++)
            {
                float[] trainingInput = new float[input.Length];
                float[] trainingOutput = new float[2];
                for (int j = 0; j < input.Length; j++)
                {
                    trainingInput[j] = UnityEngine.Random.Range(-1f, 1f);
                }
                trainingOutput[0] = trainingInput[0] * trainingInput[1];
                trainingOutput[1] = (trainingInput[0] + trainingInput[1]) / 2f;
                batchInput[i] = trainingInput;
                batchOutput[i] = trainingOutput;
            }
            costSum = ann.Train(batchInput, batchOutput);
            UpdateVisualization();
            Debug.Log(costSum);
            Debug.Log(costSum / batchSize / maxIterations);
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            ann.learningSpeed = learningSpeed;
            float costSum = 0;
            for (int iteration = 0; iteration < maxIterations; iteration++)
            {
                float[][] batchInput = new float[batchSize][];
                float[][] batchOutput = new float[batchSize][];
                for (int i = 0; i < batchSize; i++)
                {
                    float[] trainingInput = new float[input.Length];
                    float[] trainingOutput = new float[2];
                    for (int j = 0; j < input.Length; j++)
                    {
                        trainingInput[j] = UnityEngine.Random.Range(-1f, 1f);
                    }
                    trainingOutput[0] = trainingInput[0] * trainingInput[1];
                    trainingOutput[1] = (trainingInput[0] + trainingInput[1]) / 2f;
                    batchInput[i] = trainingInput;
                    batchOutput[i] = trainingOutput;
                }
                costSum += ann.Train(batchInput, batchOutput);
            }
            UpdateVisualization();
            Debug.Log(costSum);
            Debug.Log(costSum / batchSize / maxIterations);
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            ann.learningSpeed = learningSpeed;
            float[] trainingInput = new float[input.Length];
            float[] trainingOutput = new float[2];
            for (int j = 0; j < input.Length; j++)
            {
                trainingInput[j] = UnityEngine.Random.Range(-1f, 1f);
            }
            trainingOutput[0] = trainingInput[0] * trainingInput[1];
            trainingOutput[1] = (trainingInput[0] + trainingInput[1])/2f;
            ann.Train(trainingInput, trainingOutput);
            ann.NewCalculateWithDerivatives(trainingInput);
            input = trainingInput;
            expectedOutput = trainingOutput;
            UpdateVisualization();
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            ann.Save(fileName);
        }
        if (Input.GetKeyDown(KeyCode.L))
        {
            if (ann != null)
            {
                ann.Load(fileName);
            }
            else
            {
                ann = new ANN(fileName);
            }
        }
        if (Input.GetKeyDown(KeyCode.N))
        {
            ann = new ANN(layerSizes);
        }
        if (Input.GetKeyDown(KeyCode.V))
        {
            CreateVisualization();
        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            ann.NewCalculateWithDerivatives(input);
        }
        if (Input.GetKeyDown(KeyCode.U))
        {
            UpdateVisualization();
        }
    }

    public void CreateVisualization()
    {
        int layerIndex = 0;
        foreach (VertexLayer vertexLayer in ann.vertexLayers)
        {
            List<GameObject> verticesInLayer = new List<GameObject>();
            Transform newLayer = Instantiate(layerPrefab, layersContainer).transform;
            for (int vertexInLayerIndex = 0; vertexInLayerIndex < vertexLayer.values.Length; vertexInLayerIndex++)
            {
                GameObject newVertex = Instantiate(vertexPrefab, newLayer);
                verticesInLayer.Add(newVertex);

                newVertex.GetComponentInChildren<TextMeshProUGUI>().text = vertexLayer.values[vertexInLayerIndex].ToString("N3");
            }
            vertices.Add(verticesInLayer);
            layerIndex++;
        }
    }

    public void UpdateVisualization()
    {
        float maxDerivative = float.NegativeInfinity;
        for (int layerIndex = 0; layerIndex < ann.vertexLayers.Length; layerIndex++)
        {
            for (int vertexIndex = 0; vertexIndex < vertices[layerIndex].Count; vertexIndex++)
            {
                maxDerivative = Mathf.Max(maxDerivative, Mathf.Abs(ann.vertexLayers[layerIndex].valuesDerivatives[vertexIndex]));
            }
        }
        Debug.Log(maxDerivative);
        for (int layerIndex = 0; layerIndex < ann.vertexLayers.Length; layerIndex++)
        {
            for (int vertexIndex = 0; vertexIndex < vertices[layerIndex].Count; vertexIndex++)
            {
                vertices[layerIndex][vertexIndex].GetComponentInChildren<TextMeshProUGUI>().text =
                    ann.vertexLayers[layerIndex].values[vertexIndex].ToString("N3");
                if (maxDerivative != 0)
                {
                    vertices[layerIndex][vertexIndex].GetComponent<Image>().color =
                        gradient.Evaluate((ann.vertexLayers[layerIndex].valuesDerivatives[vertexIndex] / maxDerivative + 1f) / 2f);
                }
                vertices[layerIndex][vertexIndex].GetComponent<VertexInfo>().UpdateInfo(ann, layerIndex, vertexIndex);
            }
        }
    }
}