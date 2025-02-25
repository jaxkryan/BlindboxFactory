using System;
using System.Collections.Generic;
using Script.Controller;
using Script.HumanResource.Worker;
using UnityEngine;
using UnityEngine.UI;

public class WorkerNeedsAutomatedUI : UIScrollable {

    public override void Clear() {
        var children = _container.GetComponentsInChildren<WorkerNeedsContentUI>();
        children.ForEach(c => Destroy(c.gameObject));
    }

    public override void Spawn() {
        var list = GameController.Instance.WorkerController.WorkerList;
        if (list?.Keys is null) return;
        foreach (var workerType in list.Keys) {
            var obj = Instantiate(_itemPrefab, _container.transform);
            var content = obj.GetComponent<WorkerNeedsContentUI>();
            content.Setup(GameController.Instance.WorkerController, workerType);
        }
    }

    public override void Save() {
        var children = _container.GetComponentsInChildren<WorkerNeedsContentUI>();
        var controller = GameController.Instance.WorkerController;
        children.ForEach(c => 
            controller.SetNeeds(c.WorkerType, (CoreType.Happiness, c.HappinessValue), (CoreType.Hunger, c.HungerValue)));
        
        base.Save();
    }

}
