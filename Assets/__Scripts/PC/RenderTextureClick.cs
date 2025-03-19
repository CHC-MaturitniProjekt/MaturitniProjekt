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

    [Header("Objects")]
    [SerializeField] private Camera uiCamera;
    [SerializeField] private GameObject Screen;
    [SerializeField] private RenderTexture renderTexture;
    [SerializeField] private RectTransform uiCanvas;
    [SerializeField] private RectTransform cursorImage;


    private GameObject _currentTarget;
    void Start()
    {
        var eventSystem = EventSystem.current;
        if (eventSystem != null)
        {
            var inputModule = eventSystem.GetComponent<StandaloneInputModule>();
            if (inputModule != null)
            {
                inputModule.enabled = false;
            }
        }

        input.PcLeftClickStart += OnPcleftClickStart;
        input.PcLeftClickEnd += OnPcleftClickEnd;
    }

    private void OnPcleftClickEnd()
    {
        Vector2 uv = GetScreenPos();
        Vector2 mousePoint = GetCanvasPos(uv);

        HandleCanvasClick(mousePoint, PointerEventData.InputButton.Left, ExecuteEvents.pointerUpHandler);
    }

    private void OnPcleftClickStart()
    {
        Vector2 uv = GetScreenPos();
        Vector2 mousePoint = GetCanvasPos(uv);

        HandleCanvasClick(mousePoint, PointerEventData.InputButton.Left , ExecuteEvents.pointerDownHandler);
        HandleCanvasClick(mousePoint, PointerEventData.InputButton.Left, ExecuteEvents.pointerClickHandler);
    }

    void Update()
    {
        Vector2 uv = GetScreenPos();
        Vector2 mousePoint = GetCanvasPos(uv);

        cursorImage.anchoredPosition = mousePoint;
    }

    private void HandleCanvasClick<T>(Vector2 mousePoint, PointerEventData.InputButton inputButton , ExecuteEvents.EventFunction<T> eventFunction) where T : IEventSystemHandler
    {
        Vector2 screenPosition = RectTransformUtility.WorldToScreenPoint(uiCamera, cursorImage.position);

        PointerEventData eventData = new PointerEventData(EventSystem.current)
        {
            position = screenPosition,
            button = inputButton
        };

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        if (results.Count > 0)
        {
            GameObject clickedObject = results[0].gameObject;
            ExecuteEvents.Execute(clickedObject, eventData, eventFunction);
        }
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
