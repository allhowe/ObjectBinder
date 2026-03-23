using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class TestUIEvent : MonoBehaviour
{
    #region ObjectBinder Auto Generated

    private ObjectBinder binder;
    public Toggle Toggle { get; private set; }
    public Slider Slider { get; private set; }
    public InputField InputField { get; private set; }
    public Button Button { get; private set; }
    public TestMonobehaviour TestMonobehaviour { get; private set; }

    public void InitBind()
    {
        binder = GetComponent<ObjectBinder>();
        Toggle = binder.Get<Toggle>(nameof(Toggle));
        Slider = binder.Get<Slider>(nameof(Slider));
        InputField = binder.Get<InputField>(nameof(InputField));
        Button = binder.Get<Button>(nameof(Button));
        TestMonobehaviour = binder.Get<TestMonobehaviour>(nameof(TestMonobehaviour));

        ObjectBinderUtility.SetToggle(Toggle,ToggleOnToggle);
        ObjectBinderUtility.SetSlider(Slider,SliderOnSlider);
        ObjectBinderUtility.SetInputField(InputField,InputFieldOnInput);
        ObjectBinderUtility.SetButton(Button,ButtonOnClick);
    }

    private void ToggleOnToggle(bool isOn)
    {
        TestMonobehaviour.gameObject.SetActive(isOn);
    }

    private void SliderOnSlider(float value)
    {
        TestMonobehaviour.Image.transform.localScale = Vector3.one * (value);
    }

    private void InputFieldOnInput(string value)
    {
        TestMonobehaviour.Text.text = value;
    }

    private void ButtonOnClick()
    {
        TestMonobehaviour.ChangeImage();
    }


    #endregion ObjectBinder Auto Generated

    void Start()
    {
        InitBind();
    }
}
