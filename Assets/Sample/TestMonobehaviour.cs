using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestMonobehaviour : MonoBehaviour
{
    #region ObjectBinder Auto Generated

    private ObjectBinder binder;
    public GameObject Cube { get; private set; }
    public MeshRenderer MeshRenderer_Cube { get; private set; }
    public Transform Transform_UniversalBinder { get; private set; }
    public Material Material_White { get; private set; }
    public Material Material_Black { get; private set; }

    public void InitBind()
    {
        binder = GetComponent<ObjectBinder>();
        Cube = binder.Get<GameObject>(nameof(Cube));
        MeshRenderer_Cube = binder.Get<MeshRenderer>(nameof(MeshRenderer_Cube));
        Transform_UniversalBinder = binder.Get<Transform>(nameof(Transform_UniversalBinder));
        Material_White = binder.Get<Material>(nameof(Material_White));
        Material_Black = binder.Get<Material>(nameof(Material_Black));
    }

    #endregion ObjectBinder Auto Generated


    public TestUniversal universal;
    [HideInInspector]public float speed = 5;

    private void Start()
    {
        InitBind();

        universal = new TestUniversal();
        universal.InitBind(Transform_UniversalBinder.gameObject);
    }


    void Update()
    {
        var start = transform.position;
        var end = Transform_UniversalBinder.position;

        var t = (Cube.transform.position.x - start.x) / (end.x - start.x);

        var color = Color.Lerp(Material_White.color, Material_Black.color, t);

        MeshRenderer_Cube.material.color = color;


        var dir = speed * Time.deltaTime * Vector3.right;

        Cube.transform.Translate(dir);

        if (end.x- Cube.transform.position.x<.5f)
        {
            Cube.transform.position = start;
            universal.MeshRenderer_Cube.material.color = Material_Black.color;
            Invoke("Hide", .1f);
        }
    }

    void Hide()
    {
        universal.MeshRenderer_Cube.material.color = Material_White.color;
    }
}
