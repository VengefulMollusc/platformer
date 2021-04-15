using CMF;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrappleTarget : MonoBehaviour
{
    [SerializeField]
    Transform targetTransform;

    //List<CharacterGrappleController> inRangeControllers;
    //List<CharacterGrappleController> controllersSeenThisUpdate;

    //private void OnTriggerStay(Collider other)
    //{
    //    if (other.tag != "Player" || !targetTransform)
    //        return;

    //    CharacterGrappleController grappleController = other.GetComponent<CharacterGrappleController>();
    //    if (grappleController == null)
    //        return;

    //    if (controllersSeenThisUpdate == null)
    //        controllersSeenThisUpdate = new List<CharacterGrappleController>();

    //    if (!controllersSeenThisUpdate.Contains(grappleController))
    //    {
    //        controllersSeenThisUpdate.Add(grappleController);
    //    }
    //}

    //private void LateUpdate()
    //{
    //    if (inRangeControllers == null)
    //        inRangeControllers = new List<CharacterGrappleController>();

    //    if (controllersSeenThisUpdate == null)
    //        controllersSeenThisUpdate = new List<CharacterGrappleController>();

    //    if (inRangeControllers.Count <= 0 && controllersSeenThisUpdate.Count <= 0)
    //        return;

    //    // iterate through inRangeControllers and remove any that haven't been sees this update
    //    for (int i = inRangeControllers.Count - 1; i >= 0; i--)
    //    {
    //        if (controllersSeenThisUpdate.Count < 1 || !controllersSeenThisUpdate.Contains(inRangeControllers[i]))
    //        {
    //            inRangeControllers[i].RemoveGrappleTarget(this);
    //            inRangeControllers.RemoveAt(i);
    //        }
    //    }

    //    // iterate through controllersSeenThisUpdate and add any not in inRangeControllers
    //    foreach (CharacterGrappleController seen in controllersSeenThisUpdate)
    //    {
    //        if (!inRangeControllers.Contains(seen))
    //        {
    //            seen.AddGrappleTarget(this);
    //            inRangeControllers.Add(seen);
    //        }
    //    }

    //    // clear controllersSeenThisUpdate
    //    controllersSeenThisUpdate = new List<CharacterGrappleController>();
    //}

    private void OnTriggerEnter(Collider other)
    {
        AddRemoveTarget(other, true);
    }

    private void OnTriggerExit(Collider other)
    {
        AddRemoveTarget(other, false);
    }

    private void AddRemoveTarget(Collider other, bool add)
    {
        if (other.tag != "Player" || !targetTransform)
            return;

        CharacterGrappleController grappleController = other.GetComponent<CharacterGrappleController>();
        if (grappleController)
        {
            if (add)
            {
                grappleController.AddGrappleTarget(this);
            }
            else
            {
                grappleController.RemoveGrappleTarget(this);
            }
        }
    }

    public Transform GetTargetTransform()
    {
        return targetTransform;
    }
}
