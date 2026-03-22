using UnityEngine;

namespace ObjectBinderEditor
{
    public class CustomNamingRule : INamingRule
    {
        public string Name => "Custom";
        public string Naming(Object target)
        {
            return "";
        }
    }

}