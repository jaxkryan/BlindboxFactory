using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Script.Controller;
using Script.Gacha.Base;
using Script.Gacha.Machine;
using Script.Utils;
using UnityEngine;

namespace Script.HumanResource.Administrator
{
    [CreateAssetMenu(fileName = "AdministratorGacha", menuName = "HumanResource/Administrator/AdministratorGacha")]
    public class AdministratorGacha : ScriptableObject, ILootbox<Mascot, AdministratorSetting>
    {
        [SerializeField] public NameRandomizer NameRandomizer;
        [SerializeField] public PortraitRandomizer PortraitRandomizer;
        [SerializeField] public List<EmployeeName> Names;
        [SerializeField] public PolicyGacha PolicyGacha;

        public int Pulls { get; set; }
        public IList<Mascot> PullHistory { get; set; } = new List<Mascot>();

        [CanBeNull]
        public Mascot Pull()
        {
            var grade = this.PullGrade();
            Debug.Log($"Pulling mascot with grade: {grade}");
            var setting = grade switch
            {
                Grade.Common => CommonSettings,
                Grade.Rare => RareSettings,
                Grade.Special => SpecialSettings,
                Grade.Epic => EpicSettings,
                Grade.Legendary => LegendarySettings,
                _ => throw new ArgumentOutOfRangeException()
            };

            Mascot admin = ScriptableObject.CreateInstance<CommonMascot>();
            admin.SetGrade(grade);

            // Randomize name
            if (NameRandomizer.UseGachaRequirements) NameRandomizer.SetRequirement(setting.nameRequirements.Compose());
            admin.Name = NameRandomizer.Pull() ?? new EmployeeName { FirstName = "Unnamed", LastName = "" };

            // Randomize portrait
            if (PortraitRandomizer.UseGachaRequirements) PortraitRandomizer.SetRequirement(setting.portraitRequirements.Compose());
            admin.Portrait = PortraitRandomizer.Pull();

            // Assign buffs using PolicyGacha
            admin.Policies = PolicyGacha.GeneratePoliciesForMascot(grade);
            Debug.Log($"Assigned {admin.Policies.Count()} policies to {grade} mascot");

            Pulls++;
            PullHistory.Add(admin);
            GameController.Instance.MascotController.AddMascot(admin);
            return admin;
        }

        public IEnumerable<Mascot> PullMultiple(int times)
        {
            var pulledItems = new List<Mascot>();
            while (times-- > 0)
            {
                var pulled = Pull();
                if (pulled == null) break;
                pulledItems.Add(pulled);
            }
            return pulledItems;
        }

        public AdministratorSetting CommonSettings => _commonSettings;
        [SerializeField] private AdministratorSetting _commonSettings;

        public AdministratorSetting RareSettings => _rareSettings;
        [SerializeField] private AdministratorSetting _rareSettings;

        public AdministratorSetting SpecialSettings => _specialSettings;
        [SerializeField] private AdministratorSetting _specialSettings;

        public AdministratorSetting EpicSettings => _epicSettings;
        [SerializeField] private AdministratorSetting _epicSettings;

        public AdministratorSetting LegendarySettings => _legendarySettings;
        [SerializeField] private AdministratorSetting _legendarySettings;
    }
}