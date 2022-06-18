using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

public class Hallway : MonoBehaviour
{
    public Vector2 Size;
    SizeScaler sizeScaler;
    public void scaleToSize(float parrentScale = 1)
    {
        if(sizeScaler == null)
        {
            sizeScaler = GetComponent<SizeScaler>();
            if(sizeScaler == null)
            {
                Debug.Log(gameObject.name + ": SizeScaler component not found");
            }
        }

        SpriteRenderer largestSprite = findLargestSprite();
        float scale = sizeScaler.scaleObjectDimensions2D(largestSprite, Size, transform);
        sizeScaler.scalePointLights2D(searchChildren<Light2D>(transform).ToArray(), scale * parrentScale);
    }

    public void scaleLights(float scale, float parrentScale = 1)
    {
        sizeScaler.scalePointLights2D(searchChildren<Light2D>(transform).ToArray(), scale * parrentScale);
    }

    private SpriteRenderer findLargestSprite()
    {
        List<SpriteRenderer> sprites = searchChildren<SpriteRenderer>(transform);
        if (sprites.Count == 0)
        {
            return null;
        }
        SpriteRenderer largestSprite = sprites[0];
        foreach (SpriteRenderer sprite in sprites)
        {
            if (sprite.bounds.size.magnitude > largestSprite.bounds.size.magnitude)
            {
                largestSprite = sprite;
            }
        }
        return largestSprite;
    }

    private List<T> searchChildren<T>(Transform parent)
    {
        List<T> list = new List<T>();
        searchChildren<T>(parent, list);
        return list;
    }

    private void searchChildren<T>(Transform parent, List<T> list)
    {
        for (int i = 0; i < parent.childCount; i++)
        {
            var component = parent.GetChild(i).GetComponent<T>();
            if (component != null)
            {
                if (component.ToString().Equals("null") == false)
                {
                    list.Add(component);
                }
            }
            searchChildren(parent.GetChild(i), list);
        }
    }
}
