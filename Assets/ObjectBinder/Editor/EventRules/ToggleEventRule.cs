using System;
using UnityEngine.UI;

namespace ObjectBinderEditor
{
    public class ToggleEventRule : IEventRule
    {
        public bool IsMatch(ObjectBinder.Item item) => item.target is Toggle;

        public string GenerateEventMethodSignature(ObjectBinder.Item item) => $"private void {item.name}OnToggle(bool isOn)";

        public string GenerateEventCode(ObjectBinder.Item item) =>
            $"{nameof(ObjectBinderUtility)}.{nameof(ObjectBinderUtility.SetToggle)}({item.name},{item.name}OnToggle);";
    }

}