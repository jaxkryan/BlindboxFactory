using System.Collections.Generic;
using ZLinq;
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

    WorkerController _controller;
    
    public void Setup(WorkerController controller, WorkerType type) {
        _controller = controller;
        WorkerType = type;
        UpdateText();
    }

    public void OnSliderValueChanged() {
        UpdateText();
    }

    private void UpdateText() {
        if (!_controller.WorkerList.TryGetValue(WorkerType, out var workers)) {
            Debug.LogError($"Worker type {WorkerType} not found");
            Destroy(gameObject);
            return;
        }
        var needs = _controller.WorkerNeedsList.GetValueOrDefault(WorkerType);
        var needFloat = new Dictionary<CoreType, float>();
        needs.ForEach(n => needFloat.Add(n.Key, n.Value));

        _workerType.text = $"{workers.Count}x {workers.AsValueEnumerable().First().GetType().Name.ToNormalString(StringExtension.StringCapitalizationSetting.CapitalizeEachWords)}";
        _description.text = workers?.AsValueEnumerable().First().Description;
        _portrait.sprite = workers?.AsValueEnumerable().First().Portrait;
        
        _happinessBar.value = needs.AsValueEnumerable().FirstOrDefault(c => c.Key == CoreType.Happiness).Value;
        _hungerBar.value = needs.AsValueEnumerable().FirstOrDefault(c => c.Key == CoreType.Hunger).Value;

        var happinessBonusText = "";
        var hungerBonusText = "";
        
        foreach (var bonus in workers.AsValueEnumerable().First().Bonuses.AsValueEnumerable().Where(b => b.Condition.IsApplicable(b.Worker))) {
            var condition = bonus.Condition;
        
            if (condition.Conditions.ContainsKey(CoreType.Hunger)) {
                hungerBonusText += $"{bonus.Description}\n";
            }
            if (condition.Conditions.ContainsKey(CoreType.Happiness)) {
                happinessBonusText += $"{bonus.Description}\n";
            }
        }
        _happinessBonus.text = happinessBonusText;
        _hungerBonus.text = hungerBonusText;
        
#warning Cost
    }
}