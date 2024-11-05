using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using UnityEngine.Networking;
using System.Net.WebSockets;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public AudioSource levelMusic;
    public bool levelMusicHasPlayed;
    public bool songLoadComplete; 
    public float songBPM;


    public static GameManager instance;
    public int currentScore;
    public int scorePerNote = 100;

    public Text scoreText;

    public int totalNotes; 
    public int noteHits;
    public int missedHits;
    public int leftHits;
    public int leftMiss;
    public int rightHits;
    public int rightMiss;
    public int upHits;
    public int upMiss;
    public int downHits;
    public int downMiss;

    public GameObject resultsScreen;
    public Text totalHitText;
    public Text finalsScoreText;
    public Text leftHitText;
    public Text downHitText;
    public Text upHitText;
    public Text rightHitText;
    public Text worstGestureText;
    public float startTime;
    public GameObject noteSpawnerObject; 
    public GameObject noteColliderObject;
    public float noteTravelTime; // travel time of the note from start to end line in seconds


    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        Application.targetFrameRate = 120; // limit frame rate to 120 fps
        scoreText.text = "Score: 0";

        // silenceMusic = gameObject.AddComponent<AudioSource>();
        levelMusic = gameObject.AddComponent<AudioSource>();

        songLoadComplete = false;
        StartCoroutine(LoadSong());
        startTime = 0;

        noteSpawnerObject = GameObject.Find("NoteSpawner");
        noteColliderObject = GameObject.Find("NoteCollider");

        float distance = Vector3.Distance(noteSpawnerObject.transform.position, noteColliderObject.transform.position);
        noteTravelTime = distance / (songBPM/60.0f);
        Debug.Log("Time to reach: " + noteTravelTime + " seconds with BPM: " + songBPM);
    }

    // Update is called once per frame
    void Update()
    {
        // only start interactability once the audio file has been loaded
        // typically only 1 - 2 cycles
        if (songLoadComplete)
        {
            if (!levelMusicHasPlayed && levelMusic.isPlaying)
            {
                Debug.Log("levelmusic is playing");
                levelMusicHasPlayed = true;
            }

            // Game is completed
            if(levelMusicHasPlayed && !levelMusic.isPlaying && !resultsScreen.activeInHierarchy)
            //if(silenceMusicHasPlayed && levelMusicHasPlayed && !levelMusic.isPlaying && !resultsScreen.activeInHierarchy)
            {
                resultsScreen.SetActive(true);
                GameObject gameManagerObject = GameObject.Find("GameManager");
                SongReader sread = gameManagerObject.GetComponent<SongReader>();
                totalNotes = sread.GetNumOfNotes();

                // TODO: replace with a calculate score function
                float percentHit = (noteHits / totalNotes) * 100.0f;
                // percentHitText.text = percentHit.ToString("F2") + "%";
                // missedHitText.text = missedHits.ToString();
                int totalLeftNotes = leftHits + leftMiss;
                int totalRightNotes = rightHits + rightMiss;
                int totalDownNotes = downHits + downMiss;
                int totalUpNotes = upHits + upMiss;
                
                finalsScoreText.text = currentScore.ToString();
                totalHitText.text = noteHits.ToString() + "/" + totalNotes.ToString();
                leftHitText.text = leftHits.ToString() + "/" + totalLeftNotes.ToString();
                downHitText.text = downHits.ToString() + "/" + totalDownNotes.ToString();
                upHitText.text = upHits.ToString() + "/" + totalUpNotes.ToString();
                rightHitText.text = rightHits.ToString() + "/" + totalRightNotes.ToString();
                worstGestureText.text = CalculateWorstGesture();

                // Debug.Log("FinalScore: " + finalsScoreText.text);
                // Debug.Log("totalHit: " + totalHitText.text);
                // Debug.Log("leftHit: " + leftHitText.text);
                // Debug.Log("downHit: " + downHitText.text);
                // Debug.Log("upHit: " + upHitText.text);
                // Debug.Log("rightHit: " + rightHitText.text);

                LogScoreToCsvFile(worstGestureText.text);

                StartCoroutine(WaitAndLoadScene(10));
            }
        }
    }

    private IEnumerator LoadSong()
    {
        GameObject gameManagerObject = GameObject.Find("GameManager");
        SongReader sread = gameManagerObject.GetComponent<SongReader>();
        songBPM = sread.GetSongBPM();
        AudioClip musicClip = null;
        double musicClipDuration = 0;
        string songName = sread.GetAudioFileName();
        string songFolder = InterSceneData.songFolder;
        string songFullPath = songFolder + songName;
        Debug.Log("path to song to load: " + songFullPath);

        using (UnityWebRequest webRequest = UnityWebRequestMultimedia.GetAudioClip(songFullPath, AudioType.OGGVORBIS))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.ConnectionError ||
                webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.Log("WebReq error: " + webRequest.error);
            } else
            {
                musicClip = DownloadHandlerAudioClip.GetContent(webRequest);
                musicClip.name = songName;
                musicClipDuration = (double)musicClip.samples/musicClip.frequency;
            }
        }

        // create a blank audio file that is 5.16 seconds long
        // 5.16 is the time it takes for a note block to go from start to noteCollider
        // has 44100 frequency and 2 channel audio
        int totalSamples = (int)(noteTravelTime*44100); // 5.16 seconds, 44100 frequency
        AudioClip blankAudioClip = AudioClip.Create("BlankAudio", totalSamples, 2, 44100, false);
        double blankAudioClipDuration = (double)blankAudioClip.samples/(double)blankAudioClip.frequency;
        // combine with original audio file. Two files should have same frequency and channel numbers
        AudioClip levelMusicClip = CombineAudioFiles(blankAudioClip, musicClip);
        double levelMusicClipDuration = (double)levelMusicClip.samples/(double)levelMusicClip.frequency;

        if ((musicClipDuration <= 0) || 
            (blankAudioClipDuration <= 0) ||
            (levelMusicClipDuration != (musicClipDuration + blankAudioClipDuration)))
        {
            Debug.LogError("Music did not load correctly.");
        } else 
        {
            songLoadComplete = true;
            levelMusic.clip = levelMusicClip;
            levelMusic.Play();
            Debug.Log("level music samples: " + levelMusicClip.samples);
        }
    }

    public AudioClip ConvertAudio(AudioClip origClip)
    {
        if (origClip != null)
        {
            Debug.Log("Old audio length: " + origClip.length);
            int newFreq = 44100; // sample rate of most songs
            int newChan = 2;     // channels is 2 for most songs

            // create new Audioclip with desired settings
            AudioClip newClip = AudioClip.Create("ConvertedClip", origClip.samples, newChan, newFreq, false);
            float[] data = new float[(int)(origClip.samples * origClip.channels * 5.5f)];
            Debug.Log("data length: " + data.Length);
            Debug.Log("origClip samples: " + origClip.samples);
            Debug.Log("origClip channels: " + origClip.channels);
            origClip.GetData(data, 0);
            newClip.SetData(data, 0);
            Debug.Log("Converted audio length: " + newClip.length);
            Debug.Log("Converted samples: " + newClip.samples);
            Debug.Log("Converted channels: " + newClip.channels);
            return newClip;
        } else
        {
            Debug.LogError("Please assign an AudioClip to convert.");
            return null;
        }
    }

    public AudioClip CombineAudioFiles(AudioClip clipOne, AudioClip clipTwo)
    {
        // Debug.Log("Clip1 Length: " + clipOne.length + ", Clip2 Length: " + clipTwo.length);
        if ((clipOne != null) && (clipTwo != null))
        {
            // Calculate the length of the combined audio clip
            int combinedSamples = clipOne.samples + clipTwo.samples;
            float[] clip1Array = new float[clipOne.samples*clipOne.channels];
            float[] clip2Array = new float[clipTwo.samples*clipTwo.channels];
            float[] data = new float[(clip1Array.Length + clip2Array.Length)];
            bool check1 = clipOne.GetData(clip1Array, 0);
            bool check2 = clipTwo.GetData(clip2Array, 0);
            Array.Copy(clip1Array, 0, data, 0, clip1Array.Length);
            Array.Copy(clip2Array, 0, data, clip1Array.Length, clip2Array.Length);


            // Create a new AudioClip to store the combined audio
            AudioClip combinedClip = AudioClip.Create("CombinedClip", combinedSamples, clipOne.channels, clipOne.frequency, false);
            // Set the data to the combined clip
            combinedClip.SetData(data, 0);
            // Debug.Log("clip1Array length: " + clip1Array.Length);
            // Debug.Log("clip2Array length: " + clip2Array.Length);
            // Debug.Log("check1: " + check1 + ", check2: " + check2);
            // Debug.Log("combined length: " + combinedSamples);
            // Debug.Log("data array length: " + data.Length);
            // Debug.Log("clip1 freq: "      + clipOne.frequency);
            // Debug.Log("clip1 chan: "      + clipOne.channels);
            // Debug.Log("clip1 samples: "   + clipOne.samples);
            // Debug.Log("clip2 freq: "      + clipTwo.frequency);
            // Debug.Log("clip2 chan: "      + clipTwo.channels);
            // Debug.Log("clip2 samples: "   + clipTwo.samples); 
            // Debug.Log("combined clip length: " + combinedClip.length);
            // Debug.Log("combined freq: "      + combinedClip.frequency);
            // Debug.Log("combined chan: "      + combinedClip.channels);
            // Debug.Log("combined samples: "   + combinedClip.samples);
            // Assign the combined clip to the AudioSource
            return combinedClip;
        } else{
            Debug.Log("Clip 1 or Clip 2 is NULL");
            return null;
        }
    }

    public float GetNoteTravelTime()
    {
        return noteTravelTime;
    }

    public void NoteHit(KeyCode keyToPress)
    {
        // update which arrow got hit
        if (keyToPress == KeyCode.UpArrow)
        {
            upHits++;
        } else if (keyToPress == KeyCode.DownArrow)
        {
            downHits++;
        } else if (keyToPress == KeyCode.LeftArrow)
        {
            leftHits++;
        } else if (keyToPress == KeyCode.RightArrow)
        {
            rightHits++;
        } else
        {
            Debug.Log("ERROR: invalid key");
        }
        currentScore += scorePerNote;
        noteHits++;
        scoreText.text = ("Score: ") + currentScore.ToString();
        // Debug.Log("total hits: " + noteHits + "[LDUR]: [" + leftHits + "|" + downHits + "|" + upHits + "|" + rightHits + "]");
    }

    public void NoteMissed(KeyCode keyToPress)
    {
            // update which arrow got hit
        if (keyToPress == KeyCode.UpArrow)
        {
            upMiss++;
        } else if (keyToPress == KeyCode.DownArrow)
        {
            downMiss++;
        } else if (keyToPress == KeyCode.LeftArrow)
        {
            leftMiss++;
        } else if (keyToPress == KeyCode.RightArrow)
        {
            rightMiss++;
        } else
        {
            Debug.Log("ERROR: invalid key");
        }
        missedHits++;
        // Debug.Log("total misses: " + missedHits + "[LDUR]: [" + leftMiss + "|" + downMiss + "|" + upMiss + "|" + rightMiss + "]");
    }

    public void LogScoreToCsvFile(string worstGesture)
    {
        string userId = InterSceneData.interSceneUserId;
        string userIdPath = InterSceneData.interSceneUserIdFullPath;
        Debug.Log("userId: " + userId);
        Debug.Log("userIdPath: " + userIdPath);

        // check and make sure csv file exists
        if (!File.Exists(userIdPath))
        {
            Debug.Log("Error: UserID no longer exists");
            return; // exit out early if file does not exist
        }

        // Create line to write to csv file
        int totalMiss = totalNotes - noteHits;
        string logDate          = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");;
        string logSongName      = logDate          + "," + InterSceneData.chosenSong;
        string logLeftHit       = logSongName      + "," + leftHits.ToString();
        string logLeftMiss      = logLeftHit       + "," + leftMiss.ToString();
        string logDownHit       = logLeftMiss      + "," + downHits.ToString();
        string logDownMiss      = logDownHit       + "," + downMiss.ToString();
        string logUpHit         = logDownMiss      + "," + upHits.ToString();
        string logUpMiss        = logUpHit         + "," + upMiss.ToString();
        string logRightHit      = logUpMiss        + "," + rightHits.ToString();
        string logRightMiss     = logRightHit      + "," + rightMiss.ToString();
        string logTotalHit      = logRightMiss     + "," + noteHits.ToString();
        string logTotalMiss     = logTotalHit      + "," + totalMiss.ToString();
        string logFinalScore    = logTotalMiss     + "," + currentScore.ToString();
        string logWorstGesture  = logFinalScore    + "," + worstGesture;
        string logLine = logWorstGesture + "\n"; 
        
        // Append data to log file
       
        File.AppendAllText(userIdPath, logLine);
    }

    // worst gesture is calculated as one with the lowest hit percentage
    public string CalculateWorstGesture()
    {
        string retVal = "";
        float leftHitPercent  = (float)(100.0f*leftHits/(leftMiss+leftHits));
        float downHitPercent  = (float)(100.0f*downHits/(downMiss+downHits));
        float upHitPercent    = (float)(100.0f*upHits/(upMiss+upHits));
        float rightHitPercent = (float)(100.0f*rightHits/(rightMiss+rightHits));
        float[] tempArray = {leftHitPercent, downHitPercent, upHitPercent, rightHitPercent};

        int i = 0;
        float minVal = tempArray[0];
        int minIdx = 0;
        for (i = 0; i < tempArray.Length; i++)
        {
            if (tempArray[i] < minVal)
            {
                minVal = tempArray[i];
                minIdx = i;
            }
        }

        if (minIdx == 0)
        {
            retVal = retVal + "left(" + leftHitPercent.ToString("F2") + "%)";
        } else if (minIdx == 1)
        {
            retVal = retVal + "down(" + downHitPercent.ToString("F2") + "%)";
        } else if (minIdx == 2)
        {
            retVal = retVal + "up(" + upHitPercent.ToString("F2") + "%)";
        } else if (minIdx == 3)
        {
            retVal = retVal + "right(" + rightHitPercent.ToString("F2") + "%)";
        } else
        {
            Debug.Log("ERROR: invalid worst gesture index");
        }

        // Keep around for debug only
        // Debug.Log("left: "   + leftHitPercent.ToString("F2"));
        // Debug.Log("down: "   + downHitPercent.ToString("F2"));
        // Debug.Log("up: "     + upHitPercent.ToString("F2"));
        // Debug.Log("right: "  + rightHitPercent.ToString("F2"));
        // Debug.Log("minVal: " + minVal.ToString());
        // Debug.Log("minIdx: " + minIdx.ToString());
        // Debug.Log("retVal: " + retVal);
        return retVal;
    }

    private IEnumerator WaitAndLoadScene(int waitTime)
    {
        Debug.Log("10 second wait start");
        yield return new WaitForSeconds(waitTime);
        Debug.Log("10 second wait end");

        // destroy hub object to prevent duplicates
        GameObject myoHubObject = GameObject.Find("Hub - 1 Myo");
        Destroy(myoHubObject);

        SceneManager.LoadScene("03_SongSelect");
    }
}