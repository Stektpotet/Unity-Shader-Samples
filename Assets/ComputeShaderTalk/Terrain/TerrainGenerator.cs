using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public class TerrainGenerator : MonoBehaviour
{
    const int 
        NUMTHREADS_X = 16, 
        NUMTHREADS_Y = 16;

    public Terrain terrain;
    public ComputeShader shader;
    public int resolution;
    public Vector2Int offset;

    private RenderTexture _heightMap, _nrmMap;
    private ComputeBuffer _heightBuffer;

    // The Kernel IDs for the different compute shaders
    private int _heightBufferKernelID, _heightMapKernelID, _bumpMapKernelID;
    
    private float[,] heights;

#if UNITY_EDITOR //only execute this code if within the Unity Editor Environment, i.e. not in your build!
    private void OnValidate()
    {
        if (resolution % NUMTHREADS_X != 0)
            Debug.LogWarning($"resolution in the X-axis: {resolution} is not divisable by NUMTHREADS_X: {NUMTHREADS_X}. The execution will be flawed");
        if (resolution % NUMTHREADS_Y != 0)
            Debug.LogWarning($"resolution in the Y-axis: {resolution} is not divisable by NUMTHREADS_X: {NUMTHREADS_Y}. The execution will be flawed");
        if(terrain?.terrainData.heightmapWidth - 1 != resolution)
            Debug.LogWarning($"resolution is not the same as the terrain's: resolution - {resolution}, terrain - {terrain?.terrainData.heightmapWidth - 1 ?? 0}. The execution will be flawed");

    }
#endif

    [ContextMenu("Run Awake()")]
    void Awake()
    {
        if (!shader || !terrain)
            return;

        CommonSetup();
        SetupHeightBufferGenerator();
        SetupHeightMapGenerator();
        SetupBumpMapGenerator();
    }
    
    private void SetupHeightBufferGenerator()
    {
        _heightBuffer = new ComputeBuffer(resolution * resolution, sizeof(float), ComputeBufferType.Structured);

        _heightBufferKernelID = shader.FindKernel("GenerateHeights");
        shader.SetBuffer(_heightBufferKernelID, "heightBuffer", _heightBuffer);
    }
    private void SetupHeightMapGenerator()
    {
        _heightMap = new RenderTexture(resolution, resolution, 0) {
            enableRandomWrite = true
        };
        _heightMap.Create();

        _heightMapKernelID = shader.FindKernel("GenerateHeightMap");
        shader.SetTexture(_heightMapKernelID, "heightMap", _heightMap);
    }
    private void SetupBumpMapGenerator()
    {
        _nrmMap = new RenderTexture(resolution, resolution, 0) {
            enableRandomWrite = true
        };
        _nrmMap.Create();

        _bumpMapKernelID = shader.FindKernel("GenerateBumpMap");
        shader.SetTexture(_heightMapKernelID, "bumpMap", _nrmMap);
    }
    private void CommonSetup()
    {
        shader.SetInt("terrainSize", resolution);
        heights = new float[resolution, resolution];
    }


    // Update is called once per frame
    void Update()
    {
        if (!Input.GetKeyDown(KeyCode.Space))
            return;

        shader.SetInts("ofsettingSeed", offset.x, offset.y);
        shader.Dispatch(_heightBufferKernelID, resolution / NUMTHREADS_X, resolution / NUMTHREADS_Y, 1);

        _heightBuffer.GetData(heights); //read the buffer into the local heights-array
        
        //The terrain has an extra vertex in both axes, we're not dealing with them right now, 
        // though they could easily be wrapped to be the same as on the opposite side
        terrain.terrainData.SetHeights(1, 1, heights); 
    }

    private void OnDisable()
    {
        _heightBuffer?.Dispose();
    }
    private void OnDestroy()
    {
        _heightBuffer?.Dispose();
    }
}
