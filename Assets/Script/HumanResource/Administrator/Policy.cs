using Script.Utils;
using System;
using Script.Gacha.Base;
using Unity.Cinemachine;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Script.HumanResource.Administrator {
    [CreateAssetMenu(menuName = "HumanResource/Administrator/Policy")]
    public abstract class Policy : Gacha.Base.Loot {
        public string Name {get => FormatName();}

        protected virtual string FormatName() => $"{Grade} {_name}";

        [SerializeField] protected string _name;
        public string Description {get => FormatDescription();}

        [SerializeField] protected string _descriptionKey;
        [SerializeField] protected string[] _descriptionArgs;

        protected virtual string FormatDescription()
        {
            if (string.IsNullOrEmpty(_descriptionKey))
                return _description; // Fallback nếu không có key
            return LocalizationExtension.GetTranslation(_descriptionKey, _descriptionArgs);
        }

        [TextArea]
        [SerializeField] protected string _description = "";
        public abstract void OnAssign();
        public virtual void OnDismiss() { ResetValues();}
        public virtual void OnUpdate(float deltaTime) {}
        protected abstract void ResetValues();

        public virtual SaveData Save() =>
             new SaveData()
             {
                 Grade = Grade,
                 Type = this.GetType().Name,
                 Name = _name,
                 Description = _description ?? "",
                 DescriptionKey = _descriptionKey,
                 DescriptionArgs = _descriptionArgs
             };

        public virtual void Load(SaveData data)
        {
            Grade = data.Grade;
            _name = data.Name;
            _description = data.Description ?? "";
            _descriptionKey = data.DescriptionKey;
            _descriptionArgs = data.DescriptionArgs;
        }

        public class SaveData {
            public Grade Grade;
            public string Type;
            public string Name;
            public string Description;
            public string DescriptionKey;
            public string[] DescriptionArgs;
        }
    }
}