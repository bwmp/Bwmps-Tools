using UnityEngine;

namespace BwmpsTools.OtherScripts
{
    public class ToggleSync : MonoBehaviour
    {
        public GameObject object1;
        public GameObject object2;

        private void Update()
        {
            LinkChildrenEnabledState();
        }

        private void LinkChildrenEnabledState()
        {
            if (object1 == null || object2 == null)
            {
                Debug.LogWarning("Both GameObjects must be assigned for this to work");
                return;
            }

            for (int i = 0; i < object1.transform.childCount; i++)
            {
                Transform child1 = object1.transform.GetChild(i);
                Transform child2 = object2.transform.Find(child1.name);

                if (child2 == null)
                {
                    Debug.LogWarning("No matching child found for object2 for " + child1.name + ".");
                    continue;
                }

                child2.gameObject.SetActive(child1.gameObject.activeSelf);

                SkinnedMeshRenderer renderer1 = child1.GetComponent<SkinnedMeshRenderer>();
                SkinnedMeshRenderer renderer2 = child2.GetComponent<SkinnedMeshRenderer>();

                if (renderer1 != null && renderer2 != null && renderer1.sharedMesh.blendShapeCount > 0 && renderer1.sharedMesh.blendShapeCount == renderer2.sharedMesh.blendShapeCount)
                {
                    for (int j = 0; j < renderer1.sharedMesh.blendShapeCount; j++)
                    {
                        float blendShapeValue = renderer1.GetBlendShapeWeight(j);
                        renderer2.SetBlendShapeWeight(j, blendShapeValue);
                    }
                }
            }
        }

    }

}
