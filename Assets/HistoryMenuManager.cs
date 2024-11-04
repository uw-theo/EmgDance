using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class HistoryMenuMgr : MonoBehaviour
{
    private String userId;
    private String userIdFullPath;
    public TextMeshProUGUI userIdText;
    public Button LoadSongButton;
    public TMP_InputField inputSongName;    
    public string prevInputSongName;
    private RectTransform graphContainer;
    private RectTransform labelTemplateX;
    private RectTransform labelTemplateY;
    private RectTransform dashTemplateX;
    private RectTransform dashTemplateY;
    [SerializeField] private Sprite circleSprite;
    private List<GameObject> labelObjectList;
    private List<string> dateList;
    private List<int> leftHitList;
    private List<int> leftMissList;
    private int leftHitMin;
    private int leftHitMax;
    private List<int> downHitList;
    private List<int> downMissList;
    private int downHitMin;
    private int downHitMax;
    private List<int> upHitList;
    private List<int> upMissList;
    private int upHitMin;
    private int upHitMax;
    private List<int> rightHitList;
    private List<int> rightMissList;
    private int rightHitMin;
    private int rightHitMax;
    private List<int> totalHitList;
    private List<int> totalMissList;
    private int totalHitMin;
    private int totalHitMax;
    private List<int> finalScoreList;
    private int finalScoreMin;
    private int finalScoreMax;
    private List<GameObject> circleObjectList;
    public Button[] buttons;
    private Button selectedButton;

    private void Awake()
    {
        GameObject graphObject = GameObject.Find("GraphContainer");
        graphContainer = graphObject.GetComponent<RectTransform>();
        labelObjectList= new List<GameObject>();
        dateList       = new List<string>();
        leftHitList    = new List<int>();
        leftMissList   = new List<int>();
        downHitList    = new List<int>();
        downMissList   = new List<int>();
        upHitList      = new List<int>();
        upMissList     = new List<int>();
        rightHitList   = new List<int>();
        rightMissList  = new List<int>();
        totalHitList   = new List<int>();
        totalMissList  = new List<int>();
        finalScoreList = new List<int>();
        circleObjectList  = new List<GameObject>();
        prevInputSongName = "";
        leftHitMin    = int.MaxValue;
        leftHitMax    = 0;
        downHitMin    = int.MaxValue;
        downHitMax    = 0;
        upHitMin      = int.MaxValue;
        upHitMax      = 0;
        rightHitMin   = int.MaxValue;
        rightHitMax   = 0;
        totalHitMin   = int.MaxValue;
        totalHitMax   = 0;
        finalScoreMin = int.MaxValue;
        finalScoreMax = 0;

        labelTemplateX = graphContainer.Find("LabelTemplateX").GetComponent<RectTransform>();
        labelTemplateY = graphContainer.Find("LabelTemplateY").GetComponent<RectTransform>();
        // supposed to be flipped so that dash axes don't stack in wrong direction
        dashTemplateY = graphContainer.Find("DashLabelX").GetComponent<RectTransform>();
        dashTemplateX = graphContainer.Find("DashLabelY").GetComponent<RectTransform>();
    }

    private void CreateCircle(Vector2 anchoredPosition)
    {
        GameObject gameObject = new GameObject("circle", typeof(Image));
        gameObject.transform.SetParent(graphContainer, false);
        gameObject.GetComponent<Image>().sprite = circleSprite;
        RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = new Vector2(11,11);
        rectTransform.anchorMin = new Vector2(0,0);
        rectTransform.anchorMax = new Vector2(0,0);
        circleObjectList.Add(gameObject);
    }

    private void ShowGraph(List<int> valueList, int valMin, int valMax)
    {
        // dyanamically calculate the top Y axis value
        float yMax = (float)valueList[0]; 
        float yMin = (float)valueList[0];
        foreach(int value in valueList)
        {
            if (value > yMax)
            {
                yMax = value;
            }
            if (value < yMin)
            {
                yMin = value;
            }
        }
        // calculate a little above and below the min and max to 
        // make sure values aren't right on the edge
        yMax = yMax + ((yMax - yMin) * 0.2f);
        yMin = 0; //yMin - ((yMax - yMin) * 0.2f);

        float graphHeight = graphContainer.sizeDelta.y;
        float graphWidth = graphContainer.sizeDelta.x;
        float xSize = graphWidth / valueList.Count - (graphWidth / valueList.Count)* 0.1f;
        for (int i = 0; i < valueList.Count; i++)
        {
            // Set point
            float xPos = xSize + i * xSize;
            float yPos = ((valueList[i] - yMin) / (yMax - yMin)) * graphHeight;
            CreateCircle(new Vector2(xPos, yPos));

            // Set X labels
            RectTransform labelX = Instantiate(labelTemplateX);
            labelX.SetParent(graphContainer);
            labelX.gameObject.SetActive(true);
            labelX.anchoredPosition = new Vector2(xPos-10, -100f);
            labelX.transform.localRotation = Quaternion.Euler(0,0,90);
            labelX.GetComponent<Text>().text = dateList[i];

            // Set X dashes
            RectTransform dashX = Instantiate(dashTemplateX);
            dashX.SetParent(graphContainer);
            dashX.gameObject.SetActive(true);
            dashX.anchoredPosition = new Vector2(xPos, -3f);

            // add both x label and x dashes to label gameobject list
            labelObjectList.Add(labelX.gameObject);
            labelObjectList.Add(dashX.gameObject);
        }


        int separatorCount = 5;
        for (int i = 0; i <= separatorCount; i++)
        {
            // Set Y labels
            RectTransform labelY = Instantiate(labelTemplateY);
            labelY.SetParent(graphContainer, false);
            labelY.gameObject.SetActive(true);
            float normalizedValue = i * 1f / separatorCount;
            labelY.anchoredPosition = new Vector2(-7f, normalizedValue * graphHeight);
            labelY.GetComponent<Text>().text = Mathf.RoundToInt(yMin + (normalizedValue * (yMax - yMin))).ToString();

            // Set Y dashes
            RectTransform dashY = Instantiate(dashTemplateY);
            dashY.SetParent(graphContainer, false);
            dashY.gameObject.SetActive(true);
            dashY.anchoredPosition = new Vector2(-4f, normalizedValue * graphHeight);

            // add both y label and y dashes to label gameobject list
            labelObjectList.Add(labelY.gameObject);
            labelObjectList.Add(dashY.gameObject);
        }   
    }

    void ReadFromCsv(string songName)
    {
        StreamReader strReader = new StreamReader(userIdFullPath);

        // If user already existed, the first line should have the userID
        string data_singleLine = strReader.ReadLine();
        string[] data_values = data_singleLine.Split(',');
        // should have 2 values in the first line. "userid" category and 
        // the actual userid string value   
        if (data_values.Length == 2)
        {
            string readUserId = data_values[1];
            if (userId != readUserId.ToString())
            {
                Debug.Log("User IDs do not match, exit early");
                return;
            }
        }

        // read next line which should just have "Song History" string
        data_singleLine = strReader.ReadLine();
        data_values = data_singleLine.Split(',');
        if ((data_values.Length != 1) || (data_values[0] != "Song History"))
        {
            Debug.Log("More than just SongHistory in this line");
            return;
        }

        // read next line which should just have categories
        data_singleLine = strReader.ReadLine();
        data_values = data_singleLine.Split(',');
        if ((data_values.Length != 14) || 
            (data_values[0] != "Date") ||
            (data_values[1] != "Song Name") ||
            (data_values[2] != "RDorsiflexion Hit") ||
            (data_values[3] != "RDorsiflexion Miss") ||
            (data_values[4] != "LPlantarflexion Hit") ||
            (data_values[5] != "LPlantarflexion Miss") ||
            (data_values[6] != "LDorsiflexion Hit") ||
            (data_values[7] != "LDorsiflexion Miss") ||
            (data_values[8] != "RPlantarflexion Hit") ||
            (data_values[9] != "RPlantarflexion Miss") ||
            (data_values[10] != "Total Hit") ||
            (data_values[11] != "Total Miss") ||
            (data_values[12] != "Final Score") ||
            (data_values[13] != "Worst Gesture"))
        {
            Debug.Log("Category line is incorrect, exiting early.");
            return;
        }

        // read through the song history
        bool endofFile = false;
        List<List<string>> stringList2D = new List<List<string>>();
        while (!endofFile)
        {
            // read single line from csv file
            // if read line is null, we break out since we are at end of file
            data_singleLine = strReader.ReadLine();
            if (data_singleLine == null)
            {
                endofFile = true;
                break;
            }

            // tokenize into data_values since we know its a csv
            // file and split by ','
            data_values = data_singleLine.Split(',');
            List<string> lineList = new List<string>(data_values);
            // only add to the list the lines that match the song name
            if (lineList.Contains(songName))
            {
                stringList2D.Add(lineList);
            }
        }

        // fill lists with values
        int numberOfRows = stringList2D.Count;
        for (int i = 0; i < numberOfRows; i++)
        {
            dateList.Add(stringList2D[i][0]);

            leftHitList.Add(int.Parse(stringList2D[i][2]));
            SetGraphBounds(int.Parse(stringList2D[i][2]), "left");
            leftMissList.Add(int.Parse(stringList2D[i][3]));

            downHitList.Add(int.Parse(stringList2D[i][4]));
            SetGraphBounds(int.Parse(stringList2D[i][4]), "down");
            downMissList.Add(int.Parse(stringList2D[i][5]));

            upHitList.Add(int.Parse(stringList2D[i][6]));
            SetGraphBounds(int.Parse(stringList2D[i][6]), "up");
            upMissList.Add(int.Parse(stringList2D[i][7]));

            rightHitList.Add(int.Parse(stringList2D[i][8]));
            SetGraphBounds(int.Parse(stringList2D[i][8]), "right");
            rightMissList.Add(int.Parse(stringList2D[i][9]));

            totalHitList.Add(int.Parse(stringList2D[i][10]));
            SetGraphBounds(int.Parse(stringList2D[i][10]), "total");
            totalMissList.Add(int.Parse(stringList2D[i][11]));

            finalScoreList.Add(int.Parse(stringList2D[i][12]));
            SetGraphBounds(int.Parse(stringList2D[i][12]), "final");
        }
        Debug.Log("Song Load Complete");
    }

    public void BackButton()
    {
        SceneManager.LoadScene("01_MainMenu");
    }

    // Start is called before the first frame update
    void Start()
    {
        // Debug.Log("History Menu scene ID: " + InterSceneData.interSceneUserId);
        userId          = InterSceneData.interSceneUserId;
        userIdFullPath  = InterSceneData.interSceneUserIdFullPath;
        userIdText.text = userId;
        LoadSongButton.onClick.AddListener(OnLoadSongButtonClick);
    }

    public void OnLoadSongButtonClick()
    {
        // check if the input field is empty
        if(inputSongName.text.Length == 0)
        {
            Debug.Log("input songname is empty");
        }

        // Debug.Log("songname input field: " + inputSongName.text);
        if (prevInputSongName != inputSongName.text)
        {
            ReadFromCsv(inputSongName.text);
        }
        prevInputSongName = inputSongName.text;
    }

    private void SetGraphBounds(int val, string valType)
    {
        switch (valType)
        {
            case "left":
                if (val < leftHitMin)
                {
                    leftHitMin = val;
                }
                if (val > leftHitMax)
                {
                    leftHitMax = val;
                }
                break;
            case "down":
                if (val < downHitMin)
                {
                    downHitMin = val;
                }
                if (val > downHitMax)
                {
                    downHitMax = val;
                }
                break;
            case "up":
                if (val < upHitMin)
                {
                    upHitMin = val;
                }
                if (val > upHitMax)
                {
                    upHitMax = val;
                }
                break;
            case "right":
                if (val < rightHitMin)
                {
                    rightHitMin = val;
                }
                if (val > rightHitMax)
                {
                    rightHitMax = val;
                }
                break;
            case "total":
                if (val < totalHitMin)
                {
                    totalHitMin = val;
                }
                if (val > totalHitMax)
                {
                    totalHitMax = val;
                }
                break;
            case "final":
                if (val < finalScoreMin)
                {
                    finalScoreMin = val;
                }
                if (val > finalScoreMax)
                {
                    finalScoreMax = val;
                }
                break;
        }
    }

    private void ClearGraph()
    {
        // clear previous game objects so they don't overlap
        foreach (GameObject gameObject in labelObjectList)
        {
            Destroy(gameObject);
        }
        labelObjectList.Clear();

        // clear circle objects
        for (int i = 0; i < circleObjectList.Count; i++)
        {
            GameObject tmpRef = circleObjectList[i];
            Destroy(tmpRef);
        }
        circleObjectList.Clear();
    }

    public void OnButtonClick(Button clickedButton)
    {
        // Reset the color of the previously selected button (if any)
        if (selectedButton != null)
        {
            ColorBlock colorBlock = selectedButton.colors;
            colorBlock.normalColor = Color.white;  // Set the default color (white in this case)
            selectedButton.colors = colorBlock;
        }

        // Set the clicked button to green
        selectedButton = clickedButton;
        ColorBlock clickedColorBlock = selectedButton.colors;
        clickedColorBlock.selectedColor = Color.green;
        selectedButton.colors = clickedColorBlock;
    }

    public void OnLeftHitButtonClick()
    {
        ClearGraph();
        if (leftHitList.Count > 0)
        {
            ShowGraph(leftHitList, leftHitMin, leftHitMax);
        } else
        {
            Debug.Log("RDorsiflexion Hit list is empty");
        }
    }

    public void OnDownHitButtonClick()
    {
        ClearGraph();
        if (downHitList.Count > 0)
        {
            ShowGraph(downHitList, downHitMin, downHitMax);
        } else
        {
            Debug.Log("LPlantarflexion Hit list is empty");
        }
    }

    public void OnUpHitButtonClick()
    {
        ClearGraph();
        if (upHitList.Count > 0)
        {
            ShowGraph(upHitList, upHitMin, upHitMax);
        } else
        {
            Debug.Log("LDorsiflexion Hit list is empty");
        }
    }

    public void OnRightHitButtonClick()
    {
        ClearGraph();
        if (rightHitList.Count > 0)
        {
            ShowGraph(rightHitList, rightHitMin, rightHitMax);
        } else
        {
            Debug.Log("RPlantarflexion Hit list is empty");
        }
    }

    public void OnTotalHitButtonClick()
    {
        ClearGraph();
        if (totalHitList.Count > 0)
        {
            ShowGraph(totalHitList, totalHitMin, totalHitMax);
        } else
        {
            Debug.Log("Total Hit list is empty");
        }
    }

    public void OnFinalScoreButtonClick()
    {
        ClearGraph();
        if (finalScoreList.Count > 0)
        {
            ShowGraph(finalScoreList, finalScoreMin, finalScoreMax);
        } else
        {
            Debug.Log("Final Score list is empty");
        }
    }
}
