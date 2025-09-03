using System.Linq;
using TMPro;
using UnityEngine;

public class RaceManager : MonoBehaviour
{
    public static RaceManager Instance;

    [SerializeField] private int totalLaps = 3;

    [SerializeField] private TextMeshProUGUI lapCountText;
    [SerializeField] private TextMeshProUGUI lastLapTimeText;
    [SerializeField] private TextMeshProUGUI bestLapTimeText;

    private int currentLap = 0;
    private bool raceStarted = false;

    private float sectorStartTime;
    private float sectorEndTime;
    private float[] sectorTimes = new float[3];
    private float lapTime;
    private float bestLapTime = Mathf.Infinity;

    private void Awake()
    {
        Instance = this;
    }
    public void CheckpointEntered(int checkPointIndex)
    {
        if (checkPointIndex == 0 && raceStarted == false)
        {
            raceStarted = true;
            sectorStartTime = Time.time;

        }
        if (raceStarted == true && checkPointIndex == 1)
        {
            sectorEndTime = Time.time;
            sectorTimes[0] = sectorEndTime - sectorStartTime;
            sectorStartTime = Time.time;
        }
        if (raceStarted == true && checkPointIndex == 2)
        {
            sectorEndTime = Time.time;
            sectorTimes[1] = sectorEndTime - sectorStartTime;
            sectorStartTime = Time.time;
        }
        if (raceStarted == true && checkPointIndex == 0)
        {
            sectorEndTime = Time.time;
            sectorTimes[2] = sectorEndTime - sectorStartTime;
            sectorStartTime = Time.time;
            lapTime = sectorTimes.Sum();
            Debug.Log("Lap Time: " + lapTime);
            Debug.Log("Lap " + currentLap + "completed.");
            currentLap++;
            UpdateVisuals();
        }
    }

    private void UpdateVisuals()
    {
        if (lapTime < bestLapTime && lapTime != 0) bestLapTime = lapTime;
        lapCountText.text = "Lap: " + currentLap + "/" + totalLaps;
        bestLapTimeText.text = bestLapTime.ToString() + "\n\n";
        lastLapTimeText.text = lapTime.ToString();
    }
}
