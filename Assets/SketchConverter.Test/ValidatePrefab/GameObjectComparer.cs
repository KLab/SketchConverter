/**
 * SketchConverter
 * Copyright(c) 2020 KLab, Inc. All Rights Reserved.
 * Proprietary and Confidential - This source code is not for redistribution
 *
 * Subject to the prior written consent of KLab, Inc(Licensor) and its terms and
 * conditions, Licensor grants to you, and you hereby accept nontransferable,
 * nonexclusive limited right to access, obtain, use, copy and/or download
 * a copy of this product only for requirement purposes. You may not rent,
 * lease, loan, time share, sublicense, transfer, make generally available,
 * license, disclose, disseminate, distribute or otherwise make accessible or
 * available this product to any third party without the prior written approval
 * of Licensor. Unauthorized copying of this product, including modifications 
 * of this product or programs in which this product has been merged or included
 * with other software products is expressly forbidden.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

/// <summary>
/// GameObject の比較処理
/// </summary>
public static class GameObjectComparer
{
    class NodeInfo
    {
        public string Breadcrumbs;
        public GameObject NodeObject;
    }

    public static void Test(GameObject prefabA, GameObject prefabB)
    {
        var transformsA = GetTransforms(prefabA).ToArray();
        var transformsB = GetTransforms(prefabB).ToArray();
        var errorMessages = new List<string>();

        // GameObjectの比較
        var transformLength = Math.Min(transformsA.Length, transformsB.Length);
        for (var i = 0; i < transformLength; i++)
        {
            var gameObjectA = transformsA[i].NodeObject;
            var gameObjectB = transformsB[i].NodeObject;
            var breadcrumbs = transformsB[i].Breadcrumbs;
            if (gameObjectA.activeSelf != gameObjectB.activeSelf)
            {
                errorMessages.Add($"[{breadcrumbs}]activeSelfが一致しません");
            }

            TestObjectProperties(gameObjectA, gameObjectB, breadcrumbs, errorMessages);

            // Componentの比較
            var componentsA = gameObjectA.GetComponents<Component>();
            var componentsB = gameObjectB.GetComponents<Component>();
            var componentLength = Math.Min(componentsA.Length, componentsB.Length);
            for (var j = 0; j < componentLength; j++)
            {
                TestObjectProperties(componentsA[j], componentsB[j], breadcrumbs, errorMessages);
            }
            if (componentsA.Length != componentsB.Length)
            {
                errorMessages.Add($"[{breadcrumbs}]Componentの数が一致しません");
            }
        }
        if (transformsA.Length != transformsB.Length)
        {
            errorMessages.Add("GameObjectの数が一致しません");
        }

        Assert.IsTrue(
            errorMessages.Count == 0,
            $"Prefabが一致しません\n{string.Join("\n", errorMessages.ToArray())}");
    }

    static IEnumerable<NodeInfo> GetTransforms(GameObject baseGameObject, string breadcrumbs = "")
    {
        var separator = !string.IsNullOrEmpty(breadcrumbs) ? "->" : "";
        var currentBreadcrumbs = $"{breadcrumbs}{separator}{baseGameObject.name}";
        yield return new NodeInfo
        {
            Breadcrumbs = currentBreadcrumbs,
            NodeObject = baseGameObject,
        };
        foreach (Transform child in baseGameObject.transform)
        {
            foreach (var r in GetTransforms(child.gameObject, currentBreadcrumbs))
            {
                yield return r;
            }
        }
    }

    static void TestObjectProperties(Object objectA, Object objectB, string breadcrumbs, List<string> errorMessages)
    {
        using var gameObjectPropertiesA = ToSerializedProperties(objectA).GetEnumerator();
        using var gameObjectPropertiesB = ToSerializedProperties(objectB).GetEnumerator();
        while (true)
        {
            var hasNextA = gameObjectPropertiesA.MoveNext();
            var hasNextB = gameObjectPropertiesB.MoveNext();
            if (!hasNextA && !hasNextB)
            {
                break;
            }
            if (!hasNextA || !hasNextB)
            {
                errorMessages.Add($"[{breadcrumbs}:{objectB.GetType().FullName}]Propertyの数が一致しません");
                break;
            }
            if (!SerializedProperty.DataEquals(gameObjectPropertiesA.Current, gameObjectPropertiesB.Current))
            {
                errorMessages.Add($"[{breadcrumbs}]{objectB.GetType().FullName}({gameObjectPropertiesB.Current.name})が異なります");
            }
        }
    }

    static IEnumerable<SerializedProperty> ToSerializedProperties(Object target)
    {
        var property = new SerializedObject(target).GetIterator();
        while (property.NextVisible(true))
        {
            yield return property;
        }
    }
}
