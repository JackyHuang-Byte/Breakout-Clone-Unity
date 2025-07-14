using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class LevelManagerScript : MonoBehaviour
{
    [Tooltip("MAX: [13, 5]")] public Vector2Int Size;
    [Tooltip("REF: [1.4, 0.7]")] public Vector2 Offset;
    
    public Gradient BrickGradient;
    public GameObject BrickPrefab;
    public GameObject SelectMenu;

    public int MaxHits = 3;

    [SerializeField]private int _totalHits;

    private float _timer = 10f;
    private bool _isExist = false;
    private bool _isRunning = false;
    [SerializeField] private int _count = 3;
    private List<Transform> Bricks = new();

    private Color _yellow = new Color(0.9921f, 1f, 0f, 0.8f);
    private Color _white = new Color(0.9849057f, 0.9849057f, 0.9849057f, 1f);
    private Color _grey = new Color(0.5396226f, 0.5396226f, 0.5396226f);
    private Color _background = new Color(1f, 0.8503991f, 0.6245283f,0.6f);
    private HashSet<GameObject> _clickedObjects = new HashSet<GameObject>();

    [Serializable]
    private class PresetList
    {
        public List<PresetData> DataLists = new List<PresetData>();
    }
    private PresetList myPresetList = new PresetList();

    [Serializable]
    private class PresetData
    {
        public string Title;
        public List<Vector3> Positions = new List<Vector3>();
        public List<string> Hits = new List<string>();
    }

    private bool keyIsValid = false;
    
    // Start is called before the first frame update
    void Awake()
    {
        if (SceneManager.GetActiveScene().buildIndex == 0)
        {
            GeneratePreset();
        }
        else if (SceneManager.GetActiveScene().buildIndex == 1)
        {
            PlayerPrefs.GetString("Current Key");
            Debug.Log("Current Key On Screen: " + PlayerPrefs.GetString("Current Key"));
            LoadPresetData();
        }
        //Debug.LogError("Index Order Fix");
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (SceneManager.GetActiveScene().buildIndex == 0)
        {
            if (Input.GetMouseButton(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                OnClickBrick(ray);
            }

            if (Input.GetMouseButtonDown(0))
            {
                if (!EventSystem.current.IsPointerOverGameObject())
                {
                    ClearAllHighlighted();
                }
                //Debug.Log(EventSystem.current.IsPointerOverGameObject());
            }

            if (Input.GetKeyDown(KeyCode.A))
            {
                GenerateSelected();
            }
            else if (Input.GetKeyDown(KeyCode.S))
            {
                OnSelectIncreaseHit();
            }
            else if (Input.GetKeyDown(KeyCode.D))
            {
                OnSelectDecreaseHit();
            }
            else if (Input.GetKeyDown(KeyCode.C))
            {
                DestroySelected();
            }
            else if (Input.GetKeyDown(KeyCode.Escape))
            {
                SelectMenu.SetActive(false);
            }
        }

        if (SceneManager.GetActiveScene().buildIndex == 1)
        {
            if (_count > 0 && !_isRunning)
            {
                _isRunning = true;
                _isExist = false;
                List<Transform> myList = new List<Transform>();
                int tmpInt;
                foreach (Transform child in gameObject.transform)
                {
                    if (child.tag != "Brick: Normal")
                    {
                        _isExist = true;
                        myList.Clear();
                        break;
                    }
                    tmpInt = Convert.ToInt32(child.GetComponentInChildren<TextMeshProUGUI>().text);
                    if (tmpInt < 2)
                    {
                        myList.Add(child);
                    }
                }
                Bricks = new List<Transform>(myList);
                _isRunning = false;
            }

            GameObject ball = GameObject.FindGameObjectWithTag("Ball: Normal");
            bool isTrue = (Bricks.Count > 0 && !_isExist && ball != null); 

            if (isTrue)
            {
                if (_timer == 10f) { Debug.Log("Timer Start"); }
                _timer -= Time.deltaTime; //Debug.Log($"Timer: {_timer:0.00}");
            }

            if (_timer < 0 && !_isRunning)
            {
                Debug.Log("Timer End");
                _isRunning = true;
                List<Transform> myList = new List<Transform>();
                int tmpInt;
                foreach (Transform child in gameObject.transform)
                {
                    tmpInt = Convert.ToInt32(child.GetComponentInChildren<TextMeshProUGUI>().text);
                    if (tmpInt < 2)
                    {
                        myList.Add(child);
                    }
                }
                Bricks = new List<Transform>(myList);

                if (Bricks.Count > 0)
                {
                    int RandomIndex = UnityEngine.Random.Range(0, Bricks.Count);
                    Bricks[RandomIndex].tag = "Brick: Special";
                    Bricks[RandomIndex].GetComponent<SpriteRenderer>().color = Color.magenta; 
                    _isExist = true; Debug.Log("Triggered: Unique Occurred");
                    _count--;
                    _timer = 10f;
                }
                _isRunning = false;
            }
        }
    }

    public void OnClickStart()
    {
        //DontDestroyOnLoad(this.gameObject);
        //RenderLevel();
        string filePath = Application.persistentDataPath + "/PresetData.json";
        if (System.IO.File.Exists(filePath))
        {
            string presetData = System.IO.File.ReadAllText(filePath);
            myPresetList = JsonUtility.FromJson<PresetList>(presetData);
            if (myPresetList.DataLists.Count > 0)
            {
                PresetData myPreset = new PresetData();
                bool isTrue = false;
                Debug.Log("Data Length:" + myPresetList.DataLists.Count);

                for (int i = 0; i < myPresetList.DataLists.Count; i++)
                {
                    if (myPresetList.DataLists[i].Title == PlayerPrefs.GetString("Current Key"))
                    {
                        myPreset = myPresetList.DataLists[i];
                        isTrue = true;
                        Debug.Log("Preset Found");
                        break;
                    }
                }

                SelectMenu.SetActive(false);

                if (isTrue)
                {
                    SceneManager.LoadScene(1);
                }
                else
                {
                    Debug.LogError("Preset is empty");
                }
            }
        }
        else
        {
            SelectMenu.SetActive(false);
            Debug.LogError("Invalid Path");
        }
    }

    public void OnClickSelectMenu()
    {
        ClearAllHighlighted();
        SelectMenu.SetActive(true);
    }

    public void OnSelectLevel(Transform clicked)
    {
        //PlayerPrefs.SetInt("Preset ID", clicked.GetSiblingIndex());
        //presetID = clicked.GetSiblingIndex();
        PlayerPrefs.SetString("Current Key", ("Preset" + clicked.GetSiblingIndex()));
        Debug.Log(clicked.name + ", Slot " + (clicked.GetSiblingIndex()) + ", Current Key On Select: " + PlayerPrefs.GetString("Current Key"));
    }

    public void SavePresetData() //To Json
    {
        bool empty = true; //check if preset is empty
        
        PresetData myPreset = new PresetData();
        foreach (Transform child in gameObject.transform)
        {
            SpriteRenderer sr = child.GetComponent<SpriteRenderer>();
            if (sr.color != _grey)
            {
                TextMeshProUGUI tmp = child.gameObject.GetComponentInChildren<TextMeshProUGUI>();
                int tmpInt = Convert.ToInt32(tmp.text);
                myPreset.Hits.Add(tmp.text);
                myPreset.Positions.Add(child.gameObject.transform.position);
                empty = false;
            }
        }

        if (!empty)
        {
            myPreset.Title = PlayerPrefs.GetString("Current Key");
            Debug.Log("Current Key on Save: " + PlayerPrefs.GetString("Current Key"));
            myPresetList.DataLists.Add(myPreset);
            string presetData = JsonUtility.ToJson(myPresetList);
            string filePath = Application.persistentDataPath + "/PresetData.json";
            //Debug.Log(filePath);
            System.IO.File.WriteAllText(filePath, presetData);
        }
        else
        {
            Debug.LogError("Preset Data is Empty");
        }

        SelectMenu.SetActive(false);
    }

    private void LoadPresetData()
    {
        string filePath = Application.persistentDataPath + "/PresetData.json";
        string presetData = System.IO.File.ReadAllText(filePath);
        myPresetList = JsonUtility.FromJson<PresetList>(presetData);

        _totalHits = 0;
        Debug.Log("Current Key On Load :" + PlayerPrefs.GetString("Current Key"));

        PresetData myPreset = new PresetData();

        for (int i = 0; i < myPresetList.DataLists.Count; i++)
        {
            if (myPresetList.DataLists[i].Title == PlayerPrefs.GetString("Current Key"))
            {
                myPreset = myPresetList.DataLists[i];
                Debug.Log("Preset Found From Load: " + myPreset.Title);
                break;
            }
        }

        for (int i = 0; i < myPreset.Positions.Count; i++)
        {
            GameObject gameObject = Instantiate(BrickPrefab, transform);
            gameObject.transform.position = myPreset.Positions[i];
            TextMeshProUGUI tmp = gameObject.GetComponentInChildren<TextMeshProUGUI>();
            tmp.text = myPreset.Hits[i];
            int tmpInt = Convert.ToInt32(tmp.text);

            _totalHits += tmpInt < 2 ? 0 : (tmpInt - 1);
        }

        PlayerPrefs.SetInt("Total Hits", _totalHits);

        RenderLevel();
    }

    [ContextMenu("Generate Preset (Editor)")]
    private void GeneratePreset()
    {
        for (int i = 0; i < Size.x; i++)
        {
            for (int j = 0; j < Size.y; j++)
            {
                //instantiate
                GameObject newBrick = Instantiate(BrickPrefab, transform); //set instantiated prefabs as children

                //set Location
                newBrick.transform.position = transform.position + new Vector3((float)((Size.x - 1) * 0.5f - i) * Offset.x, j * Offset.y, 0);

                //set hit (tmp)
                TextMeshProUGUI tmp = newBrick.gameObject.GetComponentInChildren<TextMeshProUGUI>();
                tmp.text = "0"; //default
                int tmpInt = Convert.ToInt32(tmp.text);

                //set outline width & color
                LineRenderer lr = newBrick.gameObject.GetComponentInChildren<LineRenderer>();
                lr.startColor = lr.endColor = _yellow;
                //float width = tmpInt > 0 ? (float)(tmpInt - 1) / (_maxHits - 1) * 0.1f : 0;
                //lr.startWidth = width;
                lr.startWidth = 0;

                newBrick.GetComponent<SpriteRenderer>().color = _grey;
            }
        }
        gameObject.SetActive(true);
    }

    private void RenderLevel()
    {
        foreach (Transform child in gameObject.transform) 
        {
            //set brick color
            float t = (float)(child.transform.position.y / 0.7f) / (Size.y - 1);
            child.GetComponent<SpriteRenderer>().color = BrickGradient.Evaluate(t);

            //set outline width & color
            TextMeshProUGUI tmp = child.gameObject.GetComponentInChildren<TextMeshProUGUI>();
            int tmpInt = Convert.ToInt32(tmp.text);
            LineRenderer lr = child.gameObject.GetComponentInChildren<LineRenderer>();
            lr.startColor = lr.endColor = _grey;
            float width = tmpInt > 0 ? (float)(tmpInt - 1) / (MaxHits - 1) * 0.1f : 0;
            lr.startWidth = width;

            //set hit color
            if (tmpInt > 1) { tmp.color = _background; }
        }
    }

    [ContextMenu("Clear Level")]
    private void ClearLevel()
    {
        foreach (Transform child in gameObject.transform)
        {
            Destroy(child.gameObject);
        }
    }

    [ContextMenu("Clear Preset Data")]
    private void ClearPresetData()
    {
        myPresetList.DataLists.Clear();
        string presetData = JsonUtility.ToJson(myPresetList);
        string filePath = Application.persistentDataPath + "/PresetData.json";
        System.IO.File.WriteAllText(filePath, presetData);
        Debug.Log("Data Cleared");
    }

    private void OnClickBrick(Ray ray)
    {
        RaycastHit2D hit = Physics2D.GetRayIntersection(ray);

        if (hit.collider != null && hit.collider.gameObject.CompareTag("Brick: Normal"))
        {
            GameObject clickedObject = hit.collider.gameObject;

            if (!_clickedObjects.Contains(clickedObject))
            {
                GetHighlighted(clickedObject);

                //if (clickedObjects.Count != 0)
                //{
                //    clickedObjects.Clear();
                //}
                _clickedObjects.Add(clickedObject); //Debug.Log("Clicked Count: " + _clickedObjects.Count);
            }
        }
    }

    private void GetHighlighted(GameObject myObject, string str = null)
    {
        LineRenderer lr = myObject.GetComponentInChildren<LineRenderer>();
        float width = lr.startWidth == 0 ? 0.1f : 0;
        if (str == "DeHighlightAll") { width = 0; }
        lr.startWidth = width;
    }

    public void ClearAllHighlighted()
    {
        foreach (GameObject gameObject in _clickedObjects)
        {
            GetHighlighted(gameObject, "DeHighlightAll");
        }
        _clickedObjects.Clear();
    }

    public void DestroySelected()
    {
        OnSelectSetHit(str: "DestroyHit"); //set hits to zero
        OnSelectSetColor(false); //set color to default
        ClearAllHighlighted(); //clear highlighted
    }

    public void GenerateSelected()
    {
        OnSelectSetColor();
    }

    private void OnSelectSetColor(bool isTrue = true)
    {
        foreach (GameObject gameObject in _clickedObjects)
        {
            //set brick color
            SpriteRenderer clickedSpriteRenderer = gameObject.GetComponent<SpriteRenderer>();
            Color color = isTrue ? _white : _grey;
            clickedSpriteRenderer.color = color;
        }
    }

    public void OnSelectIncreaseHit()
    {
        OnSelectSetColor();
        OnSelectSetHit(increaseHit: true);
    }

    public void OnSelectDecreaseHit()
    {
        OnSelectSetColor();
        OnSelectSetHit(increaseHit: false);
    }

    private void OnSelectSetHit(bool increaseHit = true, string str = null)
    {
        foreach (GameObject gameObject in _clickedObjects)
        {
            //set hit (tmp)
            TextMeshProUGUI tmp = gameObject.GetComponentInChildren<TextMeshProUGUI>();
            int tmpInt = Convert.ToInt32(tmp.text);
            if (increaseHit)
            {
                tmpInt = tmpInt < 2 ? 2 : 3;
            }
            else
            {
                tmpInt = tmpInt <= 2 ? 0 : 2;
            }
            if (str == "DestroyHit"){ tmpInt = 0; }
            tmp.text = tmpInt.ToString();

            Color color = tmp.color;
            if (tmpInt < 2)
            {
                color.a = 0f;
            }
            else
            {
                color.a = 0.5f;
            }
            tmp.color = color;
        }
    }

    [ContextMenu("Show Debug Log")]
    private void ShowDebugLog()
    {
        //Debug.Log($"Preset List Count: {myPresetList.DataLists.Count}");
        //for(int i = 0; i < myPresetList.DataLists.Count; i++)
        //{
        //    Debug.Log(myPresetList.DataLists[i].Title);
        //}

        //GameObject myGamo = GameObject.Find("Preset Panel");
        //if (myGamo != null)
        //{
        //    Debug.Log($"Panel Child Count: {myGamo.transform.childCount}");
        //} else 
        //{
        //    Debug.Log("Preset Panel Not Found");
        //}

        //Debug.Log(Application.persistentDataPath + "/PresetData.json");

        Debug.Log("Data Length:" + myPresetList.DataLists.Count);

    }

}
