using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Script.Utils {
    public static class SaveDataExtension {
        public static TChild CastToSubclass<TChild, TParent>(this TParent obj) where TChild : class, TParent {
            try {
                //New
                var child = Activator.CreateInstance<TChild>();
                //Check if Child is a direct descendant
                switch (GetInheritanceDepth(typeof(TChild), typeof(TParent), out var path)) {
                    case > 1:
                        try {
                            var directParent = path.ElementAtOrDefault(path.Count - 2);
                            if (directParent is null) throw new NullReferenceException();
                            child = (TChild)Convert.ChangeType(SafeCastToSubclass(child, directParent
                                    , typeof(TParent)), typeof(TChild));
                            if (child is null) throw new NullReferenceException();
                        }
                        catch(System.Exception ex) {
                            Debug.LogException(ex);
                            return obj as TChild;
                        }

                        break;
                    case 0:
                        Debug.LogException(new System.Exception($"Child {typeof(TChild)} is not a subclass of Parent {typeof(TParent)}"));
                        return obj as TChild;
                }

                //Cast field
                foreach (var fieldInfo in typeof(TParent).GetFields()) {
                    try {
                        if (fieldInfo.GetValue(child) is not null) continue;
                        fieldInfo.SetValue(child, fieldInfo.GetValue(obj));
                    }
                    catch (System.Exception e) {
                        Debug.LogWarning(new InvalidCastException($"Cannot cast field {fieldInfo.Name}", e));
                    }
                }

                //Cast properties
                foreach (var propertyInfo in typeof(TParent).GetProperties()) {
                    try { propertyInfo.SetValue(child, propertyInfo.GetValue(obj)); }
                    catch (System.Exception e) {
                        Debug.LogWarning(new InvalidCastException($"Cannot cast field {propertyInfo.Name}", e));
                    }
                }

                return child;
            }
            catch (System.Exception ex) { Debug.LogException(ex); }

            return obj as TChild;
        }
        
        static object? SafeCastToSubclass(object obj, Type childType, Type parentType)
        {
            try
            {
                if (!parentType.IsAssignableFrom(childType))
                    throw new InvalidCastException($"{childType.Name} is not a subclass of {parentType.Name}");

                var method = typeof(SaveDataExtension)
                    .GetMethod("CastToSubclass", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public)
                    ?.MakeGenericMethod(childType, parentType);

                if (method == null)
                    throw new MissingMethodException("CastToSubclass method not found or invalid.");

                return method.Invoke(null, new object[] { obj });
            }
            catch (System.Exception ex)
            {
                Console.WriteLine($"[Cast Error] {ex.GetType().Name}: {ex.Message}");
                return null;
            }
        }

        public static int GetInheritanceDepth(Type child, Type parent, out List<Type> path) {
            path = new List<Type>();
            Type current = child.BaseType;
            int depth = 0;

            while (current != null) {
                path.Add(current);

                if (current == parent) {
                    path.Reverse();
                    return depth + 1;
                }

                current = current.BaseType;
                depth++;
            }

            path.Clear(); // Not a subclass — clear the invalid path
            return -1;
        }
    }
}