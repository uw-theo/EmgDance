using System.Collections;
using System.Collections.Generic;

//using Thalmic.Myo;
using Unity.VisualScripting;
using UnityEngine;

public class NoteSpawnerController : MonoBehaviour
{
    public GameObject leftArrowPrefab;
    public GameObject rightArrowPrefab;
    public GameObject upArrowPrefab;
    public GameObject downArrowPrefab;
    // private Vector3 _startSize;
    public bool hasStarted;
    private int currNoteIdx; 
    List<string> allNotes;
    public Transform targetPosition;

    // Start is called before the first frame update
    void Start()
    {
        currNoteIdx = 0;
        allNotes = new List<string>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!hasStarted)
        {
            hasStarted = true;
            loadNotes();
        }

        // move the notes down once instantiated
        // transform.localScale = Vector3.Lerp(transform.localScale, _startSize, Time.deltaTime * _returnSpeed);
    }

    public void loadNotes()
    {
        GameObject otherGameObject = GameObject.Find("GameManager");
        if (otherGameObject != null)
        {
            SongReader sread = otherGameObject.GetComponent<SongReader>();
            allNotes = sread.GetAllNotes();
            Debug.Log("allNotes size: " + allNotes.Count);
        } else{
            Debug.Log("loadNotes failed");
        }

    }

    public void spawnNote()
    {
        // make sure not to go out of bounds of all the notes
        // -1 since the last "note" is ";" to signify the end of the song
        if (currNoteIdx >= (allNotes.Count-1))
        {
            // Debug.Log("currNoteIdx: " + currNoteIdx.ToString() + ", allNotes count: " + allNotes.Count.ToString());
            return;
        }

        //instantiate randomly
        GameObject objToInstantiate = null;
        

        // grab a single line of notes to play
        string oneLine = allNotes[currNoteIdx];
        // Debug.Log("currNoteIdx: " + currNoteIdx.ToString() + "/" + allNotes.Count.ToString());
        
        // conver singular char from the line into an int
        int leftDirection  = (int)oneLine[0] - (int)'0';
        int downDirection  = (int)oneLine[1] - (int)'0';
        int upDirection    = (int)oneLine[2] - (int)'0';
        int rightDirection = (int)oneLine[3] - (int)'0';
        int restNote = int.Parse(oneLine); // used to check if all 0s

        //Debug.Log("parsed: " + leftDirection.ToString() + downDirection.ToString() + upDirection.ToString() + rightDirection.ToString());

        // Vector3 positionalAdjustment = new Vector3(0.0f, 0.0f, 0.0f);
        Vector3 positionalAdjustment = transform.position;
        // each note object is 0.16 wide
        if (leftDirection > 0) // right foot, dorsiflexion
        {
            objToInstantiate = leftArrowPrefab;
            // positionalAdjustment.x = -1.0f*((0.8f)/2.0f*3.0f);
            positionalAdjustment.x = 1.0f*leftArrowPrefab.transform.localScale.x/2f;
        } else if (downDirection > 0) // left foot, plantarflexion
        {
            objToInstantiate = downArrowPrefab;
            // positionalAdjustment.x = -1.0f*((0.8f)/2.0f*1.0f);
            positionalAdjustment.x = -1.0f*downArrowPrefab.transform.localScale.x*1.5f;
        } else if (upDirection > 0) // left foot, dorsiflexion
        {
            objToInstantiate = upArrowPrefab;
            // positionalAdjustment.x = 1.0f*((0.8f)/2.0f*1.0f);
            positionalAdjustment.x = -1.0f*upArrowPrefab.transform.localScale.x/2f;
        } else if (rightDirection > 0) // right foot, plantarflexion
        {
            objToInstantiate = rightArrowPrefab;
            positionalAdjustment.x = 1.0f*rightArrowPrefab.transform.localScale.x*1.5f;
        } else if (restNote == 0)
        {
            currNoteIdx++;
        } else 
        {
            Debug.Log("Line does not make sense");
        }

        //only instantiate if we have a valid object to instantiate
        //otherwise don't do anything
        if (objToInstantiate != null)
        {
            objToInstantiate.SetActive(true);
            Instantiate(objToInstantiate, positionalAdjustment, Quaternion.identity);
            currNoteIdx++;
        }

    }
}
