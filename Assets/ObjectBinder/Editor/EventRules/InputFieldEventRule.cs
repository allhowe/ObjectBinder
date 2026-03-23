using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ObjectBinderEditor
{
    public class InputFieldEventRule : IEventRule
    {
        public bool IsMatch(ObjectBinder.Item item) => item.target is InputField;

        public string GenerateEventMethodSignature(ObjectBinder.Item item) => $"private void {item.name}OnInput(string value)";

        public string GenerateEventCode(ObjectBinder.Item item) =>
            $"{nameof(ObjectBinderUtility)}.{nameof(ObjectBinderUtility.SetInputField)}({item.name},{item.name}OnInput);";
    }

}