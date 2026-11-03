using System.Collections.Generic;
using MicrojamEvolution.Creature;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

namespace MicrojamEvolution.GenomeEditor
{
    /// <summary>
    /// Genome editor on an endless grid. Self-contained: put it on an empty GameObject in an empty scene.
    /// Left click on a free cell next to the creature adds a body block, right click on a block opens a menu
    /// (rotate, remove module, delete). Modules are dragged from the sidebar onto blocks.
    /// Everything built since the last confirmed form costs DNA. Middle mouse drag pans, the wheel zooms.
    /// </summary>
    public class GenomeEditor : MonoBehaviour
    {
        [Tooltip("Camera to use. When empty, the main camera is used, or a new one is created.")]
        [SerializeField]
        private Camera cam;

        private const float MinZoom = 3f;
        private const float MaxZoom = 15f;

        private static readonly Color GridColorA = new Color(0.12f, 0.13f, 0.16f);
        private static readonly Color GridColorB = new Color(0.14f, 0.15f, 0.18f);
        private static readonly Color BodyColor = new Color(0.45f, 0.62f, 0.5f);

        private static readonly ModuleType[] SidebarModules = { ModuleType.Flagellum, ModuleType.Spikes, ModuleType.Armor };

        private readonly List<SpriteRenderer> gridCells = new List<SpriteRenderer>();
        private readonly List<GameObject> blockObjects = new List<GameObject>();

        private CreatureConfig config;
        private Genome committed;
        private Genome genome;
        private int dna;
        private Vector3 lastMousePosition;
        private SpriteRenderer ghost;

        private RectTransform canvasRect;
        private RectTransform menuPanel;
        private Button rotateButton;
        private Button removeModuleButton;
        private Button deleteButton;
        private TextMeshProUGUI messageText;
        private TextMeshProUGUI statusText;
        private float messageTimer;
        private Vector2Int menuCell;
        private bool menuOpen;

        private bool dragging;
        private ModuleType dragModule;
        private RectTransform dragVisual;
        private Image dragImage;

        private void Awake()
        {
            config = CreatureConfig.Default;
            committed = PlayerProfile.Form.Clone();
            genome = committed.Clone();
            dna = PlayerProfile.Dna;
            SetupCamera();
            SetupGhost();
            SetupUI();
            RefreshBlocks();
        }

        public void BeginModuleDrag(ModuleType module)
        {
            CloseMenu();
            dragging = true;
            dragModule = module;
            Color color = ColorOf(module);
            color.a = 0.85f;
            dragImage.color = color;
            dragVisual.gameObject.SetActive(true);
        }

        public void EndModuleDrag()
        {
            bool overUI = EventSystem.current && EventSystem.current.IsPointerOverGameObject();
            dragging = false;
            dragVisual.gameObject.SetActive(false);
            ghost.gameObject.SetActive(false);
            if (overUI == false)
                TrySetModule(MouseCell(), dragModule);
        }

        private void Update()
        {
            HandleCamera();
            UpdateGrid();

            Vector2Int cell = MouseCell();
            bool overUI = EventSystem.current && EventSystem.current.IsPointerOverGameObject();

            if (dragging)
            {
                UpdateDrag(cell, overUI);
            }
            else
            {
                UpdateGhost(cell, overUI);
                if (overUI == false)
                    HandleClicks(cell);
            }

            UpdateMessage();
        }

