using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("混音器配置")]
    [SerializeField] private AudioMixer audioMixer;
    private const string BGM_VOL_PARAM = "BGMVolume";
    private const string SFX_VOL_PARAM = "SFXVolume";

    [Header("播放器组件")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("音频库配置")]
    [SerializeField] private List<BGMClipEntry> bgmClips = new List<BGMClipEntry>();
    [SerializeField] private List<SFXClipEntry> sfxClips = new List<SFXClipEntry>();

    [Header("音效手感参数 (策划/调音可调)")]
    [Tooltip("提取种子等单音效的音调抖动范围")]
    [SerializeField] private float seedPitchMin = 0.92f;
    [SerializeField] private float seedPitchMax = 1.08f;

    private Dictionary<BGMType, AudioClip> bgmDict = new Dictionary<BGMType, AudioClip>();
    private Dictionary<SFXType, List<AudioClip>> sfxDict = new Dictionary<SFXType, List<AudioClip>>();
    
    // 记录每种音效上次播放的索引，防止连续两次抽到同一个
    private Dictionary<SFXType, int> lastPlayedIndices = new Dictionary<SFXType, int>();

    private Coroutine bgmFadeCoroutine;
    private float currentBGMVol = 0.8f;
    private float currentSFXVol = 0.8f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeDictionaries();
            LoadVolumeSettings();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        // 自动订阅 EventBus 的农场与生态事件
        EventBus.OnPhaseChanged += HandlePhaseChanged;
        EventBus.OnToolModeChanged += HandleToolModeChanged;
        EventBus.OnPlantFlowering += HandlePlantFlowering;
        // EventBus.OnCluePurchased += HandleCluePurchased;
        EventBus.OnSpeciesPerfected += HandleSpeciesPerfected;
    }
    private void OnDisable()
    {
        // 取消订阅
        EventBus.OnPhaseChanged -= HandlePhaseChanged;
        EventBus.OnToolModeChanged -= HandleToolModeChanged;
        EventBus.OnPlantFlowering -= HandlePlantFlowering;
        // EventBus.OnCluePurchased -= HandleCluePurchased;
        EventBus.OnSpeciesPerfected -= HandleSpeciesPerfected;
    }
    /// <summary>
    /// 初始化音频字典，便于快速查找音频剪辑
    /// </summary>
    private void InitializeDictionaries()
    {
        foreach (var entry in bgmClips)
        {
            if (entry.clip != null && !bgmDict.ContainsKey(entry.type))
                bgmDict.Add(entry.type, entry.clip);
        }

        foreach (var entry in sfxClips)
        {
            if (entry.clips != null && !sfxDict.ContainsKey(entry.type))
                sfxDict.Add(entry.type, entry.clips);
                lastPlayedIndices.Add(entry.type, -1);
        }
    }
    /// <summary>
    /// 音量设置加载
    /// </summary>
    private void LoadVolumeSettings()
    {
        currentBGMVol = PlayerPrefs.GetFloat("Vol_BGM", 0.8f);
        currentSFXVol = PlayerPrefs.GetFloat("Vol_SFX", 0.8f);

        SetBGMVolume(currentBGMVol);
        SetSFXVolume(currentSFXVol);
    }
    // 提供外部接口
    public float GetBGMVolume() => currentBGMVol;
    public float GetSFXVolume() => currentSFXVol;

    public void SetBGMVolume(float volume)
    {
        currentBGMVol = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat("Vol_BGM", currentBGMVol);
        // 对数分贝转换公式：[0.0001, 1] 映射到 [-80dB, 0dB]
        float db = currentBGMVol > 0.001f ? Mathf.Log10(currentBGMVol) * 20f : -80f;
        audioMixer.SetFloat(BGM_VOL_PARAM, db);
    }
    public void SetSFXVolume(float volume)
    {
        currentSFXVol = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat("Vol_SFX", currentSFXVol);
        float db = currentSFXVol > 0.001f ? Mathf.Log10(currentSFXVol) * 20f : -80f;
        audioMixer.SetFloat(SFX_VOL_PARAM, db);
    }
    /// <summary>
    /// BGM 播放与平滑过渡
    /// </summary>
    public void PlayBGM(BGMType type, float fadeDuration = 1.0f)
    {
        if (!bgmDict.TryGetValue(type, out AudioClip newClip) || newClip == null) return;
        if (bgmSource.clip == newClip && bgmSource.isPlaying) return;

        if (bgmFadeCoroutine != null) StopCoroutine(bgmFadeCoroutine);
        bgmFadeCoroutine = StartCoroutine(CrossFadeBGMRoutine(newClip, fadeDuration));
    }
    private IEnumerator CrossFadeBGMRoutine(AudioClip targetClip, float duration)
    {
        float halfDuration = duration * 0.5f;

        // 1. 淡出当前音乐
        if (bgmSource.isPlaying)
        {
            float startVol = bgmSource.volume;
            float elapsed = 0f;
            while (elapsed < halfDuration)
            {
                elapsed += Time.unscaledDeltaTime; // 使用 unscaledDeltaTime，暂停游戏时切歌不受影响
                bgmSource.volume = Mathf.Lerp(startVol, 0f, elapsed / halfDuration);
                yield return null;
            }
        }

        // 2. 切换音频并重置
        bgmSource.Stop();
        bgmSource.clip = targetClip;
        bgmSource.Play();

        // 3. 淡入新音乐
        float inElapsed = 0f;
        while (inElapsed < halfDuration)
        {
            inElapsed += Time.unscaledDeltaTime;
            bgmSource.volume = Mathf.Lerp(0f, 1f, inElapsed / halfDuration);
            yield return null;
        }

        bgmSource.volume = 1f;
        bgmFadeCoroutine = null;
    }

    /// <summary>
    /// 常规播放音效：自动处理多音频随机抽取与音调微偏移
    /// </summary>
    public void PlaySFX(SFXType type)
    {
if (!sfxDict.TryGetValue(type, out List<AudioClip> clips) || clips.Count == 0) return;

        // 1. 获取要播放的具体音频片段（防连续重复随机）
        AudioClip clipToPlay = GetRandomClipWithoutImmediateRepeat(type, clips);
        if (clipToPlay == null) return;

        // 2. 针对特定音效做音调微调（Pitch Shift）
        if (type == SFXType.HarvestSeed)
        {
            // 种子仅1个音频，通过音调随机偏移解决重复感
            sfxSource.pitch = Random.Range(seedPitchMin, seedPitchMax);
        }
        else
        {
            // 其他音效恢复默认音调（或给予轻微的 0.98~1.02 自然微扰）
            sfxSource.pitch = 1.0f;
        }

        // 3. 播放音效
        sfxSource.PlayOneShot(clipToPlay);
    }

    private AudioClip GetRandomClipWithoutImmediateRepeat(SFXType type, List<AudioClip> clips)
    {
        if (clips.Count == 1) return clips[0];

        int lastIndex = lastPlayedIndices[type];
        int newIndex;

        // 若有多个音频片段，随机抽取时跳过上次播放的那个
        do
        {
            newIndex = Random.Range(0, clips.Count);
        } while (newIndex == lastIndex);

        lastPlayedIndices[type] = newIndex;
        return clips[newIndex];
    }
    public void PlaySFX(AudioClip clip)
    {
        if (clip != null) sfxSource.PlayOneShot(clip);
    }
    public void OpenBook()
    {
        PlaySFX(SFXType.OpenBook);
    }
    public void PlayInGameBGM()
    {
        DayPhase currentPhase = TimeManager.Instance != null ? TimeManager.Instance.IsDay ? DayPhase.Day : DayPhase.Night : DayPhase.Day;
        PlayBGM(currentPhase == DayPhase.Day ? BGMType.Day : BGMType.Night);
    }
    // 事件响应
    private void HandlePhaseChanged(DayPhase phase)
    {
        PlayBGM(phase == DayPhase.Day ? BGMType.Day : BGMType.Night);
    }
    private void HandleToolModeChanged(ToolMode mode)
    {
        PlaySFX(SFXType.ToolSwitch);
    }
    private void HandlePlantFlowering(UI_PlotSlot slot)
    {
        PlaySFX(SFXType.Flowering);
    }
    private void HandleSpeciesPerfected(EventBus.OnSpeciesPerfectedEvent evt)
    {
        PlaySFX(SFXType.PerfectAchieved);
    }
}
