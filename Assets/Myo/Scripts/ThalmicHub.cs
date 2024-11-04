using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

using System.Collections.Generic;

using LockingPolicy = Thalmic.Myo.LockingPolicy;
using VibrationType = Thalmic.Myo.VibrationType;
using Unity.VisualScripting;

// Allows access to one or more Myo armbands, which must be immediate children of the GameObject this script is attached
// to. ThalmicHub is a singleton; only one ThalmicHub instance is allowed in a scene, which can be accessed through
// ThalmicHub.instance. ThalmicHub will persist across scene changes.
public class ThalmicHub : MonoBehaviour
{
    // The single instance of ThalmicHub. Set during Awake.
    public static ThalmicHub instance {
        get { return _instance; }
    }

    // Unique application identifier. applicationIdentifier must follow a reverse domain name format
    // (ex. com.domainname.appname). Application identifiers can be formed from the set of alphanumeric ASCII characters
    // (a-z, A-Z, 0-9). The hyphen (-) and underscore (_) characters are permitted if they are not adjacent to a period
    // (.) character  (i.e. not at the start or end of each segment), but are not permitted in the top-level domain.
    // Application identifiers must have three or more segments. For example, if a company's domain is example.com and
    // the application is named hello-world, one could use "com.example.hello-world" as a valid application identifier.
    // applicationIdentifier can an empty string.
    public string applicationIdentifier = "com.example.myo-unity";

    // If set to None, pose events are always sent. If set to Standard, pose events are not sent while a Myo is locked.
    public LockingPolicy lockingPolicy;

    // True if and only if the hub initialized successfully; typically this is set during Awake, but it can also be
    // set by calling ResetHub() explicitly. The typical reason for initialization to fail is that Myo Connect is not
    // running.
    public bool hubInitialized {
        get { return _hub != null; }
    }

    // Reset the hub. This function is typically used if initialization failed to attempt to initialize again (e.g.
    // after asking the user to ensure that Myo Connect is running).
    public bool ResetHub() {
        if (_hub != null) {
            _hub.Dispose ();
            _hub = null;

            foreach (ThalmicMyo myo in _myos) {
                myo.internalMyo = null;
            }
        }
#if UNITY_ANDROID && !UNITY_EDITOR
        AndroidJavaClass unityPlayerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        var unityActivity = unityPlayerClass.GetStatic<AndroidJavaObject>("currentActivity");

        // The Hub needs to be initialized on the Android UI thread.
        unityActivity.Call("runOnUiThread", new AndroidJavaRunnable(() => {
            createHub();
        }));
        return true; // Return true assuming the hub constructor will succeed. Debug.Log if it fails.
#else
        return createHub();
#endif
    }

