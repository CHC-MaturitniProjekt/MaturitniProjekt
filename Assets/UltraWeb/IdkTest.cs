using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class IdkTest : MonoBehaviour
{
    //private UltraWeb ultraWeb;
    //public RawImage pomoc;
    //private Coroutine updateCoroutine;

    //void Awake()
    //{
    //    ultraWeb = new UltraWeb(1080, 1920);
    //    ultraWeb.LoadUrl("https://www.google.com");
    //    updateCoroutine = StartCoroutine(UpdateTexture());
    //}

    //IEnumerator UpdateTexture()
    //{
    //    while (true)
    //    {
    //        yield return new WaitForEndOfFrame();
    //        Texture2D texture = ultraWeb.getTexture();
    //        if (texture != null)
    //        {
    //            pomoc.texture = texture;
    //        }
    //        else
    //        {
    //            Debug.LogError("Failed to get texture from UltraWeb!");
    //        }
    //    }
    //}

    //void OnDestroy()
    //{
    //    if (updateCoroutine != null)
    //        StopCoroutine(updateCoroutine);

    //    ultraWeb?.Dispose();
    //}
    private UltraWeb ultraWeb;

    void Awake()
    {
        ultraWeb = new UltraWeb(100, 100); // Malé rozlišení pro test
        Debug.Log("MinimalUltralightTest: UltraWeb initialized.");
    }

    void OnDestroy()
    {
        ultraWeb?.Dispose();
        Debug.Log("MinimalUltralightTest: UltraWeb disposed.");
    }
}