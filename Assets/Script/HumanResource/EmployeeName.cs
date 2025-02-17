using System;
using UnityEngine;

namespace Script.HumanResource {
    [Serializable]
    public class EmployeeName {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        [SerializeField] private string _firstName;
        [SerializeField] private string _lastName = "";
        public override string ToString() => $"{FirstName}{(LastName == "" ? LastName : " " + LastName)}";
    }
}