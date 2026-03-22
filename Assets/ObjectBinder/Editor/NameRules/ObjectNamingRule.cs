using UnityEngine;

namespace ObjectBinderEditor
{
    public class ObjectNamingRule : INamingRule
    {
        public string Name => "ObjectNaming";
        public string Naming(Object target)
        {
            if (target == null)
            {
                return "Unnamed";
            }
            return target.name;
        }
    }
}