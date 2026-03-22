using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class TestUIEvent : MonoBehaviour
{
    #region ObjectBinder Auto Generated

    private ObjectBinder binder;
    public Slider Slider_Slider { get; private set; }
    public Button Button_Button { get; private set; }
    public Toggle Toggle { get; private set; }
    public TestMonobehaviour TestMonobehaviour { get; private set; }

    public void InitBind()
    {
        binder = GetComponent<ObjectBinder>();
        Slider_Slider = binder.Get<Slider>(nameof(Slider_Slider));
        Button_Button = binder.Get<Button>(nameof(Button_Button));
        Toggle = binder.Get<Toggle>(nameof(Toggle));
        TestMonobehaviour = binder.Get<TestMonobehaviour>(nameof(TestMonobehaviour));

        ObjectBinderUtility.SetSlider(Slider_Slider,Slider_SliderOnSlider);
        ObjectBinderUtility.SetButton(Button_Button,Button_ButtonOnClick);
        ObjectBinderUtility.SetToggle(Toggle,ToggleOnToggle);
    }

    private void Slider_SliderOnSlider(float value)
    {
        float max = 10;
        TestMonobehaviour.speed = max * value;
    }

    private void Button_ButtonOnClick()
    {
        Slider_Slider.value = .5f;
    }

    private void ToggleOnToggle(bool isOn)
    {
        TestMonobehaviour.speed = isOn ? Slider_Slider.value * 8 : 0;
    }


    #endregion ObjectBinder Auto Generated

    void Start()
    {
        InitBind();
    }
}
