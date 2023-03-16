using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Plugins.SerializedCollections.Editor.Search
{
    public class PropertySearchResult
    {
        public SerializedProperty Property;

        public PropertySearchResult(SerializedProperty property)
        {
            Property = property;
        }

        public override string ToString()
        {
            return $"Found match in in {Property.propertyPath}";
        }
    }
}