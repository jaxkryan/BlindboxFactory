using Script.HumanResource.Worker;

namespace Script.Machine {
    public class TestMachine : MachineBase {
        TestWorker testWorker;
        void A() {
            testWorker.Machine = this;
            AddWorker(testWorker);
        }
    }
}