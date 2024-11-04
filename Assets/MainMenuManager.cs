using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    static private string userId;
    static private string userIdFullPath;
    public TMP_InputField userIdInput;
    static bool popUpShowing;
    public GameObject NewUserPopup;

    private bool IsNewUser()
    {
        string userIdCsvFile = string.Concat(userId,".csv");
        string userIdUserProfilePath = string.Concat("/UserProfiles/", userIdCsvFile);
        userIdFullPath = string.Concat(Application.dataPath, userIdUserProfilePath);
        InterSceneData.interSceneUserIdFullPath = userIdFullPath;
        bool retVal = !System.IO.File.Exists(userIdFullPath);
        if (retVal)
        {
            Debug.Log("We have a new user: " + userIdFullPath);
        } else
        {
            Debug.Log("User already exists: " + userIdFullPath);
        }
        
        return retVal;
    }

    private void CreateUserProfile()
    {
        string userIdCsvFile = string.Concat(userId,".csv");
        string userIdUserProfilePath = string.Concat("/UserProfiles/", userIdCsvFile);
        userIdFullPath = string.Concat(Application.dataPath, userIdUserProfilePath);
        StreamWriter strWriter = new StreamWriter(userIdFullPath, false);
        strWriter.WriteLine("UserId," + userId);
        strWriter.WriteLine("Song History");
        strWriter.WriteLine("Date,Song Name,RDorsiflexion Hit,RDorsiflexion Miss,LPlantarflexion Hit,LPlantarflexion Miss,LDorsiflexion Hit,LDorsiflexion Miss,RPlantarflexion Hit,RPlantarflexion Miss,Total Hit,Total Miss,Final Score,Worst Gesture");
        strWriter.Close();
    }

    // Start is called before the first frame update
    void Start()
    {
        popUpShowing = false;
        Application.targetFrameRate = 30;
    }

    public void ReadUserIdInput ()
    {
        userId = userIdInput.text;
        InterSceneData.interSceneUserId = userId;
    }

    public string GetUserId()
    {
        return userId;
    }

    public void GameStartButton()
    {
        // only do button logic if popup is not showing
        if (!popUpShowing)
        {
            Debug.Log("Going to SongSelect");
            // if the user does not exist, create new user profile
            if (IsNewUser())
            {
                CreateUserProfile();
            }
            SceneManager.LoadScene("03_SongSelect");
        }
    }

    public void HistoryButton()
    {
        // only do button logic if popup is not showing
        if (!popUpShowing)
        {
            // check to see if they created the new user or have already existing user
            if ((userIdInput.text.Length <= 0) || IsNewUser())
            {
                Debug.Log("No history available...");
                // show user pop up to create new user
                popUpShowing = true;
                NewUserPopup.SetActive(true);
            } else
            {
                // Debug.Log("Going to History...");
                InterSceneData.interSceneUserId = userId;
                SceneManager.LoadScene("02_HistoryMenu");
            }
        }
    }

    public void CloseUserDoesNotExistPopupButton()
    {
        popUpShowing = false;
        NewUserPopup.SetActive(false);
    }

    public void ExitButton()
    {
        // only do button logic if popup is not showing
        if (!popUpShowing)
        {
            Debug.Log("Quitting Game...");
            Application.Quit();
        }
    }
}
