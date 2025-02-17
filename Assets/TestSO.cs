using System;
using Script.HumanResource.Administrator;
using UnityEngine;

public class TestSO : MonoBehaviour {
    [SerializeField] private Policy _policy;
    private void OnValidate() {
        foreach (var fieldInfo in typeof(Policy).GetFields()) {
            Debug.Log(fieldInfo.Name + " : " + fieldInfo.GetValue(_policy));
        }

        foreach (var propertyInfo in typeof(Policy).GetProperties()) {
            Debug.Log(propertyInfo.Name + " : " + propertyInfo.GetValue(_policy));
        }
    }
}
