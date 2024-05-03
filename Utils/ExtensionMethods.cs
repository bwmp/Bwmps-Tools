using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using static BwmpsTools.Utils.Structs;

namespace BwmpsTools.Utils
{
    public static class ExtensionMethods
    {

        public static List<Transform> FindChildrenByPartialNames(this Transform transform, List<string> partialNames, List<string> exclude = null)
        {
            var query = transform.Cast<Transform>();

            if (exclude != null)
            {
                query = query.Where(child => !exclude.Any(name => child.name.ToLower().Contains(name.ToLower())));
            }

            var matchingChildren = query
                .Where(child => partialNames.Any(name => child.name.ToLower().Contains(name.ToLower())))
                .ToList();

            return matchingChildren;
        }

        public static Transform FindDescendant(this Transform transform, string name)
        {
            Transform descendant = transform.Find(name);

            if (descendant != null)
            {
                return descendant;
            }

            foreach (Transform child in transform)
            {
                descendant = child.FindDescendant(name);

                if (descendant != null)
                {
                    return descendant;
                }
            }

            return null;
        }

        public static Transform FindDescendantByNames(this Transform transform, List<string> names)
        {
            foreach (Transform child in transform)
            {
                if (names.Contains(child.name.ToLower()))
                {
                    return child;
                }

                Transform descendant = child.FindDescendantByNames(names);

                if (descendant != null)
                {
                    return descendant;
                }
            }

            return null;
        }

        public static List<Transform> FindDescendantsByPartialNames(this Transform transform, List<string> partialNames, List<string> exclude = null)
        {
            List<Transform> matchingDescendants = new List<Transform>();

            foreach (Transform child in transform)
            {
                if (exclude != null && exclude.Contains(child.name.ToLower()))
                {
                    continue;
                }

                if (partialNames.Any(name => child.name.ToLower().Contains(name.ToLower())))
                {
                    matchingDescendants.Add(child);
                }

                matchingDescendants.AddRange(child.FindDescendantsByPartialNames(partialNames, exclude));
            }

            return matchingDescendants;
        }

        public static Transform FindDescendantByHBBOrNames(this Transform transform, HBBAndNames hBBAndNames)
        {
            Animator animator = transform.GetComponent<Animator>();
            Transform hbb = animator.GetBoneTransform(hBBAndNames.bone);
            if (hbb != null) return hbb;
            return FindDescendantByNames(transform, hBBAndNames.names);
        }

        public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component
        {
            T val = gameObject.GetComponent<T>() ?? Undo.AddComponent<T>(gameObject);
            return val;
        }

        public static void SetPlaceholderText(this TextField textField, string placeholder)
        {
            string placeholderClass = TextField.ussClassName + "__placeholder";

            onFocusOut();
            textField.RegisterCallback<FocusInEvent>(evt => onFocusIn());
            textField.RegisterCallback<FocusOutEvent>(evt => onFocusOut());

            void onFocusIn()
            {
                if (textField.ClassListContains(placeholderClass))
                {
                    textField.value = string.Empty;
                    textField.RemoveFromClassList(placeholderClass);
                }
            }

            void onFocusOut()
            {
                if (string.IsNullOrEmpty(textField.text))
                {
                    textField.SetValueWithoutNotify(placeholder);
                    textField.AddToClassList(placeholderClass);
                }
            }
        }


    }
}
