using UnityEngine;

public class AudioManager : Singleton<AudioManager>
{
    [Header("Audio Sources")]
    public AudioSource sfxSource;    // 播放音效
    public AudioSource musicSource;  // 播放BGM（可选）
    public AudioSource stepSource;

    [Header("SFX Clips")]
    public AudioClip attackClip;
    public AudioClip axeClip;
    public AudioClip axeClip2;
    public AudioClip hitClip;
    public AudioClip hitplayerCilp;
    public AudioClip rollClip;
    public AudioClip counterClip;
    public AudioClip executeClip;
    public AudioClip swordWave;
    public AudioClip swordWaveHit;
    public AudioClip executeDamageCip;
    public AudioClip beginExecutionClip;
    public AudioClip throwClip;
    public AudioClip playerFootStep;
    public AudioClip throwFireBall;
    public AudioClip fireballHit;
    public AudioClip axeExplode;
    public AudioClip getImpulse;
    private bool isPlayFootStep;


    [Header("Music Clips")]
    public AudioClip bossMusic;

    
    // ----------- 公共接口 -----------

    public void PlaySFX(AudioClip clip)
    {
        if (clip == null || sfxSource == null) return;
        sfxSource.PlayOneShot(clip);
    }

    public void PlayAttack() => PlaySFX(attackClip);
    public void PlayAxeAttack() => PlaySFX(axeClip);
    public void PlayJumpAttack() => PlaySFX(axeClip2);
    public void PlayHit() => PlaySFX(hitClip);
    public void PlayHitPlayer() => PlaySFX(hitplayerCilp);
    public void PlayRoll() => PlaySFX(rollClip);

    public void PlaySwordWave() => PlaySFX(swordWave);

    public void PlayExecuteDamage() => PlaySFX(executeDamageCip);
    public void PlayCounter() => PlaySFX(counterClip);
    public void PlayExecute() => PlaySFX(executeClip);

    public void PlayBeginExecution() => PlaySFX(beginExecutionClip);

    public void PlayThrow() => PlaySFX(throwClip);

    public void PlayThrowFireBall() => PlaySFX(throwFireBall);

    public void PlayFireBallHit() => PlaySFX(fireballHit);

    public void PlayAxeExplode() => PlaySFX(axeExplode);

    public void PlayGetImpulse() => PlaySFX(getImpulse);

    public void PlayMusic(AudioClip clip, bool loop = true)
    {
        if (clip == null || musicSource == null) return;

        musicSource.loop = loop;
        musicSource.clip = clip;
        musicSource.Play();
    }

    public void PlayBossMusic() => PlayMusic(bossMusic, true);

    public void StopMusic()
    {
        if (musicSource != null)
            musicSource.Pause();
    }


    public void PlayFootStepSFX() => PlayFootStep(playerFootStep);
    public void PlayFootStep(AudioClip clip, bool loop = true)
    {
        if (clip == null || stepSource == null) return;
        if (!isPlayFootStep)
        {

            stepSource.loop = loop;
            stepSource.clip = clip;
            stepSource.Play();
            isPlayFootStep = true;
        }
    }

    public void StopFootStep()
    {
        if (stepSource != null &&isPlayFootStep)
        {

            stepSource.Stop();
            isPlayFootStep = false;
        }
    }
}
