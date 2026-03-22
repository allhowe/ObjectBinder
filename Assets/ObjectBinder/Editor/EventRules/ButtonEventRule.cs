using UnityEngine.UI;

namespace ObjectBinderEditor
{

    public class ButtonEventRule : IEventRule
    {
        public bool IsMatch(ObjectBinder.Item item) => item.target is Button;

        public string GenerateEventMethodSignature(ObjectBinder.Item item) => $"private void {item.name}OnClick()";

        public string GenerateEventCode(ObjectBinder.Item item) =>
            $"{nameof(ObjectBinderUtility)}.{nameof(ObjectBinderUtility.SetButton)}({item.name},{item.name}OnClick);";
    }
}