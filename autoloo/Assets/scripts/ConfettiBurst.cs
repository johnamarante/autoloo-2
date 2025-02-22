using System.Collections;
using UnityEngine;

public class ConfettiBurst : MonoBehaviour
{
    public GameObject confettiPrefab; // Assign a confetti prefab with a Particle System in the Inspector
    public AudioClip[] shatterSounds; // Array of shatter sounds
    private AudioSource audioSource;

    void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
    }

    public void BurstConfetti(Color color1, Color color2, Color color3)
    {
        AudioClip selectedClip = shatterSounds[Random.Range(0, shatterSounds.Length)];
        audioSource.clip = selectedClip;
        audioSource.Play();
        StartCoroutine(SpawnConfetti(color1));
        StartCoroutine(SpawnConfetti(color2));
        StartCoroutine(SpawnConfetti(color3));
    }

    public IEnumerator SpawnConfetti(Color color)
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
                main.startSpeed = new int[] { 50, 75, 100, 150, 200, 300, 400 }[Random.Range(0, 7)];
                main.startSize = new ParticleSystem.MinMaxCurve(0.3f, 1f);
                main.gravityModifier = 25; // Flutter down effect
                main.startLifetime = 10f; // Linger longer
                main.maxParticles = 30; // Ensure a burst effect
                main.duration = 0.1f; // Short burst duration
                main.loop = false; // Prevent continuous emission

                var colorOverLifetime = particleSystem.colorOverLifetime;
                colorOverLifetime.enabled = false; // Disable color fading

                var sizeOverLifetime = particleSystem.sizeOverLifetime;
                sizeOverLifetime.enabled = false; // Prevent particles from shrinking

                var renderer = confetti.GetComponent<ParticleSystemRenderer>();
                if (renderer != null)
                {
                    renderer.renderMode = ParticleSystemRenderMode.Mesh;
                    renderer.mesh = Resources.GetBuiltinResource<Mesh>("Cube.fbx"); // Use cube mesh for crisp edges

                    // Create a new material dynamically to ensure crisp edges
                    Material confettiMaterial = new Material(Shader.Find("Unlit/Color"));
                    confettiMaterial.color = color; // Set the color to match the particle color
                    renderer.material = confettiMaterial;
                }

                // Add rotation over lifetime for spinning effect
                var rotationOverLifetime = particleSystem.rotationOverLifetime;
                rotationOverLifetime.enabled = true;
                rotationOverLifetime.z = new ParticleSystem.MinMaxCurve(-360f, 360f); // Random spin speed in degrees per second

                particleSystem.Emit(10); // Emit all particles at once
                particleSystem.Play();

                // Destroy confetti object after particles are done
                Destroy(confetti, 2);
            }
        }
        yield return new WaitForSeconds(0);
    }
}
