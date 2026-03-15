using UnityEngine;


public class AutoTextureTiler : MonoBehaviour
{
    private Renderer myRenderer;

    void Start()
    {
        myRenderer = GetComponent<Renderer>();


        if (Application.isPlaying)
        {
            if (myRenderer != null && myRenderer.sharedMaterial != null)
            {
                myRenderer.material = new Material(myRenderer.sharedMaterial);
            }
        }

        UpdateTiling();
    }

    void Update()
    {

        if (Application.isPlaying)
        {
            UpdateTiling();
        }
    }

    void UpdateTiling()
    {

        if (myRenderer != null && myRenderer.material != null)
        {
            Vector3 scale = transform.localScale;
            myRenderer.material.mainTextureScale = new Vector2(scale.x, scale.y);
        }
    }
}
