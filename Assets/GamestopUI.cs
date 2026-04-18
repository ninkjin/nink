using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GamestopUI : CloseParentWindow
{
    public GameObject targetWindow;
    public GameObject gameprocess;
    public Button btnContinue;   // 继续游戏按钮
    public GameObject escUI;
    
    public Button btnSetting;    // 调节音量
    public Button btnQuit;       // 退出按钮

    // 新增：音量滑块变量（在Inspector中拖拽绑定）
    public Slider sliderBGM;     // BGM音量滑块
    public Slider sliderSFX;     // 音效音量滑块

    void Start()
    {
        // 给每个按钮绑定事件（先判空，避免空引用报错）
        if (btnContinue != null)
        {
            btnContinue.onClick.AddListener(OnContinueClick);
        }
        if (btnSetting != null)
        {
            btnSetting.onClick.AddListener(SoundAdjust);
        }
        if (btnQuit != null)
        {
            btnQuit.onClick.AddListener(CloseTargetWindow);
        }

        // 新增：绑定滑块监听事件 + 初始化滑块值
        InitVolumeSliders();
    }

    void Update()
    {
        // 检测ESC键的按下（GetKeyDown只在按下瞬间触发一次）
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleEscUI();
        }
    }

    // 新增：初始化音量滑块（绑定事件 + 同步初始音量）
    private void InitVolumeSliders()
    {
        // 确保AudioManager单例存在
        if (AudioManager.Instance == null)
        {
            Debug.LogError("AudioManager单例未找到！请检查是否挂载并初始化");
            return;
        }

        // 初始化BGM滑块
        if (sliderBGM != null)
        {
            // 同步滑块初始值为AudioManager的BGM音量
            sliderBGM.value = AudioManager.Instance.bgmVolume;
            // 绑定滑块值改变事件（实时调节音量）
            sliderBGM.onValueChanged.AddListener(AdjustBGMVolume);
        }

        // 初始化音效滑块
        if (sliderSFX != null)
        {
            // 同步滑块初始值为AudioManager的音效音量
            sliderSFX.value = AudioManager.Instance.sfxVolume;
            // 绑定滑块值改变事件（实时调节音量）
            sliderSFX.onValueChanged.AddListener(AdjustSFXVolume);
        }
    }

    // 核心：调节BGM音量（Slider回调）
    public void AdjustBGMVolume(float volume)
    {
        if (AudioManager.Instance != null)
        {
            // 修改AudioManager的BGM音量
            AudioManager.Instance.bgmVolume = volume;
            // 同步到音频源（立即生效）
            AudioManager.Instance.bgmAudioSource.volume = volume;
            
        }
    }

    // 核心：调节音效音量（Slider回调）
    public void AdjustSFXVolume(float volume)
    {
        if (AudioManager.Instance != null)
        {
            // 修改AudioManager的音效音量
            AudioManager.Instance.sfxVolume = volume;
            // 同步到音频源（立即生效）
            AudioManager.Instance.sfxAudioSource.volume = volume;
            
        }
    }


    void SoundAdjust()
    {
        // 1. 获取所有层级的子物体（包含隐藏的）
        Transform[] allChildren = GetComponentsInChildren<Transform>(includeInactive: true);

        // 2. 遍历找到第一个隐藏的子物体（排除自身）
        Transform firstHiddenChild = null;
        foreach (Transform child in allChildren)
        {
            // 排除脚本自身的Transform，只找子物体
            if (child != transform && !child.gameObject.activeSelf)
            {
                firstHiddenChild = child;
                break;
            }
        }

        // 3. 显示找到的子物体
        if (firstHiddenChild != null)
        {
            firstHiddenChild.gameObject.SetActive(true);
            
        }
        
    }

    void ToggleEscUI()
    {
        if (escUI != null)
        {
            // 反转UI的激活状态
            bool isActive = !escUI.activeSelf;
            escUI.SetActive(isActive);

            
            if (isActive)
            {
                
               
                
                Time.timeScale = 0; // 时间缩放为0，游戏暂停
            }
            else
            {
                // 关闭UI时：隐藏鼠标（可选）、解锁鼠标、恢复游戏时间
                
                Time.timeScale = 1; // 恢复正常时间流速
            }
        }
        else
        {
            Debug.LogError("请在Inspector面板中指定ESC UI面板！");
        }
    }

    // 继续游戏逻辑
    void OnContinueClick()
    {
        targetWindow.SetActive(false);
        Time.timeScale = 1;
        
    }

    // 提供给UI按钮的关闭方法（比如“继续游戏”按钮）
    public void CloseEscUI()
    {
        
    }
    public override void CloseTargetWindow()
    {
        // 完全复制父类的逻辑，只删掉countdown.StartCountdown()这一行
        Transform targetWindow = FindParentWindow(transform, targetWindowName);

        if (targetWindow != null)
        {
            targetWindow.gameObject.SetActive(false);

            OpenAnotherUI();
            countdown.ResetCountTime();

            gameprocess.SetActive(false);
            Time.timeScale = 1; // 恢复正常时间流速
        }
        else
        {
            // 关闭窗口（保留父类逻辑）
            
            
        }
    }
}
