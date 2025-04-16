using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Script.Controller;
using Script.Machine.Machines.Canteen;
using Script.Resources;
using Unity.VisualScripting;
using UnityEngine;

namespace Script.Machine.ResourceManager
{
    public class ResourceManager
    {
        public ReadOnlyDictionary<Resource, int> LockedResources
        {
            get => new(_lockedResources);
        }

        private Dictionary<Resource, int> _lockedResources = new();
        private List<ResourceUse> _resourceUse = new();
        private MachineBase _machine;

        public ResourceManager(MachineBase machine)
        {
            _machine = machine;
        }

        public void SetResourceUses(params ResourceUse[] uses) => _resourceUse = uses.ToList();

        public bool TryConsumeResources(int times, out int actualTimes)
        {
            actualTimes = 0;

            while (actualTimes < times)
            {
                foreach (var resourceUse in _resourceUse)
                {
                    if (!_lockedResources.TryGetValue(resourceUse.Resource, out var lockedAmount)
                        || lockedAmount < resourceUse.Amount) { return actualTimes > 0; }
                }

                foreach (var resourceUse in _resourceUse)
                {
                    _lockedResources[resourceUse.Resource] -= resourceUse.Amount;
                }

                actualTimes++;
            }

            return true;
        }

        public bool TryConsumeResources(ResourceUse resourceUse, int times, out int actualTimes)
        {
            actualTimes = 0;
            while (actualTimes < times)
            {
                if (!_lockedResources.TryGetValue(resourceUse.Resource, out var lockedAmount)
                    || lockedAmount < resourceUse.Amount)
                {
                    return actualTimes > 0;
                }

                _lockedResources[resourceUse.Resource] -= resourceUse.Amount;

                actualTimes++;
            }

            return true;
        }

        public bool TryPullResource(int times, out int actualTimes)
        {
            Dictionary<Resource, int> resources = new();
            _resourceUse.ForEach(r => resources.TryAdd(r.Resource, 0));

            actualTimes = 0;
            while (actualTimes < times)
            {
                foreach (var resourceUse in _resourceUse)
                {
                    var storage = ResourceStorageInterface.Get(resourceUse.Resource, _machine);
                    var amount = resourceUse.Amount;
                    if (!storage.TryGetAmount(out var storedAmount) || storedAmount < amount)
                    {
                        return actualTimes > 0;
                    }

                    resources[resourceUse.Resource] = amount;
                }

                resources.ForEach(r =>
                {
                    var storage = ResourceStorageInterface.Get(r.Key, _machine);
                    if (_lockedResources.ContainsKey(r.Key)) _lockedResources[r.Key] += r.Value;
                    else _lockedResources.Add(r.Key, r.Value);

                    if (storage.TryGetAmount(out var storedAmount))
                        storage.TrySetAmount(storedAmount - r.Value);
                });

                actualTimes++;
            }


            return true;
        }

        public bool HasResourcesForWork(out int count)
        {
            count = Int32.MaxValue;
            foreach (var resourceUse in _resourceUse)
            {
                if (!_lockedResources.TryGetValue(resourceUse.Resource, out var lockedAmount))
                {
                    count = 0;
                    return false;
                }

                if (resourceUse.Amount > 0) count = count > lockedAmount / resourceUse.Amount ? lockedAmount / resourceUse.Amount : count;
            }

            return count > 0;
        }

        public void UnlockResource() { UnlockResource(LockedResources.Keys?.ToArray()); }

        public void UnlockResource(params Resource[] resources)
        {
            Func<Resource, long> newAmount = (resource) =>
            {
                var storage = ResourceStorageInterface.Get(resource, _machine);
                if (!_lockedResources.TryGetValue(resource, out var locked)) locked = 0;
                if (!storage.TryGetAmount(out var oldAmount)) oldAmount = 0;
                return locked + oldAmount;
            };
            foreach (var resource in resources)
            {
                if (!_lockedResources.ContainsKey(resource) || _lockedResources[resource] <= 0) continue;
                var storage = ResourceStorageInterface.Get(resource, _machine);
                Debug.LogWarning($"Resource {resource}, Unlock: {newAmount(resource)}");
                if (newAmount(resource) > 0) storage.TrySetAmount(newAmount(resource));
                _lockedResources.Remove(resource);
            }
        }

