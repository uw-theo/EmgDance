using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VRTemplate;
using UnityEngine;
using UnityEngine.Events;

public class BeatClass : MonoBehaviour
{
    public int leftNote;
    public int downNote;
    public int upNote;
    public int rightNote;
}

[System.Serializable]
public class Intervals
{
    [SerializeField] private float _steps;
    [SerializeField] private UnityEvent _trigger;
    private int _lastInterval;

    public float GetIntervalLength (float bpm)
    {
        return (60.0f / (bpm * _steps));
    }

    public void CheckForNewInterval (float interval)
    {
        bool comparison = Mathf.FloorToInt(interval) != _lastInterval;
        if (comparison)
        {
            //Debug.Log("(" + Time.time + ")" + "comparison: " + comparison.ToString() + ", interval: " + interval.ToString());
            _lastInterval = Mathf.FloorToInt(interval);
            _trigger.Invoke();
        }
    }
}

public class SongReader : MonoBehaviour
{
    string stepFileName;
    string stepFilePath; 
    [SerializeField] private float songBPM;
    [SerializeField] private string audioFileName;
    public List<string> allNotes;

    private AudioSource levelMusic;
    // private AudioSource silenceMusic;

    [SerializeField] public Intervals[] _intervals;
    public GameManager gameManager;

    void Awake()
    {
        allNotes = new List<string>();
        // before all other "starts" load the song file info
        LoadSongFile();
    }

    // Start is called before the first frame update
    void Start()
    {
        string userID = InterSceneData.interSceneUserId;
        string fullPath = InterSceneData.interSceneUserIdFullPath;
        string songName = InterSceneData.chosenSong;
        string songFolder = InterSceneData.songFolder;
        string[] matchingFiles = Directory.GetFiles(songFolder, "*.sm");

        GameObject managerObject = GameObject.Find("GameManager");
        levelMusic = managerObject.GetComponent<AudioSource>();
        gameManager = managerObject.GetComponent<GameManager>();
    }

    void Update()
    {
        // if (gameManager.songLoadComplete && (songBPM > 0))
        // if ((levelMusic.isPlaying || silenceMusic.isPlaying) && (songBPM > 0))
        if (levelMusic.isPlaying && (songBPM > 0))
        {
            
            // using a foreach not necessary for singular spawn on beat
            // the for each is useful if you have multiple things going
            // to the beat at 50% of beat or 150% of beat, etc. 
            foreach (Intervals interval in _intervals)
            {
                // Debug.Log("timesamples: " + levelMusic.timeSamples);
                float sampledTime = (levelMusic.timeSamples) / 
                    (levelMusic.clip.frequency * interval.GetIntervalLength(songBPM));
                interval.CheckForNewInterval(sampledTime);
            }
            
            // float sampledTime = (levelMusic.timeSamples) / 
            //     (levelMusic.clip.frequency * _intervals[0].GetIntervalLength(songBPM));
            // _intervals[0].CheckForNewInterval(sampledTime);
        }

    }

    public void LoadSongFile()
    {
        string[] matchingFiles = Directory.GetFiles(InterSceneData.songFolder, "*.sm");
        if (matchingFiles.Length > 1) // should only have one match
        {
            Debug.LogError("More than one match");
        }
        stepFilePath = matchingFiles[0];
        LoadSongSteps();
    }

    public void LoadSongSteps()
    {
        StreamReader inputStream = new StreamReader (stepFilePath);
        ProcessSongMetaInfo(inputStream); // grab song meta information
        LoadNotesToList(inputStream);
        inputStream.Close();
    }

    public void ProcessSongMetaInfo(StreamReader inputStream) 
    {
        
        bool endOfMetaInfo = false;
        while (!endOfMetaInfo)
        {
            string readLine = inputStream.ReadLine();
            //Debug.Log(readLine);

            // grab the audio file name
            if (readLine.Contains("#MUSIC"))
            {
                string parseLine = readLine;
                string[] splitLine = parseLine.Split(":");
                int lengthOfSongName = splitLine[1].Length;
                audioFileName = splitLine[1].Substring(0, lengthOfSongName-1);
            }

            // grab the BPMs
            if (readLine.Contains("#BPM"))
            {
                string parseLine = readLine;
                string[] splitLine = parseLine.Split("=");
                songBPM = float.Parse(splitLine[1]);
            }

            if (readLine.Contains("#ATTACKS"))
            {
                endOfMetaInfo = true;
            }
        }
    }

    public void LoadNotesToList(StreamReader inputStream)
    {
        bool foundDifficulty = false;
        while (!foundDifficulty)
        {
            string singleLine = inputStream.ReadLine();
            // found general header section for song difficulty info
            if (singleLine.Contains("dance-single -"))
            {
                string noteLine    = inputStream.ReadLine();
                string modeLine    = inputStream.ReadLine();
                string delimLine   = inputStream.ReadLine();
                string diffLine    = inputStream.ReadLine();
                string diffLvlLine = inputStream.ReadLine();
                string valLine     = inputStream.ReadLine();
                if (diffLine.Contains("Beginner") && diffLvlLine.Contains("1"))
                {
                    foundDifficulty = true;
                }
            }
        }

        // Debug.Log("Found difficulty level where notes start");

        // header portion processed, now to process notes
        string beatLine = "";
        while (!beatLine.Contains(";")) // uses ; to end section
        {
            beatLine = inputStream.ReadLine();
            if (!beatLine.Contains(",")) // if we see a ',' we skip line
            {
                allNotes.Add(beatLine);
            }
        } 
        // Debug.Log("Finished loading all notes");
    }

    public List<string> GetAllNotes()
    {
        if (allNotes.Count == 0)
        {
            Debug.Log("allnotes List is empty");
        }
        // foreach (var singleLine in allNotes)
        // {
        //     Debug.Log(singleLine);
        // }
        return allNotes;
    }

    public int GetNumOfNotes()
    {
        int noteCounter = 0;
        foreach (string line in allNotes)
        {
            // Debug.Log("single line: " + line);
            if (line != ";") // end of song
            {
                int restNote = int.Parse(line); // used to check if all 0s
                if (restNote > 0)
                {
                    noteCounter++;
                }
            }
        }
        
        return noteCounter;
    }

    public float GetSongBPM()
    {
        return songBPM;
    }

    public string GetAudioFileName()
    {
        return audioFileName;
    }
}
