using Script.Machine;
using UnityEngine;

public class IncreaseProgessPerSec : WorkDetail
{
    [SerializeField] public float IntervalSeconds;
    protected override void OnStart()
    {
        base.OnStart();

        _timer = new CountdownTimer(IntervalSeconds);
        _timer.OnTimerStop += () => {
            Machine.IncreaseProgress(1);
            _timer.Start();
        };
        _timer.Start();
    }


    protected override void OnUpdate(float deltaTime)
    {
        base.OnStop();
        _timer.Tick(deltaTime);
    }

    protected override void OnStop()
    {
        base.OnStop();
        _timer.Reset();
        _timer.Pause();
    }
}
