using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Window : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputReader input;

    [Header("References")]
    [SerializeField] private RectTransform windowTransform;
    [SerializeField] private GameObject tabObject;
    [SerializeField] private Image cursorImage;
    [SerializeField] private RectTransform cursorTransform;
    [SerializeField] private Canvas canvas;
    [SerializeField] private Sprite defaultCursors;
    [SerializeField] private Sprite resizeCursors;

    [Header("Settings")]
    [SerializeField] private float resizeMargin = 10f;
    [SerializeField] private Vector2 minWindowSize = new Vector2(100, 100);

    private enum cursorSprite {Default, Resize };
    private enum ResizeDirection { None, Left, Right, Top, Bottom, TopLeft, TopRight, BottomLeft, BottomRight }
    private ResizeDirection currentDirection;
    private bool isDragging;
    private bool isResizing;
    private Vector3 dragOffset;
    private Vector2 initialSize;
    private Vector2 initialMousePosition;
    private cursorSprite currentCursorSprite = cursorSprite.Default;

    void Start()
    {
        if (!windowTransform) windowTransform = GetComponent<RectTransform>();
        input.PcLeftClickStart += OnPcLeftClickStart;
        input.PcLeftClickEnd += OnPcLeftClickEnd;
    }

    void Update()
    {
        if (isDragging)
        {
            windowTransform.position = cursorTransform.position + dragOffset;
        }
        else
        {
            Vector2 localMousePosition;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                windowTransform,
                Input.mousePosition,
                canvas.worldCamera,
                out localMousePosition
            );

            ResizeDirection direction = GetResizeDirection(localMousePosition, windowTransform.rect);

            if (direction != ResizeDirection.None && currentCursorSprite != cursorSprite.Resize)
            {
                SetCursorSprite(cursorSprite.Resize);
            }
            else if (direction == ResizeDirection.None && currentCursorSprite != cursorSprite.Default)
            {
                SetCursorSprite(cursorSprite.Default);
            }
        }

        if (isResizing)
        {
            HandleResize();
        }
    }


    private void SetCursorSprite(cursorSprite state)
    {
        currentCursorSprite = state;
        switch(state)
        {
            case cursorSprite.Default:
                cursorImage.sprite = defaultCursors;
                    break;
            case cursorSprite.Resize:
                cursorImage.sprite = resizeCursors;
                break;
        }
    }

    private void OnPcLeftClickStart()
    {
        var results = new List<RaycastResult>();
        var eventData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };

        EventSystem.current.RaycastAll(eventData, results);

        if (results.Count == 0) return; // Pokud nic nebylo kliknuto, ukonči

        RaycastResult topmostObject = results[0];

        // Pokud klikám na jiný objekt než tohle okno nebo jeho child, tak return
        if (!(topmostObject.gameObject == gameObject || topmostObject.gameObject.transform.IsChildOf(transform)))
        {
            return;
        }

        // Zjistíme, zda už nějaké jiné okno právě resizuje
        if (IsAnyOtherWindowResizing())
        {
            return; // Pokud ano, nebudeme tohle posouvat nahoru
        }

        // Pokud kliknu na topbar (hlavičku okna)
        if (topmostObject.gameObject == tabObject)
        {
            transform.SetAsLastSibling();
            isDragging = true;
            dragOffset = windowTransform.position - cursorTransform.position;
        }
        else
        {
            // Pokud kliknu na okraj, zkusíme resize
            if (!CheckForResizeStart())
            {
                // Pokud to nebyl resize a žádné jiné okno neresizuje, posuneme nahoru
                transform.SetAsLastSibling();
            }
        }
    }

    private void OnPcLeftClickEnd()
    {
        isDragging = false;
        isResizing = false;
    }

    private bool CheckForResizeStart()
    {
        Vector2 localMousePosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            windowTransform,
            Input.mousePosition,
            canvas.worldCamera,
            out localMousePosition
        );

        Rect rect = windowTransform.rect;
        currentDirection = GetResizeDirection(localMousePosition, rect);

        if (currentDirection != ResizeDirection.None)
        {
            SetPivot();
            isResizing = true;
            initialSize = windowTransform.sizeDelta;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                windowTransform,
                Input.mousePosition,
                canvas.worldCamera,
                out initialMousePosition
            );
            return true;
        }

        return false;
    }

    // Kontrola, zda nějaké jiné okno právě neprovádí resize
    private bool IsAnyOtherWindowResizing()
    {
        Window[] allWindows = FindObjectsByType<Window>(FindObjectsSortMode.None);
        foreach (Window window in allWindows)
        {
            if (window != this) // Pokud jiné okno resizuje
            {
                Vector2 localMousePosition;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    window.GetComponent<RectTransform>(),
                    Input.mousePosition,
                    canvas.worldCamera,
                    out localMousePosition
                );

                Rect rect = windowTransform.rect;
                currentDirection = GetResizeDirection(localMousePosition, rect);

                if(currentDirection != ResizeDirection.None)
                    return true;
            }
        }
        return false;
    }
    private void HandleResize()
    {
            Vector2 currentMousePosition;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                windowTransform,
                Input.mousePosition,
                canvas.worldCamera,
                out currentMousePosition
            );

            Vector2 delta = currentMousePosition - initialMousePosition;
            Vector2 newSize = initialSize;

            switch (currentDirection)
            {
                case ResizeDirection.Left:
                    newSize.x -= delta.x;
                    break;
                case ResizeDirection.Right:
                    newSize.x += delta.x;
                    break;
                case ResizeDirection.Top:
                    newSize.y += delta.y;
                    break;
                case ResizeDirection.Bottom:
                    newSize.y -= delta.y;
                    break;
                case ResizeDirection.TopLeft:
                    newSize.x -= delta.x;
                    newSize.y += delta.y;
                    break;
                case ResizeDirection.TopRight:
                    newSize.x += delta.x;
                    newSize.y += delta.y;
                    break;
                case ResizeDirection.BottomLeft:
                    newSize.x -= delta.x;
                    newSize.y -= delta.y;
                    break;
                case ResizeDirection.BottomRight:
                    newSize.x += delta.x;
                    newSize.y -= delta.y;
                    break;
            }

            newSize.x = Mathf.Max(newSize.x, minWindowSize.x);
            newSize.y = Mathf.Max(newSize.y, minWindowSize.y);
            windowTransform.sizeDelta = newSize;
    }

    private void SetPivot()
    {
        switch (currentDirection)
        {
            case ResizeDirection.Left:
                SetPivotWithPosition(windowTransform, new Vector2(1, 0.5f));
                break;
            case ResizeDirection.Right:
                SetPivotWithPosition(windowTransform , new Vector2(0, 0.5f));
                break;
            case ResizeDirection.Top:
                SetPivotWithPosition(windowTransform , new Vector2(0.5f, 0));
                break;
            case ResizeDirection.Bottom:
                SetPivotWithPosition(windowTransform , new Vector2(0.5f, 1));
                break;
            case ResizeDirection.TopLeft:
                SetPivotWithPosition(windowTransform , new Vector2(1, 0));
                break;
            case ResizeDirection.TopRight:
                SetPivotWithPosition(windowTransform , new Vector2(0, 0));
                break;
            case ResizeDirection.BottomLeft:
                SetPivotWithPosition(windowTransform , new Vector2(1, 1));
                break;
            case ResizeDirection.BottomRight:
                SetPivotWithPosition(windowTransform , new Vector2(0, 1));
                break;
        }
    }

    private ResizeDirection GetResizeDirection(Vector2 localPos, Rect rect)
    {
        if (localPos.x >= rect.xMin - resizeMargin && localPos.x <= rect.xMin + resizeMargin) // Levý okraj
        {
            if (localPos.y >= rect.yMax - resizeMargin && localPos.y <= rect.yMax + resizeMargin) return ResizeDirection.TopLeft;
            if (localPos.y >= rect.yMin - resizeMargin && localPos.y <= rect.yMin + resizeMargin) return ResizeDirection.BottomLeft;
            return ResizeDirection.Left;
        }
        if (localPos.x >= rect.xMax - resizeMargin && localPos.x <= rect.xMax + resizeMargin) // Pravý okraj
        {
            if (localPos.y >= rect.yMax - resizeMargin && localPos.y <= rect.yMax + resizeMargin) return ResizeDirection.TopRight;
            if (localPos.y >= rect.yMin - resizeMargin && localPos.y <= rect.yMin + resizeMargin) return ResizeDirection.BottomRight;
            return ResizeDirection.Right;
        }
        if (localPos.y >= rect.yMax - resizeMargin && localPos.y <= rect.yMax + resizeMargin) return ResizeDirection.Top; // Horní okraj
        if (localPos.y >= rect.yMin - resizeMargin && localPos.y <= rect.yMin + resizeMargin) return ResizeDirection.Bottom; // Spodní okraj

        return ResizeDirection.None;
    }


    private void SetPivotWithPosition(RectTransform rectTransform, Vector2 pivot)
    {
        if (rectTransform == null) return;

        Vector2 size = rectTransform.rect.size;
        Vector2 deltaPivot = rectTransform.pivot - pivot;
        Vector3 deltaPosition = new Vector3(deltaPivot.x * size.x, deltaPivot.y * size.y);
        rectTransform.pivot = pivot;
        rectTransform.localPosition -= deltaPosition;
    }

    void OnDestroy()
    {
        input.PcLeftClickStart -= OnPcLeftClickStart;
        input.PcLeftClickEnd -= OnPcLeftClickEnd;
    }
}