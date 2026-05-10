using UnityEngine;

namespace Cainos.PixelArtTopDown_Basic
{
    public class StairsLayerTrigger : MonoBehaviour
    {
        public Direction direction;
        [Space]
        public string layerUpper;
        public string sortingLayerUpper;
        [Space]
        public string layerLower;
        public string sortingLayerLower;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                if (direction == Direction.South && other.transform.position.y < transform.position.y)
                    UpdatePlayerLayer(other.gameObject, layerUpper, sortingLayerUpper);
                else if (direction == Direction.West && other.transform.position.x < transform.position.x)
                    UpdatePlayerLayer(other.gameObject, layerUpper, sortingLayerUpper);
                else if (direction == Direction.East && other.transform.position.x > transform.position.x)
                    UpdatePlayerLayer(other.gameObject, layerUpper, sortingLayerUpper);
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                // 남쪽(위아래) 계단 기준
                if (direction == Direction.South)
                {
                    if (other.transform.position.y > transform.position.y) 
                        UpdatePlayerLayer(other.gameObject, layerUpper, sortingLayerUpper); // 위로 나감
                    else 
                        UpdatePlayerLayer(other.gameObject, layerLower, sortingLayerLower); // 아래로 나감
                }
            }
        }
        // 핵심 수정 부분
        private void UpdatePlayerLayer(GameObject target, string layerName, string sortingLayerName)
        {
            // 1. 물리 레이어 변경 (2층 벽과 충돌하기 위함)
            target.layer = LayerMask.NameToLayer(layerName);

            // 2. 플레이어에게 붙인 PlayerLayerSync 스크립트를 찾아 모든 스프라이트 레이어 변경
            // GetComponentInParent를 쓰는 이유는 콜라이더가 자식에 있을 수 있기 때문입니다.
            PlayerLayerSync sync = target.GetComponentInParent<PlayerLayerSync>();
            if (sync != null)
            {
                sync.UpdateAllSortingLayers(sortingLayerName);
            }
            else
            {
                // 만약 스크립트를 못 찾았다면 직접 변경 시도 (비상용)
                SpriteRenderer[] srs = target.GetComponentsInChildren<SpriteRenderer>(true);
                foreach (var sr in srs) sr.sortingLayerName = sortingLayerName;
            }
        }

        public enum Direction { North, South, West, East }
    }
}