using System;
using UnityEngine.UI;

namespace ObjectBinderEditor
{
    public class SliderEventRule : IEventRule
    {
        public bool IsMatch(ObjectBinder.Item item) => item.target is Slider;

        public string GenerateEventMethodSignature(ObjectBinder.Item item) => $"private void {item.name}OnSlider(float value)";

        public string GenerateEventCode(ObjectBinder.Item item) =>
            $"{nameof(ObjectBinderUtility)}.{nameof(ObjectBinderUtility.SetSlider)}({item.name},{item.name}OnSlider);";
    }

}