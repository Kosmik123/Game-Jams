using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Genome editor on an endless grid. Self-contained: put it on an empty GameObject in an empty scene.
/// Left click on a free cell next to the creature adds a body block, right click on a block opens a menu with Delete.
/// Middle mouse drag pans, the wheel zooms.
/// </summary>
public class GenomeEditor : MonoBehaviour
{
    private const float MinZoom = 3f;
    private const float MaxZoom = 15f;

    private static readonly Color GridColorA = new Color(0.12f, 0.13f, 0.16f);
    private static readonly Color GridColorB = new Color(0.14f, 0.15f, 0.18f);
    private static readonly Color BodyColor = new Color(0.45f, 0.62f, 0.5f);

    private Genome genome;
    private Camera cam;
    private Vector3 lastMousePosition;

    private readonly List<SpriteRenderer> gridCells = new List<SpriteRenderer>();
    private readonly List<GameObject> blockObjects = new List<GameObject>();
    private SpriteRenderer ghost;

    private RectTransform canvasRect;
    private Button menuButton;
    private TextMeshProUGUI messageText;
    private float messageTimer;
    private Vector2Int menuCell;
    private bool menuOpen;

    private void Awake()
    {
        genome = Genome.CreateStarting();
        SetupCamera();
        SetupGhost();
        SetupUI();
        RefreshBlocks();
    }

