using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;


public class Test : MonoBehaviour
{
    #region ObjectBinder Auto Generated

    private ObjectBinder binder;
    public Toggle Toggle { get; private set; }
    public Slider Slider { get; private set; }
    public InputField InputField { get; private set; }
    public Button Button { get; private set; }
    public Dropdown Dropdown { get; private set; }
    public Text Text { get; private set; }
    public RawImage Image { get; private set; }
    public ObjectBinder ObjectBinder_Items { get; private set; }

    public void InitBind()
    {
        binder = GetComponent<ObjectBinder>();
        Toggle = binder.Get<Toggle>(nameof(Toggle));
        Slider = binder.Get<Slider>(nameof(Slider));
        InputField = binder.Get<InputField>(nameof(InputField));
        Button = binder.Get<Button>(nameof(Button));
        Dropdown = binder.Get<Dropdown>(nameof(Dropdown));
        Text = binder.Get<Text>(nameof(Text));
        Image = binder.Get<RawImage>(nameof(Image));
        ObjectBinder_Items = binder.Get<ObjectBinder>(nameof(ObjectBinder_Items));

        ObjectBinderUtility.SetToggle(Toggle,ToggleOnToggle);
        ObjectBinderUtility.SetSlider(Slider,SliderOnSlider);
        ObjectBinderUtility.SetInputField(InputField,InputFieldOnInput);
        ObjectBinderUtility.SetButton(Button,ButtonOnClick);
        ObjectBinderUtility.SetDropdown(Dropdown,DropdownOnDrop);
    }

    private void ToggleOnToggle(bool isOn)
    {
        ObjectBinder_Items.gameObject.SetActive(isOn);
    }

    private void SliderOnSlider(float value)
    {
        Image.transform.localScale = Vector3.one * (value);
    }

    private void InputFieldOnInput(string value)
    {
        Text.text = value;
    }

    private void ButtonOnClick()
    {
        ChangeImage();
    }

    private void DropdownOnDrop(int value)
    {
        SetImage(value);
    }


    #endregion ObjectBinder Auto Generated

    void Start()
    {
        InitBind();


        var data = ObjectBinder_Items.items;

        var dropData = data.Select(p => new Dropdown.OptionData() { text = p.name }).ToList();

        Dropdown.options = dropData;

        SetImage(0);
    }


    private int index;
    public void ChangeImage()
    {
        if (index >= ObjectBinder_Items.items.Count)
            index = 0;

        SetImage(index++);
    }
    public void SetImage(int index)
    {
        if (index >= ObjectBinder_Items.items.Count)
            return;
        var image = ObjectBinder_Items.items[index].target as Texture2D;
        Image.texture = image;
    }
}
