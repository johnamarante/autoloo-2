using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ResultPopup : MonoBehaviour
{
    public string displayText;
    public GameManager gameManager;
    public float delayBeforeSelfDestruct;

    // Start is called before the first frame update
    void Start()
    {
        TMP_Text tmp_text = GetComponent<TMP_Text>();
        tmp_text.text = displayText;
        Destroy(gameObject, delayBeforeSelfDestruct);
    }
}
