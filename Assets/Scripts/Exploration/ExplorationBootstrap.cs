using UnityEngine;

/// <summary>
/// Exploration (action) mode. Self-contained: put it on an empty GameObject in an empty scene.
/// Creates the camera, a placeholder background and a capsule-shaped player, and makes the camera follow the player.
/// </summary>
public class ExplorationBootstrap : MonoBehaviour
{
    [SerializeField]
    private float cameraSize = 6f;

    private static readonly Color PlayerColor = new Color(0.45f, 0.62f, 0.5f);

    private Camera cam;
    private Transform player;
    private InfiniteGrid grid;

    private void Awake()
    {
        SetupCamera();
        grid = new GameObject("Background").AddComponent<InfiniteGrid>();
        grid.transform.SetParent(transform, false);
        player = CreatePlayer();
    }

    private void LateUpdate()
    {
        Vector3 position = player.position;
        cam.transform.position = new Vector3(position.x, position.y, -10f);
        grid.Refresh(cam);
    }

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
        cam.orthographicSize = cameraSize;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.07f, 0.08f, 0.1f);
        cam.transform.position = new Vector3(0f, 0f, -10f);
    }

    /// <summary>Placeholder player: a capsule with an arrow marking its front.</summary>
    private Transform CreatePlayer()
    {
        var go = new GameObject("Player", typeof(Rigidbody2D), typeof(CapsuleCollider2D), typeof(CreatureMover));
        go.transform.SetParent(transform, false);

        var collider = go.GetComponent<CapsuleCollider2D>();
        collider.direction = CapsuleDirection2D.Vertical;
        collider.size = new Vector2(0.8f, 1.2f);

        go.GetComponent<CreatureMover>().AimCamera = cam;

        var body = new GameObject("Body");
        body.transform.SetParent(go.transform, false);
        body.transform.localScale = new Vector3(0.8f, 1.2f, 1f);
        var bodyRenderer = body.AddComponent<SpriteRenderer>();
        bodyRenderer.sprite = PixelSprites.Square;
        bodyRenderer.color = PlayerColor;
        bodyRenderer.sortingOrder = 1;

        var front = new GameObject("Front");
        front.transform.SetParent(go.transform, false);
        front.transform.localPosition = new Vector3(0f, 0.3f, 0f);
        front.transform.localScale = Vector3.one * 0.4f;
        var frontRenderer = front.AddComponent<SpriteRenderer>();
        frontRenderer.sprite = PixelSprites.Arrow;
        frontRenderer.color = new Color(0.1f, 0.1f, 0.1f, 0.85f);
        frontRenderer.sortingOrder = 2;

        return go.transform;
    }
}
