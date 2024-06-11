using NaughtyAttributes;
using System.Collections;
using UnityEngine;

public class ThreadAttacher : MonoBehaviour
{
    [SerializeField]
    private ThreadPhysics threadPrefab;
    [SerializeField]
    private ThreadPhysics thread;

    [SerializeField]
    private Transform threadOrigin;

    [SerializeField]
    private LayerMask attachableLayers;
    [SerializeField]
    private float maxDistance;

    [SerializeField, ReadOnly]
    private Vase currentlyAttachedVase;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
            AttachThread();

        else if (Input.GetMouseButtonDown(1)) 
            DetachThread();
    }

    private void DetachThread()
    {
        if (thread != null) 
            thread.gameObject.SetActive(false);
        currentlyAttachedVase = null;
    }

    private void AttachThread()
    {
        var ray = new Ray(transform.position, transform.forward);
        if (Physics.Raycast(ray, out var hitInfo, maxDistance, attachableLayers))
        {
            if (thread == null)
            {
                thread = Instantiate(threadPrefab, Vector3.zero, Quaternion.identity);
                thread.gameObject.SetActive(false);
                thread.Origin = threadOrigin;
            }

            if (thread.gameObject.activeSelf == false)
            {
                thread.Ending.position = ray.GetPoint(hitInfo.distance - thread.Thickness);
                thread.gameObject.SetActive(true);

                if (hitInfo.collider.TryGetComponent<Vase>(out var vase))
                {
                    currentlyAttachedVase = vase;
                    var lineRenderer = thread.GetComponent<LineRenderer>();
                    lineRenderer.endColor = lineRenderer.startColor = vase.VaseType.Color;
                }
                else
                {
                    currentlyAttachedVase = null;
                }
            }
            else
            {
                bool hitVase = hitInfo.collider.TryGetComponent<Vase>(out var vase);
                if (hitVase)
                {
                    thread.GetComponent<LineRenderer>().startColor = vase.VaseType.Color;
                }

                if (currentlyAttachedVase)
                {
                    if (hitVase)
                    {
                        if (vase != currentlyAttachedVase && vase.VaseType == currentlyAttachedVase.VaseType)
                        {
                            Debug.Log("Po³¹czono 2 takie same wazy");
                            Game.AddConnectedVaseType(vase.VaseType);
                        }
                        else
                        {
                            Destroy(thread);
                        }
                    }
                    else
                    {
                        thread.GetComponent<LineRenderer>().startColor = Color.gray;
                    }
                }

                var newOrigin = new GameObject().transform;
                newOrigin.parent = hitInfo.transform;
                newOrigin.position = hitInfo.point;
                thread.Origin = newOrigin;
                StartCoroutine(DelayedDisable(thread));
                thread = null;
                currentlyAttachedVase = null;
            }
        }
    }

    private IEnumerator DelayedDisable(Behaviour behaviourToDisable)
    {
        yield return new WaitForSeconds(1);
        if (behaviourToDisable != null) 
            behaviourToDisable.enabled = false;
    }
}
