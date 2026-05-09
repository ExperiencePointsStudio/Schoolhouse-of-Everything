using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Renderer))]
public class AutoTile : MonoBehaviour
{
    [Tooltip("Размер одной плитки в юнитах")]
    public Vector2 cellSize = new Vector2(1f, 1f);
    [Tooltip("Множитель размера плитки (1 = нормальный)")]
    public float scaleFactor = 1f;

    Renderer rend;
    Vector3 lastBoundsSize = Vector3.zero;

    void Awake()
    {
        rend = GetComponent<Renderer>();
        if (rend == null) return;
        // создаём уникальную копию материала для этого объекта,
        // чтобы изменения Tiling не влияли на другие объекты
        if (rend.sharedMaterial != null)
            rend.material = new Material(rend.sharedMaterial);

        UpdateTile();
    }

    void OnValidate()
    {
        // вызывается в редакторе при изменениях в инспекторе
        UpdateTile();
    }

    void Update()
    {
        // В редакторе обновляем только если изменился реальный размер
        if (!Application.isPlaying)
        {
            Vector3 boundsSize = GetWorldBoundsSize();
            if (boundsSize != lastBoundsSize)
            {
                UpdateTile();
            }
        }
    }

    Vector3 GetWorldBoundsSize()
    {
        if (rend == null) rend = GetComponent<Renderer>();
        if (rend != null) return rend.bounds.size;
        return transform.lossyScale;
    }

    void UpdateTile()
    {
        if (rend == null) rend = GetComponent<Renderer>();
        if (rend == null) return;
        if (rend.material == null) return;

        Vector3 size = GetWorldBoundsSize();
        lastBoundsSize = size;

        float sx = Mathf.Max(0.0001f, (size.x / Mathf.Max(0.0001f, cellSize.x)) / Mathf.Max(0.0001f, scaleFactor));
        float sz = Mathf.Max(0.0001f, (size.z / Mathf.Max(0.0001f, cellSize.y)) / Mathf.Max(0.0001f, scaleFactor));

        rend.material.mainTextureScale = new Vector2(sx, sz);
    }
}
