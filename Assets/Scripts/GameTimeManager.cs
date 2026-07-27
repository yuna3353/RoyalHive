using System;
using TMPro;
using UnityEngine;

public class GameTimeManager : MonoBehaviour
{
    public static GameTimeManager Instance { get; private set; }

    public enum Season
    {
        Spring,
        Summer,
        Autumn,
        Winter
    }

    public enum Weather
    {
        Sunny,
        Cloudy,
        Rainy,
        Snowy
    }

    [Serializable]
    public class WeatherSetting
    {
        [Tooltip("날씨 종류")]
        public Weather weather;

        [Tooltip("해당 날씨가 선택될 상대적인 확률")]
        [Min(0f)]
        public float weight = 25f;

        [Tooltip("해당 날씨에서 꿀 생산량에 적용할 배율")]
        [Min(0f)]
        public float honeyProductionMultiplier = 1f;
    }

    [Header("하루 시간 설정")]
    [Tooltip("현실 시간 기준 하루 길이입니다. 정식 값은 600초입니다.")]
    [Min(1f)]
    [SerializeField] private float dayDuration = 600f;

    [Tooltip("게임 시작과 동시에 시간을 흐르게 합니다.")]
    [SerializeField] private bool timeRunsAutomatically = true;

    [Header("달력 설정")]
    [Min(1)]
    [SerializeField] private int daysPerSeason = 10;

    [Min(1)]
    [SerializeField] private int startingYear = 1;

    [SerializeField] private Season startingSeason = Season.Spring;

    [Min(1)]
    [SerializeField] private int startingDay = 1;

    [Header("날씨 설정")]
    [SerializeField] private bool randomizeWeatherOnStart = true;
    [SerializeField] private bool randomizeWeatherEveryDay = true;

    [SerializeField]
    private WeatherSetting[] weatherSettings =
    {
        new WeatherSetting
        {
            weather = Weather.Sunny,
            weight = 25f,
            honeyProductionMultiplier = 1.2f
        },
        new WeatherSetting
        {
            weather = Weather.Cloudy,
            weight = 25f,
            honeyProductionMultiplier = 0.8f
        },
        new WeatherSetting
        {
            weather = Weather.Rainy,
            weight = 25f,
            honeyProductionMultiplier = 0.5f
        },
        new WeatherSetting
        {
            weather = Weather.Snowy,
            weight = 25f,
            honeyProductionMultiplier = 0.2f
        }
    };

    [Header("UI 연결")]
    [SerializeField] private TMP_Text seasonText;
    [SerializeField] private TMP_Text dayText;
    [SerializeField] private TMP_Text weatherText;
    [SerializeField] private TMP_Text timerText;

    private int currentYear;
    private int currentDay;
    private Season currentSeason;
    private Weather currentWeather;

    private float remainingTime;
    private float currentHoneyProductionMultiplier = 1f;

    private bool isTimeRunning;
    private bool isChangingDay;

    public int CurrentYear => currentYear;
    public int CurrentDay => currentDay;
    public Season CurrentSeason => currentSeason;
    public Weather CurrentWeather => currentWeather;
    public float RemainingTime => remainingTime;
    public float HoneyProductionMultiplier =>
        currentHoneyProductionMultiplier;

    private void Awake()
    {
        // Scene 안에 GameTimeManager가 두 개 생기는 것을 방지한다.
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning(
                "GameTimeManager가 두 개 존재하여 중복 오브젝트를 제거합니다.",
                gameObject
            );

            Destroy(gameObject);
            return;
        }

        Instance = this;

        currentYear = Mathf.Max(1, startingYear);
        currentSeason = startingSeason;
        currentDay = Mathf.Clamp(
            startingDay,
            1,
            Mathf.Max(1, daysPerSeason)
        );

        remainingTime = Mathf.Max(1f, dayDuration);
        isTimeRunning = timeRunsAutomatically;

        if (randomizeWeatherOnStart)
        {
            SelectRandomWeather();
        }
        else
        {
            currentWeather = Weather.Sunny;
            ApplyCurrentWeatherMultiplier();
        }

