using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class GridManager : MonoBehaviour
{
    [SerializeField]
    private int rows = 1;
    public int Rows
    {
        get { return rows; }
        set { rows = value; }
    }

    [SerializeField]
    private int columns = 1;
    public int Columns
    {
        get { return columns; }
        set { columns = value; }
    }

    [SerializeField]
    private float tileScale = 1.0f;
    public float TileScale
    {
        get { return tileScale; }
        set { tileScale = value; }
    }

    private readonly HashSet<int> selectedTiles = new HashSet<int>();

    private double? lastInterval;
    private int countColumn, countRow;

    private AudioManager audioManager;

    public GameObject tileObject;
    public Sprite selectedSprite;
    public Sprite countingSprite;

    public Button PlayOrPause;

    #region Private

    private void PrepareSound()
    {
        audioManager = FindObjectOfType<AudioManager>();
    }

    private void GenerateGrid()
    {
        tileObject.transform.localScale = new Vector3(tileScale, tileScale);
        var size = tileObject.GetComponent<SpriteRenderer>().bounds.size;
        //size.x *= tileScale;
        //size.y *= tileScale;

        float xOrigin = (float)((Columns - 1) * size.x * -0.5);
        float yOrigin = (float)((Rows - 1) * size.y * -0.5);

        float xPos = xOrigin;
        float yPos = yOrigin;

        for (var i = 0; i < Rows; i++)
        {
            for (var j = 0; j < Columns; j++)
            {
                var newTile = Instantiate(tileObject);

                newTile.transform.parent = transform;
                newTile.name = (j + i * Columns).ToString();
                newTile.transform.position = new Vector3(xPos, yPos, 0);
                xPos += size.x;
            }
            xPos = xOrigin;
            yPos += size.y;
        }
    }
    #endregion

    private void Awake()
    {
        Debug.Assert(tileObject != null);
        PrepareSound();
    }

    // Start is called before the first frame update
    void Start()
    {
        transform.position = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width / 2, Screen.height / 2, Camera.main.nearClipPlane));
        GenerateGrid();
    }

    void StartCountinig()
    {
        Debug.Log("Started counting");
        lastInterval = Time.realtimeSinceStartup;
        countColumn = 0;
        countRow = 0;
    }

    void StopCountring()
    {
        lastInterval = null;
    }

    bool IsCounting
    {
        get { return lastInterval != null; }
    }

    void UpdateCount()
    {
        if (!IsCounting)
            return;

        var diff = (int)Mathf.Ceil((float)(Time.realtimeSinceStartup - lastInterval));
        var diffCondition = diff >= 2.5 * 1.0 / Math.Max(float.Epsilon, Time.timeScale);
        if (diffCondition || countRow == 0 && countColumn == 0)
        {
            if (diffCondition)
            {
                lastInterval = Time.realtimeSinceStartup;
                var oldIndex = countRow * Columns + countColumn;
                ChangeChildState(oldIndex.ToString(), selectedTiles.Contains(oldIndex));

                if (Rows - 1 == countRow)
                {
                    countRow = 0;
                    if (countColumn == Columns - 1)
                    {
                        countColumn = 0;
                    }
                    else
                        countColumn += 1;
                }
                else
                    countRow += 1;
            }

            var index = countRow * Columns + countColumn;
            if (selectedTiles.Contains(index)) {
                Debug.Assert(index >= 0 && index < Rows);
                audioManager.Play(countRow);
            } else {
                ChangeChildState(index.ToString(), countingSprite);
            }

            Debug.LogFormat("Row {0} Column {1}", countRow, countColumn);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("Position " + Input.mousePosition);
            HandlePosition(Input.mousePosition);
        }
        else
            HandleTouchOnNeed();
    }

    void FixedUpdate()
    {
        UpdateCount();
    }

    private Animator PlayOrPauseAnimator => PlayOrPause.GetComponent<Animator>();

    public void OnPlayPressed()
    {
        bool playClick;
        if (IsCounting) {
            StopCountring();
            playClick = false;
        } else {
            playClick = true;
            StartCountinig();
        }
        PlayOrPauseAnimator.SetBool("PlayClick", playClick);
        //TODO: change image or smth..
    }

    private void HandleTouchOnNeed()
    {
        foreach (var touch in Input.touches)
        {
            if (touch.phase == TouchPhase.Began)
            {
                //Debug.Log("!!! Position " + touch.position);
                HandlePosition(touch.position);
            }
        }
    }

    private void HandlePosition(Vector2 position)
    {
        var wp = Camera.main.ScreenToWorldPoint(position);
        var touchPosition = new Vector2(wp.x, wp.y);

        var collider = Physics2D.OverlapPoint(touchPosition);
        if (!collider)
            return;

        if (int.TryParse(collider.gameObject.name, out int res))
        {
            var rowIndex = res / Columns;
            var colIndex = res - rowIndex * Columns;
            Debug.Assert(rowIndex <= Rows);
            Debug.Assert(colIndex <= Columns);
            Debug.Log("!!! Item " + res);
            SwapChildState(res);
        }
    }

    void SwapChildState(int index)
    {
        if (selectedTiles.Contains(index))
            selectedTiles.Remove(index);
        else
            selectedTiles.Add(index);

        var shouldPlay = selectedTiles.Contains(index);
        var rowIndex = index / Rows;
        if (shouldPlay)
            audioManager.Play(rowIndex, true);
        else
            audioManager.StopPlaying(rowIndex);

        ChangeChildState(index.ToString(), shouldPlay);
    }

    void ChangeChildState(string name, bool selected)
    {
        ChangeChildState(name, selected, selectedSprite, tileObject.GetComponent<SpriteRenderer>().sprite);
    }

    void ChangeChildState(string name, bool selected, Sprite selectedSprite, Sprite nonSelectedSprite)
    {
        ChangeChildState(name, selected ? selectedSprite : nonSelectedSprite);
    }

    void ChangeChildState(string name, Sprite sprite)
    {
        var childTranform = gameObject.transform.Find(name);
        var renderer = childTranform.GetComponent<SpriteRenderer>();
        renderer.sprite = sprite;
    }

}
