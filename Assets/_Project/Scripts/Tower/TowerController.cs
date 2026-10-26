using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ZeroHour.Tower
{
    // The base platform with the stacked floors. Follows the pointer/finger on X only.
    public class TowerController : MonoBehaviour
    {
        private const float MaxInstability = 100f;

        [SerializeField]
        private float platformHeight = 0.3f;

        [Header("Stability")]
        [Tooltip("Instability added for an offset of one full floor width (scales with offset squared).")]
        [SerializeField]
        private float instabilityPerOffset = 120f;

        [Tooltip("Offsets below this fraction of floor width count as perfect and calm the tower.")]
        [SerializeField]
        private float perfectOffset = 0.12f;

        [SerializeField]
        private float perfectCalm = 15f;

        [SerializeField]
        private int collapsedFloors = 3;

        [SerializeField]
        private float instabilityAfterCollapse = 40f;

        private readonly List<FallingFloor> floors = new List<FallingFloor>();
        private TowerGame game;
        private TowerSway sway;
        private RelativeDrag drag;
        private int lastTouchId = -1;
        private bool isTouchBlocked;
        private float platformWidth;
        private float localTop;
        private float instability;
        private bool hasSeenTouch;

        // A floor joined the tower (raised before a collapse it may cause).
        public event Action<FallingFloor, bool> FloorLanded;

        public event Action Collapsed;

        public IReadOnlyList<FallingFloor> Floors => floors;

        public float TopY => transform.position.y + localTop;

        // Height of the tower top above the platform.
        public float LocalTop => localTop;

        // Calms the visual sway to zero (end-of-round shot).
        public bool SuppressSway { get; set; }

        // No more floors join the tower, so it matches the frozen end-of-round stats.
        public bool IsFrozen { get; set; }

        // World bounds of the platform and the stacked floors, from their logical positions.
        public void GetBounds(out float minX, out float maxX, out float bottomY, out float topY)
        {
            float x = transform.position.x;
            minX = x - platformWidth / 2f;
            maxX = x + platformWidth / 2f;
            foreach (FallingFloor floor in floors)
            {
                minX = Mathf.Min(minX, x + floor.TowerPosition.x - floor.Width / 2f);
                maxX = Mathf.Max(maxX, x + floor.TowerPosition.x + floor.Width / 2f);
            }
            bottomY = transform.position.y - platformHeight / 2f;
            topY = TopY;
        }

        public float TopX => transform.position.x + TopOffset;

        // Roof X relative to the platform, from the logical floor positions (without the visual sway).
        public float TopOffset => floors.Count > 0 ? floors[floors.Count - 1].TowerPosition.x : 0f;

        // 1 = rock solid, 0 = about to collapse.
        public float Stability => 1f - instability / MaxInstability;

        public void Init(TowerGame game, Sprite sprite, float width)
        {
            this.game = game;
            platformWidth = width;
            sway = game.Ruleset.CreateSway();
            if (game.Ruleset.RelativeControl)
                drag = new RelativeDrag(game.Ruleset.DragSensitivity);
            localTop = platformHeight / 2f;

            var spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = sprite;
            spriteRenderer.color = new Color(0.3f, 0.3f, 0.35f);
            spriteRenderer.drawMode = SpriteDrawMode.Sliced;
            spriteRenderer.size = new Vector2(width, platformHeight);
        }

        private void Update()
        {
            Vector3 position = transform.position;
            position.y = game.BaseY;
            float halfRange = game.PlayWidth / 2f;
            if (drag != null)
            {
                // The range keeps the roof (where floors land) within the play width, even when the tower leans.
                float topOffset = TopOffset;
                PlatformRange.Get(topOffset, halfRange, out float minX, out float maxX);

                // Touch drags the platform; the mouse puts the roof under the cursor.
                bool isTouching = TryGetTouch(out Vector2 touchPosition, out int touchId);
                // A touch that starts on a HUD button (mute) never drags the platform.
                if (isTouching && touchId != lastTouchId)
                    isTouchBlocked = game.IsOverHudButton(touchPosition);
                lastTouchId = isTouching ? touchId : -1;
                bool isDragging = isTouching && !isTouchBlocked;
                float touchX = isDragging ? game.Camera.ScreenToWorldPoint(touchPosition).x : 0f;
                position.x = drag.Update(isDragging, touchId, touchX, position.x, minX, maxX);
                if (!isTouching && TryGetMouseWorldX(out float mouseX) && !game.IsOverHudButton(Mouse.current.position.ReadValue()))
                    position.x = Mathf.Clamp(mouseX - topOffset, minX, maxX);
            }
            else if (TryGetPointerWorldX(out float pointerX))
            {
                position.x = Mathf.Clamp(pointerX, -halfRange, halfRange);
            }
            transform.position = position;
        }

        private void LateUpdate()
        {
            sway.Advance(Time.time, Time.deltaTime, SuppressSway ? 0f : instability / MaxInstability, floors.Count);
            for (int i = 0; i < floors.Count; i++)
                floors[i].SetVisualOffset(sway.Offset(i, floors.Count));
        }

        public void Attach(FallingFloor floor)
        {
            float previousX = floors.Count > 0 ? floors[floors.Count - 1].TowerPosition.x : 0f;
            float offsetX = floor.transform.position.x - transform.position.x;
            float placeX = Landing.PlaceX(offsetX, previousX, floor.Width, perfectOffset, game.Ruleset.SnapPerfectLandings, out bool isPerfect);

            floor.transform.SetParent(transform, true);
            floor.PlaceInTower(new Vector2(placeX, localTop + floor.Height / 2f), offsetX - placeX, game.Ruleset.ShowJuice);
            localTop += floor.Height;
            floors.Add(floor);

            float offset = Mathf.Abs(placeX - previousX) / floor.Width;
            if (isPerfect)
                instability = Mathf.Max(0f, instability - perfectCalm);
            else
                instability += offset * offset * instabilityPerOffset;

            FloorLanded?.Invoke(floor, isPerfect);

            if (instability >= MaxInstability)
                CollapseTop();
        }

        // Calms the tower down (the golden floor); never below zero.
        public void RelieveInstability(float amount) => instability = Mathf.Max(0f, instability - amount);

        private void CollapseTop()
        {
            int count = Mathf.Min(collapsedFloors, floors.Count);
            for (int i = 0; i < count; i++)
            {
                FallingFloor floor = floors[floors.Count - 1];
                floors.RemoveAt(floors.Count - 1);
                localTop -= floor.Height;
                floor.Collapse();
            }
            instability = instabilityAfterCollapse;
            Collapsed?.Invoke();
        }

        // Without a finger on the screen the platform stays put. The mouse is ignored on phones,
        // where its position is (0,0) when nothing is touching.
        private bool TryGetPointerWorldX(out float worldX)
        {
            if (TryGetTouch(out Vector2 touchPosition, out _))
            {
                worldX = game.Camera.ScreenToWorldPoint(touchPosition).x;
                return true;
            }
            return TryGetMouseWorldX(out worldX);
        }

        private bool TryGetTouch(out Vector2 screenPosition, out int touchId)
        {
            var touchscreen = Touchscreen.current;
            if (touchscreen != null && touchscreen.primaryTouch.press.isPressed)
            {
                hasSeenTouch = true;
                screenPosition = touchscreen.primaryTouch.position.ReadValue();
                touchId = touchscreen.primaryTouch.touchId.ReadValue();
                return true;
            }
            screenPosition = default;
            touchId = 0;
            return false;
        }

        // Mouse position, followed without any button. Ignored once a touch was seen and on phones.
        private bool TryGetMouseWorldX(out float worldX)
        {
            worldX = 0f;
            if (Mouse.current == null || hasSeenTouch || Application.isMobilePlatform)
                return false;
            worldX = game.Camera.ScreenToWorldPoint(Mouse.current.position.ReadValue()).x;
            return true;
        }
    }
}