        private void HandleClicks(Vector2Int cell)
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (menuOpen)
                    CloseMenu();
                else
                    TryAddBlock(cell);
            }
            else if (Input.GetMouseButtonDown(1))
            {
                CloseMenu();
                if (genome.Contains(cell))
                    OpenMenu(cell);
            }
        }

        // --- Editing ---

        private void TryAddBlock(Vector2Int cell)
        {
            if (genome.Contains(cell))
                return;

            if (HasNeighbour(cell) == false)
            {
                ShowMessage("Build next to an existing block");
                return;
            }

            Genome candidate = genome.Clone();
            candidate.Add(new BlockGene(cell));
            TryAccept(candidate);
        }

        private void DeleteBlock(Vector2Int cell)
        {
            CloseMenu();
            Genome candidate = genome.Clone();
            if (candidate.TryGetBlock(cell, out BlockGene block) == false)
                return;

            if (block.Module == ModuleType.Brain)
            {
                ShowMessage("The brain cannot be deleted");
                return;
            }

            candidate.Remove(cell);
            if (GenomeValidator.TryValidate(candidate, out string error) == false)
            {
                ShowMessage(error);
                return;
            }

            TryAccept(candidate);
        }

        private void RotateModule(Vector2Int cell)
        {
            CloseMenu();
            Genome candidate = genome.Clone();
            if (candidate.TryGetBlock(cell, out BlockGene block) && UsesDirection(block.Module))
            {
                candidate.Replace(block.WithDirection(block.Direction.RotateClockwise()));
                TryAccept(candidate);
            }
        }

        private void RemoveModule(Vector2Int cell)
        {
            CloseMenu();
            Genome candidate = genome.Clone();
            if (candidate.TryGetBlock(cell, out BlockGene block) && UsesDirection(block.Module))
            {
                candidate.Replace(block.WithModule(ModuleType.None));
                TryAccept(candidate);
            }
        }

        private void TrySetModule(Vector2Int cell, ModuleType module)
        {
            Genome candidate = genome.Clone();
            if (candidate.TryGetBlock(cell, out BlockGene block) == false)
            {
                ShowMessage("Drop the module on a block");
                return;
            }

            if (block.Module == ModuleType.Brain)
            {
                ShowMessage("The brain cannot hold a module");
                return;
            }

            if (block.Module == module)
                return;

            candidate.Replace(block.WithModule(module));
            TryAccept(candidate);
        }

        /// <summary>Makes the candidate the working genome, unless it costs more DNA than the player has.</summary>
        private bool TryAccept(Genome candidate)
        {
            if (CostOf(candidate) > dna)
            {
                ShowMessage("Not enough DNA");
                return false;
            }

            genome = candidate;
            RefreshBlocks();
            return true;
        }

        /// <summary>
        /// DNA needed to turn the committed form into the given genome: a new block pays for its body and module,
        /// a changed module pays for the new module. Everything else, including rotating and removing, is free.
        /// </summary>
        private int CostOf(Genome target)
        {
            int total = 0;
            for (int i = 0; i < target.Count; i++)
            {
                BlockGene block = target.Blocks[i];
                if (committed.TryGetBlock(block.Position, out BlockGene old))
                {
                    if (block.Module != old.Module)
                        total += config.CostOf(block.Module);
                }
                else
                {
                    total += config.BodyCost + config.CostOf(block.Module);
                }
            }

            return total;
        }

        private void ConfirmForm()
        {
            CloseMenu();
            if (GenomeValidator.TryValidate(genome, out string error) == false)
            {
                ShowMessage(error);
                return;
            }

            int cost = CostOf(genome);
            if (cost > dna)
            {
                ShowMessage("Not enough DNA");
                return;
            }

            dna -= cost;
            committed = genome.Clone();
            PlayerProfile.Dna = dna;
            PlayerProfile.Form = genome.Clone();
            RefreshStatus();
            ShowMessage("Form confirmed");
        }

        private void RevertForm()
        {
            CloseMenu();
            genome = committed.Clone();
            RefreshBlocks();
        }

        private bool HasNeighbour(Vector2Int cell)
        {
            for (int d = 0; d < 4; d++)
                if (genome.Contains(cell + ((Direction)d).ToVector()))
                    return true;

            return false;
        }

        // --- Dragging modules from the sidebar ---

        private void UpdateDrag(Vector2Int cell, bool overUI)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, Input.mousePosition, null, out Vector2 local);
            dragVisual.anchoredPosition = local;

            bool validTarget = overUI == false && genome.TryGetBlock(cell, out BlockGene block) && block.Module != ModuleType.Brain;
            ghost.gameObject.SetActive(validTarget);
            if (validTarget)
            {
                Color color = ColorOf(dragModule);
                color.a = 0.6f;
                ghost.transform.position = new Vector3(cell.x, cell.y, 0f);
                ghost.color = color;
            }
        }

        // --- Camera and grid ---

        private void SetupCamera()
        {
            if (cam == null)
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
                cam.orthographicSize = Mathf.Clamp(cam.orthographicSize - scroll, MinZoom, MaxZoom);

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
                        gridCells.Add(CreateSprite("Grid Cell", PixelSprites.Square, 0, 0.96f));

                    SpriteRenderer cell = gridCells[index++];
                    cell.gameObject.SetActive(true);
                    cell.transform.position = new Vector3(x, y, 0f);
                    cell.color = (x + y) % 2 == 0 ? GridColorA : GridColorB;
                }
            }

            for (; index < gridCells.Count; index++)
                gridCells[index].gameObject.SetActive(false);
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
                Destroy(blockObjects[i]);

            blockObjects.Clear();

            for (int i = 0; i < genome.Count; i++)
            {
                BlockGene block = genome.Blocks[i];
                SpriteRenderer blockRenderer = CreateSprite("Block " + block.Position, PixelSprites.Square, 1, 0.92f);
                blockRenderer.transform.position = new Vector3(block.Position.x, block.Position.y, 0f);
                blockRenderer.color = ColorOf(block.Module);
                blockObjects.Add(blockRenderer.gameObject);

                if (UsesDirection(block.Module))
                {
                    SpriteRenderer arrow = CreateSprite("Direction", PixelSprites.Arrow, 2, 0.6f);
                    arrow.transform.SetParent(blockRenderer.transform, false);
                    arrow.transform.localScale = Vector3.one * (0.6f / 0.92f);
                    arrow.transform.localRotation = Quaternion.Euler(0f, 0f, block.Direction.ToAngle());
                    arrow.color = new Color(0.1f, 0.1f, 0.1f, 0.85f);
                }
            }

            RefreshStatus();
        }

        private void RefreshStatus()
        {
            if (statusText == null)
                return;

            int cost = CostOf(genome);
            statusText.text = "DNA: " + dna + "\nCost: " + cost + "\nLeft: " + (dna - cost);
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
            if (occupied == false && HasNeighbour(cell) == false)
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
            var spriteRenderer = go.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = sprite;
            spriteRenderer.sortingOrder = sortingOrder;
            return spriteRenderer;
        }

        // --- UI ---

        private void SetupUI()
        {
            if (EventSystem.current == null)
                new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));

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
                "LMB: add block    RMB: menu    Drag module onto a block    MMB drag: pan    Wheel: zoom\nNew block: "
                + config.BodyCost + " DNA", 28f, TextAlignmentOptions.BottomLeft);
            Stretch(hint.rectTransform, new Vector2(24f, 16f), new Vector2(-24f, -16f));

            messageText = CreateText(canvasObject.transform, "Message", "", 36f, TextAlignmentOptions.Top);
            Stretch(messageText.rectTransform, new Vector2(24f, 24f), new Vector2(-24f, -24f));
            messageText.color = new Color(1f, 0.85f, 0.4f);

            SetupSidebar(canvasObject.transform);
            SetupMenu(canvasObject.transform);

            var dragObject = new GameObject("Drag Visual", typeof(RectTransform), typeof(Image));
            dragObject.transform.SetParent(canvasObject.transform, false);
            dragVisual = (RectTransform)dragObject.transform;
            dragVisual.sizeDelta = new Vector2(80f, 80f);
            dragImage = dragObject.GetComponent<Image>();
            dragImage.raycastTarget = false;
            dragObject.SetActive(false);

            RefreshStatus();
        }

        private void SetupSidebar(Transform parent)
        {
            var sidebar = new GameObject("Sidebar", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup));
            sidebar.transform.SetParent(parent, false);
            var rect = (RectTransform)sidebar.transform;
            rect.anchorMin = new Vector2(1f, 0f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(1f, 0.5f);
            rect.sizeDelta = new Vector2(360f, 0f);
            rect.anchoredPosition = Vector2.zero;
            sidebar.GetComponent<Image>().color = new Color(0.1f, 0.11f, 0.14f, 0.95f);

            var layout = sidebar.GetComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(20, 20, 20, 20);
            layout.spacing = 14f;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            CreateText(sidebar.transform, "Header", "MODULES", 40f, TextAlignmentOptions.Left);

            for (int i = 0; i < SidebarModules.Length; i++)
            {
                ModuleType module = SidebarModules[i];
                var item = new GameObject(module + " Item", typeof(RectTransform), typeof(Image), typeof(LayoutElement), typeof(ModuleDragSource));
                item.transform.SetParent(sidebar.transform, false);
                item.GetComponent<Image>().color = ColorOf(module);
                item.GetComponent<LayoutElement>().preferredHeight = 96f;
                item.GetComponent<ModuleDragSource>().Init(this, module);
                TextMeshProUGUI label = CreateText(item.transform, "Label", module + "\n" + config.CostOf(module) + " DNA", 32f, TextAlignmentOptions.Center);
                label.color = new Color(0.1f, 0.1f, 0.1f);
                Stretch(label.rectTransform, Vector2.zero, Vector2.zero);
            }

            var spacer = new GameObject("Spacer", typeof(RectTransform), typeof(LayoutElement));
            spacer.transform.SetParent(sidebar.transform, false);
            spacer.GetComponent<LayoutElement>().flexibleHeight = 1f;

            statusText = CreateText(sidebar.transform, "Status", "", 36f, TextAlignmentOptions.Left);
            CreateButton(sidebar.transform, "Confirm", "Confirm", ConfirmForm, 72f);
            CreateButton(sidebar.transform, "Revert", "Revert", RevertForm, 72f);
        }

        private void SetupMenu(Transform parent)
        {
            var menu = new GameObject("Block Menu", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            menu.transform.SetParent(parent, false);
            menuPanel = (RectTransform)menu.transform;
            menuPanel.anchorMin = menuPanel.anchorMax = new Vector2(0.5f, 0.5f);
            menuPanel.pivot = new Vector2(0f, 1f);
            menuPanel.sizeDelta = new Vector2(260f, 0f);
            menu.GetComponent<Image>().color = new Color(0.15f, 0.16f, 0.2f, 0.95f);

            var layout = menu.GetComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(6, 6, 6, 6);
            layout.spacing = 4f;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            var fitter = menu.GetComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            rotateButton = CreateButton(menu.transform, "Rotate", "Rotate", () => RotateModule(menuCell), 56f);
            removeModuleButton = CreateButton(menu.transform, "Remove Module", "Remove module", () => RemoveModule(menuCell), 56f);
            deleteButton = CreateButton(menu.transform, "Delete", "Delete block", () => DeleteBlock(menuCell), 56f);
            menu.SetActive(false);
        }

        private static Button CreateButton(Transform parent, string objectName, string text, UnityAction onClick, float height)
        {
            var go = new GameObject(objectName, typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
            go.transform.SetParent(parent, false);
            go.GetComponent<Image>().color = new Color(0.25f, 0.27f, 0.34f);
            go.GetComponent<LayoutElement>().preferredHeight = height;
            var button = go.GetComponent<Button>();
            button.onClick.AddListener(onClick);
            TextMeshProUGUI label = CreateText(go.transform, "Label", text, 30f, TextAlignmentOptions.Center);
            Stretch(label.rectTransform, Vector2.zero, Vector2.zero);
            return button;
        }

        private void OpenMenu(Vector2Int cell)
        {
            if (genome.TryGetBlock(cell, out BlockGene block) == false)
                return;

            if (block.Module == ModuleType.Brain)
            {
                ShowMessage("The brain cannot be changed");
                return;
            }

            menuCell = cell;
            bool hasModule = UsesDirection(block.Module);
            rotateButton.gameObject.SetActive(hasModule);
            removeModuleButton.gameObject.SetActive(hasModule);
            deleteButton.gameObject.SetActive(true);

            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, Input.mousePosition, null, out Vector2 local);
            menuPanel.anchoredPosition = local;
            menuPanel.gameObject.SetActive(true);
            LayoutRebuilder.ForceRebuildLayoutImmediate(menuPanel);
            menuOpen = true;
        }

        private void CloseMenu()
        {
            menuPanel.gameObject.SetActive(false);
            menuOpen = false;
        }

        private void ShowMessage(string text)
        {
            messageText.text = text;
            messageTimer = 2f;
        }

        private void UpdateMessage()
        {
            if (messageTimer <= 0f)
                return;

            messageTimer -= Time.deltaTime;
            if (messageTimer <= 0f)
                messageText.text = "";
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
}
