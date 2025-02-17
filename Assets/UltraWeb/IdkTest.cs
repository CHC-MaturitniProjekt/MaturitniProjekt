using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class IdkTest : MonoBehaviour
{
    [Header("Settings")]
    public int width = 1024;
    public int height = 768;
    public string url = "https://example.com";

    [Header("References")]
    [SerializeField] private RawImage _rawImage;

    private Coroutine _updateCoroutine;

    private void Start()
    {
        try
        {
            // Inicializace UltraWeb
            UltraWeb.Initialize(width, height);

            // Načtení URL
            UltraWeb.Instance.LoadUrl(url);

            // Spuštění coroutine pro aktualizaci textury
            _updateCoroutine = StartCoroutine(UpdateTextureRoutine());

            // Nastavení počáteční velikosti RawImage
            _rawImage.rectTransform.sizeDelta = new Vector2(width, height);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Initialization failed: {e.Message}");
            enabled = false;
        }
    }

    private IEnumerator UpdateTextureRoutine()
    {
        while (!UltraWeb.Instance.IsDisposed)
        {
            yield return new WaitForEndOfFrame();

            // Získání textury z UltraWeb
            var texture = UltraWeb.Instance.getTexture();

            if (texture != null && _rawImage != null)
            {
                // Aktualizace RawImage
                _rawImage.texture = texture;

                // Optimalizace: Přeskočit 1 snímek pro snížení vytížení CPU
                yield return null;
            }
            else
            {
                Debug.LogWarning("Texture or RawImage is null");
            }
        }
    }

    private void OnDestroy()
    {
        // Zastavení coroutine
        if (_updateCoroutine != null)
            StopCoroutine(_updateCoroutine);

        // Uvolnění prostředků
        if (UltraWeb.Instance != null && !UltraWeb.Instance.IsDisposed)
        {
            UltraWeb.Instance.Dispose();
        }

        UltraWeb.ResetStaticState();
    }

    private void OnApplicationQuit()
    {
        // Nastavíme globální flag pro všechny instance

        if (UltraWeb.Instance != null && !UltraWeb.Instance.IsDisposed)
        {
            UltraWeb.Instance.Dispose();
        }

        UltraWeb.ResetStaticState();
    }

}