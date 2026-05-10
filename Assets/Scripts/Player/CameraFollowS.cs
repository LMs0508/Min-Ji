using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Cinemachine;

namespace Cainos.PixelArtTopDown_Basic
{
    //let camera follow target
    public class CameraFollowS : MonoBehaviour
    {
        public Transform target;
        public float lerpSpeed = 1.0f;

        private Vector3 offset;

        private Vector3 targetPos;
        private CinemachineBrain brain;

        private void Start()
        {
            brain = GetComponent<CinemachineBrain>();
            if (target == null) return;

            offset = transform.position - target.position;
        }

        private void Update()
        {
            if (target == null) return;
            
            // 시네머신 가상 카메라가 작동 중이면 이 스크립트의 이동 로직을 중단합니다.
            if (brain != null && brain.ActiveVirtualCamera != null) return;

            targetPos = target.position + offset;
            transform.position = Vector3.Lerp(transform.position, targetPos, lerpSpeed * Time.deltaTime);
        }

        public void SnapToTarget()
        {
            if (target == null) return;
            transform.position = target.position + offset;
        }
    }
}
