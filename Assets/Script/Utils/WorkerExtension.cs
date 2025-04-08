using Script.Controller;
using Script.HumanResource.Worker;

namespace Script.Utils {
    public static class WorkerExtension {
        public static WorkerType ToWorkerType(this Worker worker) => IWorker.ToWorkerType(worker);
    }
}