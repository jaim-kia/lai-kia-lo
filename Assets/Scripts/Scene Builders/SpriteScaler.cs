using UnityEngine;

[ExecuteAlways]
public class MatchQuadToTexture : MonoBehaviour
{
    public Texture2D texture;
    public float pixelsPerUnit = 100f;

    void OnValidate() => Apply();
    void Reset() => Apply();
    void Awake() => Apply();

    void Apply()
    {
        if (texture == null) return;
        transform.localScale = new Vector3(
            texture.width / pixelsPerUnit,
            texture.height / pixelsPerUnit,
            1f
        );
    }
}