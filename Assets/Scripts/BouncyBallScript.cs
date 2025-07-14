using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System;
using Unity.VisualScripting;

public class TagData
{
    public Color TagColor { get; set; }
    public Action TriggerEvent { get; set; }
    public Action EndEvent { get; set; }

}
public class BouncyBallScript : MonoBehaviour
{
    [Tooltip("[KEEP CONSISTENT WITH SCRIPT]")] public float MinY = -5.5f;
    [Tooltip("[KEEP CONSISTENT WITH SCRIPT]")] public float MaxMagnitude = 15.2f;
    public int Timer;

    public TextMeshProUGUI ScoreText;
    public TextMeshProUGUI TimerText;
    public GameObject BallPrefab;
    public GameObject Paddle;
    public GameObject FourthWall;
    public GameObject GameOverPanel;
    public GameObject PausePanel;
    public GameObject YouWinPanel;
    public GameObject LiveImage;
    public List<GameObject> LiveImages;

    private GameObject _bouncyBall;
    private Rigidbody2D _ballRB2D;
    private float _velocity = 6.0f; //velocity magnitude
    private int _lives;
    private int _brickCount;
    private int _maxHits;
    private bool _isHittingSame = false;

    private List<GameObject> _clonedBalls = new List<GameObject>();
    private bool _isCloning = false;
    private bool _isPaused = false;
    private bool _isOver = false;
    private int _score = 0;
    private float _elapsed = 0;
    private int _remaining;
    private int _specialCount = 21;
    /*
    Clone    = round(num/4) = 5
    Piercing = num/3        = 7
    Bouncy   = num          = 21
    Split    = num/3        = 7 
    */

    private static readonly Dictionary<int, string> _TagIndexMap = new()
    {
        {0, "Ball: Normal"  },
        {1, "Ball: Special1"}, //Clone
        {2, "Ball: Special2"}, //Piercing
        {3, "Ball: Special3"}, //Bouncy
        {4, "Ball: Special4"}  //Split
    };

    private static Dictionary<string, TagData> _TagDataEntry;
    private void Awake()
    {
        _TagDataEntry = new Dictionary<string, TagData>()
        {
            {"Ball: Normal"  , new TagData{ TagColor = Color.white}},
            {"Ball: Special1", new TagData{ TagColor = Color.cyan  , TriggerEvent = CloneBall,       EndEvent = DestroyClone}},
            {"Ball: Special2", new TagData{ TagColor = Color.grey  , TriggerEvent = GivePiercing,    EndEvent = CancelPiercing}},
            {"Ball: Special3", new TagData{ TagColor = Color.yellow, TriggerEvent = GiveSuperBouncy, EndEvent = CancelSuperBouncy}},
            {"Ball: Special4", new TagData{ TagColor = Color.blue  , TriggerEvent = GiveSplit,       EndEvent = CancelSplit}}
        };
    
    }

    // Start is called before the first frame update
    void Start()
    {
        Time.timeScale = 1.0f;
        _bouncyBall = Instantiate(BallPrefab, transform);
        _bouncyBall.transform.position = gameObject.transform.position;
        _ballRB2D = _bouncyBall.GetComponent<Rigidbody2D>();
        _ballRB2D.velocity = Vector2.down * _velocity; //velocity is a vector2 variable
        _brickCount = GameObject.Find("Level Manager")?.transform.childCount ?? 0;
        _maxHits = FindFirstObjectByType<LevelManagerScript>()?.MaxHits ?? 3;
        _lives = _brickCount / 10;  
        _lives++;
        Timer += (PlayerPrefs.GetInt("Total Hits") * 10);
        int x = _lives <= 6 ? _lives : 6;
        for (int i  = 0; i < x; i++)
        {
            GameObject newLiveImage = Instantiate(LiveImage, GameObject.Find("Lives").transform);
            LiveImages.Add(newLiveImage);
        }

    }

