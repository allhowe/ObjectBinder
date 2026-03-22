# ObjectBinder

ObjectBinder 是一个用于 Unity 的通用对象绑定工具，方便在 Inspector 面板中以字符串键值对的方式管理和获取各种 Unity 对象引用。

## 特性

- 通过字符串名称绑定和获取任意 UnityEngine.Object 派生对象。
- 支持可视化编辑，Inspector，Hierarchy面板友好。
- 支持代码生成，减少手动维护绑定代码的工作量。


## 用法

1. 将 `ObjectBinder` 组件挂载到任意 GameObject 上。
2. 在 Inspector 面板的 `Items` 列表中添加条目(支持拖拽快速绑定)，设置 `target`和选择命名方式`name`。
3. 点击`Generate Code`按钮生成代码(默认代码生成器中，实现了更新绑定组件同时不替换绑定事件逻辑功能）。

### 生成示例

```csharp
public class MyScript : MonoBehaviour
{	
    #region ObjectBinder Auto Generated

    private ObjectBinder binder;
    public Button Button_Button { get; private set; }

    public void InitBind()
    {
        binder = GetComponent<ObjectBinder>();
        Button_Button = binder.Get<Button>(nameof(Button_Button));

        ObjectBinderUtility.SetButton(Button_Button,Button_ButtonOnClick);
    }

    private void Button_ButtonOnClick()
    {
        //可保留绑定事件逻辑，更新绑定组件时不替换事件逻辑
        Debug.Log("clicked");
    }

    #endregion ObjectBinder Auto Generated

    void Start()
    {
        InitBind();
    }
}

```

全局绑定事件

```csharp
ObjectBinderUtility.onBinded += (target) => {
    if (target is Button)
    {
        //全局绑定事件，所有绑定的Button都会调用这个事件
        //额外调用统计等其他逻辑
    }
};

```



### 扩展开发

可以通过实现`IBuildRule`，`INamingRule` 和 `IEventRule` 接口来自定义规则，以满足特定项目的需求。

脚本需放在 `Editor` 文件夹下，编译后会自动被 ObjectBinder查找使用。