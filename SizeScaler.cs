using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

public class SizeScaler : MonoBehaviour
{
    public float scaleObjectDimensions2D(SpriteRenderer toScale, Vector2 size, Transform parent)
    {
        GameObject scaledObject = parent.gameObject;

        if (toScale == null || size.x == 0 || size.y == 0)
        {
            return -1;
        }
        Quaternion orientation = parent.rotation;
        parent.rotation = Quaternion.Euler(0, 0, 0);
        Vector2 currentSize = toScale.bounds.size;
        if (currentSize.x < 0.01f || currentSize.y < 0.01f)
        {
            parent.localScale = Vector3.one;
            currentSize = toScale.bounds.size;
        }
        Vector3 scale = parent.localScale;
        Vector3 newScale = new Vector3((size.x / currentSize.x) * scale.x, (size.y / currentSize.y) * scale.y, 1);
        parent.localScale = newScale;
        parent.rotation = orientation;
        return (((Vector2)newScale).magnitude / ((Vector2)scale).magnitude);
    }

    public void scalePointLights2D(Light2D[] lights, float scale)
    {
        for (int i = 0; i < lights.Length; i++)
        {
            lights[i].pointLightInnerRadius *= scale;
            lights[i].pointLightOuterRadius *= scale;
        }
    }
}
