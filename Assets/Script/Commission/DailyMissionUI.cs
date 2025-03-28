using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Script.Quest;
using System.Linq;
using Script.Controller;

public class DailyMissionUI : MonoBehaviour
{
    [SerializeField] private GameObject missionPrefab; // Assign in Inspector
    [SerializeField] private Transform missionContainer; // The Content object in ScrollView

    private List<Quest> missions;

    private DailyMissionController missionController => GameController.Instance.DailyMissionController;

    private void Start()
    {
        if (missionController == null)
        {
            Debug.LogError("DailyMissionController not found in the scene!");
            return;
        }
        SetMissions(missionController.DailyMissions);
    }

    public void SetMissions(List<Quest> dailyMissions)
    {
        missions = dailyMissions;
        RefreshUI();
    }

    private void RefreshUI()
    {
        foreach (Transform child in missionContainer)
        {
            Destroy(child.gameObject);
        }

        foreach (var mission in missions)
        {
            GameObject missionItem = Instantiate(missionPrefab, missionContainer);

            missionItem.transform.Find("MissionName").GetComponent<TextMeshProUGUI>().text = mission.Name;

            missionItem.transform.Find("Description").GetComponent<TextMeshProUGUI>().text = mission.Description;

            missionItem.transform.Find("State").GetComponent<TextMeshProUGUI>().text = mission.State.ToString();

            if (missionItem.transform.Find("ProgressBar") != null)
            {
                Slider progressBar = missionItem.transform.Find("ProgressBar").GetComponent<Slider>();
                float progress = mission.Objectives.Count > 0 ?
                    mission.Objectives.Count(o => o.Evaluate(mission)) / (float)mission.Objectives.Count : 0;
                progressBar.value = progress;
            }

            Button claimButton = missionItem.transform.Find("ClaimButton").GetComponent<Button>();
            claimButton.interactable = mission.State == QuestState.Complete;
            claimButton.onClick.AddListener(() => ClaimMission(mission));
        }
    }

    private void ClaimMission(Quest mission)
    {
        if (mission.State == QuestState.Complete)
        {
            mission.Rewards.ForEach(r => r.Grant());
            Debug.Log($"Rewards granted for {mission.Name}");
        }
    }
}
