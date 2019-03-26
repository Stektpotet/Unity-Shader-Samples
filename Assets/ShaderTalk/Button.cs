using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
public class Button : MonoBehaviour
{
    public KeyCode interactKey = KeyCode.E;
    public bool _isPressed;
    public Color pressedColor;
    public Color unpressedColor;

    public UnityEvent onPress;
    public UnityEvent onUnpress;

    MeshRenderer meshRenderer;
    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
    }
    public void Update()
    {
        if (Input.GetKeyDown(interactKey))
            Interact();
    }
    public void Interact()
    {
        if (_isPressed)
            Unpress();
        else
            Press();
    }

    private void Press()
    {
        //Realistically you might want to do this differently for performance reasons
        meshRenderer.sharedMaterial.SetColor("_MyColor", pressedColor);
        onPress.Invoke();
        _isPressed = true;
    }
    private void Unpress()
    {

        meshRenderer.sharedMaterial.SetColor("_MyColor", unpressedColor);
        onUnpress.Invoke();
        _isPressed = false;
    }
}
