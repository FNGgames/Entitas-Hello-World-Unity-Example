using System;
using UnityEditor;
using UnityEngine;

namespace Entitas.Unity.VisualDebugging {

    public class BoundsTypeDrawer : ITypeDrawer {

        public bool HandlesType(Type type) {
            return type == typeof(Bounds);
        }

        public object DrawAndGetNewValue(Type memberType, string memberName, object value, IComponent component) {
            return EditorGUILayout.BoundsField(memberName, (Bounds)value);
        }
    }
}
