using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System;

public class SongSelectManager : MonoBehaviour
{
    public TextMeshProUGUI currentSong;
    public string currentSongFolder;
    public string mp3Name;

    // Start is called before the first frame update
    void Start()
    {
        Application.targetFrameRate = 30;
        Debug.Log("History Menu scene ID: " + InterSceneData.interSceneUserId);
    }

    public void grabSongInfo(StreamReader reader)
    {
        string[] splitLine = new string[2];
        string songLine = reader.ReadLine();
        if (songLine.Contains("#TITLE")) // grab the title
        {
            splitLine = songLine.Split(":");
            currentSong.text = splitLine[1]; 
            Debug.Log("songname: " + currentSong.text);
        }

        // grab the next line until #Music is found
        while(!songLine.Contains("#MUSIC"))
        {
            songLine = reader.ReadLine();
        }

        splitLine = songLine.Split(":");
        mp3Name = splitLine[1];
    }

    public void SelectSong1()
    {
        currentSongFolder = String.Concat(Application.dataPath, "/Songs/Goin' Under/");
        string[] splitFolderPath = currentSongFolder.Split("/");
        int songNamePosition = splitFolderPath.Length-2;
        currentSong.text = splitFolderPath[songNamePosition];
        // Debug.Log("chosen song: " + currentSong.text);
        // Debug.Log("song folder: " + currentSongFolder);
    } 

    public void SelectSong2()
    {
        currentSongFolder = String.Concat(Application.dataPath, "/Songs/Prelude VII/");
        string[] splitFolderPath = currentSongFolder.Split("/");
        int songNamePosition = splitFolderPath.Length-2;
        currentSong.text = splitFolderPath[songNamePosition];
        Debug.Log("chosen song: " + currentSong.text);
        Debug.Log("song folder: " + currentSongFolder);
    }

    public void BackButton()
    {
        SceneManager.LoadScene("01_MainMenu");
    }

    public void PlaySong()
    {
        InterSceneData.chosenSong = currentSong.text;
        InterSceneData.songFolder = currentSongFolder;
        // Debug.Log("chosen song: " + InterSceneData.chosenSong);
        // Debug.Log("song folder: " + InterSceneData.songFolder);
        SceneManager.LoadScene("04_GamePlay");
    }
}
