using Bipolar.CablePhysics;
using NaughtyAttributes;
using System.Collections;
using UnityEngine;

public class ThreadAttacher : MonoBehaviour
{
    public static readonly Color defaultThreadColor = 0.3f * Color.white;

    [SerializeField]
    private Cable3D threadPrefab;
    [SerializeField]
    private Cable3D currentThread;

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
        if (currentThread != null) 
            currentThread.gameObject.SetActive(false);
        currentlyAttachedVase = null;
    }

    private void AttachThread()
    {
        var ray = new Ray(transform.position, transform.forward);
        if (Physics.Raycast(ray, out var hitInfo, maxDistance, attachableLayers))
        {
            if (currentThread == null)
            {
                currentThread = Instantiate(threadPrefab, Vector3.zero, Quaternion.identity);
                currentThread.gameObject.SetActive(false);
                currentThread.Origin = threadOrigin;
            }

            if (currentThread.gameObject.activeSelf == false)
            {
                currentThread.Ending.position = ray.GetPoint(hitInfo.distance - currentThread.Thickness);
                currentThread.gameObject.SetActive(true);

                if (hitInfo.collider.TryGetComponent<Vase>(out var vase))
                {
                    currentlyAttachedVase = vase;
                    var lineRenderer = currentThread.GetComponent<LineRenderer>();
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
                    currentThread.GetComponent<LineRenderer>().startColor = vase.VaseType.Color;
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
                            Destroy(currentThread);
                        }
                    }
                    else
                    {
                        currentThread.GetComponent<LineRenderer>().startColor = defaultThreadColor;
                    }
                }

                var newOrigin = new GameObject().transform;
                newOrigin.parent = hitInfo.transform;
                newOrigin.position = hitInfo.point;
                currentThread.Origin = newOrigin;
                StartCoroutine(DelayedDisable(currentThread));
                currentThread = null;
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
