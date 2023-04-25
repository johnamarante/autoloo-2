using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(BoxCollider))]

public class DragIt : MonoBehaviour
{
    private bool wasConfirmedDrag;
    private Vector3 screenPoint;
    private Vector3 offset;
    private Vector3 startPosition;
    private GameObject belowGameObject;

    void OnMouseDown()
    {
        screenPoint = Camera.main.WorldToScreenPoint(gameObject.transform.position);
        offset = gameObject.transform.position - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z));
        startPosition = transform.position;
    }

    void OnMouseDrag()
    {
        if (TryGetComponent(out Unit unit)
            && !unit.gameManager.InBattleModeAndNotDeploymentMode//units should not be draggable in battle mode
            && unit.gameManager.selectedUnit != null
            && unit.gameManager.selectedUnit.name == unit.name)//unit should not be draggable if it is not the selected unit
        {
                Vector3 curScreenPoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z);
                Vector3 curPosition = Camera.main.ScreenToWorldPoint(curScreenPoint) + offset;
                transform.position = curPosition;
                ProcessHit();
            }

        //revisit this point in the code after the dragged-unit-to-front issue is solved
        //because that will change the distance required to flip this bit
        if (Vector3.Distance(transform.position, startPosition) > 1)
        {
            wasConfirmedDrag = true;
            unit.ClearOldDeployMarkerOccupancy();
        }
    }

    private void ProcessHit()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward) * 1000f, out hit, Mathf.Infinity))
        {
            if (hit.collider.gameObject.GetComponent<DeploymentMarker>()) //.name.Contains("marker"))
            {
                if (belowGameObject == null)
                {
                    belowGameObject = hit.collider.gameObject;
                    belowGameObject.GetComponent<DeploymentMarker>().ShowHoverIndicator(true);
                }
            }
            else //backgroundPlane
            {
                if (belowGameObject != null)
                {
                    //unflag object which was previously belowObject
                    if (belowGameObject.GetComponent<DeploymentMarker>())
                    {
                        belowGameObject.GetComponent<DeploymentMarker>().ShowHoverIndicator(false);
                        //if this is a deployed unit, then retain he assigned alue of the belowObject, which woiuld be adeployment marker.
                        //that way, snap back will have a full effect and the hop-stack bug will not occur
                        if (TryGetComponent(out Unit unit))
                        {
                            if (unit.Deployed)
                            {
                                return;
                            }
                        }
                        belowGameObject = null;
                    }
                }
            }
        }
    }

    private void OnMouseUp()
    {
        if (TryGetComponent(out Unit unit)
            && wasConfirmedDrag)
        {
            unit.gameManager.deployment.TrySnapToDeploymentQueueSpace(unit, belowGameObject, startPosition);
            unit.gameManager.Deselect();
            wasConfirmedDrag = false;
        }
    }
}