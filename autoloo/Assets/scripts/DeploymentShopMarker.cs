using UnityEngine;

public class DeploymentShopMarker : MonoBehaviour
{
    public int positionKey;
    public string side;
    public Deployment deployment;
    public Unit _occupant;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public bool IsFrozenShopUnitAboveMe()
    {
        Debug.DrawRay(transform.position + new Vector3(0,0,1), transform.TransformDirection(Vector3.back) * 1000f, Color.red, Mathf.Infinity);
        RaycastHit hit;
        if (Physics.Raycast(transform.position + new Vector3(0, 0, 1), transform.TransformDirection(Vector3.back) * 1000f, out hit, Mathf.Infinity))
        {
            var unitHit = hit.collider.gameObject.GetComponent<Unit>();
            if (unitHit.gameObject != null && unitHit.Freezed)
            {
                return true;
            }
        }
        return false;
    }
}
