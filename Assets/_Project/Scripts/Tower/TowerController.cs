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

        [SerializeField]
        private float maxWobble = 0.6f;

        private readonly List<FallingFloor> floors = new List<FallingFloor>();
        private TowerGame game;
        private float localTop;
        private float instability;
        private bool hasSeenTouch;

        public event Action<FallingFloor> PerfectLanding;

        public IReadOnlyList<FallingFloor> Floors => floors;

        public float TopY => transform.position.y + localTop;

        public float TopX => transform.position.x + (floors.Count > 0 ? floors[floors.Count - 1].TowerPosition.x : 0f);

        // 1 = rock solid, 0 = about to collapse.
        public float Stability => 1f - instability / MaxInstability;

        public void Init(TowerGame game, Sprite sprite, float width)
        {
            this.game = game;
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
            if (TryGetPointerWorldX(out float pointerX))
            {
                float halfRange = game.PlayWidth / 2f;
                position.x = Mathf.Clamp(pointerX, -halfRange, halfRange);
            }
            transform.position = position;
        }

        private void LateUpdate()
        {
            // Visual sway grows with instability and height.
            float sway = Mathf.Sin(Time.time * 3f) * instability / MaxInstability;
            for (int i = 0; i < floors.Count; i++)
                floors[i].SetVisualOffset(Mathf.Clamp(sway * 0.06f * (i + 1), -maxWobble, maxWobble));
        }

        public void Attach(FallingFloor floor)
        {
            float previousX = floors.Count > 0 ? floors[floors.Count - 1].TowerPosition.x : 0f;
            float offsetX = floor.transform.position.x - transform.position.x;

            floor.transform.SetParent(transform, true);
            floor.PlaceInTower(new Vector2(offsetX, localTop + floor.Size / 2f));
            localTop += floor.Size;
            floors.Add(floor);

            float offset = Mathf.Abs(offsetX - previousX) / floor.Size;
            if (offset < perfectOffset)
            {
                instability = Mathf.Max(0f, instability - perfectCalm);
                PerfectLanding?.Invoke(floor);
            }
            else
                instability += offset * offset * instabilityPerOffset;

            if (instability >= MaxInstability)
                CollapseTop();
        }

        private void CollapseTop()
        {
            int count = Mathf.Min(collapsedFloors, floors.Count);
            for (int i = 0; i < count; i++)
            {
                FallingFloor floor = floors[floors.Count - 1];
                floors.RemoveAt(floors.Count - 1);
                localTop -= floor.Size;
                floor.Collapse();
            }
            instability = instabilityAfterCollapse;
        }

        // Without a finger on the screen the platform stays put. The mouse is ignored on phones,
        // where its position is (0,0) when nothing is touching.
        private bool TryGetPointerWorldX(out float worldX)
        {
            worldX = 0f;
            Vector2 screenPosition;
            var touchscreen = Touchscreen.current;
            bool isTouching = touchscreen != null && touchscreen.primaryTouch.press.isPressed;
            if (isTouching)
                hasSeenTouch = true;

            if (isTouching)
                screenPosition = touchscreen.primaryTouch.position.ReadValue();
            else if (Mouse.current != null && !hasSeenTouch && !Application.isMobilePlatform)
                screenPosition = Mouse.current.position.ReadValue();
            else
                return false;

            worldX = game.Camera.ScreenToWorldPoint(screenPosition).x;
            return true;
        }
    }
}
