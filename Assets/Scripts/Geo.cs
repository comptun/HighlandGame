using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using UnityEngine;

[System.Serializable]
public class SerializableList<T>
{
    public List<T> list;
}

public class Geo : MonoBehaviour
{

    [SerializeField] private SerializableList<float> globalHeights;

    [Header("Grid")]
    public int width = 200;   // x dimension
    public int height = 200;  // z dimension
    public float cellSize = 50f;

    [Header("Heights")]
    public float heightScale = 1f; // multiply heights by this
    public bool recalcNormals = true;

    public GameObject geoObject;

    // Start is called before the first frame update
    void Start()
    {
        int index = 0;
        for (int i = 28; i < 39; i++)
        {
            NewGeoMesh("nd/ND" + i.ToString() + ".asc", index);
            index += 1;
        }

        //List<string> Citations = ReadCitationsFile();
        //print(Citations.Count);
        //for (int i = 0; i < Citations.Count; i++)
        //{

        //}
    }

    // Update is called once per frame
    void Update()
    {

    }

    List<string> ReadCitationsFile()
    {
        string path = Application.streamingAssetsPath + "/GeoData/citations.txt";

        string text = File.ReadAllText(path);

        // split by whitespace (space, newline, tab etc.)
        string[] tokens = text.Split(",", System.StringSplitOptions.RemoveEmptyEntries);

        return new List<string>(tokens);
    }

    List<float> GetHeights(string name)
    {
        string fileName = "/GeoData/Data/" + name;

        List<string> words = ReadHeightFile(fileName);
        List<float> heights = new List<float>();

        int xCorner = int.Parse(words[5]);
        int yCorner = int.Parse(words[7]);

        for (int i = 10; i < words.Count; i++)
            heights.Add(float.Parse(words[i]));

        return heights;
    }

    List<string> ReadHeightFile(string fileName)
    {
        string path = Application.streamingAssetsPath + fileName;

        string text = File.ReadAllText(path);

        // split by whitespace (space, newline, tab etc.)
        string[] tokens = text.Split((char[])null, System.StringSplitOptions.RemoveEmptyEntries);

        return new List<string>(tokens);
    }

    void NewGeoMesh(string name, int index)
    {
        List<float> heights = GetHeights(name);

        int vertCount = width * height;
        Vector3[] verts = new Vector3[vertCount];
        Vector2[] uvs = new Vector2[vertCount];

        // Build vertices + uvs
        for (int z = 0; z < height; z++)
        {
            for (int x = 0; x < width; x++)
            {
                int i = x + z * width;     // row-major
                float y = heights[i] * heightScale;

                float offsetX = (width - 1) * cellSize * 0.5f;
                float offsetZ = (height - 1) * cellSize * 0.5f;
                verts[i] = new Vector3(x * cellSize - offsetX, y, z * cellSize - offsetZ);
                uvs[i] = new Vector2((float)x / (width - 1), (float)z / (height - 1));
            }
        }

        // Each quad (cell) becomes 2 triangles => 6 indices
        int quadCount = (width - 1) * (height - 1);
        int[] tris = new int[quadCount * 6];

        int t = 0;
        for (int z = 0; z < height - 1; z++)
        {
            for (int x = 0; x < width - 1; x++)
            {
                int i = x + z * width;

                // Vert indices of the quad
                int a = i;             // (x, z)
                int b = i + width;     // (x, z+1)
                int c = i + 1;         // (x+1, z)
                int d = i + width + 1; // (x+1, z+1)

                // Two triangles (choose winding; this is Unity-friendly clockwise from above)
                tris[t++] = a;
                tris[t++] = b;
                tris[t++] = c;

                tris[t++] = c;
                tris[t++] = b;
                tris[t++] = d;
            }
        }

        Mesh mesh = new Mesh();

        // For > 65535 vertices you must use UInt32 indices. 200*200=40000 so UInt16 is fine.
        // mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        mesh.name = "HeightmapMesh";
        mesh.vertices = verts;
        mesh.triangles = tris;
        mesh.uv = uvs;

        if (recalcNormals)
            mesh.RecalculateNormals();

        mesh.RecalculateBounds();

        GameObject newGeo = Instantiate(geoObject);
        newGeo.GetComponent<MeshFilter>().sharedMesh = mesh;
        newGeo.transform.position = new Vector3(0,0,-index * cellSize * (height));
    }
}
