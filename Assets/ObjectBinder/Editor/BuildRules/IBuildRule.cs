using UnityEngine;

namespace ObjectBinderEditor
{
    public interface IBuildRule
    {
        int Priority { get; }
        TextAsset Bind(GameObject target);
        bool Validate(TextAsset asset);
        void Build(ObjectBinder binder);
    }

}