    void Awake ()
    {
        // Ensure that there is only one ThalmicHub.
        if (_instance != null) {
#if UNITY_EDITOR
            EditorUtility.DisplayDialog("Can only have one ThalmicHub",
                                        "Your scene contains more than one ThalmicHub. Remove all but one ThalmicHub.",
                                        "OK");
#endif
            Destroy (this.gameObject);
            return;
        } else {
            _instance = this;
        }

        // Do not destroy this game object. This will ensure that it remains active even when
        // switching scenes.
        DontDestroyOnLoad(this);

        for (int i = 0; i < transform.childCount; ++i) {
            Transform child = transform.GetChild (i);

            var myo = child.gameObject.GetComponent<ThalmicMyo> ();
            if (myo != null) {
                _myos.Add(myo);
            }
        }

        if (_myos.Count < 1) {
            string errorMessage = "The ThalmicHub's GameObject must have at least one child with a ThalmicMyo component.";
#if UNITY_EDITOR
            EditorUtility.DisplayDialog ("Thalmic Hub has no Myo children", errorMessage, "OK");
#else
            throw new UnityException (errorMessage);
#endif
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        AndroidJavaClass unityPlayerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        var unityActivity = unityPlayerClass.GetStatic<AndroidJavaObject>("currentActivity");
        var applicationContext = unityActivity.Call<AndroidJavaObject>("getApplicationContext");

        // Need to pass the Android Application Context to the Myo Android plugin before initializing the Hub.
        AndroidJavaClass nativeEventsClass = new AndroidJavaClass("com.thalmic.myo.NativeEvents");
        nativeEventsClass.CallStatic("setApplicationContext", applicationContext);

        // The Hub needs to be initialized on the Android UI thread.
        unityActivity.Call("runOnUiThread", new AndroidJavaRunnable(() => {
            createHub ();
        }));
#else
        createHub ();
#endif
    }

    private bool createHub () {
        try {
            _hub = new Thalmic.Myo.Hub (applicationIdentifier, hub_MyoPaired);

            _hub.SetLockingPolicy (lockingPolicy);
        } catch (System.Exception) {
            Debug.Log ("ThalmicHub failed to initialize.");
            return false;
        }
        return true;
    }

    void OnApplicationQuit ()
    {
        if (_hub != null) {
            _hub.Dispose ();
            _hub = null;
        }
    }

    void Start()
    {
        Thalmic.Myo.Vector3 firstFirmwareVer = new Thalmic.Myo.Vector3(0f, 0f, 0f);
        Thalmic.Myo.Vector3 secondFirmwareVer = new Thalmic.Myo.Vector3(0f, 0f, 0f);
        firstFirmwareVer = _hub.GetFirstFirmwareVer();
        secondFirmwareVer = _hub.GetSecondFirmwareVer();

        Debug.Log("ThalmicHub.cs firstFirmware ver: " + 
            firstFirmwareVer.X + "." + firstFirmwareVer.Y + "." + firstFirmwareVer.Z);
        Debug.Log("ThalmicHub.cs secondFirmware ver: " + 
            secondFirmwareVer.X + "." + secondFirmwareVer.Y + "." + secondFirmwareVer.Z);

        // both have a 1 major firmware version
        if(firstFirmwareVer.X == 1 &&
            secondFirmwareVer.X == 1)
        {
            // both have a 5 minor firmware version
            if (firstFirmwareVer.Y == 5 &&
                secondFirmwareVer.Y == 5)
            {
                // patch numbers should differ
                if (firstFirmwareVer.Z == 1931)
                {
                    // Debug.Log("myo0 = left and myo1 = right");
                    _myos[0].arm = Thalmic.Myo.Arm.Left;
                    _myos[1].arm = Thalmic.Myo.Arm.Right;
                    _myos[0].firstFirmware = firstFirmwareVer;
                    _myos[1].secondFirmware = secondFirmwareVer;
                } else if (firstFirmwareVer.Z == 1970) {
                    // Debug.Log("myo0 = right and myo1 = left");
                    _myos[0].arm = Thalmic.Myo.Arm.Right;
                    _myos[1].arm = Thalmic.Myo.Arm.Left;
                    _myos[0].secondFirmware = secondFirmwareVer;
                    _myos[1].firstFirmware = firstFirmwareVer;
                } else{
                    Debug.Log("Invalid firmware version in thalmichub.cs");
                }
            }
        }

        // vibrate the Myo to signify which one is the left and which one is the right
        // the left has a short vibrate
        // the right has a medium vibrate
        foreach (ThalmicMyo myo in _myos)
        {
            // Debug.Log("vibrate from ThalmicHub.cs");
            if (myo.arm == Thalmic.Myo.Arm.Left)
            {
                myo.Vibrate(VibrationType.Short);
            }
            if (myo.arm == Thalmic.Myo.Arm.Right)
            {
                myo.Vibrate(VibrationType.Medium);
            }
        }
    }

    void Update ()
    {
        // do nothing
    }

    void hub_MyoPaired (object sender, Thalmic.Myo.MyoEventArgs e)
    {
        foreach (ThalmicMyo myo in _myos) {
            if (myo.internalMyo == null) {
                myo.internalMyo = e.Myo;
                break;
            }
        }
    }

    private static ThalmicHub _instance = null;

    private Thalmic.Myo.Hub _hub = null;

    private List<ThalmicMyo> _myos = new List<ThalmicMyo>();
}
