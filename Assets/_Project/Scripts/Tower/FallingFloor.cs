using UnityEngine;

namespace ZeroHour.Tower
{
    public class FallingFloor : MonoBehaviour
    {
        private static readonly Color UnpoweredTint = new Color(0.45f, 0.45f, 0.5f);
        private const float SlideDuration = 0.08f;
        private const float SquashDuration = 0.15f;
        private static readonly Vector2 SquashScale = new Vector2(1.15f, 0.85f);

        private TowerGame game;
        private float fallSpeed;
        private SpriteRenderer spriteRenderer;
        private bool isDebris;
        private float spin;
        private bool hasArt;
        private float slideFromX;
        private float slideTime;
        private Vector3 baseScale = Vector3.one;
        private float squashTime = float.MaxValue;

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

            // Floor art from Resources/Floors/<type name>; the tinted square is the fallback.
            Sprite art = Resources.Load<Sprite>($"Floors/{type.Name}");
            hasArt = art != null;
            if (hasArt)
            {
                spriteRenderer.sprite = art;
                spriteRenderer.drawMode = SpriteDrawMode.Simple;
                transform.localScale = Vector3.one * (size / art.bounds.size.x);
                baseScale = transform.localScale;
            }
            else
            {
                spriteRenderer.sprite = sprite;
                spriteRenderer.drawMode = SpriteDrawMode.Sliced;
                spriteRenderer.size = new Vector2(size, size);
            }
            SetPowered(true);
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
            if (!isDebris && !tower.IsFrozen && crossedTop && Mathf.Abs(position.x - tower.TopX) < Size)
            {
                IsAttached = true;
                tower.Attach(this);
                return;
            }

            if (position.y < game.BottomY - Size)
                Destroy(gameObject);
        }

        // slideFromX: visual offset at the moment of landing that eases to zero (a snapped floor slides into place).
        // squash: the floor flattens for a moment and springs back (visual only).
        public void PlaceInTower(Vector2 towerPosition, float slideFromX = 0f, bool squash = false)
        {
            squashTime = squash ? 0f : float.MaxValue;
            TowerPosition = towerPosition;
            this.slideFromX = slideFromX;
            slideTime = 0f;
            transform.localPosition = new Vector3(towerPosition.x + slideFromX, towerPosition.y, 0f);
            transform.localRotation = Quaternion.identity;
        }

        public void SetVisualOffset(float offsetX)
        {
            float slide = 0f;
            if (slideFromX != 0f && slideTime < SlideDuration)
            {
                slideTime += Time.deltaTime;
                slide = slideFromX * (1f - Mathf.Clamp01(slideTime / SlideDuration));
            }
            // The squash keeps the floor's bottom on the floor below.
            float squashY = 1f;
            if (squashTime < SquashDuration)
            {
                squashTime += Time.deltaTime;
                float t = Mathf.Clamp01(squashTime / SquashDuration);
                Vector2 scale = Vector2.Lerp(SquashScale, Vector2.one, t);
                squashY = scale.y;
                transform.localScale = new Vector3(baseScale.x * scale.x, baseScale.y * scale.y, baseScale.z);
            }
            transform.localPosition = new Vector3(TowerPosition.x + offsetX + slide, TowerPosition.y - (1f - squashY) * Size / 2f, 0f);
        }

        public void SetPowered(bool isPowered)
        {
            bool producesPower = Type.Energy > 0;
            Color baseColor = hasArt ? Color.white : Type.Color;
            spriteRenderer.color = isPowered || producesPower ? baseColor : baseColor * UnpoweredTint;
        }

        // Falls off the collapsing tower and never attaches again.
        public void Collapse()
        {
            IsAttached = false;
            isDebris = true;
            squashTime = float.MaxValue;
            transform.localScale = baseScale;
            transform.SetParent(null, true);
            fallSpeed *= 1.5f;
            spin = Random.Range(-200f, 200f);
            SetPowered(true);
        }
    }
}
