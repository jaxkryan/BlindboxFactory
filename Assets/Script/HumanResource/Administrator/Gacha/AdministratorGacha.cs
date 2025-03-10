using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Script.Controller;
using Script.Gacha.Base;
using Script.Gacha.Machine;
using Script.Utils;
using UnityEngine;

namespace Script.HumanResource.Administrator {
    [CreateAssetMenu(fileName = "AdministratorGacha", menuName = "HumanResource/Administrator/AdministratorGacha")]
    public class AdministratorGacha : ScriptableObject, ILootbox<Mascot, AdministratorSetting> {
        public int Pulls { get; set; }
        // [SerializeField] private PolicyGacha 

        [SerializeField] public NameRandomizer NameRandomizer;
        [SerializeField] public PortraitRandomizer PortraitRandomizer;

        [SerializeField] public List<EmployeeName> Names;
        [SerializeField] public PolicyGacha PolicyGacha;

        [CanBeNull]
        public Mascot Pull() {
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
            Mascot admin = ScriptableObject.CreateInstance<CommonMascot>();

            admin.SetGrade(grade);

            //Randomize name
            if (NameRandomizer.UseGachaRequirements)NameRandomizer.SetRequirement(setting.nameRequirements.Compose());
            admin.Name = NameRandomizer.Pull();
            //Randomize portrait
            if (PortraitRandomizer.UseGachaRequirements)PortraitRandomizer.SetRequirement(setting.portraitRequirements.Compose());
            admin.Portrait = PortraitRandomizer.Pull();
            //policies
            admin.Policies = PolicyGacha.PullByAdminGrade(admin.Grade);

            Pulls++;
            PullHistory.Add(admin);

            GameController.Instance.MascotController.AddMascot(admin);
            return admin;
        }

        public IEnumerable<Mascot> PullMultiple(int times) {
            var pulledItems = new List<Mascot>();
            while (times-- > 0) {
                var pulled = Pull();
                if (pulled == null) break;
                pulledItems.Add(pulled);
            }

            return pulledItems;
        }

        public IList<Mascot> PullHistory { get; set; } = new List<Mascot>(); 

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