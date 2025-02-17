namespace Script.Controller {
    public abstract class ControllerBase {
        public virtual void OnUpdate(float deltaTime) { }
        public virtual void OnStart(){}
        public virtual void OnAwake() { }
        public virtual void OnDestroy() { }
        public virtual void OnEnable() { }
        public virtual void OnDisable() { }
    }
}