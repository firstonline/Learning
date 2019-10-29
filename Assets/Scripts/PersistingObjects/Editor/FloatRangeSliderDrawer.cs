﻿using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(FloatRangeSliderAttribute))]
public class FloatRangeSliderDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        int originalIndentLevel = EditorGUI.indentLevel;
        EditorGUI.BeginProperty(position, label, property);
        
        position = EditorGUI.PrefixLabel(
            position, GUIUtility.GetControlID(FocusType.Passive), label
        );
        EditorGUI.indentLevel = 0;
        
        SerializedProperty minProperty = property.FindPropertyRelative("min");
        SerializedProperty maxProperty = property.FindPropertyRelative("max");

        float minValue = minProperty.floatValue;
        float maxValue = maxProperty.floatValue;
        float fieldWidth = position.width / 4;
        float sliderWidth = position.width / 2;
        
        
        position.width = fieldWidth;
        minValue = EditorGUI.FloatField(position, minValue);
        position.x += fieldWidth + 4f;
        position.width = sliderWidth;
        
        FloatRangeSliderAttribute limit = attribute as FloatRangeSliderAttribute;
        EditorGUI.MinMaxSlider(position, ref minValue, ref maxValue, limit.Min, limit.Max);

        position.width = fieldWidth;
        position.x += sliderWidth + 4f;
        maxValue = EditorGUI.FloatField(position, maxValue);

        if (minValue < limit.Min)
        {
            minValue = limit.Min;
        }
        else if (minValue > limit.Max)
        {
            minValue = limit.Max;
        }

        if (maxValue < minValue)
        {
            maxValue = minValue;
        }
        else if (maxValue > limit.Max)
        {
            maxValue = limit.Max;
        }
        
        
        minProperty.floatValue = minValue;
        maxProperty.floatValue = maxValue;
        EditorGUI.EndProperty();
        EditorGUI.indentLevel = originalIndentLevel;
        
    }
}
