using System;
using System.Collections.Generic;
using System.Linq;
using MyBox;
using Script.Controller;
using Script.HumanResource.Worker;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WorkerNeedsContentUI : MonoBehaviour {
    [SerializeField] TextMeshProUGUI _workerType;
    [SerializeField] TextMeshProUGUI _description;
    [SerializeField] Image _portrait;
    [SerializeField] TextMeshProUGUI _cost;
    [SerializeField] Slider _happinessBar;
    [SerializeField] TextMeshProUGUI _happinessBonus;
    [SerializeField] Slider _hungerBar;
    [SerializeField] TextMeshProUGUI _hungerBonus;

    public WorkerType WorkerType { get; private set; }
    public int HappinessValue {get => (int)_happinessBar.value;}
    public int HungerValue {get => (int)_hungerBar.value;}

    public void Setup(WorkerController controller, WorkerType type) {
        if (!controller.WorkerList.TryGetValue(type, out var workers)) {
            Debug.LogError($"Worker type {type} not found");
            Destroy(gameObject);
        }

        _workerType.text = $"{workers.Count}x {ToName(workers.First().GetType().Name)}";
        _description.text = workers?.First().Description;
        _portrait.sprite = workers?.First().Portrait;
        
        var needs = controller.WorkerNeedsList.GetValueOrDefault(type);
        _happinessBar.value = needs.FirstOrDefault(c => c.Key == CoreType.Happiness).Value;
        _hungerBar.value = needs.FirstOrDefault(c => c.Key == CoreType.Hunger).Value;

        var needFloat = new Dictionary<CoreType, float>();
        needs.ForEach(n => needFloat.Add(n.Key, n.Value));
        
        var happinessBonusText = "";
        var hungerBonusText = "";
        var bonusManager = workers.First().BonusManager;
        foreach (var bonus in bonusManager.GetApplicableBonuses(needFloat)) {
            var condition = bonusManager.BonusConditions[bonus];

            if (condition.Conditions.ContainsKey(CoreType.Hunger)) {
                hungerBonusText += $"{bonus.Description}\n";
            }
            if (condition.Conditions.ContainsKey(CoreType.Happiness)) {
                happinessBonusText += $"{bonus.Description}\n";
            }
        }
        _happinessBonus.text = happinessBonusText;
        _hungerBonus.text = hungerBonusText;

        throw new NotImplementedException(nameof(_cost));

        string ToName(string name) {
            string separate = "";
            foreach (var c in name) {
                if (char.IsUpper(c)) separate += " ";
                separate += c;
            }
            var parts = separate.Split(' ').ToList();
            if (parts.Last().Equals("Worker")) parts.Remove(parts.Last());

            return string.Join(" ", parts).Trim();
        }
    }
}