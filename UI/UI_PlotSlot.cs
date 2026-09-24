using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public enum TileState{Empty,Growing,Flowering,Mature} 
public class UI_PlotSlot : MonoBehaviour{
    [Header("视觉组件")]
    public Image shadowImage;    // 根部阴影
    public Image plantImage;     // 植物/小芽图片
    public Slider progressBar; // 预留：头顶或地块内部绑定的进度条 UI

    [Header("网格坐标 (用于数据层匹配)")]
    public Vector2Int gridPos;
    
    [Header("当前土坑数据")]
    public SeedEntry? calculatedSeedEntry; // 存放杂交/自交后的种子基因等待收获（该种子基因可能会突变成别的植物，所以用SeedEntry）
    public int defaultCurrentMatchPriority = 99; // 存放自交的种子基因等待收获（该种子基因可能会突变成别的植物，所以用SeedEntry）
    public int currentMatchPriority = 99; // 记录当前种子的匹配优先级：0:左, 1:上, 2:右, 3:下, 4:自交(默认)
    public TileState currentState = TileState.Empty;    //初始化土地状态
    public PlantInstanceData currentPlantData;          //当前持有该植物的实例数据（含基因）

    [Header("播种时间")]    
    private float growthOffset; // 每个人独一无二的生长偏置（小时）
    public float currentGrowthTimer = 0f;    // 当前已生长累计秒数
    public float totalGrowthDuration = 0f;   // 该作物所需的总秒数
    public float speedBuffMultiplier = 1.0f; // 道具/环境额外加速倍率（默认 1.0）
    // 状态防重触发标记
    private bool isFloweringTriggered = false;
    // public SpeciesData currentSpecies;
    

    // 防止动画重叠播放的标记
    private Coroutine currentGrowthAnimRoutine;

    // 由 PlotGridManager 初始化时调用获取坐标信息
    public void Init(Vector2Int pos)
    {
        gridPos = pos;
        UpdateVisuals();
    }
    public void Update()
    {
        // 只有在生长中或开花中，才执行倒计时推演
        if (currentState == TileState.Growing || currentState == TileState.Flowering)
        {
            UpdateGrowth(Time.deltaTime);
        }
    }
    // 事件订阅与退订
    void OnEnable()
    {
        // EventBus.OnHourChanged += HandleHourlyGrowth;
    }

    void OnDisable()
    {
        // EventBus.OnHourChanged -= HandleHourlyGrowth;
    }
    /// <summary>
    /// 播种方法：接收的是一个包含基因信息的实例数据包
    /// </summary>
    /// public void Plant(PlantInstanceData plantData)
    public void Plant(PlantInstanceData plantData)
    {
        currentPlantData = plantData;

        // 初始化计时器
        // 利用随机数来产生种植成熟偏差
        int dnaHash = Random.Range(0, 1000);
        growthOffset = (dnaHash % 17) / 10f - 0.8f; 
        currentGrowthTimer = 0f + growthOffset; // 加上偏置时间（秒）
        totalGrowthDuration = currentPlantData.GetActualGrowthDuration();
        speedBuffMultiplier = 1.0f;
        isFloweringTriggered = false;
        currentState = TileState.Growing;
        UpdateVisuals(); // 刷新贴图
        PlayPlantSeedAnimation();
        Debug.Log($"播种成功：{currentPlantData.speciesTemplate.speciesName}，基因：{currentPlantData.dna}");
    }
    /// <summary>
    /// 平滑推进生长进度
    /// </summary>
    private void UpdateGrowth(float deltaTime)
    {
        if (totalGrowthDuration <= 0f) return;

        // 累加时间（支持外加加速倍率）
        currentGrowthTimer += deltaTime * speedBuffMultiplier;
        float progress = Mathf.Clamp01(currentGrowthTimer / totalGrowthDuration);

        // 1. 实时更新 UI 进度条（如果有绑定）
        if (progressBar != null)
        {
            progressBar.gameObject.SetActive(true);
            progressBar.value = progress;
        }

        // 2. 进度达到开花标准：进入开花期并触发突变/授粉
        float flowerStageRatio = currentPlantData.speciesTemplate.flowerStageRatio; 
        if (progress >= flowerStageRatio && !isFloweringTriggered)
        {
            isFloweringTriggered = true;
            
            // 触发开花过渡
            PlayStageTransitionAnimation(() =>
            {
                currentState = TileState.Flowering;
                // 通知管理器进行环境突变与邻居授粉检测
                TransitionTo(currentState);
            });
        }

        // 3. 进度达到 100%：完全成熟
        if (progress >= 1.0f && currentState != TileState.Mature)
        {
            currentState = TileState.Mature;
            isFloweringTriggered = false;
            TransitionTo(currentState);
        }
    
    }
    /// <summary>
    /// 道具加速接口（支持化肥、加速药水直接调用）
    /// </summary>
    /// <param name="instantSeconds">瞬间缩短的秒数</param>
    public void ApplyInstantGrowth(float instantSeconds)
    {
        if (currentState == TileState.Growing || currentState == TileState.Flowering)
        {
            UpdateGrowth(instantSeconds); // 模拟瞬间走过指定的秒数
        }
    }
    // 状态切换与授粉事件触发
    private void TransitionTo(TileState newState)
    {
        PlayStageTransitionAnimation(() =>
        {
            currentState = newState;
            UpdateVisuals(); // 在下蹲最扁的瞬间刷新贴图
        });

        if (newState == TileState.Flowering)
        {
            currentMatchPriority = defaultCurrentMatchPriority; // 每次开花重置优先级
            Debug.Log($"{currentPlantData.speciesTemplate.speciesName} 在 {gridPos} 发起授粉请求");
            
            // 触发开花授粉事件，把当前 UI_PlotSlot 传递给杂交计算器
            EventBus.TriggerPlantFlowering(this); 
        }
        else if (newState == TileState.Mature)
        {
            Debug.Log($"{currentPlantData.speciesTemplate.speciesName} 已成熟！");
        }
    }

