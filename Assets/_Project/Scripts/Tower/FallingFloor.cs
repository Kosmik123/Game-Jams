using UnityEngine;

namespace ZeroHour.Tower
{
    public class FallingFloor : MonoBehaviour
    {
        private static readonly Color UnpoweredTint = new Color(0.35f, 0.35f, 0.4f);

        private TowerGame game;
        private float fallSpeed;
        private SpriteRenderer spriteRenderer;
        private bool isDebris;
        private float spin;

        public FloorType Type { get; private set; }
        public float Size { get; private set; }
        public bool IsAttached { get; private set; }

        // Position in the tower, without the visual wobble.
        public Vector2 TowerPosition { get; private set; }

        public void Init(TowerGame game, FloorType type, Sprite sprite, float size, float fallSpeed)
        {
            this.game = game;
            this.fallSpeed = fallSpeed;
            Type = type;
            Size = size;

            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = sprite;
            spriteRenderer.color = type.Color;
            spriteRenderer.drawMode = SpriteDrawMode.Sliced;
            spriteRenderer.size = new Vector2(size, size);
        }

        private void Update()
        {
            if (IsAttached)
                return;

            Vector3 position = transform.position;
            float previousBottom = position.y - Size / 2f;
            position.y -= fallSpeed * Time.deltaTime;
            transform.position = position;
            transform.Rotate(0f, 0f, spin * Time.deltaTime);

            TowerController tower = game.Tower;
            float bottom = position.y - Size / 2f;
            bool crossedTop = previousBottom >= tower.TopY && bottom < tower.TopY;
            if (!isDebris && crossedTop && Mathf.Abs(position.x - tower.TopX) < Size)
            {
                IsAttached = true;
                tower.Attach(this);
                return;
            }

            if (position.y < game.BottomY - Size)
                Destroy(gameObject);
        }

        public void PlaceInTower(Vector2 towerPosition)
        {
            TowerPosition = towerPosition;
            transform.localPosition = towerPosition;
            transform.localRotation = Quaternion.identity;
        }

        public void SetVisualOffset(float offsetX)
        {
            transform.localPosition = new Vector3(TowerPosition.x + offsetX, TowerPosition.y, 0f);
        }

        public void SetPowered(bool isPowered)
        {
            bool producesPower = Type.Energy > 0;
            spriteRenderer.color = isPowered || producesPower ? Type.Color : Type.Color * UnpoweredTint;
        }

        // Falls off the collapsing tower and never attaches again.
        public void Collapse()
        {
            IsAttached = false;
            isDebris = true;
            transform.SetParent(null, true);
            fallSpeed *= 1.5f;
            spin = Random.Range(-200f, 200f);
            SetPowered(true);
        }
    }
}
