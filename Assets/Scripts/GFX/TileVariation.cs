using UnityEngine;

public class TileVariation : MonoBehaviour
{
    [Range(0f, 0.15f)]
    public float colorVariation = 0.05f;

    [Range(0f, 0.3f)]
    public float smoothnessVariation = 0.1f;

    void Awake()
    {
        var renderer = GetComponent<Renderer>();
        var block = new MaterialPropertyBlock();
        renderer.GetPropertyBlock(block);

        // ✅ Deterministic seed per tile
        int tileSeed =
            LevelSeed.Seed ^
            transform.position.GetHashCode() ^
            gameObject.name.GetHashCode();

        System.Random rng = new System.Random(tileSeed);

        Color baseColor = renderer.sharedMaterial.color;

        float r = 1f + RandomRange(rng, -colorVariation, colorVariation);
        float g = 1f + RandomRange(rng, -colorVariation, colorVariation);
        float b = 1f + RandomRange(rng, -colorVariation, colorVariation);

        Color variedColor = new Color(
            baseColor.r * r,
            baseColor.g * g,
            baseColor.b * b,
            baseColor.a
        );

        float baseSmoothness = renderer.sharedMaterial.GetFloat("_Glossiness");
        float variedSmoothness = baseSmoothness + RandomRange(
            rng,
            -smoothnessVariation,
            smoothnessVariation
        );

        block.SetColor("_Color", variedColor);
        block.SetFloat("_Glossiness", Mathf.Clamp01(variedSmoothness));

        renderer.SetPropertyBlock(block);
    }

    float RandomRange(System.Random rng, float min, float max)
    {
        return (float)(min + rng.NextDouble() * (max - min));
    }
}