        #region Resource storage
        private abstract class ResourceStorageInterface
        {
            public Resource Resource;
            public abstract bool TrySetAmount(long amount);
            public abstract bool TryGetAmount(out long amount);
            protected ResourceStorageInterface(Resource resource) => Resource = resource;

            public static ResourceStorageInterface Get(Resource resource, MachineBase machine)
            {
                var defaultResources = new List<Resource> {
                    Resource.Gold,
                    Resource.Gem,
                    Resource.Diamond,
                    Resource.Cloud,
                    Resource.Rainbow,
                    Resource.Gummy,
                    Resource.Ruby,
                    Resource.Star,
                };
                return resource switch
                {
                    Resource.Meal => new MealStorageInterface(machine),
                    _ => defaultResources.Contains(resource) ? new ControllerStorageInterface(resource) : throw new ArgumentOutOfRangeException(nameof(resource), resource, null)
                };
            }
        }
        private class ControllerStorageInterface : ResourceStorageInterface
        {
            ResourceController _controller;
            public ControllerStorageInterface(Resource resource) : base(resource)
            {
                _controller = GameController.Instance.ResourceController;
            }

            public override bool TrySetAmount(long amount) => _controller.TrySetAmount(Resource, amount);
            public override bool TryGetAmount(out long amount) => _controller.TryGetAmount(Resource, out amount);
        }
        private class MealStorageInterface : ResourceStorageInterface
        {
            private CanteenFoodStorage _foodStorage;
            public MealStorageInterface(MachineBase machine) : base(Resource.Meal)
            {
                if (machine.TryGetComponent<CanteenFoodStorage>(out var foodStorage))
                    _foodStorage = foodStorage;
                else if (machine.GetComponentInChildren<CanteenFoodStorage>() is not null)
                    _foodStorage = machine.GetComponentInChildren<CanteenFoodStorage>();
                else throw new NullReferenceException(nameof(CanteenFoodStorage));
            }

            public override bool TrySetAmount(long amount) => _foodStorage.TryChangeMealAmount((int)amount);

            public override bool TryGetAmount(out long amount)
            {
                amount = _foodStorage.AvailableMeals;
                return true;
            }
        }
        #endregion

        public SaveData ToSaveData() =>
            new SaveData()
            {
                LockedResources = _lockedResources,
                ResourceUse = _resourceUse.Select(r => new IProduct.SaveData.ResourceUseData()
                {
                    Amount = r.Amount,
                    Resource = r.Resource,
                    IsResourceUseOnProductCreated = r.GetType().IsSubclassOf(typeof(ResourceUseOnProductCreated)),
                    CurrentTime = r is ResourceUseOvertime rOvertime1 ? rOvertime1.Timer.Time : 0f,
                    TimeInterval = r is ResourceUseOvertime rOvertime2 ? rOvertime2.TimeInterval : 0f,
                }).ToList()
            };

        public class SaveData        {
            public Dictionary<Resource, int> LockedResources;
            public List<IProduct.SaveData.ResourceUseData> ResourceUse;

            public ResourceManager ToResourceManager(MachineBase machine) => new ResourceManager(machine)            {
                _lockedResources = LockedResources,
                _resourceUse = ResourceUse.Select(r => {
                    var timer = new CountdownTimer(r.TimeInterval);
                    timer.Time = r.CurrentTime;
                    return r.IsResourceUseOnProductCreated
                        ? (ResourceUse)new ResourceUseOnProductCreated() { Resource = r.Resource, Amount = r.Amount }
                        : (ResourceUse)new ResourceUseOvertime()
                        { Resource = r.Resource, Amount = r.Amount, TimeInterval = r.TimeInterval, Timer = timer };
                }).ToList()
            };
        }
    }
}