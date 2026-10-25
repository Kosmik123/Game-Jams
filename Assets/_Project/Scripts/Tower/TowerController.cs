using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ZeroHour.Tower
{
    // The base platform with the stacked floors. Follows the pointer/finger on X only.
    public class TowerController : MonoBehaviour
    {
        [SerializeField]
        private float platformHeight = 0.3f;

        private readonly List<FallingFloor> floors = new List<FallingFloor>();
        private TowerGame game;
        private float localTop;

        public IReadOnlyList<FallingFloor> Floors => floors;

        public float TopY => transform.position.y + localTop;

        public float TopX => transform.position.x + (floors.Count > 0 ? floors[floors.Count - 1].transform.localPosition.x : 0f);

        public void Init(TowerGame game, Sprite sprite, float width)
        {
            this.game = game;
            localTop = platformHeight / 2f;

            var spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = sprite;
            spriteRenderer.color = new Color(0.3f, 0.3f, 0.35f);
            transform.localScale = Vector3.one;
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

        public void Attach(FallingFloor floor)
        {
            floor.transform.SetParent(transform, true);
            floor.transform.localPosition = new Vector3(floor.transform.position.x - transform.position.x, localTop + floor.Size / 2f, 0f);
            localTop += floor.Size;
            floors.Add(floor);
        }

        private bool TryGetPointerWorldX(out float worldX)
        {
            worldX = 0f;
            Vector2 screenPosition;
            var touchscreen = Touchscreen.current;
            if (touchscreen != null && touchscreen.primaryTouch.press.isPressed)
                screenPosition = touchscreen.primaryTouch.position.ReadValue();
            else if (Mouse.current != null)
                screenPosition = Mouse.current.position.ReadValue();
            else
                return false;

            worldX = game.Camera.ScreenToWorldPoint(screenPosition).x;
            return true;
        }
    }
}
