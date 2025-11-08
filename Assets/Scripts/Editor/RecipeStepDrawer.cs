using UnityEditor;
using UnityEngine;

//[CustomPropertyDrawer(typeof(RecipeStep))]
//public class RecipeStepDrawer : PropertyDrawer
//{
//    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
//    {
//        // Begin property context (enables prefab override handling)
//        EditorGUI.BeginProperty(position, label, property);

//        // Find child properties
//        var ingredientProp = property.FindPropertyRelative("ingredient");
//        var countProp = property.FindPropertyRelative("piecesCount");
//        var doneProp = property.FindPropertyRelative("isDone");

//        // Split line into sections
//        float third = position.width / 3f;
//        Rect r1 = new Rect(position.x, position.y, third, position.height);
//        Rect r2 = new Rect(position.x + third, position.y, third, position.height);
//        Rect r3 = new Rect(position.x + 2f * third, position.y, third, position.height);

//        // Draw fields inline
//        EditorGUI.PropertyField(r1, ingredientProp, GUIContent.none);
//        EditorGUI.PropertyField(r2, countProp, GUIContent.none);
//        EditorGUI.PropertyField(r3, doneProp, GUIContent.none);

//        EditorGUI.EndProperty();
//    }

//    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
//    {
//        return EditorGUIUtility.singleLineHeight;
//    }
//}
