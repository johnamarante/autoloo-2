using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Background : MonoBehaviour
{
    public GameManager gameManager;
    // Start is called before the first frame update
    void Start()
    {
        gameManager = (GameManager)FindObjectOfType(typeof(GameManager));
    }

    private void OnMouseDown()
    {
        if (!gameManager.InBattleModeAndNotDeploymentMode)
        {
            gameManager.Deselect();
        }
    }
}
