using ObjectBinderEditor;
using UnityEngine;

public class MyBuildRule : IBuildRule
{
    public int Priority => 100000;

    public TextAsset Bind(GameObject target)
    {
        // 自定义绑定逻辑，通过分析 GameObject 的结构查找用于代码生成的TextAsset。
        return null;
    }

    public void Build(ObjectBinder binder)
    {
        // 自定义构建逻辑，根据 ObjectBinder 中的数据生成代码。
        Debug.Log("CustomBuild");
    }

    public bool IsValid(TextAsset asset)
    {
        // 自定义验证逻辑，判断TextAsset 是否符合这个规则。
        return true;
    }
}