    private void Update()
    {
        HandleCamera();
        UpdateGrid();

        Vector2Int cell = MouseCell();
        bool overUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();

        UpdateGhost(cell, overUI);

        if (!overUI)
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (menuOpen)
                {
                    CloseMenu();
                }
                else
                {
                    TryAddBlock(cell);
                }
            }
            else if (Input.GetMouseButtonDown(1))
            {
                CloseMenu();
                if (genome.Contains(cell))
                {
                    OpenMenu(cell);
                }
            }
        }

        UpdateMessage();
    }

    // --- Editing ---

    private void TryAddBlock(Vector2Int cell)
    {
        if (genome.Contains(cell))
        {
            return;
        }
        if (!HasNeighbour(cell))
        {
            ShowMessage("Build next to an existing block");
            return;
        }
        genome.Add(new BlockGene(cell));
        RefreshBlocks();
    }

    private void DeleteBlock(Vector2Int cell)
    {
        CloseMenu();
        Genome candidate = genome.Clone();
        if (!candidate.TryGetBlock(cell, out BlockGene block))
        {
            return;
        }
        if (block.module == ModuleType.Brain)
        {
            ShowMessage("The brain cannot be deleted");
            return;
        }
        candidate.Remove(cell);
        if (!GenomeValidator.TryValidate(candidate, out string error))
        {
            ShowMessage(error);
            return;
        }
        genome = candidate;
        RefreshBlocks();
    }

    private bool HasNeighbour(Vector2Int cell)
    {
        for (int d = 0; d < 4; d++)
        {
            if (genome.Contains(cell + ((Direction)d).ToVector()))
            {
                return true;
            }
        }
        return false;
    }

    // --- Camera and grid ---

    private void SetupCamera()
    {
        cam = Camera.main;
        if (cam == null)
        {
            var go = new GameObject("Main Camera", typeof(Camera));
            go.tag = "MainCamera";
            cam = go.GetComponent<Camera>();
        }
        cam.orthographic = true;
        cam.orthographicSize = 7f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.07f, 0.08f, 0.1f);
        cam.transform.position = new Vector3(0f, 0f, -10f);
    }

    private void HandleCamera()
    {
        float scroll = Input.mouseScrollDelta.y;
        if (scroll != 0f)
        {
            cam.orthographicSize = Mathf.Clamp(cam.orthographicSize - scroll, MinZoom, MaxZoom);
        }

        if (Input.GetMouseButtonDown(2))
        {
            lastMousePosition = Input.mousePosition;
        }
        else if (Input.GetMouseButton(2))
        {
            Vector3 delta = cam.ScreenToWorldPoint(lastMousePosition) - cam.ScreenToWorldPoint(Input.mousePosition);
            cam.transform.position += new Vector3(delta.x, delta.y, 0f);
            lastMousePosition = Input.mousePosition;
        }
    }

    /// <summary>Keeps just enough grid cells to cover the camera view, so the grid looks endless.</summary>
    private void UpdateGrid()
    {
        float halfHeight = cam.orthographicSize;
        float halfWidth = halfHeight * cam.aspect;
        Vector3 center = cam.transform.position;
        int minX = Mathf.FloorToInt(center.x - halfWidth) - 1;
        int maxX = Mathf.CeilToInt(center.x + halfWidth) + 1;
        int minY = Mathf.FloorToInt(center.y - halfHeight) - 1;
        int maxY = Mathf.CeilToInt(center.y + halfHeight) + 1;

        int index = 0;
        for (int y = minY; y <= maxY; y++)
        {
            for (int x = minX; x <= maxX; x++)
            {
                if (index == gridCells.Count)
                {
                    gridCells.Add(CreateSprite("Grid Cell", PixelSprites.Square, 0, 0.96f));
                }
                SpriteRenderer cell = gridCells[index++];
                cell.gameObject.SetActive(true);
                cell.transform.position = new Vector3(x, y, 0f);
                cell.color = (x + y) % 2 == 0 ? GridColorA : GridColorB;
            }
        }
        for (; index < gridCells.Count; index++)
        {
            gridCells[index].gameObject.SetActive(false);
        }
    }

    private Vector2Int MouseCell()
    {
        Vector3 world = cam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, -cam.transform.position.z));
        return new Vector2Int(Mathf.RoundToInt(world.x), Mathf.RoundToInt(world.y));
    }

    // --- Blocks ---

    private void RefreshBlocks()
    {
        for (int i = 0; i < blockObjects.Count; i++)
        {
            Destroy(blockObjects[i]);
        }
        blockObjects.Clear();

        for (int i = 0; i < genome.Count; i++)
        {
            BlockGene block = genome.Blocks[i];
            SpriteRenderer renderer = CreateSprite("Block " + block.position, PixelSprites.Square, 1, 0.92f);
            renderer.transform.position = new Vector3(block.position.x, block.position.y, 0f);
            renderer.color = ColorOf(block.module);
            blockObjects.Add(renderer.gameObject);

            if (UsesDirection(block.module))
            {
                SpriteRenderer arrow = CreateSprite("Direction", PixelSprites.Arrow, 2, 0.6f);
                arrow.transform.SetParent(renderer.transform, false);
                arrow.transform.localScale = Vector3.one * (0.6f / 0.92f);
                arrow.transform.localRotation = Quaternion.Euler(0f, 0f, block.direction.ToAngle());
                arrow.color = new Color(0.1f, 0.1f, 0.1f, 0.85f);
            }
        }
    }

    private static Color ColorOf(ModuleType module)
    {
        switch (module)
        {
            case ModuleType.Brain: return new Color(0.9f, 0.55f, 0.7f);
            case ModuleType.Flagellum: return new Color(0.95f, 0.8f, 0.3f);
            case ModuleType.Spikes: return new Color(0.85f, 0.3f, 0.3f);
            case ModuleType.Armor: return new Color(0.4f, 0.55f, 0.9f);
            default: return BodyColor;
        }
    }

    private static bool UsesDirection(ModuleType module)
    {
        return module == ModuleType.Flagellum || module == ModuleType.Spikes || module == ModuleType.Armor;
    }

    private void SetupGhost()
    {
        ghost = CreateSprite("Ghost", PixelSprites.Square, 3, 0.92f);
        ghost.gameObject.SetActive(false);
    }

    /// <summary>Shows where a click would build a block, and highlights an existing block under the cursor.</summary>
    private void UpdateGhost(Vector2Int cell, bool overUI)
    {
        if (overUI || menuOpen)
        {
            ghost.gameObject.SetActive(false);
            return;
        }

        bool occupied = genome.Contains(cell);
        if (!occupied && !HasNeighbour(cell))
        {
            ghost.gameObject.SetActive(false);
            return;
        }

        ghost.gameObject.SetActive(true);
        ghost.transform.position = new Vector3(cell.x, cell.y, 0f);
        ghost.color = occupied ? new Color(1f, 1f, 1f, 0.15f) : new Color(BodyColor.r, BodyColor.g, BodyColor.b, 0.45f);
    }

    private SpriteRenderer CreateSprite(string objectName, Sprite sprite, int sortingOrder, float scale)
    {
        var go = new GameObject(objectName);
        go.transform.SetParent(transform, false);
        go.transform.localScale = Vector3.one * scale;
        var renderer = go.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.sortingOrder = sortingOrder;
        return renderer;
    }

    // --- UI ---

    private void SetupUI()
    {
        if (FindObjectOfType<EventSystem>() == null)
        {
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }

        var canvasObject = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasObject.transform.SetParent(transform, false);
        var canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
        canvasRect = canvasObject.GetComponent<RectTransform>();

        TextMeshProUGUI hint = CreateText(canvasObject.transform, "Hint",
            "LMB: add block    RMB: menu    MMB drag: pan    Wheel: zoom", 28f, TextAlignmentOptions.BottomLeft);
        Stretch(hint.rectTransform, new Vector2(24f, 16f), new Vector2(-24f, -16f));

        messageText = CreateText(canvasObject.transform, "Message", "", 36f, TextAlignmentOptions.Top);
        Stretch(messageText.rectTransform, new Vector2(24f, 24f), new Vector2(-24f, -24f));
        messageText.color = new Color(1f, 0.85f, 0.4f);

        var buttonObject = new GameObject("Delete Menu", typeof(RectTransform), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(canvasObject.transform, false);
        var buttonRect = (RectTransform)buttonObject.transform;
        buttonRect.anchorMin = buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
        buttonRect.pivot = new Vector2(0f, 1f);
        buttonRect.sizeDelta = new Vector2(220f, 64f);
        buttonObject.GetComponent<Image>().color = new Color(0.15f, 0.16f, 0.2f, 0.95f);
        menuButton = buttonObject.GetComponent<Button>();
        menuButton.onClick.AddListener(() => DeleteBlock(menuCell));
        TextMeshProUGUI label = CreateText(buttonObject.transform, "Label", "Delete", 32f, TextAlignmentOptions.Center);
        Stretch(label.rectTransform, Vector2.zero, Vector2.zero);
        buttonObject.SetActive(false);
    }

    private void OpenMenu(Vector2Int cell)
    {
        menuCell = cell;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, Input.mousePosition, null, out Vector2 local);
        ((RectTransform)menuButton.transform).anchoredPosition = local;
        menuButton.gameObject.SetActive(true);
        menuOpen = true;
    }

    private void CloseMenu()
    {
        menuButton.gameObject.SetActive(false);
        menuOpen = false;
    }

    private void ShowMessage(string text)
    {
        messageText.text = text;
        messageTimer = 2f;
    }

    private void UpdateMessage()
    {
        if (messageTimer > 0f)
        {
            messageTimer -= Time.deltaTime;
            if (messageTimer <= 0f)
            {
                messageText.text = "";
            }
        }
    }

    private static TextMeshProUGUI CreateText(Transform parent, string objectName, string text, float size, TextAlignmentOptions alignment)
    {
        var go = new GameObject(objectName, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = size;
        tmp.alignment = alignment;
        tmp.raycastTarget = false;
        return tmp;
    }

    private static void Stretch(RectTransform rect, Vector2 offsetMin, Vector2 offsetMax)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = offsetMin;
        rect.offsetMax = offsetMax;
    }
}