        UpdateAllUI();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void Update()
    {
        if (!isTimeRunning || isChangingDay)
        {
            return;
        }

        remainingTime -= Time.deltaTime;

        if (remainingTime <= 0f)
        {
            remainingTime = 0f;
            AdvanceToNextDay();
            return;
        }

        UpdateTimerUI();
    }

    public void SkipToNextDay()
    {
        if (isChangingDay)
        {
            return;
        }

        AdvanceToNextDay();
    }

    private void AdvanceToNextDay()
    {
        isChangingDay = true;

        currentDay++;

        if (currentDay > daysPerSeason)
        {
            currentDay = 1;
            AdvanceSeason();
        }

        if (randomizeWeatherEveryDay)
        {
            SelectRandomWeather();
        }

        remainingTime = Mathf.Max(1f, dayDuration);

        Debug.Log(
            $"Year {currentYear}, " +
            $"{currentSeason}, " +
            $"Day {currentDay}, " +
            $"{currentWeather}"
        );

        UpdateAllUI();

        isChangingDay = false;
    }

    private void AdvanceSeason()
    {
        switch (currentSeason)
        {
            case Season.Spring:
                currentSeason = Season.Summer;
                break;

            case Season.Summer:
                currentSeason = Season.Autumn;
                break;

            case Season.Autumn:
                currentSeason = Season.Winter;
                break;

            case Season.Winter:
                currentSeason = Season.Spring;
                currentYear++;
                break;
        }
    }

    private void SelectRandomWeather()
    {
        if (weatherSettings == null ||
            weatherSettings.Length == 0)
        {
            currentWeather = Weather.Sunny;
            currentHoneyProductionMultiplier = 1f;
            return;
        }

        float totalWeight = 0f;

        foreach (WeatherSetting setting in weatherSettings)
        {
            if (setting != null)
            {
                totalWeight += Mathf.Max(0f, setting.weight);
            }
        }

        if (totalWeight <= 0f)
        {
            currentWeather = Weather.Sunny;
            currentHoneyProductionMultiplier = 1f;
            return;
        }

        float randomValue = UnityEngine.Random.Range(
            0f,
            totalWeight
        );

        float accumulatedWeight = 0f;

        foreach (WeatherSetting setting in weatherSettings)
        {
            if (setting == null)
            {
                continue;
            }

            accumulatedWeight += Mathf.Max(
                0f,
                setting.weight
            );

            if (randomValue <= accumulatedWeight)
            {
                currentWeather = setting.weather;
                currentHoneyProductionMultiplier =
                    Mathf.Max(
                        0f,
                        setting.honeyProductionMultiplier
                    );

                return;
            }
        }
    }

    private void ApplyCurrentWeatherMultiplier()
    {
        if (weatherSettings == null)
        {
            currentHoneyProductionMultiplier = 1f;
            return;
        }

        foreach (WeatherSetting setting in weatherSettings)
        {
            if (setting != null &&
                setting.weather == currentWeather)
            {
                currentHoneyProductionMultiplier =
                    setting.honeyProductionMultiplier;

                return;
            }
        }

        currentHoneyProductionMultiplier = 1f;
    }

    private void UpdateAllUI()
    {
        if (seasonText != null)
        {
            seasonText.text = currentSeason.ToString();
        }

        if (dayText != null)
        {
            dayText.text = $"Day {currentDay}";
        }

        if (weatherText != null)
        {
            weatherText.text = currentWeather.ToString();
        }

        UpdateTimerUI();
    }

    private void UpdateTimerUI()
    {
        if (timerText == null)
        {
            return;
        }

        int totalSeconds = Mathf.CeilToInt(
            Mathf.Max(0f, remainingTime)
        );

        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;

        timerText.text = $"{minutes:00} : {seconds:00}";
    }

    public void PauseTime()
    {
        isTimeRunning = false;
    }

    public void ResumeTime()
    {
        isTimeRunning = true;
    }
}