using UnityEditor;
using UnityEngine;

namespace BwmpsTools.Utils
{
    public class ObjectNameInput
    {
        private GameObject gameObject;
        private string objectName;
        public ObjectNameInput(string title, string tooltip)
        {
            gameObject = EditorGUILayout.ObjectField(new GUIContent(title, tooltip), gameObject, typeof(GameObject), true, GUILayout.MinWidth(100), GUILayout.ExpandWidth(true)) as GameObject;
            EditorGUILayout.TextField(objectName, GUILayout.MinWidth(100), GUILayout.ExpandWidth(true));
        }

        public GameObject GetGameObject()
        {
            return gameObject;
        }

        public string GetName()
        {
            return objectName;
        }

        public void SetGameObject(GameObject gameObject)
        {
            this.gameObject = gameObject;
        }

        public void SetName(string name)
        {
            objectName = name;
        }
    }
}
