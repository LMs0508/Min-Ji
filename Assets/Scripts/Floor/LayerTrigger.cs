using UnityEngine;

namespace Cainos.PixelArtTopDown_Basic
{
    public class LayerTrigger : MonoBehaviour
    {
        public Direction direction;

        [Header("Upper (when going up)")]
        public string layerUpper = "Layer 2";
        public string sortingLayerUpper = "Layer 2";

        [Header("Lower (when going down)")]
        public string layerLower = "Layer 1";
        public string sortingLayerLower = "Layer 1";

        [Header("Debug")]
        public bool debugLog = true;

        private void OnTriggerEnter2D(Collider2D other)
        {
            var target = FindPlayerRoot(other);
            if (target == null) return;

            bool goUpper = IsOnUpperSide(other.transform.position);

            if (debugLog)
                Debug.Log($"[StairsTrigger ENTER] other={other.name}, target={target.name}, dir={direction}, goUpper={goUpper}");

            if (goUpper) SetLayerAndSortingLayer(target, layerUpper, sortingLayerUpper);
            else SetLayerAndSortingLayer(target, layerLower, sortingLayerLower);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            var target = FindPlayerRoot(other);
            if (target == null) return;

            bool nowUpperSide = IsOnUpperSide(other.transform.position);

            // Exit할 때는 "현재 위치 기준"으로 자연스럽게 반대쪽으로 정착
            if (debugLog)
                Debug.Log($"[StairsTrigger EXIT] other={other.name}, target={target.name}, dir={direction}, nowUpperSide={nowUpperSide}");

            if (nowUpperSide) SetLayerAndSortingLayer(target, layerUpper, sortingLayerUpper);
            else SetLayerAndSortingLayer(target, layerLower, sortingLayerLower);
        }

        // 자식 콜라이더여도 'Player' 태그가 붙은 루트를 찾는다
        private GameObject FindPlayerRoot(Collider2D other)
        {
            Transform cur = other.transform;
            while (cur != null)
            {
                if (cur.CompareTag("Player"))
                    return cur.gameObject;

                cur = cur.parent;
            }

            // 플레이어 구조가 태그가 루트에 없으면, 최후로 루트 오브젝트
            // (이 경우엔 플레이어 루트에 Player 태그를 달아주는 게 정답)
            return other.transform.root.gameObject;
        }

        private bool IsOnUpperSide(Vector3 pos)
        {
            switch (direction)
            {
                case Direction.North: return pos.y > transform.position.y;
                case Direction.South: return pos.y < transform.position.y;
                case Direction.West: return pos.x < transform.position.x;
                case Direction.East: return pos.x > transform.position.x;
                default: return false;
            }
        }

        private void SetLayerAndSortingLayer(GameObject target, string layer, string sortingLayer)
        {
            int layerIndex = LayerMask.NameToLayer(layer);
            if (layerIndex < 0)
            {
                Debug.LogError($"[StairsTrigger] Layer '{layer}' does not exist. (Project Settings > Tags and Layers)");
                return;
            }

            target.layer = layerIndex;

            var srs = target.GetComponentsInChildren<SpriteRenderer>(true);
            foreach (var sr in srs)
                sr.sortingLayerName = sortingLayer;

            if (debugLog)
                Debug.Log($"[StairsTrigger] APPLY target={target.name}, physicsLayer={layer}, sortingLayer={sortingLayer}, spriteRenderers={srs.Length}");
        }

        public enum Direction { North, South, West, East }
    }
}
