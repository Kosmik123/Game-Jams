using UnityEngine;
using UnityEngine.AI;

[RequireComponent (typeof(NavMeshAgent))]
public class MinotaurEnemy : MonoBehaviour
{
    private const string movingParam = "Moving";

    private NavMeshAgent agent;

    [SerializeField]
    private MazeGenerator mazeGenerator;
    [SerializeField]
    private Transform currentTarget;
    [SerializeField]
    private Animator animator;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        mazeGenerator.OnMazeGenerated += DetermineTarget;
    }

    [ContextMenu("New Target")]
    private void DetermineTarget()
    {
        currentTarget = mazeGenerator.GetRandomMazeBlock().transform;
        StartMovingToTarget();
    }

    private void StartMovingToTarget()
    {
        agent.SetDestination(currentTarget.position);
        animator.SetBool(movingParam, true);
    }

    private void Update()
    {
        if (currentTarget)
        {
            if (Mathf.Abs(transform.position.x - currentTarget.position.x) < 0.1f && Mathf.Abs(transform.position.z - currentTarget.position.z) < 0.1f)
            {
                currentTarget = null;
                animator.SetBool(movingParam, false);
                Invoke(nameof(DetermineTarget), Random.Range(1f, 3f));
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            animator.SetBool("Attack", true);
            agent.enabled = false;
            transform.LookAt(other.transform);
            Game.Loose();
        }
    }
}
