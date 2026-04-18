using UnityEngine;
using System.Collections;

// 全局音频管理器（单例模式）
public class AudioManager : MonoBehaviour
{
    // 单例实例（全局唯一，其他脚本可通过AudioManager.Instance调用）
    public static AudioManager Instance;

    [Header("背景音乐（BGM）配置")]
    public AudioClip bgmClip; // 拖拽你的BGM音频文件
    [Range(0f, 1f)] public float bgmVolume = 0.7f; // BGM音量
    public bool isBgmLoop = true; // 是否循环播放BGM
    public AudioSource bgmAudioSource; // BGM音频源

    [Header("红点音效配置")]
    public AudioClip dotCoverDestroyClip; // 红点覆盖消除音效
    public AudioClip dotNormalDestroyClip; // 红点自然消失音效
    [Range(0f, 1f)] public float sfxVolume = 1f; // 音效音量
    public AudioSource sfxAudioSource; // 音效音频源（用于播放一次性音效）

    // 单例初始化：确保全局唯一，不随场景销毁
    private void Awake()
    {
        // 单例逻辑：如果已有实例，销毁新的；否则保留并标记为不销毁
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 全局生效
            InitAudioSources(); // 初始化音频源
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 初始化音频源（自动创建，无需手动添加组件）
    private void InitAudioSources()
    {
        // 创建BGM音频源（用于循环播放）
        bgmAudioSource = gameObject.AddComponent<AudioSource>();
        bgmAudioSource.volume = bgmVolume;
        bgmAudioSource.loop = isBgmLoop; // 开启循环
        bgmAudioSource.playOnAwake = false; // 不自动播放（手动控制）

        // 创建音效音频源（用于播放一次性音效）
        sfxAudioSource = gameObject.AddComponent<AudioSource>();
        sfxAudioSource.volume = sfxVolume;
        sfxAudioSource.loop = false; // 音效不循环
        sfxAudioSource.playOnAwake = false;
    }

    void Start()
    {
        // 启动时自动播放BGM（可根据需要注释）
        if (bgmClip != null)
        {
            PlayBGM();
        }
    }

    #region BGM控制方法（外部可调用）
    // 播放BGM（循环）
    public void PlayBGM()
    {
        if (bgmClip == null)
        {
            Debug.LogWarning("BGM音频未赋值！请在AudioManager面板拖拽音频文件");
            return;
        }
        bgmAudioSource.clip = bgmClip;
        bgmAudioSource.Play();
        
    }

    // 暂停BGM
    public void PauseBGM()
    {
        if (bgmAudioSource.isPlaying)
        {
            bgmAudioSource.Pause();
            Debug.Log("BGM已暂停");
        }
    }

    // 恢复BGM播放
    public void ResumeBGM()
    {
        if (!bgmAudioSource.isPlaying)
        {
            bgmAudioSource.Play();
            Debug.Log("BGM恢复播放");
        }
    }

    // 停止BGM
    public void StopBGM()
    {
        bgmAudioSource.Stop();
        Debug.Log("BGM已停止");
    }
    #endregion

    #region 音效播放方法（外部可调用）
    // 播放红点覆盖消除音效
    public void PlayDotCoverDestroySFX()
    {
        PlayOneShotSFX(dotCoverDestroyClip, "覆盖消除");
    }

    // 播放红点自然消失音效
    public void PlayDotNormalDestroySFX()
    {
        PlayOneShotSFX(dotNormalDestroyClip, "自然消失");
    }

    // 通用播放一次性音效（私有方法，封装重复逻辑）
    private void PlayOneShotSFX(AudioClip clip, string sfxName)
    {
        if (clip == null)
        {
            Debug.LogWarning($"{sfxName}音效未赋值！请在AudioManager面板拖拽音频文件");
            return;
        }
        // 使用PlayOneShot避免音效重叠（比如多个红点同时消失）
        sfxAudioSource.PlayOneShot(clip, sfxVolume);
        
    }
    #endregion
}