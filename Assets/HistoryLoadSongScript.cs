using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HistoryLoadSongScript : MonoBehaviour
{
    public InputField inputSongName;    
    public void OnButtonClick()
    {
        if (inputSongName.text == null)
        {
            Debug.Log("invalid song name");
        }

        
    }
}
