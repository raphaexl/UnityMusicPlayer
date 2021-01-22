
using System;
using UnityEngine;
using UnityEngine.UI;

enum MusicState
{
    PAUSED, PLAYING
};

[System.Serializable]
public class Song
{
    public string name;
    public AudioClip clip;
    [HideInInspector]
    public AudioSource source;
};

public class AppManager : MonoBehaviour
{
    [SerializeField] Song[] songs;
    [SerializeField] Text title;
    [SerializeField] Text elapsedTime;
    [SerializeField] Text remainingTime;
    [SerializeField] Slider slider;
    [SerializeField] Button prevButton;
    [SerializeField] Button playButton;
    [SerializeField] Button pauseButton;
    [SerializeField] Button nextButton;
    [SerializeField] Button exitButton;

    private Song currentSong;
    private int currentIndex;
    private MusicState state;

    private string originalText;
    private float intervalTime;

    public float Duration
    {
        get { return  currentSong.source.clip.length; }
    }

    public float Time
    {
        set { currentSong.source.time = value; }
        get { return  currentSong.source.time; }
    }


    private void Awake()
    {
        Array.ForEach<Song>(songs, (song) =>
        {
            song.source = gameObject.AddComponent<AudioSource>();
            song.source.clip = song.clip;
            song.source.volume = 1.0f;
        });
    }

    string LoopOnString(string str)
    {
        int length = str.Length;
        char[] ch = str.ToCharArray();
        char temp;
        temp = ch[0];
        for (int i = 0; i < length - 1; i++)
        {
            ch[i] = ch[i + 1];
        }
        ch[length - 1] = temp;
        return new string(ch);
    }
    // Start is called before the first frame update
    void Start()
    {
        currentSong = songs[0];
        currentIndex = 0;
        title.text = currentSong.name;
        originalText = String.Copy(currentSong.name) + " ";
        SetupUI();
        UpdateUI();
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
    }

    void SetupUI()
    {
        prevButton.onClick.AddListener(prevSong);
        nextButton.onClick.AddListener(nextSong);
        playButton.onClick.AddListener(Play);
        pauseButton.onClick.AddListener(Pause);
        exitButton.onClick.AddListener(Exit);
        slider.onValueChanged.AddListener(delegate { SliderTime(slider); });
    }

    void UpdateSlider()
    {
        slider.minValue = 0;
        slider.maxValue = Duration - 0.01f;
        slider.value = Time;
    }

    void SliderTime(Slider slider)
    {
        if (currentSong.source)
        currentSong.source.time = slider.value;
    }

    void UpdateSong()
    {
        Pause();
        Stop();
        currentSong = songs[currentIndex];
        title.text = currentSong.name;
        originalText = String.Copy(currentSong.name) + " ";
        Time = 0;
    }

    private void Play()
    {
        state = MusicState.PLAYING;
        currentSong.source.Play();
        UpdateUI();
    }

    private void Pause()
    {
        state = MusicState.PAUSED;
        currentSong.source.Pause();
        UpdateUI();
    }

    private void Stop()
    {
        currentSong.source.Stop();
    }

    private void prevSong()
    {
        currentIndex -= 1;
        if (currentIndex < 1)
        {
            currentIndex = 0;
        }
        UpdateSong();
    }

    private void nextSong()
    {
        currentIndex += 1;
        if (currentIndex > songs.Length - 1)
        {
            currentIndex = songs.Length - 1;
        }
        UpdateSong();
    }

    void UpdateUI()
    {
        playButton.gameObject.SetActive(false);
        pauseButton.gameObject.SetActive(false);
        switch (state)
        {
            case MusicState.PAUSED:
                playButton.gameObject.SetActive(true);
                pauseButton.gameObject.SetActive(false);
                break;
            case MusicState.PLAYING:
                playButton.gameObject.SetActive(false);
                pauseButton.gameObject.SetActive(true);
                break;
        }
    }

    
    void UpdateUIText()
    {
        elapsedTime.text = SetTimeString((float)Time);
        remainingTime.text = SetTimeString((float)(Duration - Time) < 0 ? 0 : (float)(Duration - Time));
    }

    string SetTimeString(float time, bool isLong = false)
    {
        float hf = time / 3600f;
        int hours = (int)Mathf.Floor(hf);
        float mf = (hf - hours) * 60f;
        int minutes = (int)Mathf.Floor(mf);
        int seconds = (int)Mathf.Floor((mf - minutes) * 60);
        string msg = "";
        if (isLong)
        {
            if (hours < 10)
            {
                msg += "0";
            }
            msg += hours.ToString();
            msg += ":";
        }
        if (minutes < 10)
        {
            msg += "0";
        }
        msg += minutes.ToString();
        msg += ":";
        if (seconds < 10)
        {
            msg += "0";
        }
        msg += seconds.ToString();
        return msg;
    }

    void AutoPlay()
    {
        if (Time >= Duration)
        {
            nextSong();
            Play();
        }
    }

    void ScrollingText()
    {
        originalText = LoopOnString(originalText);
        title.text = originalText;
        //   yield return WaitForSecondsRealtime();
    }

    private void Update()
    {
        AutoPlay();
        UpdateSlider();
        UpdateUIText();

        if (currentSong.source.isPlaying && intervalTime > 0.5)
        {
            ScrollingText();
            intervalTime = 0;
        }
        intervalTime += UnityEngine.Time.deltaTime;
    }

    private void Exit()
    {
        Application.Quit();
    }
}
