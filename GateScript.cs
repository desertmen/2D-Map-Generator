using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

public class GateScript : MonoBehaviour
{
    public bool isOpen = false;
    public float lightIntensity;
    public float closingSpeed;

    public Vector2 size { get; private set; } = new Vector2(0.25f, 1);

    Material material;
    BoxCollider2D boxCollider;
    Light2D gateLight;

    bool last = false;


    // Start is called before the first frame update
    void Awake()
    {
        GameObject gateObject = transform.GetChild(0).gameObject;
        if(gameObject == null)
        {
            Debug.Log("Gate: gateObject child not found");
            Destroy(gameObject);
        }
        gateLight = transform.GetChild(1).gameObject.GetComponent<Light2D>();
        if(gateLight == null)
        {
            Debug.Log("Gate: Light on child 1 not found");
            Destroy(gameObject);
        }
        material = gateObject.GetComponent<SpriteRenderer>().material;
        boxCollider = gateObject.GetComponent<BoxCollider2D>();
        if(boxCollider == null)
        {
            Debug.Log("Gate: BoxCollider2D not found in child 0");
            Destroy(gameObject);
        }

        material.SetFloat("_Fill", 0);
        lightIntensity = gateLight.intensity;
        size = boxCollider.bounds.size;
        boxCollider.enabled = false;
        gateLight.intensity = 0;
    }

    private void Update()
    {
        if(last == isOpen)
        {
            return;
        }
        else if (isOpen)
        {
            open();
        }
        else
        {
            close();
        }
        last = isOpen;
    }

    public void open()
    {
        StartCoroutine(closeAnimation(-1));
        boxCollider.enabled = false;
    }

    public void close()
    {
        StartCoroutine(closeAnimation(1));
        boxCollider.enabled = true;
    }

    IEnumerator closeAnimation(int sign)
    {
        float percentage = material.GetFloat("_Fill");
        float step = closingSpeed / 10f;
        if(step <= 0)
        {
            yield return null;
        }
        for(float i = 0; i < 1; i += step)
        {
            percentage += sign * step;
            percentage = Mathf.Clamp(percentage, 0, 1);
            material.SetFloat("_Fill", percentage);
            gateLight.intensity = lightIntensity * percentage;
            yield return new WaitForSeconds(0.05f);
        }
    }
}
