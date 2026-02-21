using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using UnityEngine;

[System.Serializable]
public class SerializableDictionary<TKey, TValue> : ISerializationCallbackReceiver
{
    [SerializeField] private List<TKey> keys = new();
    [SerializeField] private List<TValue> values = new();

    private Dictionary<TKey, TValue> dictionary = new();

    public Dictionary<TKey, TValue> Dictionary => dictionary;

    public void OnBeforeSerialize()
    {
        keys.Clear();
        values.Clear();

        foreach (var kvp in dictionary)
        {
            keys.Add(kvp.Key);
            values.Add(kvp.Value);
        }
    }

    public void OnAfterDeserialize()
    {
        dictionary = new Dictionary<TKey, TValue>();

        for (int i = 0; i < Mathf.Min(keys.Count, values.Count); i++)
            dictionary[keys[i]] = values[i];
    }
}

public class Geo : MonoBehaviour
{
    [Header("Grid")]
    public int width = 200;   // x dimension
    public int height = 200;  // z dimension
    public float cellSize = 50f;

    [Header("Heights")]
    public float heightScale = 1f; // multiply heights by this
    public bool recalcNormals = true;

    public GameObject geoObject;

    [SerializeField]
    private SerializableDictionary<(int x, int y), List<float>> grid = new SerializableDictionary<(int, int), List<float>>();

    // Start is called before the first frame update
    void Start()
    {

        List<string> Citations = ReadCitationsFile();
        print(Citations.Count);
        for (int i = 0; i < Citations.Count; i++)
        {
            char[] charsStart = { Citations[i][0], Citations[i][1] };
            string start = new string(charsStart);

            char[] charsEnd = { Citations[i][2], Citations[i][3] };
            string end = new string(charsEnd);

            string fileName = "/GeoData/Data/" + start + "/" + start.ToUpper() + end + ".asc";

            print(fileName);

            List<string> words = ReadHeightFile(fileName);
            List<float> heights = new List<float>();

            int xCorner = int.Parse(words[5]);
            int yCorner = int.Parse(words[7]);

            for (int j = 10; j < words.Count; j++)
                heights.Add(float.Parse(words[j]));

            grid.Dictionary[(xCorner, yCorner)] = heights;
        }

        string json = JsonUtility.ToJson(grid);
        File.WriteAllText(Application.streamingAssetsPath + "/Grid/Data.json", json);

        print("Done");
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
