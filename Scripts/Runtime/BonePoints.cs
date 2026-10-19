#if UNITY_EDITOR
using UnityEngine;

namespace UI
{
    public class BonePoints : MonoBehaviour
    {
        void OnDrawGizmos()
        {
            if (TryGetComponent<RectTransform>(out var rT))
            {
                Gizmos.color = Color.ghostWhite;
                Gizmos.DrawWireSphere(rT.position, 15f);

                Gizmos.color = Color.grey;
                Gizmos.DrawWireSphere(rT.position + rT.rect.height * rT.up, 15f);
            }
        }
    }
}
#endif