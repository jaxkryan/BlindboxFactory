using Script.Controller.SaveLoad;

namespace Script.Controller {
    public abstract class ControllerBase : ISavable {
        public virtual void OnUpdate(float deltaTime) { }
        public virtual void OnStart() {  }
        public virtual void OnAwake() { }
        public virtual void OnDestroy() { Save(); }
        public virtual void OnEnable() { }
        public virtual void OnDisable() { }
        public abstract void Load();
        public abstract void Save();
        public virtual void OnValidate() { }
    }
}