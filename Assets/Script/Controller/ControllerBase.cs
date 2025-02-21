namespace Script.Controller {
    public abstract class ControllerBase {
        public virtual void OnUpdate(float deltaTime) { }
        public virtual void OnStart() { Load(); }
        public virtual void OnAwake() { }
        public virtual void OnDestroy() { Save(); }
        public virtual void OnEnable() { }
        public virtual void OnDisable() { }
        public abstract void Load();
        public abstract void Save();
    }
}