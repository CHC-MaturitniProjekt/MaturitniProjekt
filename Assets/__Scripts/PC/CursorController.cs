//using UnityEngine;
//using UnityEngine.UI;

//public class CursorController : MonoBehaviour
//{
//    public Camera renderTextureCamera;
//    public RectTransform cursorImage;
//    public RectTransform renderTextureUI; // UI element zobrazující RenderTexture

//    private RectTransform canvasRect;

//    void Start()
//    {
//        canvasRect = GetComponentInParent<Canvas>().GetComponent<RectTransform>();
//    }

//    void Update()
//    {
//        if (GetComponent<pc>().isInteracting)
//        {
//            Vector2 uv = GetRenderTextureUV();
//            if (uv != Vector2.zero)
//            {
//                PlaceCursor(uv);
//            }
//        }
//    }

//    Vector2 GetRenderTextureUV()
//    {
//        Vector2 localMousePos;
//        // Převod pozice myši do lokálních souřadnic UI elementu
//        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
//            renderTextureUI,
//            Input.mousePosition,
//            null,
//            out localMousePos
//        )) return Vector2.zero;

//        // Výpočet UV souřadnic (0-1) uvnitř UI elementu
//        Rect rect = renderTextureUI.rect;
//        Vector2 uv = new Vector2(
//            (localMousePos.x + rect.width * 0.5f) / rect.width,
//            (localMousePos.y + rect.height * 0.5f) / rect.height
//        );

//        // Raycast z RenderTexture kamery
//        Ray ray = renderTextureCamera.ViewportPointToRay(uv);
//        return Physics.Raycast(ray, out RaycastHit hit) ? uv : Vector2.zero;
//    }

//    void PlaceCursor(Vector2 uv)
//    {
//        // Převod UV na pozici v Canvasu
//        Vector2 renderTexturePos = renderTextureUI.anchoredPosition;
//        Vector2 renderTextureSize = renderTextureUI.rect.size;

//        cursorImage.anchoredPosition = new Vector2(
//            renderTexturePos.x + (uv.x - 0.5f) * renderTextureSize.x,
//            renderTexturePos.y + (uv.y - 0.5f) * renderTextureSize.y
//        );
//    }
//}