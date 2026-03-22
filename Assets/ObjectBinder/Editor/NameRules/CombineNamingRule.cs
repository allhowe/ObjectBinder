using UnityEngine;

namespace ObjectBinderEditor
{
    public class CombineNamingRule : INamingRule
    {
        public string Name => "CombineNaming";
        public string Naming(Object target)
        {
            if (target == null)
            {
                return "Unnamed";
            }
            return $"{target.GetType().Name}_{target.name}";
        }
    }
}