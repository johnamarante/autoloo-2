using UnityEngine;
using System.Collections;

public class CombatEffect : MonoBehaviour
{
    public GameObject effectPrefab;
    public AudioClip[] gunfireSounds;
    public AudioClip[] cavalryAttackSounds;
    public float effectRadius = 1f;
    public int interationsCap = 4;
    public bool left;
    public bool isEnabled = false;
    public bool isGunfireQueued = false;
    public bool isCavalryAttackQueued = false;
    private AudioSource audioSource;
    private bool isPlaying = false;

    void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
    }

    private void Update()
    {
        if (isGunfireQueued && isEnabled)
        {
            isGunfireQueued = false;
            PlayGunfireEffect();
        }
        if (isCavalryAttackQueued && isEnabled)
        {
            isCavalryAttackQueued = false;
            PlayCavalryAttackEffect();
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

    public void PlayCavalryAttackEffect()
    {
        if (cavalryAttackSounds.Length == 0 || effectPrefab == null)
        {
            Debug.LogWarning("Missing effectPrefab or cavalryAttackSounds in CavalryAttackEffect.");
            return;
        }

        AudioClip selectedClip = cavalryAttackSounds[Random.Range(0, cavalryAttackSounds.Length)];
        audioSource.clip = selectedClip;
        audioSource.Play();
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
