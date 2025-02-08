using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleMusicController : MonoBehaviour
{
    public AudioClip battleMusicIntro;
    public AudioClip battleMusicMid;
    public AudioClip battleMusicOutro;
    public AudioClip franceBattleMusicIntro;
    public AudioClip franceBattleMusicMid;
    public AudioClip franceBattleMusicOutro;
    public AudioClip britainBattleMusicIntro;
    public AudioClip britainBattleMusicMid;
    public AudioClip britainBattleMusicOutro;
    private AudioSource audioSource;
    private double nextStartTime;
    private bool hasStartedLooping = false;

    public void SetFranceMusic()
    {
        battleMusicIntro = franceBattleMusicIntro;
        battleMusicMid = franceBattleMusicMid;
        battleMusicOutro = franceBattleMusicOutro;
    }

    public void SetBritainMusic()
    {
        battleMusicIntro = britainBattleMusicIntro;
        battleMusicMid = britainBattleMusicMid;
        battleMusicOutro = britainBattleMusicOutro;
    }

    public void PlayLoopingBattleMusic()
    {
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.loop = false; // We handle looping manually
        }

        StopAllCoroutines(); // Prevent multiple loops from overlapping
        StartCoroutine(PlayMusicSequence());
    }

    private IEnumerator PlayMusicSequence()
    {
        yield return PlayClip(battleMusicIntro);

        while (true)
        {
            yield return PlayClip(battleMusicMid);
            yield return PlayClip(battleMusicMid);
            yield return PlayClip(battleMusicOutro);
        }
    }

    private IEnumerator PlayClip(AudioClip clip)
    {
        audioSource.clip = clip;
        audioSource.Play();
        yield return new WaitForSeconds(clip.length);
    }

}
