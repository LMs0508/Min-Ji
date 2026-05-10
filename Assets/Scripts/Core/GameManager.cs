using UnityEngine;
using System.Collections.Generic;
using Game.Player;
using Game.UI;

namespace Game.Core
{
    public class GameManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerStats playerStats;
        [SerializeField] private PlayerElement playerElement;
        [SerializeField] private HUDController hud;

        [Header("UI Panels")]
        [SerializeField] private GameObject statusPanel;
        [SerializeField] private GameObject inventoryPanel;

        // 열려있는 패널들을 순서대로 추적하기 위한 리스트 (Stack처럼 사용)
        private List<GameObject> activePanels = new List<GameObject>();

        public static GameManager Instance;

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);
        }

        private void Start()
        {
            if (hud != null && playerStats != null && playerElement != null)
                hud.Bind(playerStats, playerElement);
            else
                Debug.LogError("[GameManager] Assign playerStats, playerElement, hud in Inspector.");

            // UI 패널들은 시작할 때 닫아둡니다.
            if (statusPanel != null) statusPanel.SetActive(false);
            if (inventoryPanel != null) inventoryPanel.SetActive(false);
        }

        private void Update()
        {
            // 'O' 키로 스탯창 토글
            if (Input.GetKeyDown(KeyCode.O)) TogglePanel(statusPanel);

            // 'I' 키로 인벤토리 토글
            if (Input.GetKeyDown(KeyCode.I)) TogglePanel(inventoryPanel);

            // ESC 키 통합 처리
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                // 1순위: 아이템 버리기 팝업이 있다면 닫기
                var dropPopup = FindFirstObjectByType<DropPopupUI>(FindObjectsInactive.Include);
                if (dropPopup != null && dropPopup.gameObject.activeSelf)
                {
                    dropPopup.Close();
                    return; // 팝업만 닫고 종료
                }

                // 2순위: 스택 기반으로 최근에 열린 패널부터 하나씩 닫기
                if (activePanels.Count > 0)
                {
                    // 리스트의 뒤쪽(최근)부터 확인하여 닫음
                    for (int i = activePanels.Count - 1; i >= 0; i--)
                    {
                        if (activePanels[i].activeSelf)
                        {
                            TogglePanel(activePanels[i]);
                            return; // 한 번에 하나만 닫고 함수 종료
                        }
                        else
                        {
                            // 만약 외부 요인으로 이미 닫혀있다면 리스트에서 정리
                            activePanels.RemoveAt(i);
                        }
                    }
                }
            }
        }

        // UI를 클릭했을 때 호출하여 최상단으로 올리는 함수
        public void FocusPanel(GameObject panel)
        {
            if (panel == null) return;

            // 1. 논리적 순서 갱신 (ESC 닫기 순서를 위해 리스트 맨 뒤로 이동)
            if (activePanels.Contains(panel))
            {
                activePanels.Remove(panel);
                activePanels.Add(panel);
            }

            // 2. 시각적 순서 갱신 (화면상 맨 위로 보이기)
            panel.transform.SetAsLastSibling();
        }

        // 패널을 열고 닫으며 리스트(Stack)를 관리하는 함수
        private void TogglePanel(GameObject panel)
        {
            if (panel == null) return;

            bool isActive = panel.activeSelf;
            if (isActive)
            {
                // 닫기: 비활성화 후 리스트에서 제거
                panel.SetActive(false);
                activePanels.Remove(panel);
            }
            else
            {
                // 열기: 활성화 후 리스트 맨 뒤(최신)로 추가
                panel.SetActive(true);
                // 혹시 이미 리스트에 있다면 제거 후 다시 추가하여 순서를 갱신
                activePanels.Remove(panel);
                activePanels.Add(panel);
                
                // [추가] 창을 열 때 시각적으로도 맨 위로 올라오게 함
                panel.transform.SetAsLastSibling();
            }
        }
    }
}