    /// <summary>
    /// 特殊突变：将当前生长的植物当场蜕变为新物种（保留当前基因）
    /// </summary>
    public void MutateCurrentPlant(SpeciesData newSpecies)
    {
        if (currentPlantData == null || newSpecies == null) return;

        Debug.Log($"🌟 [物种蜕变] 位于 {gridPos} 的 {currentPlantData.speciesTemplate.speciesName} 沐浴月光，当场蜕变为 【{newSpecies.speciesName}】！");

        // 1. 保留原本的所有基因，但将物种模板彻底替换为新突变
        currentPlantData.speciesTemplate = newSpecies;

        // 2. 根据新物种重新计算后续生长所需的目标时间
        // 重新按照开花开始计算新品种所需生长时间
        float oldProgress = currentPlantData.speciesTemplate.flowerStageRatio;
        totalGrowthDuration = currentPlantData.GetActualGrowthDuration();
        currentGrowthTimer = totalGrowthDuration * oldProgress;
        // 3. 立即刷新贴图显示（换上云稻的开花/生长贴图）
        UpdateVisuals();
        
        // (可选) 可以在这里触发一个粒子特效或变身音效！
    }

    /// <summary>
    /// 收获方法（先判断收获果实还是种子）
    /// </summary>
    public void Harvest(bool isSeedMode)
    {
        //若植物没成熟直接返回
        if (currentState != TileState.Mature) return;
        EncyclopediaManager.Instance.RegisterDiscovery(currentPlantData); // 注册发现与极品检测
        //针对收获果实和种子的不同情况
        if (isSeedMode)
        {
            //取走算好的种子calculatedSeedEntry
            SeedEntry resultSeed = calculatedSeedEntry ?? new SeedEntry(currentPlantData.speciesTemplate, currentPlantData.dna);
            int count = currentPlantData.speciesTemplate.seedCount;
            InventoryManager.Instance.AddSeed(resultSeed.species, resultSeed.dna, count);
        }
        else
        {
            //取走果实。根据其基因产量决定数量
            int count = currentPlantData.GetActualHarvestQuantity();
            InventoryManager.Instance.AddFruit(currentPlantData.speciesTemplate, count);
        }
        //收获完成后需要将地块清空
        currentPlantData = null;
        calculatedSeedEntry = null;
        currentState = TileState.Empty;
        currentGrowthTimer = 0f;
        totalGrowthDuration = 0f;
        // 为进度条预留
        if (progressBar != null) progressBar.gameObject.SetActive(false);
        UpdateVisuals();
    }
    /// <summary>
    /// UI 视觉更新逻辑
    /// </summary>
    void UpdateVisuals()
    {
        // 默认置空
        if (currentState == TileState.Empty)
        {
            shadowImage.gameObject.SetActive(false);
            plantImage.gameObject.SetActive(false);
            return;
        }

        // 有植物：激活阴影与植物图片
        shadowImage.gameObject.SetActive(true);
        plantImage.gameObject.SetActive(true);
        // 根据当前植物的基因数据获取对应的物种模板
        var template = currentPlantData.speciesTemplate;
        
        // 优先读取 Sprite 贴图；若尚未制作贴图，则回退使用 template 配置的 Color 染色以防报错
        Sprite currentSprite = GetSpriteByState(template);
        if (currentSprite != null)
        {
            plantImage.sprite = currentSprite;
            plantImage.color = Color.white;

            // 根据当前 Sprite 在编辑器里设置的真实像素 Pivot，换算为 UI 的 (0~1) 归一化 Pivot
            Vector2 spritePivotNormalized = new Vector2(
            currentSprite.pivot.x / currentSprite.rect.width,
            currentSprite.pivot.y / currentSprite.rect.height
            );

        // 动态把当前 Image 的 Pivot 改为当前作物的根部，实现每种作物独立对齐！
        plantImage.rectTransform.pivot = spritePivotNormalized;
        
        // 确保把图片锚点 Pos 归零在土坑阴影中心
        plantImage.rectTransform.anchoredPosition = Vector2.zero;
            //plantImage.SetNativeSize();
        }
        else
        {
            // 色彩兼容备选方案
            switch (currentState)
            {
                case TileState.Growing: plantImage.color = template.growingColor; break;
                case TileState.Flowering: plantImage.color = template.flowerColor; break;
                case TileState.Mature: plantImage.color = template.matureColor; break;
            }
        }
    }

