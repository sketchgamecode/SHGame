# **《武松：血溅鸳鸯楼》技术设计文档 (TDD v1.0)**

## **一、 开发环境**

- **引擎:** Unity 6000 (应使用受支持的LTS版本，如 2022.3.x LTS)
- **渲染管线:** Universal RP (URP) 2D
- **IDE:** Visual Studio 2022
- **版本控制:** Git (强烈推荐)
- **目标平台:** PC Standalone

## **二、 项目设置与目录结构**

### **1. 初始设置**

1. 创建新的3D项目（稍后启用2D功能）。
2. 通过Package Manager安装 `Universal RP` 包。
3. 在项目根目录创建 `Assets/Art/RenderPipeline` 文件夹。
4. 右键创建 `Universal Render Pipeline > Pipeline Asset (Forward Renderer)`。在Project Settings > Graphics 中将其设为默认。
5. 在 `Project Settings > Editor` 中，将 `Version Control` 模式设置为 `Visible Meta Files`，并将 `Asset Serialization` 模式设置为 `Force Text`。

### **2. 推荐目录结构 (`Assets/` 下)**

```
SHGame/
├── Assets/
│   ├── Scripts/
│   │   ├── Core/
│   │   ├── Characters/
│   │   │   ├── Player/
│   │   │   └── NPC/
│   │   ├── Gameplay/
│   │   ├── Interaction/
│   │   ├── UI/
│   │   └── Utilities/
│   ├── Resources/
│   │   ├── Sprites/
│   │   ├── Backgrounds/
│   │   ├── Lighting/
│   │   └── Prefabs/
│   ├── Audio/
│   ├── Scenes/
│   │   ├── Core/
│   │   └── Levels/
│   ├── Settings/
│   └── Prefabs/
```

**说明:** 使用 `[MyCompany]` 和 `[GameName]` 命名空间可以极大避免与其他插件发生冲突。

## **三、 核心系统设计与脚本规划**

### **1. 管理器 (Managers) - 使用单例模式**

- **`GameManager.cs`** (核心总控)
    - 职责: 游戏状态管理（如游戏开始、暂停、结束）、场景加载、全局事件分发。
    - 关键属性: `public static GameManager Instance;`
- **`UIManager.cs`** (UI管理)
    - 职责: 控制所有UI面板的显示与隐藏（如对话字幕、情报日志、交互提示、QTE提示）。
    - 关键方法: `ShowSubtitle(string text)`, `HideSubtitle()`, `ShowInteractionPrompt(string promptText)`, `HideInteractionPrompt()`.
- **`AudioManager.cs`** (音频管理)
    - 职责: 播放背景音乐、环境音和音效。
    - 关键方法: `PlaySFX(AudioClip clip)`, `PlayBGM(AudioClip clip)`.

### **2. 玩家角色 (Player)**

- **`PlayerController.cs`** (核心控制)
    - 职责: 处理玩家输入（移动、跳跃、交互）、控制动画状态机。
    - 继承自 `MonoBehaviour`。
    - 关键引用: `Rigidbody2D`, `BoxCollider2D`, `Animator`, `PlayerInteraction`.
- **`PlayerStealth.cs`** (隐匿系统)
    - 职责: 检测所处环境的光照强度，判断是否隐匿。
    - 关键方法: `bool CheckIfInShadow()`。通过 `Light2D` 的 `GetIntensity` 相关API实现。
    - 关键事件: `public static event Action<bool> OnStealthStatusChanged;` (当隐匿状态改变时触发，其他脚本可订阅)。
- **`PlayerInteraction.cs`** (交互系统)
    - 职责: 检测面前的可交互物体（使用 `Raycast` 或 `OverlapCircle`），处理交互输入。
    - 关键方法: `Interactable FindNearestInteractable()`, `void PerformInteraction()`.

### **3. NPC系统**

- **`NPC_Controller.cs`** (基类)
    - 职责: NPC的通用状态机基础。
    - 关键枚举: `public enum NPCState { Idle, Patrol, Investigate, Chase, Dead }`
    - 关键方法: `void ChangeState(NPCState newState)`.
- **`NPC_Guard.cs`** (派生类：守卫)
    - 职责: 实现巡逻、调查、追击玩家的具体逻辑。
    - 关键属性: `public Transform[] patrolPoints;`
- **`NPC_Scripted.cs`** (派生类：脚本化NPC，如后槽)
    - 职责: 执行预设的脚本序列（例如：走到A点 -> 等待 -> 走到B点 -> 播放动画）。
    - 关键方法: 使用 `IEnumerator` 协程编写序列，如 `IEnumerator Routine_AfterBedtime()`.

### **4. 交互系统**

- **`Interactable.cs`** (抽象基类)
    - 所有可交互物体的基类。包含一个 `public virtual void Interact()` 方法。
