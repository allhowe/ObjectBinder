using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public static class ObjectBinderUtility
{
    public static Action<UnityEngine.Object> onBinded;

    public static void SetButton(Button btn, UnityAction action)
    {
        btn.onClick.AddListener(action);
        onBinded?.Invoke(btn);
    }

    public static void SetToggle(Toggle tog, UnityAction<bool> action)
    {
        tog.onValueChanged.AddListener(action);
        onBinded?.Invoke(tog);
    }

    public static void SetSlider(Slider slider, UnityAction<float> action)
    {
        slider.onValueChanged.AddListener(action);
        onBinded?.Invoke(slider);
    }
    public static void SetInputField(InputField input, UnityAction<string> action)
    {
        input.onValueChanged.AddListener(action);
        onBinded?.Invoke(input);
    }
}
