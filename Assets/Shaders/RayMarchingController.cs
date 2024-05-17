using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RayMarchingController : MonoBehaviour
{
    public ComputeShader shader;
    public RenderTexture renderTexture;
    public float cameraZ;
    public Vector2Int resolution;
    public Vector3 repeatVector;

    void Start()
    {
        renderTexture = new RenderTexture(resolution.x, resolution.y, 24);
        renderTexture.enableRandomWrite = true;
        GetComponent<RawImage>().texture = renderTexture;
    }

    void Update()
    {
        shader.SetFloat("cameraZ", cameraZ);
        shader.SetFloat("time", Time.time / 10f);
        shader.SetFloats("repeatVector", repeatVector.x, repeatVector.y, repeatVector.z);
        shader.SetInts("resolution", renderTexture.width, renderTexture.height);
        shader.SetTexture(0, "Result", renderTexture);
        shader.Dispatch(0, renderTexture.width / 8, renderTexture.height / 8, 1);

    }
}
