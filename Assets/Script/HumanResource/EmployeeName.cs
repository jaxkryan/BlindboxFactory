using System;
using System.Linq;
using UnityEngine;

namespace Script.HumanResource {
    [Serializable]
    public class EmployeeName {
        public string FirstName { get => _firstName; set => _firstName = value; }

        public string LastName {
            get => _lastName;
            set {
                if (value.Contains(' ')) throw new ArgumentException("Cannot have multiple last names");
                _lastName = value;
            }
        }

        [SerializeField] private string _firstName;
        [SerializeField] private string _lastName = "";
        public override string ToString() => $"{FirstName}{(LastName == "" ? LastName : " " + LastName)}";
        public static implicit operator string(EmployeeName employeeName) => employeeName.ToString();

        public static implicit operator EmployeeName(string strName) {
            var parts = strName.Split(' ');
            return parts.Length > 1
                ? new EmployeeName() {
                    FirstName = string.Join(' ', parts.SkipLast(1)),
                    LastName = parts.Last()
                }
                : new EmployeeName(){FirstName = strName};
        }
    }
}