using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace UnityEditor.ShaderGraph
{
    internal interface IShaderControl
    {
        SerializableValueStore defaultValue { get; }
        ConcreteSlotValueType[] validPortTypes { get; }
        VisualElement GetControl(IShaderValue shaderValue);
    }
}
