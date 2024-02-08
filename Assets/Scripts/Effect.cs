using UnityEngine;

public class Effect : MonoBehaviour
{
    public static event System.Action OnEffectReachedTarget;

    [SerializeField]
    private float speed;
    public Transform Target { get; set; }

    private void Update()
    {
        var position = Vector3.MoveTowards(transform.position, Target.position, speed * Time.deltaTime);
        transform.position = position;
        if (position == Target.position)
        {
            Destroy(gameObject);
            OnEffectReachedTarget?.Invoke();
        }
    }
}
