using System;
using Script.HumanResource.Worker;
using Script.HumanResource.Worker.Workers;

namespace Script.Machine {
    [Serializable]
    public class TestMachine : MachineBase {
        TestWorker testWorker;
        void A() {
            testWorker.Machine = this;
            AddWorker(testWorker);
        }
    }
}