- **`Interactable_Door.cs`** (派生类：门)
    - 重写 `Interact()` 方法，播放开门动画/音效，并可能触发事件（如惊动后槽）。
- **`Interactable_Draggable.cs`** (派生类：可拖拽物体，如尸体)
    - 重写 `Interact()`，当玩家交互时，将物体设为玩家的子物体，并跟随玩家移动。再次交互放下。
- **`Interactable_Item.cs`__ (派生类：道具，如衣服、石头)
    - 重写 `Interact()`，将道具添加到玩家库存或直接使用。

### **5. 监听与触发器**

- **`ListenTrigger.cs`** (监听区域)
    - 职责: 当玩家进入`Trigger`并保持隐匿和静止时，触发对话。
    - 关键属性: `public string[] dialogueLines;` (在Inspector中填写台词)。
    - 逻辑: 在 `OnTriggerStay2D` 中检测玩家是否静止，然后调用 `UIManager.Instance.ShowSubtitle(...)`。

## **四、 关键实现代码示例 (伪代码/思路)**

### **1. 光影检测 (`PlayerStealth.cs`)**

```csharp
public class PlayerStealth : MonoBehaviour
{
    public Light2D globalLight; // 主光源（月光）
    public float hideThreshold = 0.1f;
    private bool isHidden = false;

    void Update()
    {
        float intensity = globalLight.GetIntensity(transform.position); // 需要自定义扩展方法或使用Light2D.color
        // 简化：如果URP无法直接获取某点强度，可在玩家头顶放一个小的LightSensor对象，检测照射它的光强
        bool newHideState = intensity < hideThreshold;

        if (newHideState != isHidden)
        {
            isHidden = newHideState;
            OnStealthStatusChanged?.Invoke(isHidden); // 通知其他系统
            // 例如：改变玩家Sprite颜色
            GetComponent<SpriteRenderer>().color = isHidden ? Color.gray : Color.white;
        }
    }
}
```

### **2. 简单状态机 (`NPC_Controller.cs`)**

```csharp
public class NPC_Controller : MonoBehaviour
{
    public NPCState currentState;

    protected virtual void Update()
    {
        switch (currentState)
        {
            case NPCState.Patrol:
                UpdatePatrolState();
                break;
            case NPCState.Investigate:
                UpdateInvestigateState();
                break;
            // ... 其他状态
        }
    }

    protected virtual void UpdatePatrolState()
    {
        // 巡逻逻辑
    }

    public void ChangeState(NPCState newState)
    {
        // 退出当前状态逻辑
        OnExitState(currentState);
        currentState = newState;
        // 进入新状态逻辑
        OnEnterState(newState);
    }

    protected virtual void OnEnterState(NPCState state) { }
    protected virtual void OnExitState(NPCState state) { }
}
```

## **五、 给"AI程序员"的指令**

1. **创建项目并设置URP。**
2. **按照目录结构创建文件夹。**
3. **从核心管理器开始编写脚本:** 先创建 `GameManager`, `UIManager` 的单例框架。
4. **实现玩家基础功能:** `PlayerController` (移动), `PlayerInteraction` (检测交互物)。
5. **实现隐匿系统:** `PlayerStealth`。这是项目的技术难点，优先攻克。
6. **实现NPC系统:** 先做基础的 `NPC_Controller` 状态机，再做 `NPC_Guard`。
7. **实现交互系统:** 创建 `Interactable` 基类，然后实现门、尸体等派生类。
8. **最后实现脚本化序列和QTE:** 这些是建立在基础系统之上的具体游戏逻辑。

**重要提示:** 在开发每个功能时，**优先考虑数据驱动**。将需要在Inspector中调整的参数（如移动速度、光照阈值、巡逻点）设为`public`变量，方便你后续调试和平衡。

---

## **六、 技术实现重点**

### **光影系统 (URP 2D Light System)**
- 使用`Light2D.GetIntensityAt()`方法检测武松位置光照强度
- 设置全局光（月光）和局部光源（灯笼、油灯）
- 阴影判定：光照强度低于阈值即为隐匿状态

### **有限状态机 (FSM) 架构**
- NPC AI采用简单状态机而非复杂行为树
- 状态包括：巡逻(Patrol)、调查(Investigate)、追击(Chase)、死亡(Dead)
- 脚本化序列使用协程(Coroutine)控制时间线事件

### **QTE系统**
- 激活UI提示按键进行快速时间事件
- 成功/失败播放不同动画序列
- 集成到处决和关键剧情节点

### **音频触发系统**
- 区域性`Audio Trigger`自动播放环境音
- 对话系统集成字幕显示
- 情报日志自动记录关键信息

---

*Last Updated: 2025 | Technical Review: After Core Systems Implementation*
