using UnityEngine;

public class ToonMaterialConverter : MonoBehaviour
{
    public Shader toonShader;

    void Start()
    {
        foreach (Renderer renderer in FindObjectsOfType<Renderer>())
        {
            Material[] newMaterials = new Material[renderer.materials.Length];

            for (int i = 0; i < renderer.materials.Length; i++)
            {
                Material originalMat = renderer.materials[i];

                if (originalMat == null)
                {
                    Debug.LogError($"Object {renderer.gameObject.name} has a null material.");
                    continue;
                }

                string textureProperty = "MainTex"; 

                if (originalMat.shader.name == "Custom/Outline Mask" || originalMat.shader.name == "Custom/Outline Fill")
                {
                    newMaterials[i] = originalMat;
                    continue;
                }
                
                if (!originalMat.mainTexture)
                {
                    newMaterials[i] = originalMat;
                    continue;
                }

                Texture originalTexture = originalMat.mainTexture;

                Material toonMat = new Material(toonShader);
                toonMat.SetTexture(textureProperty, originalTexture ?? Texture2D.whiteTexture);
                toonMat.SetFloat("Toon Ramp Smoothness", 0.2f);
                toonMat.SetColor("Toon Ramp Tint", new Color32(176, 176, 176, 255));
                toonMat.SetFloat("Toon Ramp Offset", 0.64f);
                toonMat.SetFloat("Toon Ramp Offset Position", 0.5f);
                toonMat.SetFloat("Rim Power", 1.06f);
                toonMat.SetFloat("Brightness Rim", 2.83f);
                toonMat.SetFloat("Ambient", -0.3f);
                
                newMaterials[i] = toonMat;
            }

            renderer.materials = newMaterials;
            renderer.enabled = true;
        }
    }
}