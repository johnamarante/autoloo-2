using System.Collections;
using UnityEngine;

public class ConfettiBurst : MonoBehaviour
{
    public GameObject confettiPrefab; // Assign a confetti prefab with a Particle System in the Inspector
    public float delay = 3f; // Time in seconds before the confetti bursts

    void Start()
    {
        StartCoroutine(BurstConfetti());
    }

    IEnumerator BurstConfetti()
    {
        yield return new WaitForSeconds(delay);
        SpawnConfetti(Color.red);
        SpawnConfetti(Color.white);
        SpawnConfetti(Color.blue);
    }

    void SpawnConfetti(Color color)
    {
        if (confettiPrefab != null)
        {
            GameObject confetti = Instantiate(confettiPrefab, transform.position, Quaternion.identity);
            var particleSystem = confetti.GetComponent<ParticleSystem>();
            if (particleSystem != null)
            {
                particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                var main = particleSystem.main;
                main.startColor = color;
                main.startSpeed = 100; // Rapid upward shot
                main.gravityModifier = 25; // Flutter down effect
                main.startLifetime = 2f; // Linger longer
                main.maxParticles = 200; // Ensure a burst effect
                main.duration = 0.1f; // Short burst duration
                main.loop = false; // Prevent continuous emission

                var colorOverLifetime = particleSystem.colorOverLifetime;
                colorOverLifetime.enabled = false; // Disable color fading

                var sizeOverLifetime = particleSystem.sizeOverLifetime;
                sizeOverLifetime.enabled = false; // Prevent particles from shrinking

                particleSystem.Emit(200); // Emit all particles at once
                particleSystem.Play();
            }
        }
    }
}
