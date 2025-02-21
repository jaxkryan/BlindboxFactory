using System;
using System.Collections.Generic;
using System.Linq;
using MyBox;
using Script.Controller;
using Script.HumanResource.Worker;
using Script.Machine;
using Script.Machine.WorkDetails;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
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
    [SerializeField] Slider _restBar;
    [SerializeField] TextMeshProUGUI _restBonus;

    public WorkerType WorkerType { get; private set; }
    public int HappinessValue {get => (int)_happinessBar.value;}
    public int HungerValue {get => (int)_hungerBar.value;}
    public int RestValue {get => (int)_restBar.value;}

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
        var restBonusText = "";
        
        foreach (var bonus in workers.First().Bonuses.Where(b => b.Condition.IsApplicable(b.Worker))) {
            var condition = bonus.Condition;
        
            if (condition.Conditions.ContainsKey(CoreType.Hunger)) {
                hungerBonusText += $"{bonus.Description}\n";
            }
            if (condition.Conditions.ContainsKey(CoreType.Happiness)) {
                happinessBonusText += $"{bonus.Description}\n";
            }
            if (condition.Conditions.ContainsKey(CoreType.Rest)) {
                restBonusText += $"{bonus.Description}\n";
            }
        }
        _happinessBonus.text = happinessBonusText;
        _hungerBonus.text = hungerBonusText;
        _restBonus.text = restBonusText;

        float GetGoldCost(CoreType core, WorkerType worker) {
        //Calculating the estimated cost for each core
        //Get all workers of the worker type
        var workers = GameController.Instance.WorkerController.WorkerList.GetValueOrDefault(worker);
        if (workers is null || workers.Count == 0) return 0f;
        
        var machines = 
            GameController.Instance.MachineController.RecoveryMachines.Where(m =>
                m.Value.Any(recovery => recovery.Worker == worker && recovery.Core == core)).Select(m => m.Key).ToList();
        if (machines.Count == 0) return 0f;
        foreach (var w in workers) {
            MachineBase closest;
            float dist = float.MaxValue;
            //Find the nearest building that increase that core
            foreach (var m in machines) {
                NavMeshPath path = new();
                if (!w.Agent.CalculatePath(m.transform.position, path)) continue;
                if (path.GetLength() > dist) continue;
                
                closest = m;
                dist = path.GetLength();
            }
            
            //Get its operational cost
            closest.WorkDetails.Where(d => d is ResourceConsumptionWorkDetail)
                
            //Add to the total sum
        }

        }
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