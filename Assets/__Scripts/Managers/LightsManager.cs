using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class LightsManager : MonoBehaviour
{
    private List<Light> lights = new List<Light>();
    private List<GameObject> billboards = new List<GameObject>();
    private TimeManager timeManager;
    
    private Color originalEmissionColor;

    private void Start()
    {
        timeManager = FindObjectOfType<TimeManager>();
        
        GameObject[] lightObjects = GameObject.FindGameObjectsWithTag("Light");
        GameObject[] billboardObjects = GameObject.FindGameObjectsWithTag("Billboard");
        foreach (GameObject obj in lightObjects)
        {
            Light light = obj.GetComponent<Light>();
            if (light != null)
            {
                lights.Add(light);
                light.enabled = false;

                var parent = light.GetComponentInParent<MeshRenderer>();
                if (timeManager.currentTimeState == TimeManager.TimeStates.Day)
                {
                    parent.materials[1].DisableKeyword("_EMISSION");
                }
            }
        }

        foreach (var billboard in billboardObjects)
        {
            billboards.Add(billboard);
            var billboardMeshRend = billboard.GetComponent<MeshRenderer>();
            if (timeManager.currentTimeState == TimeManager.TimeStates.Day)
            {
                billboardMeshRend.material.DisableKeyword("_EMISSION");
            }
        }

        StartCoroutine(CheckTimeStateRoutine());
        
    }
    
    private IEnumerator CheckTimeStateRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);

            if (timeManager.currentTimeState == TimeManager.TimeStates.Day)
            {
                SwitchLights(false);
            }
            else if (timeManager.currentTimeState == TimeManager.TimeStates.Night)
            {
                SwitchLights(true);
            }
        }
    }

    private void SwitchLights(bool isOn)
    {
        foreach (Light light in lights)
        {
            light.enabled = isOn;
            var parent = light.GetComponentInParent<MeshRenderer>();

            if (isOn)
            {
                parent.materials[1].EnableKeyword("_EMISSION");
            }
            else
            {
                parent.materials[1].DisableKeyword("_EMISSION");
            }
        }

        foreach (var billboard in billboards)
        {
            var billboardMeshRend = billboard.GetComponent<MeshRenderer>();
            if (isOn)
            {
                billboardMeshRend.material.EnableKeyword("_EMISSION");
            }
            else
            {
                billboardMeshRend.material.DisableKeyword("_EMISSION");
            }
        }

        if (isOn && flickerRoutine == null)
        {
            flickerRoutine = StartCoroutine(FlickerLights());
        }
        else if (!isOn && flickerRoutine != null)
        {
            StopCoroutine(flickerRoutine);
            flickerRoutine = null;
        }
    }

    private Coroutine flickerRoutine;

    private IEnumerator FlickerLights()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(2f, 5f));

            if (lights.Count > 0 && Random.value < 0.15f)
            {
                Light randomLight = lights[Random.Range(0, lights.Count)];
                if (randomLight.enabled)
                {
                    var parent = randomLight.GetComponentInParent<MeshRenderer>();

                    int flickerCount = Random.Range(2, 8);
                    for (int i = 0; i < flickerCount; i++)
                    {
                        randomLight.enabled = false;
                        parent.materials[1].DisableKeyword("_EMISSION");
                        yield return new WaitForSeconds(Random.Range(0.02f, 0.2f));

                        randomLight.enabled = true;
                        parent.materials[1].EnableKeyword("_EMISSION");

                        yield return new WaitForSeconds(Random.Range(0.05f, 0.15f));
                    }
                }
            }
        }
    }
}