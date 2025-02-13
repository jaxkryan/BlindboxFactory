using Script.Gacha.Shard;
using Script.HumanResource.Administrator;

namespace Script.Controller {
    public class GameController : PersistentSingleton<GameController> {
        public MachineController MachineController = new ();
        public WorkerController WorkerController = new ();
        public ShardController ShardController = new ();
        public AdministratorController AdministratorController = new();

        protected override void Awake() {
            base.Awake();

            Load();
        }

        private void Load() { }
    }
}