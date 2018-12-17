using UnityEngine;

namespace Kingfisher
{
    [RequireComponent(typeof(Renderer))]
    public class SortingOrderAdjuster : MonoBehaviour
    {
        [SerializeField] private int m_sortingOrder;

        void Awake()
        {
            GetComponent<Renderer>().sortingOrder = m_sortingOrder;
        }
    }
}