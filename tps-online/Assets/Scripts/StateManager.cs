using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StateManager : MonoBehaviour
{
    // Start is called before the first frame update
    public int HP = 100;
    public int gunBullet = 10;
    public int bagBullet = 100;
    public int score;
    private GameManager gameMgr;

    // GUI
    [SerializeField]
    public Slider HPSlider;
    [SerializeField]
    public GameObject HPSlider_Handler;
    [SerializeField]
    public GameObject HPSlider_Fill;
    [SerializeField]
    public Text bagBulletText;
    [SerializeField]
    public Text gunBulletText;
    [SerializeField]
    public Text scoreText;
    [SerializeField]
    public GameObject overCanvas;
    [SerializeField]
    public InputField content;
    public Queue<string> contentQueue;

    void Start()
    {
        score = 0;
        HP = 100;
        gunBullet = 10;
        bagBullet = 100;
        overCanvas.SetActive(false);
        content.text = "Welcome";
        contentQueue = new Queue<string>();
        gameMgr = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateGUI();
    }
    public void UpdateGUI()
    {
        if(HP == 0) 
        {
            HPSlider_Handler.SetActive(false);
            HPSlider_Fill.SetActive(false);
        }
        HPSlider.value = HP * 0.01f;
        scoreText.text = score.ToString();
        gunBulletText.text = gunBullet.ToString();
        bagBulletText.text = bagBullet.ToString();
        while(contentQueue.Count > 4) {
            contentQueue.Dequeue();
        }
        content.text = string.Join("\n", contentQueue.ToArray());
    }

    public void GameOver()
    {
        overCanvas.SetActive(true);
    }

    public void LeaveRoom()
    {
        gameMgr.LeaveRoom();
    }
}
