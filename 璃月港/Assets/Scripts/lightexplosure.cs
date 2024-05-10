using System.Collections;
using System.Collections.Generic;
using UnityEngine;

 
public class lightexplosure : MonoBehaviour
{

    private float exp;
    private float max = 3.5f;
    private Light light;
    void Start()
    {

        exp = RenderSettings.ambientIntensity;
        light = GetComponent<Light>();
    }

    void Update()
    {
        exp = Mathf.Sin(this.transform.eulerAngles.x/360*2*Mathf.PI) *max;
        RenderSettings.ambientIntensity = exp;
        light.intensity = exp / max * 5;
    }
}
 
