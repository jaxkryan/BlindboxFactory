public class AttackStrategy : IActionStrategy {
    public bool CanPerform => true; // Agent can always attack
    public bool Complete { get; private set; }

    readonly CountdownTimer timer;
    readonly AnimationController animations;

    public AttackStrategy(AnimationController animations) {
        this.animations = animations;
        timer = new CountdownTimer(animations.GetAnimationLength(animations.attackClip));
        timer.OnTimerStart += () => Complete = false;
        timer.OnTimerStop += () => Complete = true;
    }

    public void Start() {
        timer.Start();
        animations.Attack();
    }

    public void Update(float deltaTime) => timer.Tick(deltaTime);
}