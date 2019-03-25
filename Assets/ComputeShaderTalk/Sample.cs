using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sample : MonoBehaviour
{
    const int NUMTHREADS_X = 8, NUMTHREADS_Y = 8;

    public ComputeShader shader;
    public Vector2Int resolution;
    private int _sampleKernelID;

    public Material someMaterial;

#if UNITY_EDITOR //only execute this code if within the Unity Editor Environment, i.e. not in your build!
    private void OnValidate()
    {
        if (resolution.x % NUMTHREADS_X != 0)
            Debug.LogWarning($"resolution in the X-axis: {resolution.x} is not divisable by NUMTHREADS_X: {NUMTHREADS_X}. The execution will be flawed");
        if (resolution.y % NUMTHREADS_Y != 0)
            Debug.LogWarning($"resolution in the Y-axis: {resolution.y} is not divisable by NUMTHREADS_X: {NUMTHREADS_Y}. The execution will be flawed");
    }
#endif


    private void Awake()
    {
        _sampleKernelID = shader.FindKernel("CSMain");

        RenderTexture rTex = new RenderTexture(resolution.x, resolution.y, 0)
        {   
            graphicsFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8A8_UInt,
            enableRandomWrite = true
        };
        rTex.Create();

        //NOTICE: expected type is 'Texture', you can pass normal 
        //        textures to a computeshader as well, but they 
        //        will not be writable.
        shader.SetTexture(_sampleKernelID, "Result", rTex);
        shader.SetInts("texRes", resolution.x, resolution.y);
        
        shader.Dispatch(_sampleKernelID, resolution.x / NUMTHREADS_X, resolution.y / NUMTHREADS_Y, 1);

        someMaterial.mainTexture = rTex;
    }
}
