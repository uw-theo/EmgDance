// #define SIMULATE_MYO

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices; // only for debugging
using UnityEngine;
using UnityEngine.UI;
using Thalmic.Myo;
using Unity.VRTemplate;


#if !SIMULATE_MYO
using LockingPolicy = Thalmic.Myo.LockingPolicy;
using Pose = Thalmic.Myo.Pose;
using Arm = Thalmic.Myo.Arm;
using UnlockType = Thalmic.Myo.UnlockType;
using VibrationType = Thalmic.Myo.VibrationType;
#endif

public class NoteObject : MonoBehaviour
{
#if !SIMULATE_MYO
    public ThalmicMyo leftThalmicMyo;
    public ThalmicMyo rightThalmicMyo;
    private Pose _leftLastPose = Pose.Unknown;
    private Pose _rightLastPose = Pose.Unknown;
    private KeyCode leftMyoToKeyCode;
    private KeyCode rightMyoToKeyCode;
#endif
    public bool canBePressed;
    public KeyCode keyToPress;
    public float beatTempo;
    public GameObject hitEffect;
    public GameObject goodEffect;
    public GameObject perfectEffect;
    public GameObject missEffect;
    public Transform parentTransform;
    public GameObject NoteColliderObject;
    public float noteTravelTime;
    private float instantiatedTime;

#if !SIMULATE_MYO // disable when not using actual Myo hardware
    // dorsiflexion = up arrow, toes towards shin
    // plantarflexion = down arrow, toes away from shin
    private KeyCode MyoHandleLeftGesture(ThalmicMyo thalmicMyo) 
    {
        KeyCode myoToKeyCode = KeyCode.None;
        if (thalmicMyo.pose == Pose.WaveIn) // dorsiflexion: move toes towards body
        {
            //Debug.Log("Wave In!");
            myoToKeyCode = KeyCode.UpArrow;
            //txt.text = "WaveIn: Dorsiflexion, toes up";
            //Player.transform.position = Player.transform.position + new Vector3(movementSpeed * Time.deltaTime, 0, 0);
        } else if (thalmicMyo.pose == Pose.WaveOut) // plantarflexion: move toes away from body
        {
            //Debug.Log("Wave Out!");
            myoToKeyCode = KeyCode.DownArrow;
            //txt.text = "WaveOut: Plantarflexion, toes down";
            //Player.transform.position = Player.transform.position + new Vector3(-1*movementSpeed * Time.deltaTime, 0, 0);
        } else{
            // Debug.Log("Error: command not recognized!");
        }

        return myoToKeyCode;
    }

    // dorsiflexion = left arrow, toes towards shin
    // plantarflexion = right arrow, toes away from shin
    private KeyCode MyoHandleRightGesture(ThalmicMyo thalmicMyo)
    {
        KeyCode myoToKeyCode = KeyCode.None;
        if (thalmicMyo.pose == Pose.WaveIn) // dorsiflexion: move toes towards body
        {
            //Debug.Log("Wave In!");
            myoToKeyCode = KeyCode.LeftArrow;
        } else if (thalmicMyo.pose == Pose.WaveOut) // plantarflexion: move toes away from body
        {
            //Debug.Log("Wave Out!");
            myoToKeyCode = KeyCode.RightArrow;
        } else{
            //Debug.Log("Error: command not recognized!");
        }

        return myoToKeyCode;
    }
#endif

    // Start is called before the first frame update
    void Start()
    {
        NoteColliderObject = GameObject.Find("NoteCollider");
        if (NoteColliderObject == null)
        {
            Debug.Log("NoteColliderObject is null");
        }

        GameObject parentObject = GameObject.Find("NoteSpawner");
        if (parentObject == null)
        {
            Debug.Log("parentObject is null");
        }
        parentTransform = parentObject.transform;

        GameObject gameManagerObject = GameObject.Find("GameManager");
        if (gameManagerObject == null)
        {
            Debug.Log("gameManagerObject is null");
        }

        GameManager gmanager = gameManagerObject.GetComponent<GameManager>();
        if (gmanager == null)
        {
            Debug.Log("gmanager is null");
        }
        noteTravelTime = gmanager.GetNoteTravelTime();

        SongReader sread = gameManagerObject.GetComponent<SongReader>();
        if (sread == null)
        {
            Debug.Log("sread is null");
        }
        beatTempo = sread.GetSongBPM() / 60.0f; // gives us how fast we move per second for 120bpm
        
        SetParent();
        instantiatedTime = Time.time;

#if !SIMULATE_MYO
        GameObject myoHubObject = GameObject.Find("Hub - 1 Myo");
        if (myoHubObject == null)
        {
            Debug.Log("myoHubObject is null");
        }
        myoHubObject.SetActive(true);

        List<ThalmicMyo> _myos = new List<ThalmicMyo>();
        GameObject firstMyoGameObject  = GameObject.Find("FirstMyo");
        ThalmicMyo firstMyo = firstMyoGameObject.GetComponent<ThalmicMyo>();
        _myos.Add(firstMyo);
        GameObject secondMyoGameObject = GameObject.Find("SecondMyo");
        ThalmicMyo secondMyo = secondMyoGameObject.GetComponent<ThalmicMyo>();
        _myos.Add(secondMyo);

        if (_myos[0].firstFirmware.z == 1931)
        {
            // Debug.Log("myo[0] is the left");
            leftThalmicMyo = _myos[0];
            leftThalmicMyo.arm = Arm.Left;
            rightThalmicMyo = _myos[1];
            rightThalmicMyo.arm = Arm.Right;
        } else if (_myos[0].firstFirmware.z == 1970)
        {
            // Debug.Log("myo[0] is the right");
            rightThalmicMyo = _myos[0];
            rightThalmicMyo.arm = Arm.Right;
            leftThalmicMyo = _myos[1];
            leftThalmicMyo.arm = Arm.Left;
        } else{
            Debug.Log("NoteObject invalid arms");
        }
        // Debug.Log("myo[0] z firmware ver: " + _myos[0].firstFirmware.Z.ToString() + ", arm: " + _myos[0].arm.ToString());
        // Debug.Log("myo[1] z firmware ver: " + _myos[1].secondFirmware.Z.ToString() + ", arm: " + _myos[1].arm.ToString());

        // GameObject xrDeviceSimObject = GameObject.Find("XR Device Simulator");
        // xrDeviceSimObject.SetActive(true);
#endif
    }

