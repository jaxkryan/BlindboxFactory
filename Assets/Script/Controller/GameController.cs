using Script.Gacha.Shard;

namespace Script.Controller {
    public class GameController : PersistentSingleton<GameController> {
        public MachineController MachineController = new ();
        public WorkerController WorkerController = new ();
        public IShardController ShardController = new ShardController();
    }
}