using System;
using UnityEngine;

namespace Script.Utils {
    public static class SaveDataExtension {
        public static TChild CastToSubclass<TChild, TParent>(this TParent obj) where TChild : class, TParent {
            try {
                //New
                var child = Activator.CreateInstance<TChild>();
                //Cast field
                foreach (var fieldInfo in typeof(TParent).GetFields()) {
                    try { fieldInfo.SetValue(child, fieldInfo.GetValue(obj)); }
                    catch(System.Exception e) {
                        Debug.LogWarning(new InvalidCastException($"Cannot cast field {fieldInfo.Name}", e));
                    }
                }
                //Cast properties
                foreach (var propertyInfo in typeof(TParent).GetProperties()) {
                    try { propertyInfo.SetValue(child, propertyInfo.GetValue(obj)); }
                    catch(System.Exception e) {
                        Debug.LogWarning(new InvalidCastException($"Cannot cast field {propertyInfo.Name}", e));
                    }
                }

                return child;
            }
            catch (System.Exception ex) {
                Debug.LogException(ex);
            }
            return obj as TChild;
        }
    }
}