    // Update is called once per frame
    void Update()
    {
        if ((_bouncyBall.tag == TagIndex(0) || _bouncyBall.tag == TagIndex(4))
            && _bouncyBall.transform.position.y < MinY)
        {
            if (_lives > 0)
            {
                RespawnBall();
            }
            else
            {
                GameOver();
            }

        }
        
        if (_clonedBalls != null)
        {
            _clonedBalls = new List<GameObject>(GameObject.FindGameObjectsWithTag(TagIndex(1)));
            if (_clonedBalls.Count == 1 && !_isCloning)  
            {
                DestroyClone(); //replace last clone with original
            }
            else if (_clonedBalls.Count == 1 && _isCloning)
            {
                DestroyClone();
                GiveSplit();  
            }
            else
            {
                foreach(GameObject clone in _clonedBalls)
                {
                    if(clone.transform.position.y < MinY)
                    {
                        Destroy(clone);
                        break;
                    }
                }
            }
        }

        if (_ballRB2D.velocity.magnitude > MaxMagnitude)
        {
            _ballRB2D.velocity = Vector2.ClampMagnitude(_ballRB2D.velocity, MaxMagnitude);
        }

        if (!_isOver && Input.GetKeyDown(KeyCode.Escape))
        {
            if (!_isPaused)
            {
                _isPaused = true;
                PausePanel.SetActive(true);
                Time.timeScale = 0f;
            }
            else
            {
                _isPaused = false;
                PausePanel.SetActive(false);
                Time.timeScale = 1f;

            }
        }

        if (_elapsed < (Timer))
        {
            _remaining = Mathf.FloorToInt(Timer - _elapsed);
            int m = _remaining / 60;
            int s = _remaining % 60;
            TimerText.text = $"{m:00} : {s:00}";

        }
        else
        {
            TimerText.text = "00 : 00";
            GameOver();
        }

        _elapsed += Time.deltaTime;
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void Resume()
    {
        _isPaused = false;
        Time.timeScale = 1.0f;
        PausePanel.SetActive(false);
    }

    public void Exit()
    {
        Debug.Log("Exit Button Clicked");
        //Application.Quit();
    }

    private void GameOver()
    {
        _bouncyBall.SetActive(false);
        GameOverPanel.SetActive(true);
        Time.timeScale = 0f;
        _isOver = true;
    }

    private void GameEnd()
    {
        if (_clonedBalls.Count >= 1)
        {
            for (int i = 0; i < _clonedBalls.Count; i++)
            {
                _clonedBalls[i].gameObject.SetActive(false);
            }
        }
        _bouncyBall.SetActive(false);
        YouWinPanel.SetActive(true);
        Time.timeScale = 0f;
        _isOver = true;

        string highScore = PlayerPrefs.GetString("High Score", "0");
        int remainTime = _remaining;
        int remainLive = _lives * 20;

        TextMeshProUGUI context1 = YouWinPanel.GetComponentsInChildren<TextMeshProUGUI>()[1];
        context1.text = $"SCORE: \t{_score:00000}"
                        + $"\nTIME: \t{remainTime:00000}"
                        + $"\nLIVE: \t{remainLive:00000}";

        string currentScore = GetCurrentScore().ToString("00000");
        TextMeshProUGUI context2 = YouWinPanel.GetComponentsInChildren<TextMeshProUGUI>()[2];
        if (int.Parse(highScore) < GetCurrentScore())
        {
            context2.text = "NEW SCORE !! \n" + currentScore;
            PlayerPrefs.SetString("High Score", currentScore);
        }
        else
        {
            context2.text = "TOTAL: " + currentScore + "\nBEST: " + highScore;
        }
    }

    private void RespawnBall()
    {
        _bouncyBall.transform.position = Vector3.zero;
        Paddle.transform.position = new Vector3(0, -3.5f, 0); //original position
        StartCoroutine(AfterDrop()); //give time to react position reset
        _ballRB2D.velocity = Vector2.down * _velocity;
        _lives--;
        LiveImages[_lives].SetActive(false);

        _bouncyBall.tag = TagIndex(0);
        UpdateColor();
        _specialCount = 21; //public origin if necessary
        _isCloning = false;
    }

    private int GetCurrentScore()
    {
        int totalScore = _score + _remaining + (_lives *20);
        return totalScore;
    }

    public void BouncyBallOnCollision(Collision2D collision) //Collision inherited from prefab script
    {
        //if (_bouncyBall.activeSelf) { Debug.Log("Velocity: " + _ballRB2D.velocity + ", " + _ballRB2D.velocity.magnitude);}
        if (collision.gameObject.GetComponentInChildren<TextMeshProUGUI>() != null)
        {
            // brick hit (tmp) event 
            TextMeshProUGUI tmp = collision.gameObject.GetComponentInChildren<TextMeshProUGUI>();
            int tmpInt = Convert.ToInt32(tmp.text);
            tmpInt --;
            
            if(tmpInt <= 1)
            {
                Color color = tmp.color;
                color.a = 0f;
                tmp.color = color;
            }

            if (tmpInt <= 0 && collision.gameObject.activeSelf) //trigger outcome only once
            {
                collision.gameObject.SetActive(false);
                if (!collision.gameObject.activeSelf && !_isHittingSame) 
                {
                    _isHittingSame = true;
                    _score += 10; //Debug.Log("Score: " + _score);
                    ScoreText.text = _score.ToString("00000");
                    _brickCount--; //Debug.Log("Brick Count:" + _brickCount);
                    Destroy(collision.gameObject);
                    _isHittingSame = false;
                }

                if (_brickCount == 0)
                {
                    GameEnd();
                }
            }

            tmp.text = tmpInt.ToString();

            // brick outline width change event
            LineRenderer lr = collision.gameObject.GetComponentInChildren<LineRenderer>();
            lr.startWidth = (float)(tmpInt - 1) / (_maxHits - 1) * 0.1f;

            // spilt ball countdown
            if (_bouncyBall.tag == TagIndex(4))
            {
                _specialCount -= 3;
                if (_specialCount > 0)
                {
                    SplitBall(collision.otherCollider.gameObject);
                }
                else
                {
                    _bouncyBall.tag = TagIndex(0);
                    _isCloning = false;
                    _specialCount = 21;
                }
            }

            if (collision.gameObject.CompareTag("Brick: Special")) //trigger outcome only once
            {
                _specialCount = 21;
                int random = UnityEngine.Random.Range(1, 5);
                TriggerPowerByTag(random);
                //TriggerPowerByTag(1);
            }
        }

        if (_ballRB2D.velocity.y <= 0.1f)
        {
            _ballRB2D.velocity += Vector2.down * 0.1f; //give falling speed a bit acceleration
        }
        else if (_ballRB2D.velocity.x == 0f)
        {
            float tempOff = UnityEngine.Random.value < 0.5 ? -0.1f : 0.1f; //give vertical falling a bit random offset
            _ballRB2D.velocity += Vector2.right * tempOff;
        }
        else
        {
            _ballRB2D.velocity = Vector2.ClampMagnitude(_ballRB2D.velocity, _velocity);
        }

        //bouncy ball countdown
        if(_bouncyBall.tag == TagIndex(3))
        {
            float normalspeed = 6.0f;
            float superspeed = 12.0f;

            if(collision.gameObject.CompareTag("Fourth Wall"))
            {
                _ballRB2D.velocity = _ballRB2D.velocity.normalized* normalspeed;
            }
            else if (collision.gameObject.CompareTag("Paddle"))
            {
                _ballRB2D.velocity = _ballRB2D.velocity.normalized * superspeed;
            }
            
            if(collision.gameObject.CompareTag("Brick: Normal"))
            {
                _specialCount--;
                if (_specialCount <= 0) 
                {
                    _ballRB2D.velocity = _ballRB2D.velocity.normalized * normalspeed;
                    CancelSuperBouncy();
                    _specialCount = 21;
                }
                Debug.Log("Remaining Super Bouncy Count: " + _specialCount);
            }
        }
    }

    public void BouncyBallOnTrigger(Collider2D collision) //collision inherited from prefab script
    {
        if (collision.gameObject.CompareTag("Brick: Normal"))
        {
            _score += 10; Debug.Log("Score: " + _score);
            ScoreText.text = (_score + _remaining).ToString("00000");
            _brickCount--;

            collision.gameObject.SetActive(false);

            if (_brickCount == 0)
            {
                GameEnd();
                int current = int.Parse(ScoreText.text);
                int highScore = int.Parse(PlayerPrefs.GetString("High Score", "0"));
                if (current > highScore)
                {
                    PlayerPrefs.SetString("High Score", ScoreText.text);
                }
            }
        }
        _specialCount -= 3;
        if (_specialCount <= 0)
        {
            CancelPiercing();
            _specialCount = 21; //public original if necessary
        }

        Debug.Log("Piercing Triggered");
    }

    private IEnumerator AfterDrop()
    {
        PaddleScript paddleScript = Paddle.GetComponent<PaddleScript>();
        float initial = paddleScript.Speed;
        paddleScript.Speed = 0f;
        yield return new WaitForSeconds(0.5f);
        paddleScript.Speed = initial;
    }

    public string TagIndex(int i)
    {
        if(_TagIndexMap.TryGetValue(i, out string name))
        {
            return name;
        }
        Debug.LogError("Tag ID Not Found");
        return null;
    }

    private void TriggerPowerByTag(int i)
    {
        if(_TagDataEntry.TryGetValue(TagIndex(i), out TagData tagData))
        {
            tagData.TriggerEvent();
        }
    }

    private void UpdateColor(GameObject obj = null, string tag = null)
    {
        //default
        obj ??= _bouncyBall;
        tag ??= obj.tag;

        if (_TagDataEntry.TryGetValue(tag, out TagData tagData))
        {
            obj.TryGetComponent<SpriteRenderer>(out SpriteRenderer sr);
            sr.color = tagData.TagColor;
        }
        else
        {
            Debug.LogError("UpdateColor Error Occurred");
        }
    } 
    
    [ContextMenu("1. Try Create Clone")]
    private void CloneBall()
    {
        int _numOfClones = Mathf.RoundToInt(_specialCount/4);
        for (int i = 0; i < _numOfClones; i++)
        {
            GameObject newBall = Instantiate(BallPrefab, transform);
            Rigidbody2D newRB2D = newBall.GetComponent<Rigidbody2D>();
            Vector2 randomDirection = new Vector2(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(0f, 1f)).normalized;

            newBall.tag = TagIndex(1);
            UpdateColor(newBall);
            _clonedBalls.Add(newBall);

            newBall.transform.position = _bouncyBall.transform.position;
            newRB2D.velocity = randomDirection * _ballRB2D.velocity.magnitude;

        }
        _bouncyBall.SetActive(false);

    }

    [ContextMenu("1. Try Destroy Clone")]
    private void DestroyClone()
    {
        _isCloning = false;
        for (int i = 0; i < _clonedBalls.Count; i++)
        {
            Debug.Log("Destroy Clone(s) Triggered");
            if (_clonedBalls[i].activeSelf)
            {
                _bouncyBall.SetActive(true);
                _bouncyBall.transform.position = _clonedBalls[i].transform.position;
                _ballRB2D.velocity = _clonedBalls[i].GetComponent<Rigidbody2D>().velocity;
                break;
            }
            
        }

        foreach (GameObject clone in _clonedBalls)
        {
            Destroy(clone);
        }

        UpdateColor();
    }

    [ContextMenu("2. Try Give Piercing")]
    private void GivePiercing()
    {
        BricksEnterTrigger(true);
    }

    [ContextMenu("2. Try Cancel Piercing")]
    private void CancelPiercing()
    {
        BricksEnterTrigger(false);
    }

    private void BricksEnterTrigger(bool isTrigger)
    {
        float maxMagNerf = _velocity * 0.65f;
        float maxMaxOri = 15.2f; //public original if necessary

        _bouncyBall.tag = isTrigger ? TagIndex(2) : TagIndex(0);
        {
            List<GameObject> bricks = new List<GameObject>(GameObject.FindGameObjectsWithTag("Brick: Normal"));
            foreach (GameObject brick in bricks)
            {
                if (brick.activeSelf)
                {
                    brick.GetComponent<BoxCollider2D>().isTrigger = isTrigger;
                }
            }
        }

        if (isTrigger)
        {
            MaxMagnitude = maxMagNerf;
        }
        else
        {
            MaxMagnitude = maxMaxOri; 
            _ballRB2D.velocity = _ballRB2D.velocity.normalized * _velocity;
        }

        UpdateColor();
    }

    [ContextMenu("3. Try Give Bouncy+")]
    private void GiveSuperBouncy()
    {
        BouncinessEnterTrigger(true);
    }

    [ContextMenu("3. Try Cancel Bouncy+")]
    private void CancelSuperBouncy()
    {
        BouncinessEnterTrigger(false);
    }

    private void BouncinessEnterTrigger(bool isTrigger)
    {
        float superVelMag = 15.0f;
        float oriVelMag = 6.0f;

        FourthWall.SetActive(isTrigger);
        _bouncyBall.tag = isTrigger ? TagIndex(3) : TagIndex(0);
        _velocity = isTrigger ? superVelMag : oriVelMag;

        UpdateColor();
    }

    [ContextMenu("4. Try Give Split")]
    private void GiveSplit()
    {
        if (!_isCloning)
        {
            _isCloning = true;
        }

        _bouncyBall.tag = TagIndex(4);
        UpdateColor();
    }

    [ContextMenu("4. Try Cancel Split")]
    private void CancelSplit()
    {
        DestroyClone();
    }

    private void SplitBall(GameObject obj)
    {
        GameObject newBall = Instantiate(BallPrefab, transform);
        Rigidbody2D newRB2D = newBall.GetComponent<Rigidbody2D>();
        Vector2 randomDirection = new Vector2(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(0f, 1f)).normalized;

        newBall.tag = TagIndex(1);
        _clonedBalls.Add(newBall);
        UpdateColor(newBall, _bouncyBall.tag); //assign bouncyBall's tag-color to newBall;

        newBall.transform.position = obj.transform.position;
        Rigidbody2D objRB2D = obj.GetComponent<Rigidbody2D>();
        newRB2D.velocity = objRB2D.velocity.magnitude * randomDirection;

        if (_clonedBalls.Count == 1) { SplitBall(obj); } //trigger twice at the first split
        _bouncyBall.SetActive(false);
    }

    [ContextMenu("0. Show High Score")]
    private void ShowHighScore()
    {
        Debug.Log("High Score: " + PlayerPrefs.GetString("High Score"));
    }

    [ContextMenu("0. Debug Log")]
    private void ShowDebugLog()
    {
        //Debug.Log($"Brick Count: {_brickCount}");
        Debug.Log(GameObject.Find("Level Manger") != null);
    }
}
