using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngineInternal;

public class RenderTextureClick : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputReader input;

    [Header("Params")]
    [SerializeField] private float maxDistance;
    [SerializeField] private int PcScreenLayer;

    [Header("Objects")]
    [SerializeField] private Camera uiCamera;
    [SerializeField] private GameObject Screen;
    [SerializeField] private RenderTexture renderTexture;
    [SerializeField] private RectTransform uiCanvas;
    [SerializeField] private RectTransform cursorImage;
    private GraphicRaycaster graphicRaycaster;



    private bool leftClick = false;

    void Start()
    {
        input.PcLeftClickStart += OnPcleftClickStart;
        input.PcLeftClickEnd += OnPcleftClickEnd;
        graphicRaycaster = uiCanvas.GetComponent<GraphicRaycaster>();
    }

    private void OnPcleftClickEnd()
    {
        leftClick = false;
    }

    private void OnPcleftClickStart()
    {
        leftClick = true;
    }

    void Update()
    {
        Vector2 uv = GetScreenPos();
        Vector2 mousePoint = GetCanvasPos(uv);

        cursorImage.anchoredPosition = mousePoint;

        if (leftClick)
        {
            HandleCanvasClick(mousePoint);
        }
    }

    private void HandleCanvasClick(Vector2 screenPosition)
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current)
        {
            pressPosition = screenPosition
        };
        ExecuteEvents.Execute(uiCanvas.gameObject, eventData, ExecuteEvents.pointerClickHandler);
    }

    private Vector2 GetScreenPos()
    {
        Camera mainCamera = Camera.main;
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        RaycastHit[] hits = Physics.RaycastAll(ray, maxDistance);
        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.gameObject == Screen)
            {
                Vector2 uv = hit.textureCoord;
                return uv;
            }
        }

        return Vector2.zero;
    }

    private Vector2 GetCanvasPos(Vector2 screenUvPos)
    {
        Vector2 canvasSize = uiCanvas.rect.size;
        Vector2 canvasPivot = uiCanvas.pivot;

        float x = screenUvPos.x * canvasSize.x - canvasSize.x * canvasPivot.x;
        float y = screenUvPos.y * canvasSize.y - canvasSize.y * canvasPivot.y;

        return new Vector2(x, y);
    }

}
