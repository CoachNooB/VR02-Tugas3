using UnityEngine;

public class StationClickEffect : MonoBehaviour
{
    public Color originalColor = Color.white;
    public string stationName = "";

    private Renderer rend;
    private Material mat;

    void Start()
    {
        rend = GetComponent<Renderer>();
        if (rend != null)
        {
            mat = rend.material;
        }
    }

    public void SetColor(Color color)
    {
        if (rend != null)
        {
            mat.color = color;
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", color * 0.3f);
        }
    }

    public void ResetColor()
    {
        if (rend != null)
        {
            mat.color = originalColor;
            mat.DisableKeyword("_EMISSION");
        }
    }
}