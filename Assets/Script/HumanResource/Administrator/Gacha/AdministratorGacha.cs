using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Script.Gacha.Base;
using Script.Gacha.Machine;
using Script.HumanResource.Administrator.Positions;
using Script.Utils;
using UnityEngine;

namespace Script.HumanResource.Administrator {
    [CreateAssetMenu(fileName = "AdministratorGacha", menuName = "HumanResource/Administrator/Gacha")]
    public class AdministratorGacha : ScriptableObject, ILootbox<Administrator, AdministratorSetting> {
        public int Pulls { get; set; }
        // [SerializeField] private PolicyGacha 

        [SerializeField] public NameRandomizer NameRandomizer;
        [SerializeField] public PortraitRandomizer PortraitRandomizer;

        [SerializeField] public List<EmployeeName> Names;
        [SerializeField] public PolicyGacha HrPolicyGacha;
        [SerializeField] public PolicyGacha FacilityPolicyGacha;
        [SerializeField] public PolicyGacha SupplyPolicyGacha;
        [SerializeField] public PolicyGacha FinancePolicyGacha;

        [CanBeNull]
        public Administrator Pull() {
            AdministratorSetting setting;

            // var pool = Requirement.Handle(_itemPool).ToList();
            //
            // if (pool.Count == 0) return null;
            //
            // var gradePool = new List<Grade>();
            // pool.ForEach(p => {
            //     if (!gradePool.Contains(p.Grade)) gradePool.Add(p.Grade);
            // });
            //
            // var grade = this.PullGrade(gradePool.ToArray());
            //
            // var r = new System.Random();
            //
            // pool = pool.Where(p => p.Grade == grade).ToList();
            // admin = pool[r.Next(0, pool.Count - 1)];
            var grade = this.PullGrade();
            setting = grade switch {
                Grade.Common => CommonSettings,
                Grade.Rare => RareSettings,
                Grade.Special => SpecialSettings,
                Grade.Epic => EpicSettings,
                Grade.Legendary => LegendarySettings,
                _ => throw new ArgumentOutOfRangeException()
            };
            Administrator admin = PullClass(setting, out var position);

            admin.SetGrade(grade);

            //Randomize name
            if (NameRandomizer.UseGachaRequirements)NameRandomizer.SetRequirement(setting.nameRequirements.Compose());
            admin.Name = NameRandomizer.Pull();
            //Randomize portrait
            if (PortraitRandomizer.UseGachaRequirements)PortraitRandomizer.SetRequirement(setting.portraitRequirements.Compose());
            admin.Portrait = PortraitRandomizer.Pull();
            //policies
            switch (position) {
                case AdministratorPosition.HR:
                    admin.Policies = HrPolicyGacha.PullByAdminGrade(admin.Grade);
                    break;
                case AdministratorPosition.Facility:
                    admin.Policies = FacilityPolicyGacha.PullByAdminGrade(admin.Grade);
                    break;
                case AdministratorPosition.Supply:
                    admin.Policies = SupplyPolicyGacha.PullByAdminGrade(admin.Grade);
                    break;
                case AdministratorPosition.Finance:
                    admin.Policies = FinancePolicyGacha.PullByAdminGrade(admin.Grade);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            Pulls++;
            PullHistory.Add(admin);
            return admin;
        }

        public IEnumerable<Administrator> PullMultiple(int times) {
            var pulledItems = new List<Administrator>();
            while (times-- > 0) {
                var pulled = Pull();
                if (pulled == null) break;
                pulledItems.Add(pulled);
            }

            return pulledItems;
        }

        public IList<Administrator> PullHistory { get; set; } = new List<Administrator>(); 

        Administrator PullClass(AdministratorSetting setting, out AdministratorPosition position) {
            List<WeightedOption<AdministratorPosition>> positions = new();
            positions.Add(new WeightedOption<AdministratorPosition>
                { Option = AdministratorPosition.HR, Weight = setting.hrWeight });
            positions.Add(new WeightedOption<AdministratorPosition>
                { Option = AdministratorPosition.Supply, Weight = setting.supplyWeight });
            positions.Add(new WeightedOption<AdministratorPosition>
                { Option = AdministratorPosition.Facility, Weight = setting.facilityWeight });
            positions.Add(new WeightedOption<AdministratorPosition>
                { Option = AdministratorPosition.Finance, Weight = setting.financeWeight });

            switch (positions.ToArray().PickRandom()) {
                case AdministratorPosition.HR:
                    position = AdministratorPosition.HR;
                    return ScriptableObject.CreateInstance<HRAdministrator>();
                case AdministratorPosition.Facility:
                    position = AdministratorPosition.Facility;
                    return ScriptableObject.CreateInstance<FacilityAdministrator>();
                case AdministratorPosition.Supply:
                    position = AdministratorPosition.Supply;
                    return ScriptableObject.CreateInstance<SupplyAdministrator>();
                case AdministratorPosition.Finance:
                    position = AdministratorPosition.Finance;
                    return ScriptableObject.CreateInstance<FinanceAdministrator>();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public AdministratorSetting CommonSettings {
            get => _commonSettings;
        }

        [SerializeField] private AdministratorSetting _commonSettings;

        public AdministratorSetting RareSettings {
            get => _rareSettings;
        }

        [SerializeField] private AdministratorSetting _rareSettings;

        public AdministratorSetting SpecialSettings {
            get => _specialSettings;
        }

        [SerializeField] private AdministratorSetting _specialSettings;

        public AdministratorSetting EpicSettings {
            get => _epicSettings;
        }

        [SerializeField] private AdministratorSetting _epicSettings;

        public AdministratorSetting LegendarySettings {
            get => _legendarySettings;
        }

        [SerializeField] private AdministratorSetting _legendarySettings;
        
    }
}