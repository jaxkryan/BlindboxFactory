using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Script.Controller;
using Script.Resources;
using Unity.VisualScripting;

namespace Script.Machine.ResourceManager {
    public class ResourceManager {
        public ReadOnlyDictionary<Resource, int> LockedResources {
            get => new(_lockedResources);
        }

        public Dictionary<Resource, int> _lockedResources = new();
        private HashSet<ResourceUse> _resourceUse = new();

        public void SetResourceUses(params ResourceUse[] uses) => _resourceUse = LinqUtility.ToHashSet(uses);

        public bool TryConsumeResources(int times, out int actualTimes) {
            actualTimes = 0;
            while (actualTimes < times) {
                foreach (var resourceUse in _resourceUse) {
                    if (!_lockedResources.TryGetValue(resourceUse.Resource, out var lockedAmount)
                        || lockedAmount < resourceUse.Amount) {
                        return actualTimes > 0;
                    }
                }
                foreach (var resourceUse in _resourceUse) {
                    _lockedResources[resourceUse.Resource] -= resourceUse.Amount;
                }
                
                actualTimes++;
            }

            return true;
        }

        public bool TryPullResource(int times, out int actualTimes) {
            Dictionary<Resource, int> resources = new();
            _resourceUse.ForEach(r => resources.TryAdd(r.Resource, 0));

            var controller = GameController.Instance.ResourceController;
            actualTimes = 0;
            while (actualTimes <= times) {
                foreach (var resourceUse in _resourceUse) {
                    var amount = resourceUse.Amount + resources[resourceUse.Resource];
                    if (controller.TryGetAmount(resourceUse.Resource, out var storedAmount) || storedAmount < amount) {
                        return actualTimes > 0;
                    }

                    resources[resourceUse.Resource] = amount;
                }

                resources.ForEach(r => {
                    if (_lockedResources.ContainsKey(r.Key)) _lockedResources[r.Key] += r.Value;
                    else _lockedResources.Add(r.Key, r.Value);
                });

                actualTimes++;
            }


            return true;
        }

        public bool HasResourcesForWork(out int count) {
            count = Int32.MaxValue;

            foreach (var resourceUse in _resourceUse) {
                if (!_lockedResources.TryGetValue(resourceUse.Resource, out var lockedAmount)) {
                    count = 0;
                    return false;
                }
                count = count > lockedAmount/resourceUse.Amount ? lockedAmount/resourceUse.Amount : count;
            }

            return count > 0;
        }

        public void UnlockResource() {
            UnlockResource(LockedResources.Keys?.ToArray());
        }

        public void UnlockResource(params Resource[] resources) {
            var controller = GameController.Instance.ResourceController;
            Func<Resource, int> newAmount = (resource) => {
                if (!_lockedResources.TryGetValue(resource, out var locked)) locked = 0;
                if (controller.TryGetAmount(resource, out var oldAmount)) oldAmount = 0;
                return locked + oldAmount;
            };
            foreach (var resource in resources) {
                if (newAmount(resource) > 0) controller.TrySetAmount(resource, newAmount(resource));
                _lockedResources.Remove(resource);
            }
        }
    }
}