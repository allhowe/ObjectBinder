using UnityEngine;

namespace ObjectBinderEditor
{
    public interface INamingRule
    {
        string Name { get; }
        string Naming(Object target);
    }


}