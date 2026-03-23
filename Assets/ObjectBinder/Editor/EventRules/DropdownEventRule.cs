using UnityEngine.UI;

namespace ObjectBinderEditor
{
    public class DropdownEventRule : IEventRule
    {
        public bool IsMatch(ObjectBinder.Item item) => item.target is Dropdown;

        public string GenerateEventMethodSignature(ObjectBinder.Item item) => $"private void {item.name}OnDrop(int value)";

        public string GenerateEventCode(ObjectBinder.Item item) =>
            $"{nameof(ObjectBinderUtility)}.{nameof(ObjectBinderUtility.SetDropdown)}({item.name},{item.name}OnDrop);";
    }
}
