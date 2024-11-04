using System;
using UnityEngine;

using UnityEngine.UI;
using LockingPolicy = Thalmic.Myo.LockingPolicy;
using Pose = Thalmic.Myo.Pose;
using UnlockType = Thalmic.Myo.UnlockType;
using VibrationType = Thalmic.Myo.VibrationType;

public class MyoMovement : MonoBehaviour
{
    public GameObject myo = null;

    public 
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Game Start!");
    }

    // Update is called once per frame
    void Update()
    {
        // Access the ThalmicMyo component attached to the Myo game object.
        ThalmicMyo thalmicMyo = myo.GetComponent<ThalmicMyo> ();

        //Debug.Log("CurrPose: " +  thalmicMyo.pose + " PrevPose: " + _lastPose);

        //commands
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

        if (thalmicMyo.pose == Pose.Fist) // move forward
        {
            Debug.Log("Fist!");
            //txt.text = "Fist: Toes Curled";
        } else if (thalmicMyo.pose == Pose.WaveIn) // move right from perspective of left hand
        {
            Debug.Log("Wave In!");
            //txt.text = "WaveIn: Dorsiflexion, toes up";
            //Player.transform.position = Player.transform.position + new Vector3(movementSpeed * Time.deltaTime, 0, 0);
        } else if (thalmicMyo.pose == Pose.WaveOut) // move left from perspective of left hand
        {
            Debug.Log("Wave Out!");
            //txt.text = "WaveOut: Plantarflexion, toes down";
            //Player.transform.position = Player.transform.position + new Vector3(-1*movementSpeed * Time.deltaTime, 0, 0);
        } else if (thalmicMyo.pose == Pose.DoubleTap) // don't do anything
        {
            Debug.Log("Double Tap!");
        } else if (thalmicMyo.pose == Pose.FingersSpread) // move backward
        {
            Debug.Log("Fingers Spread!");
            //txt.text = "FingersSpread: toes spread";
        } else if (thalmicMyo.pose == Pose.Rest) // don't do anything
        {
            Debug.Log("Rest!");
            //txt.text = "Rest";
        } else{
            Debug.Log("Error: command not recognized!");
        }
    }
}
