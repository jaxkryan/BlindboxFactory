using System;
using MyBox;
using MyBox.EditorTools;
using UnityEngine;
#if UNITY_EDITOR
using MyBox.Internal;
using UnityEditor;
#endif

namespace Script.Utils {
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class ValueMatchFieldAttribute : PropertyAttribute {
        public readonly string FieldName;
        public ValueMatchFieldAttribute(string fieldName) => FieldName = fieldName;
    }
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(ValueMatchFieldAttribute))]
    public class ValueFieldAttributeDrawer : PropertyDrawer {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            Color color = GUI.color;
            
            if (attribute is ValueMatchFieldAttribute valueMatch) {
	            bool isValid = true; 
                var relativeProperty = FindRelativeProperty(property, valueMatch.FieldName);
                if (relativeProperty != null) {
	                var asString = property.AsStringValue().ToUpper();
	                var asStringCompare = relativeProperty.AsStringValue().ToUpper();
	                if (asString == asStringCompare) isValid = false;
                } 
                
                if (!isValid) GUI.color = Color.red;
            }
            EditorGUI.PropertyField(position, property, label, true);
            GUI.color = color;
        }

		/// <summary>
		/// Get the other Property which is stored alongside with specified Property, by name
		/// </summary>
		private static SerializedProperty FindRelativeProperty(SerializedProperty property, string propertyName)
		{
			if (property.depth == 0) return property.serializedObject.FindProperty(propertyName);

			var path = property.propertyPath.Replace(".Array.data[", "[");
			var elements = path.Split('.');

			var nestedProperty = NestedPropertyOrigin(property, elements);

			// if nested property is null = we hit an array property
			if (nestedProperty == null)
			{
				var cleanPath = path.Substring(0, path.IndexOf('['));
				var arrayProp = property.serializedObject.FindProperty(cleanPath);
				WarningsPool.LogCollectionsNotSupportedWarning(arrayProp, nameof(ConditionalFieldAttribute));

				return null;
			}

			return nestedProperty.FindPropertyRelative(propertyName);
		}

		// For [Serialized] types with [Conditional] fields
		private static SerializedProperty NestedPropertyOrigin(SerializedProperty property, string[] elements)
		{
			SerializedProperty parent = null;

			for (int i = 0; i < elements.Length - 1; i++)
			{
				var element = elements[i];
				int index = -1;
				if (element.Contains("["))
				{
					index = Convert.ToInt32(element.Substring(element.IndexOf("[", StringComparison.Ordinal))
						.Replace("[", "").Replace("]", ""));
					element = element.Substring(0, element.IndexOf("[", StringComparison.Ordinal));
				}

				parent = i == 0
					? property.serializedObject.FindProperty(element)
					: parent != null
						? parent.FindPropertyRelative(element)
						: null;

				if (index >= 0 && parent != null) parent = parent.GetArrayElementAtIndex(index);
			}

			return parent;
		}    }
#endif
}