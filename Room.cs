using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

public class Room : MonoBehaviour
{
    public enum RoomTypes
    {
        Room,
    };

    public Vector2Int size;
    public string roomName;
    public GameObject gate;
    public Vector2 gateSize;
    public float gateThickness;
    [HideInInspector]
    public Vector2 position;
    public RoomInfo roomInfo;
    public RoomTypes roomType;
    SizeScaler sizeScaler;

    public void updatePosition()
    {
        gameObject.transform.localPosition = new Vector3(position.x, position.y, 0);
    }

    public void scaleToSize(float parrentScale = 1)
    {
        if(sizeScaler == null)
        {
            sizeScaler = gameObject.GetComponent<SizeScaler>();
            if(sizeScaler == null)
            {
                Debug.Log(roomName + ": SizeScalerComponent not found");
            }
        }
        SpriteRenderer largestSprite = findLargestSprite();
        float scale = sizeScaler.scaleObjectDimensions2D(largestSprite, size, transform);
        sizeScaler.scalePointLights2D(searchChildren<Light2D>(transform).ToArray(), scale * parrentScale);
    }

    public void scaleLights(float scale, float parrentScale = 1)
    {
        sizeScaler.scalePointLights2D(searchChildren<Light2D>(transform).ToArray(), scale * parrentScale);
    }

    private SpriteRenderer findLargestSprite()
    {
        List<SpriteRenderer> sprites = searchChildren<SpriteRenderer>(transform);
        if(sprites.Count == 0)
        {
            return null;
        }
        SpriteRenderer largestSprite = sprites[0];
        foreach(SpriteRenderer sprite in sprites)
        {
            if(sprite.bounds.size.magnitude > largestSprite.bounds.size.magnitude)
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
            if(component != null)
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
