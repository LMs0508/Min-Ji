using UnityEngine;
using Game.Core;
using Game.Player;

namespace Game.Combat
{
    public class ElementPickup : MonoBehaviour
    {
        [SerializeField] private ElementType elementType = ElementType.None;
        [SerializeField] private bool destroyOnPickup = true;


        private void OnTriggerEnter2D(Collider2D other)
        {
            Debug.Log($"[Pickup] Trigger Enter by: {other.name} tag={other.tag} layer={other.gameObject.layer}");

            if (!other.CompareTag("Player")) return;

            var playerElement = 
                other.GetComponent<PlayerElement>()
                ?? other.GetComponentInParent<PlayerElement>()
                ?? other.GetComponentInChildren<PlayerElement>();

            if (playerElement == null)
            {
                // 플레이어가 자식 오브젝트로 콜라이더를 갖는 구조면 부모에서 찾기
                playerElement = other.GetComponentInParent<PlayerElement>();
            }

            if (playerElement == null) return;

            playerElement.SetElement(elementType);

            if (destroyOnPickup)
                Destroy(gameObject);

            Debug.Log("TRIGGER ENTER: " + other.name);
            Debug.Log("TAG: " + other.tag);
        }
    }
}
