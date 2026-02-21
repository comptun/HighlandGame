using UnityEngine;

[CreateAssetMenu(menuName = "Terrain/Global Heightmap")]
public class GlobalHeightmap : ScriptableObject
{
    public int width;
    public int height;

    // Unity CAN serialize this
    public float[] heights;

    public float Get(int x, int z)
    {
        return heights[new Vector2Int(x, z).];
    }

    public void Set(int x, int z, float value)
    {
        heights[x + z * width] = value;
    }
}