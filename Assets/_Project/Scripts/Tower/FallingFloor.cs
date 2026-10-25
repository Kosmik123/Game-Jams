using UnityEngine;

namespace ZeroHour.Tower
{
    public class FallingFloor : MonoBehaviour
    {
        private TowerGame game;
        private float fallSpeed;

        public FloorType Type { get; private set; }
        public float Size { get; private set; }
        public bool IsAttached { get; private set; }

        public void Init(TowerGame game, FloorType type, Sprite sprite, float size, float fallSpeed)
        {
            this.game = game;
            this.fallSpeed = fallSpeed;
            Type = type;
            Size = size;

            var spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
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

            TowerController tower = game.Tower;
            float bottom = position.y - Size / 2f;
            bool crossedTop = previousBottom >= tower.TopY && bottom < tower.TopY;
            if (crossedTop && Mathf.Abs(position.x - tower.TopX) < Size)
            {
                IsAttached = true;
                tower.Attach(this);
                return;
            }

            if (position.y < game.BottomY - Size)
                Destroy(gameObject);
        }
    }
}
