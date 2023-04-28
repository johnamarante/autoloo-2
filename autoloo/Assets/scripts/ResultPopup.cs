using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ResultPopup : MonoBehaviour
{
    public string displayText;
    public GameManager gameManager;
    private int initiatedFrame;

    // Start is called before the first frame update
    void Start()
    {
        TMP_Text tmp_text = GetComponent<TMP_Text>();
        tmp_text.text = displayText;
        initiatedFrame = Time.frameCount;
    }

    private void Update()
    {
        if (Time.frameCount > (initiatedFrame + 200))
        {
            Destroy(gameObject);
        }
    }
}
