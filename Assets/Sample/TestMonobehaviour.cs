using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class TestMonobehaviour : MonoBehaviour
{
    #region ObjectBinder Auto Generated

    private ObjectBinder binder;
    public RawImage Image { get; private set; }
    public Text Text { get; private set; }
    public ObjectBinder ObjectBinder_TestMonobehaviour { get; private set; }

    public void InitBind()
    {
        binder = GetComponent<ObjectBinder>();
        Image = binder.Get<RawImage>(nameof(Image));
        Text = binder.Get<Text>(nameof(Text));
        ObjectBinder_TestMonobehaviour = binder.Get<ObjectBinder>(nameof(ObjectBinder_TestMonobehaviour));
    }

    #endregion ObjectBinder Auto Generated

    private void Start()
    {
        InitBind();

    }

    private int index;
    public void ChangeImage()
    {
        if (index >= ObjectBinder_TestMonobehaviour.items.Count)
            index = 0;

        var image = ObjectBinder_TestMonobehaviour.items[index++].target as Texture2D;
        Image.texture = image;
    }
}
