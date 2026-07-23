using System;
using TMPro;
using UnityEngine;

public class GameTimeManager : MonoBehaviour
{
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

        [Tooltip("이 날씨가 선택될 상대 확률입니다. 합계가 100일 필요는 없습니다.")]
        [Min(0f)]
        public float weight = 25f;

        [Tooltip("이 날씨일 때 꿀 생산량에 적용할 배율입니다.")]
        [Min(0f)]
        public float honeyProductionMultiplier = 1f;
    }

    [Header("하루 시간 설정")]
    [Tooltip("현실 시간 기준 하루의 길이입니다. 정식 값은 600초입니다.")]
    [Min(1f)]
    [SerializeField] private float dayDuration = 600f;

    [Tooltip("게임 시작과 동시에 시간이 흐를지 결정합니다.")]
    [SerializeField] private bool timeRunsAutomatically = true;

    [Tooltip("일시정지 상태에서도 시간이 흐르게 할지 결정합니다.")]
    [SerializeField] private bool useUnscaledTime = false;

    [Header("달력 설정")]
    [Tooltip("한 계절에 포함되는 날짜 수입니다.")]
    [Min(1)]
    [SerializeField] private int daysPerSeason = 10;

    [Tooltip("게임 시작 연도입니다.")]
    [Min(1)]
    [SerializeField] private int startingYear = 1;

    [Tooltip("게임 시작 계절입니다.")]
    [SerializeField] private Season startingSeason = Season.Spring;

    [Tooltip("게임 시작 날짜입니다.")]
    [Min(1)]
    [SerializeField] private int startingDay = 1;

    [Header("날씨 설정")]
    [Tooltip("게임 시작 시 날씨도 확률에 따라 무작위로 결정합니다.")]
    [SerializeField] private bool randomizeWeatherOnStart = true;

    [Tooltip("날짜가 바뀔 때마다 새로운 날씨를 뽑습니다.")]
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

    [Header("UI 표시 형식")]
    [SerializeField] private bool showYearInSeasonText = false;

    private float remainingTime;
    private int currentYear;
    private int currentDay;
    private Season currentSeason;
    private Weather currentWeather;
    private float currentHoneyProductionMultiplier = 1f;

    private bool isTimeRunning;
    private bool isChangingDay;

    public int CurrentYear => currentYear;
    public int CurrentDay => currentDay;
    public Season CurrentSeason => currentSeason;
    public Weather CurrentWeather => currentWeather;
    public float RemainingTime => remainingTime;
    public float HoneyProductionMultiplier => currentHoneyProductionMultiplier;

    private void Awake()
    {
        currentYear = Mathf.Max(1, startingYear);
        currentSeason = startingSeason;
        currentDay = Mathf.Clamp(startingDay, 1, daysPerSeason);

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

    private void Update()
    {
        if (!isTimeRunning || isChangingDay)
        {
            return;
        }

        float deltaTime = useUnscaledTime
            ? Time.unscaledDeltaTime
            : Time.deltaTime;

        remainingTime -= deltaTime;

        if (remainingTime <= 0f)
        {
            remainingTime = 0f;
            UpdateTimerUI();
            AdvanceToNextDay();
            return;
        }

        UpdateTimerUI();
    }

    /// <summary>
    /// Next Day 버튼에서 연결할 함수입니다.
    /// 현재 남은 시간과 관계없이 다음 날로 넘어갑니다.
    /// </summary>
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
            $"Year {currentYear}, {currentSeason}, Day {currentDay}, " +
            $"Weather: {currentWeather}, 생산 배율: {currentHoneyProductionMultiplier}"
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

        Debug.Log($"계절 변경: Year {currentYear}, {currentSeason}");
    }

    private void SelectRandomWeather()
    {
        if (weatherSettings == null || weatherSettings.Length == 0)
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

        float randomValue = UnityEngine.Random.Range(0f, totalWeight);
        float accumulatedWeight = 0f;

        foreach (WeatherSetting setting in weatherSettings)
        {
            if (setting == null)
            {
                continue;
            }

            accumulatedWeight += Mathf.Max(0f, setting.weight);

            if (randomValue <= accumulatedWeight)
            {
                currentWeather = setting.weather;
                currentHoneyProductionMultiplier =
                    Mathf.Max(0f, setting.honeyProductionMultiplier);

                return;
            }
        }

        WeatherSetting fallback = weatherSettings[weatherSettings.Length - 1];

        currentWeather = fallback.weather;
        currentHoneyProductionMultiplier =
            Mathf.Max(0f, fallback.honeyProductionMultiplier);
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
            if (setting != null && setting.weather == currentWeather)
            {
                currentHoneyProductionMultiplier =
                    Mathf.Max(0f, setting.honeyProductionMultiplier);

                return;
            }
        }

        currentHoneyProductionMultiplier = 1f;
    }

    private void UpdateAllUI()
    {
        UpdateSeasonUI();
        UpdateDayUI();
        UpdateWeatherUI();
        UpdateTimerUI();
    }

    private void UpdateSeasonUI()
    {
        if (seasonText == null)
        {
            return;
        }

        string seasonName = GetSeasonDisplayName(currentSeason);

        seasonText.text = showYearInSeasonText
            ? $"Year {currentYear}  {seasonName}"
            : seasonName;
    }

    private void UpdateDayUI()
    {
        if (dayText != null)
        {
            dayText.text = $"Day {currentDay}";
        }
    }

    private void UpdateWeatherUI()
    {
        if (weatherText != null)
        {
            weatherText.text = GetWeatherDisplayName(currentWeather);
        }
    }

    private void UpdateTimerUI()
    {
        if (timerText == null)
        {
            return;
        }

        int totalSeconds =
            Mathf.CeilToInt(Mathf.Max(0f, remainingTime));

        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;

        timerText.text = $"{minutes:00} : {seconds:00}";
    }

    private string GetSeasonDisplayName(Season season)
    {
        return season switch
        {
            Season.Spring => "Spring",
            Season.Summer => "Summer",
            Season.Autumn => "Autumn",
            Season.Winter => "Winter",
            _ => "Spring"
        };
    }

    private string GetWeatherDisplayName(Weather weather)
    {
        return weather switch
        {
            Weather.Sunny => "Sunny",
            Weather.Cloudy => "Cloudy",
            Weather.Rainy => "Rainy",
            Weather.Snowy => "Snowy",
            _ => "Sunny"
        };
    }

    public void PauseTime()
    {
        isTimeRunning = false;
    }

    public void ResumeTime()
    {
        isTimeRunning = true;
    }

    public void ToggleTime()
    {
        isTimeRunning = !isTimeRunning;
    }

    public void ResetCurrentDayTimer()
    {
        remainingTime = Mathf.Max(1f, dayDuration);
        UpdateTimerUI();
    }
}