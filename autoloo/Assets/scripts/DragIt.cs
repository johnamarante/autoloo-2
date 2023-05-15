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
    private Deployment deployment;
    public GameObject belowGameObject;

    void OnMouseDown()
    {
        deployment = (Deployment)FindObjectOfType(typeof(Deployment));
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
            //TODO: refactor, should be a case statement
            if (hit.collider.gameObject.GetComponent<DeploymentMarker>()) //.name.Contains("marker"))
            {
                belowGameObject = hit.collider.gameObject;
                belowGameObject.GetComponent<DeploymentMarker>().ShowHoverIndicator(true);
                //Debug.Log(hit.collider.gameObject.GetComponent<DeploymentMarker>().positionKey);
            }
            else if (hit.collider.gameObject.TryGetComponent(out Unit belowUnit) && belowUnit.Deployed)
            {
                belowGameObject = deployment.listLeftDeploymentMarkers.Where(x => x.positionKey == belowUnit.QueuePosition && x.side == belowUnit.side).First().gameObject;
                belowGameObject.GetComponent<DeploymentMarker>().ShowHoverIndicator(true);
                //Debug.Log(hit.collider.gameObject.GetComponent<Unit>().GetUnitSpriteName());
            }
            else //backgroundPlane
            {
                if (belowGameObject != null)
                {
                    //unflag object which was previously belowObject
                    if (belowGameObject.GetComponent<DeploymentMarker>())
                    {
                        belowGameObject.GetComponent<DeploymentMarker>().ShowHoverIndicator(false);
                        //if this is a deployed unit, then retain the assigned value of the belowObject, which would be a deployment marker.
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
            && belowGameObject != null
            && belowGameObject.TryGetComponent(out DeploymentMarker deploymentMarker)
            && deploymentMarker != null
            && wasConfirmedDrag)
        {
            unit.gameManager.deployment.TrySnapToDeploymentQueueSpace(unit, deploymentMarker, startPosition);
            unit.gameManager.Deselect();
            wasConfirmedDrag = false;
        }
        else
        {
            transform.position = startPosition;
        }
    }
}