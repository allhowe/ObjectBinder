using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestUniversal
{
    #region ObjectBinder Auto Generated

    private ObjectBinder binder;
    public MeshRenderer MeshRenderer_Cube { get; private set; }

    public void InitBind(GameObject target)
    {
        binder = target.GetComponent<ObjectBinder>();
        MeshRenderer_Cube = binder.Get<MeshRenderer>(nameof(MeshRenderer_Cube));
    }

    #endregion ObjectBinder Auto Generated


}
