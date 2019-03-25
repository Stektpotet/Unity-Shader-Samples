using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class ComputeTest : MonoBehaviour
{
    public ComputeShader shader;
    public RenderTexture heightMap, nrmMap;
    public int resolution;
    public int erosionParticles = 64;
    public RenderTextureDescriptor renderTextureDescriptor;
    public ComputeBuffer heightBuffer, particleBuffer;
    int generatorHeightMapKernelID,
        generatorNormalKernelID,
        generatorBufferKernelID,
        ErodeHeightMapKernelID;

    public Texture2D bump;

    public Vector2 offset;
    private float[] _offset = new float[2];
    public Terrain terrain;
    public MeshRenderer planeRenderer;

    struct particle
    {
        public Vector2 pos;
        public Vector2 dir;
        public float speed; //TODO uniform for initial value
        public float water; //TODO uniform for initial value
        public float sediment;
    };
    particle[] particles;
    private void Start()
    {
        renderTextureDescriptor = new RenderTextureDescriptor(
            width: resolution,
            height: resolution,
            colorFormat: RenderTextureFormat.ARGB64, //32 bit heightmap makes for a height variance of 1/4294967296
            depthBufferBits: 0
        );



        generatorHeightMapKernelID = shader.FindKernel("GenerateHeightMap");
        generatorBufferKernelID = shader.FindKernel("GenerateHeights");
        generatorNormalKernelID = shader.FindKernel("GenerateNormals");
        ErodeHeightMapKernelID = shader.FindKernel("Erode");

        heightMap = new RenderTexture(renderTextureDescriptor) { enableRandomWrite = true };
        heightMap.Create();

        nrmMap = new RenderTexture(renderTextureDescriptor) { enableRandomWrite = true };
        nrmMap.Create();

        heightBuffer = new ComputeBuffer(resolution * resolution, sizeof(float));
        particleBuffer = new ComputeBuffer(erosionParticles, 28);

        particles = new particle[erosionParticles];
        for (int i = 0; i < particles.Length; i++)
        {
            particles[i].speed = 1;
            particles[i].water = 1;
            particles[i].pos = new Vector2(Random.value, Random.value)*resolution;
        }

        particleBuffer.SetData(particles);


        shader.SetTexture(generatorHeightMapKernelID, "BumpTex", bump);
        shader.SetTexture(generatorHeightMapKernelID, "heightMap", heightMap);

        shader.SetTexture(generatorNormalKernelID, "nrmMap", nrmMap);
        shader.SetTexture(generatorNormalKernelID, "BumpTex", bump);

        shader.SetTexture(generatorBufferKernelID, "BumpTex", bump);
        shader.SetBuffer(generatorBufferKernelID, "heightBuffer", heightBuffer);

        shader.SetBuffer(ErodeHeightMapKernelID, "heightBuffer", heightBuffer);
        shader.SetBuffer(ErodeHeightMapKernelID, "particles", particleBuffer);


        shader.SetInt("heightMapRes", resolution);



        foreach (var item in planeRenderer.sharedMaterial.GetTexturePropertyNames())
        {
            Debug.Log(item);
        }

        planeRenderer.sharedMaterial.SetTexture("_BumpMap", nrmMap);
        planeRenderer.sharedMaterial.SetTexture("_MainTex", heightMap);
        GameObject.CreatePrimitive(PrimitiveType.Quad).GetComponent<Renderer>().sharedMaterial.mainTexture = heightMap;
    }

    bool updateTerrain;
    private void Update()
    {
        shader.SetFloat("time", Time.time*0.5f);
        _offset[0] = offset.x;
        _offset[1] = offset.y;
        shader.SetFloats("offset", _offset);

        shader.Dispatch(generatorHeightMapKernelID, renderTextureDescriptor.width / 8, renderTextureDescriptor.width / 8, 1);
        shader.Dispatch(generatorNormalKernelID, renderTextureDescriptor.width / 8, renderTextureDescriptor.width / 8, 1);


        if (Input.GetKeyDown(KeyCode.Space) && terrain?.terrainData)
            updateTerrain = !updateTerrain;

        if(updateTerrain)
        {
            shader.Dispatch(generatorBufferKernelID, resolution / 8, resolution / 8, 1);
            float[,] heights = new float[resolution, resolution];
            heightBuffer.GetData(heights);
            terrain.terrainData.SetHeights(1, 1, heights);
            //Vector3[] verts = new Vector3[resolution * resolution];
            //shader.Dispatch(GenerateVertsKernelID, resolution / 8, resolution / 8, 1);
            //vertexBuffer.GetData(verts);
            //mesh.vertices = verts;
        }
        if (Input.GetKeyDown(KeyCode.G) && terrain?.terrainData)
        {
            updateTerrain = false;
            StartCoroutine(Erode());
        }
    }

    IEnumerator Erode()
    {
        float[,] heights = new float[resolution, resolution];
        for (int i = 0; i < 32; i++)
        {
            for (int p = 0; p < particles.Length; p++)
            {
                particles[p].speed = 1;
                particles[p].water = 1;
                particles[p].pos = new Vector2(Random.value, Random.value) * resolution;
            }
            particleBuffer.SetData(particles);

            shader.Dispatch(ErodeHeightMapKernelID, erosionParticles / 32, 1, 1);
            shader.Dispatch(ErodeHeightMapKernelID, erosionParticles / 32, 1, 1);
            shader.Dispatch(ErodeHeightMapKernelID, erosionParticles / 32, 1, 1);
            shader.Dispatch(ErodeHeightMapKernelID, erosionParticles / 32, 1, 1);
            shader.Dispatch(ErodeHeightMapKernelID, erosionParticles / 32, 1, 1);
            shader.Dispatch(ErodeHeightMapKernelID, erosionParticles / 32, 1, 1);
            shader.Dispatch(ErodeHeightMapKernelID, erosionParticles / 32, 1, 1);
            shader.Dispatch(ErodeHeightMapKernelID, erosionParticles / 32, 1, 1);
            yield return new WaitForSeconds(0.25f);
            heightBuffer.GetData(heights);
            terrain.terrainData.SetHeights(1, 1, heights);
        }
    }

    private void OnDestroy()
    {
        heightBuffer.Release();
        particleBuffer.Release();
    }
}
