using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cainos.PixelArtTopDown_Basic
{
    // 이동이나 포탈 사용 시 레이어 변경을 일시적으로 막기 위한 로직이 추가되었습니다.
    public class StairsLayerTrigger_Tower : MonoBehaviour
    {
        // ★ 추가: 다른 스크립트(포탈)에서 접근하여 레이어 변경을 잠글 수 있는 변수
        public static bool IsTeleporting = false;

        public Direction direction;                                 // 계단의 방향
        [Space]
        public string layerUpper;
        public string sortingLayerUpper;
        [Space]
        public string layerLower;
        public string sortingLayerLower;

        private void OnTriggerEnter2D(Collider2D other)
        {
            // 플레이어가 아니거나 텔레포트 중이면 무시
            if (!other.CompareTag("Player") || IsTeleporting) return;

            if (direction == Direction.South && other.transform.position.y < transform.position.y) SetLayerAndSortingLayer(other.gameObject, layerUpper, sortingLayerUpper);
            else if (direction == Direction.West && other.transform.position.x < transform.position.x) SetLayerAndSortingLayer(other.gameObject, layerUpper, sortingLayerUpper);
            else if (direction == Direction.East && other.transform.position.x > transform.position.x) SetLayerAndSortingLayer(other.gameObject, layerUpper, sortingLayerUpper);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            // ★ 중요: 텔레포트 이동으로 인해 강제로 나가는 판정이 뜰 경우 레이어 복구를 막습니다.
            if (!other.CompareTag("Player") || IsTeleporting) return;

            if (direction == Direction.South && other.transform.position.y < transform.position.y) SetLayerAndSortingLayer(other.gameObject, layerLower, sortingLayerLower);
            else if (direction == Direction.West && other.transform.position.x < transform.position.x) SetLayerAndSortingLayer(other.gameObject, layerLower, sortingLayerLower);
            else if (direction == Direction.East && other.transform.position.x > transform.position.x) SetLayerAndSortingLayer(other.gameObject, layerLower, sortingLayerLower);
        }

        private void SetLayerAndSortingLayer(GameObject target, string layer, string sortingLayer)
        {
            int layerIndex = LayerMask.NameToLayer(layer);
            if (layerIndex == -1) return; // 레이어 이름이 잘못되었을 경우 방지

            target.layer = layerIndex;

            // 자식 오브젝트를 포함하여 모든 SpriteRenderer의 소팅 레이어 변경
            SpriteRenderer[] srs = target.GetComponentsInChildren<SpriteRenderer>();
            foreach (SpriteRenderer sr in srs)
            {
                sr.sortingLayerName = sortingLayer;
            }
        }

        public enum Direction
        {
            North,
            South,
            West,
            East
        }
    }
}   