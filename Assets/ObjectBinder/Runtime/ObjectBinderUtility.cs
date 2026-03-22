using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public static class ObjectBinderUtility
{
    public static Action<UnityEngine.Object> onBinded;

    public static void SetButton(Button btn, UnityAction onClick)
    {
        btn.onClick.AddListener(onClick);
        onBinded?.Invoke(btn);
    }

    public static void SetToggle(Toggle tog, UnityAction<bool> onClick)
    {
        tog.onValueChanged.AddListener(onClick);
        onBinded?.Invoke(tog);
    }

    public static void SetSlider(Slider slider, UnityAction<float> onClick)
    {
        slider.onValueChanged.AddListener(onClick);
        onBinded?.Invoke(slider);
    }
}
