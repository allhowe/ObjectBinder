using UnityEngine;
using ObjectBinderEditor;

public class StartWith_NamingRule : INamingRule
{
    // 命名规则显示名称
    public string Name => "StartWith_";

    // 自定义命名逻辑
    public string Naming(Object target)
    {
        if (target == null)
        {
            return "Unnamed";
        }
        return $"_{target.name}";

    }
}