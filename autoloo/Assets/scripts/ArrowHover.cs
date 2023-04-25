using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowHover : MonoBehaviour
{
    public PulseHover _pulseHover;
    // Start is called before the first frame update
    void Start()
    {
        _pulseHover = FindObjectOfType<PulseHover>();
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = new Vector3(transform.position.x, _pulseHover.yValue, transform.position.z);
    }
}