    // Update is called once per frame
    void Update()
    {  
        // UnityEngine.Vector3 currPos = new UnityEngine.Vector3();
        // currPos = transform.position;
        if(canBePressed)
        {
#if !SIMULATE_MYO // disable when not using actual Myo hardware
            // commands
            // Pose.Fist
            // Pose.WaveIn
            // Pose.WaveOut
            // Pose.DoubleTap
            // thalmicMyo.Vibrate (VibrationType.Medium)
            // Foot flex:
            // Dorsiflexion: toes going up
            // Plantarflexion: toes going down
            // Inversion: inside of side foot going up and outside of side foot going down
            // Eversion: inside of side foot going down and outside of side foot going up
            if (_leftLastPose != leftThalmicMyo.pose)
            {
                leftMyoToKeyCode = MyoHandleLeftGesture(leftThalmicMyo);
            }
            if (_rightLastPose != rightThalmicMyo.pose)
            {
                rightMyoToKeyCode = MyoHandleRightGesture(rightThalmicMyo);
            }

            if ((keyToPress == leftMyoToKeyCode) || (keyToPress == rightMyoToKeyCode))
#else
            if (Input.GetKeyDown(keyToPress))
#endif
            {
                gameObject.SetActive(false);
                
                // GameManager.instance.NoteHit();
                double distance = UnityEngine.Vector3.Distance(transform.position, NoteColliderObject.transform.position);
                UnityEngine.Vector3 adjustParticalEffect = new UnityEngine.Vector3(0f, 1f, 0f);
                UnityEngine.Vector3 particalPosition = transform.position + adjustParticalEffect;
                // Debug.Log("transformPos: " + transform.position + " colliderPos: " + NoteColliderObject.transform.position);
                // Debug.Log("NoteObject Update()  - distance: " + distance + " keyToPress: " + keyToPress);
                GameManager.instance.NoteHit(keyToPress);
                Instantiate(hitEffect, particalPosition, hitEffect.transform.rotation);
            }
        }


        // if its a missed object, destroy once it goes past user a certain distance
        if (transform.position.z < -15.0f)
        {
            Destroy(gameObject);
        } else
        {
            // move the notes towards the hit bar
            // DOES NOT WORK
            // transfom.position diretcly doesn't work since it only goes horizontally
            //transform.position -= new UnityEngine.Vector3(0.0f, 0.0f, (beatTempo * Time.deltaTime));
            
            // mathematical way to move it via triangle math
            // THIS ONE WORKS, for Goin' Under song only though
            double yMovement = (beatTempo*Time.deltaTime)*Math.Sin(noteTravelTime*Math.PI/180.0);
            double zMovement = (beatTempo*Time.deltaTime)*Math.Cos(noteTravelTime*Math.PI/180.0);
            transform.position -= new UnityEngine.Vector3(0.0f, (float)yMovement, (float)zMovement);

            // mathematical way to move it along the line via y=mx+b math
            // DOES NOT WORK, getting closer though...
            // float distanceTraveled = noteTravelTime*(Time.time - instantiatedTime);
            // float slope = 11.52f;
            // float deltaY = distanceTraveled/Mathf.Sqrt(1+slope*slope);
            // float deltaZ = (slope * distanceTraveled) / Mathf.Sqrt(1+slope*slope);
            // float newY = parentTransform.position.y - deltaY;
            // float newZ = parentTransform.position.z - deltaZ;
            // transform.position = new UnityEngine.Vector3(0.0f, newY, newZ);

            // Trying to use LERP
            // This one does not work
            // if (parentTransform!= null && NoteColliderObject.transform != null)
            // {
            //     // Increment lerpTime based on Time.deltaTime
            //     lerpTime += Time.deltaTime;
                
            //     // Calculate percentage of lerpTime in relation to lerpDuration
            //     float t = Mathf.Clamp01(lerpTime / 5.0f);

            //     // Interpolate between object1 and object2 positions
            //     UnityEngine.Vector3 newPosition = UnityEngine.Vector3.Lerp(parentTransform.position, NoteColliderObject.transform.position, t);
            //     newPosition.x = 0;

            //     // Assign the new position to this GameObject's transform
            //     transform.position = newPosition;
            // }
        }
         
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Activator")
        {
            canBePressed = true;
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Activator")
        {
            canBePressed = false;
            GameManager.instance.NoteMissed(keyToPress);
            UnityEngine.Vector3 adjustParticalEffect = new UnityEngine.Vector3(0f, 1f, 0f);
            UnityEngine.Vector3 particalPosition = transform.position + adjustParticalEffect;
            Instantiate(missEffect, particalPosition, missEffect.transform.rotation);
        }
    }

    private void SetParent()
    {
        transform.SetParent(parentTransform);
        // transform.SetParent(parentTransform, false);
    }
}