    /// <summary>
    /// 触发阶段切换动画（先压缩蓄力 -> 在中点切图/换数据 -> 爆发回弹）
    /// </summary>
    /// <param name="onSwitchAction">在中点执行的数据与贴图刷新</param>
    public void PlayStageTransitionAnimation(System.Action onSwitchAction)
    {
        if (!gameObject.activeInHierarchy)
        {
            // 如果物体未激活，直接静默更新数据，不跑协程
            onSwitchAction?.Invoke();
            return;
        }

        if (currentGrowthAnimRoutine != null)
        {
            StopCoroutine(currentGrowthAnimRoutine);
        }

        currentGrowthAnimRoutine = StartCoroutine(StageTransitionRoutine(onSwitchAction));  
    }
    // 生长过渡动画协程：下蹲蓄力 -> 中点切图 -> 爆发回弹
    private IEnumerator StageTransitionRoutine(System.Action onSwitchAction)
    {
        Transform t = plantImage.transform;
        Vector3 defaultScale = new Vector3(0.6f, 1f, 1f);

        // 阶段 1：下蹲挤压蓄力 (变扁变宽，持续 0.1 秒)
        float elapsed = 0f;
        float duration1 = 0.1f;
        Vector3 squishScale = new Vector3(1f, 0.7f, 1f);

        while (elapsed < duration1)
        {
            elapsed += Time.deltaTime;
            t.localScale = Vector3.Lerp(defaultScale, squishScale, elapsed / duration1);
            yield return null;
        }

        // ----------------------------------------------------
        // 阶段 2：【中点执行】在最扁的瞬间更新贴图或物种数据
        // ----------------------------------------------------
        onSwitchAction?.Invoke();

        // 阶段 3：向上弹起伸长 (爆发拉长，持续 0.15 秒)
        elapsed = 0f;
        float duration2 = 0.15f;
        Vector3 stretchScale = new Vector3(0.85f, 1.05f, 1f);

        while (elapsed < duration2)
        {
            elapsed += Time.deltaTime;
            t.localScale = Vector3.Lerp(squishScale, stretchScale, elapsed / duration2);
            yield return null;
        }

        // 阶段 4：平滑阻尼回弹到标准大小 (持续 0.1 秒)
        elapsed = 0f;
        float duration3 = 0.1f;

        while (elapsed < duration3)
        {
            elapsed += Time.deltaTime;
            t.localScale = Vector3.Lerp(stretchScale, defaultScale, elapsed / duration3);
            yield return null;
        }

        t.localScale = defaultScale;
        currentGrowthAnimRoutine = null;   
    }
    // 播种动画
    public void PlayPlantSeedAnimation()
    {
        StartCoroutine(PlantSeedRoutine());
    }

    // 协程：播种时的弹性生长动画（从 0 缩放到 1.2 再回弹到 1.0）
    private IEnumerator PlantSeedRoutine()
    {
        Transform t = plantImage.transform;
        t.localScale = Vector3.zero; // 从 0 开始

        float elapsed = 0f;
        float duration = 0.25f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
            // 使用弹性曲线：从小瞬间弹大到 1.2，再缩回 1.0
            float curveScale = Mathf.Sin(progress * Mathf.PI * 0.75f) * 1.2f;
            t.localScale = new Vector3(curveScale * 0.9f, curveScale, 1f);
            yield return null;
        }

        t.localScale = new Vector3(0.6f, 1f, 1f); // 确保最终回到标准大小
    }

    // 根据当前状态获取对应的 Sprite 贴图
    private Sprite GetSpriteByState(SpeciesData template)
    {
        if (template == null) return null;
        switch (currentState)
        {
            case TileState.Growing: return template.growingSprite;   // 小芽/幼苗图
            case TileState.Flowering: return template.flowerSprite; // 开花图
            case TileState.Mature: return template.matureSprite;   // 成熟/结果图
            default: return null;
        }
    }
}
