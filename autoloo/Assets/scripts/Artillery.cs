using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Artillery : MonoBehaviour
{
    public Unit unit;
    public int range;
    // Start is called before the first frame update
    void Start()
    {
        unit = GetComponent<Unit>();
        if (unit.spriteName.Contains("heavy"))
        {
            range = 5;
        }
        else {
            range = 3;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
