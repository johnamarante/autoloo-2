using UnityEngine;
using System.Collections;

public class GunfireEffect : MonoBehaviour
{
    public GameObject effectPrefab; // The effect to instantiate
    public AudioClip[] gunfireSounds; // Array of gunfire sounds
    public float effectRadius = 1f; // Radius within which effects will be instantiated
    public int interationsCap = 4; // sound effect has 4 distinct discharges
    public bool left;
    public bool isEnabled = false;
    public bool isQueued = false;
    private AudioSource audioSource;
    private bool isPlaying = false;

    void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
    }

    private void Start()
    {
        //PlayGunfireEffect();
    }

    private void Update()
    {
        if (isQueued && isEnabled)
        {
            isQueued = false;
            PlayGunfireEffect();
        }
    }

    public void PlayGunfireEffect()
    {
        if (gunfireSounds.Length == 0 || effectPrefab == null)
        {
            Debug.LogWarning("Missing effectPrefab or gunfireSounds in GunfireEffect.");
            return;
        }

        AudioClip selectedClip = gunfireSounds[Random.Range(0, gunfireSounds.Length)];
        audioSource.clip = selectedClip;
        audioSource.Play();

        isPlaying = true;
        StartCoroutine(SpawnEffectsWhilePlaying());
    }

    private IEnumerator SpawnEffectsWhilePlaying()
    {
        int iterations = 0;
        while (iterations < interationsCap && isPlaying && audioSource.isPlaying)
        {
            Vector3 randomPosition = transform.position + (Random.insideUnitSphere * effectRadius);
            randomPosition.z = transform.position.z; // Keep it on the same plane
            var goMusketFlash = Instantiate(effectPrefab, randomPosition, Quaternion.identity);
            goMusketFlash.GetComponent<SpriteRenderer>().flipX = left;
            if (left)
            {
                goMusketFlash.GetComponent<GifAnimator>().frameShift = (-1 * goMusketFlash.GetComponent<GifAnimator>().frameShift);
            }
            iterations++;
            yield return new WaitForSeconds(0.1f); // Small delay before checking if the sound is still playing
        }

        isPlaying = false;
    }
}
