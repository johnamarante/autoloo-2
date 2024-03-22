using System;
using UnityEngine;

public class NumberFloating : MonoBehaviour
{
    public GameObject numberPrefab; // Reference to the number prefab (TextMeshPro)

    private void Start()
    {
        numberPrefab.SetActive(true);
    }

    public void SpawnFloatingNumber(int? valueToDisplay = null, Vector3? spawnPosition = null, bool red = true)
    {
        // Use the provided spawnPosition if available, otherwise generate a random position
        var finalSpawnPosition = spawnPosition ?? new Vector3(UnityEngine.Random.Range(1, 10), UnityEngine.Random.Range(1, 10), 0f);

        // Instantiate the number prefab
        GameObject numberObject = Instantiate(numberPrefab, finalSpawnPosition, Quaternion.identity);

        // Set the number value
        TMPro.TextMeshPro textMesh = numberObject.GetComponent<TMPro.TextMeshPro>();
        int theNumber = valueToDisplay ?? 0;
        textMesh.text = theNumber.ToString();

        // Set the color based on the pattern
        Color textColor = (bool)(red) ? Color.red : Color.green;
        textMesh.color = textColor;

        // Start the floating animation
        StartCoroutine(Pulse(numberObject.transform));

        // Destroy the number after 1 second
        Destroy(numberObject, 1);
    }

    public void SpawnFloatingString(string valueToDisplay,  Color col, Vector3? spawnPosition = null, int lifetimeInSeconds = 1)
    {
        // Use the provided spawnPosition if available, otherwise generate a random position
        var finalSpawnPosition = spawnPosition ?? new Vector3(UnityEngine.Random.Range(1, 10), UnityEngine.Random.Range(1, 10), 0f);

        // Instantiate the number prefab
        GameObject numberObject = Instantiate(numberPrefab, finalSpawnPosition, Quaternion.identity);

        // Set the number value
        TMPro.TextMeshPro textMesh = numberObject.GetComponent<TMPro.TextMeshPro>();
        textMesh.text = valueToDisplay.ToString();

        // Set the color based on the pattern
        textMesh.color = col;

        // Start the floating animation
        StartCoroutine(Pulse(numberObject.transform));

        // Destroy the number after 1 second
        Destroy(numberObject, lifetimeInSeconds);
    }

    private System.Collections.IEnumerator Pulse(Transform numberTransform)
    {
        float startTime = Time.time;
        float floatSpeed = 5f; // Speed of the floating animation
        float floatAmplitude = 15f; // Amplitude of the floating animation

        while (Time.time - startTime < 1)
        {
            float verticalOffset = Mathf.Sin((Time.time - startTime) * floatSpeed) * floatAmplitude;
            if (numberTransform != null)
            { 
                numberTransform.position += Vector3.up * verticalOffset * Time.deltaTime;
            }
            yield return null;
        }
    }
}
