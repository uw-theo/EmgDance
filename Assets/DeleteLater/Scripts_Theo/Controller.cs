using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class Controller : MonoBehaviour
{
    // private SpriteRenderer theSR;
    // public Sprite defaultImage;
    // public Sprite pressedImage;

    // public KeyCode keyToPress;
    // public GameObject selfObject;
    // public XRNode inputSource;
    // public UnityEngine.XR.Interaction.Toolkit.InputHelpers.Button inputButton;
    // public float inputThreshold = 0.1f;

    // Start is called before the first frame update
    void Start()
    {
        // theSR = GetComponent<SpriteRenderer>();    
    }

    // Update is called once per frame
    void Update()
    {
        // UnityEngine.XR.Interaction.Toolkit.InputHelpers.IsPressed(InputDevices.GetDeviceAtXRNode(inputSource), inputButton, out bool isPressed, inputThreshold);
        // if (isPressed)
        // {
        //     Debug.Log("Key is pressed down");
        // } else if (!isPressed)
        // {
        //     Debug.Log("key is lifted up");
        // } else
        // {
        //     Debug.Log("error: unknown key");
        // }
        // if (Input.GetKeyDown(keyToPress))
        // {
        //     // theSR.sprite = pressedImage;
        //     selfObject.transform.localScale = new Vector3(6.0f, 0.2f, 0.2f);
        //     // Debug.Log("Pressed key down");
        // }

        // if (Input.GetKeyUp(keyToPress))
        // {
        //     // theSR.sprite = defaultImage;
        //     selfObject.transform.localScale = new Vector3(5.0f, 0.1f, 0.1f);
        //     // Debug.Log("Let go of key");
        // }
    }
}
