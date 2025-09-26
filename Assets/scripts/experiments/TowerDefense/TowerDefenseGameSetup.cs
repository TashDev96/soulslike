using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TowerDefense
{
    public class TowerDefenseGameSetup : MonoBehaviour
    {
        [Header("Game Setup")]
        [SerializeField] private bool autoSetup = true;
        
        [Header("Prefabs")]
        [SerializeField] private GameObject towerPrefab;
        [SerializeField] private GameObject uiCanvasPrefab;
        
        [Header("Spawn Points")]
        [SerializeField] private Transform goalPosition;

        private void Start()
        {
            if (autoSetup)
            {
                SetupGame();
            }
        }

        public void SetupGame()
        {
            SetupManagers();
            SetupUI();
        }

        private void SetupManagers()
        {
            var gameManager = FindFirstObjectByType<GameManager>();
            if (gameManager == null)
            {
                var gameManagerGO = new GameObject("GameManager");
                gameManager = gameManagerGO.AddComponent<GameManager>();
                
                if (towerPrefab != null)
                {
                    var gameManagerScript = gameManager;
                    var serializedObject = new UnityEditor.SerializedObject(gameManagerScript);
                    var towerPrefabProperty = serializedObject.FindProperty("towerPrefab");
                    towerPrefabProperty.objectReferenceValue = towerPrefab;
                    serializedObject.ApplyModifiedProperties();
                }
            }

            var zombieManager = FindFirstObjectByType<ZombieManager>();
            if (zombieManager == null)
            {
                var zombieManagerGO = new GameObject("ZombieManager");
                zombieManager = zombieManagerGO.AddComponent<ZombieManager>();
            }
        }

        private void SetupUI()
        {
            var existingCanvas = FindFirstObjectByType<Canvas>();
            if (existingCanvas == null)
            {
                if (uiCanvasPrefab != null)
                {
                    Instantiate(uiCanvasPrefab);
                }
                else
                {
                    CreateBasicUI();
                }
            }
        }

        private void CreateBasicUI()
        {
            var canvasGO = new GameObject("UI Canvas");
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();

            var gameUIGO = new GameObject("GameUI");
            gameUIGO.transform.SetParent(canvasGO.transform, false);
            var gameUI = gameUIGO.AddComponent<GameUI>();

            CreateUIElements(gameUIGO);

            var gameManager = FindFirstObjectByType<GameManager>();
            if (gameManager != null)
            {
                var serializedObject = new UnityEditor.SerializedObject(gameManager);
                var gameUIProperty = serializedObject.FindProperty("gameUI");
                gameUIProperty.objectReferenceValue = gameUI;
                serializedObject.ApplyModifiedProperties();
            }
        }

        private void CreateUIElements(GameObject parent)
        {
            var panelGO = new GameObject("UI Panel");
            panelGO.transform.SetParent(parent.transform, false);
            var panelImage = panelGO.AddComponent<Image>();
            panelImage.color = new Color(0, 0, 0, 0.5f);
            
            var rectTransform = panelGO.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0, 0.8f);
            rectTransform.anchorMax = new Vector2(1, 1);
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;

            CreateTextElement(panelGO, "MoneyText", "Money: $500", new Vector2(0, 0.5f), new Vector2(0.3f, 1f));
            CreateTextElement(panelGO, "WaveText", "Wave: 1", new Vector2(0.3f, 0.5f), new Vector2(0.6f, 1f));
            CreateTextElement(panelGO, "ZombieCountText", "Zombies: 0", new Vector2(0.6f, 0.5f), new Vector2(0.9f, 1f));
            
            var instructionsGO = CreateTextElement(parent, "InstructionsText", 
                "Click on the ground to place towers! Earn money by hitting and killing zombies.", 
                new Vector2(0, 0), new Vector2(1, 0.2f));
            instructionsGO.GetComponent<TextMeshProUGUI>().fontSize = 14;
        }

        private GameObject CreateTextElement(GameObject parent, string name, string text, Vector2 anchorMin, Vector2 anchorMax)
        {
            var textGO = new GameObject(name);
            textGO.transform.SetParent(parent.transform, false);
            
            var textMesh = textGO.AddComponent<TextMeshProUGUI>();
            textMesh.text = text;
            textMesh.fontSize = 18;
            textMesh.color = Color.white;
            textMesh.alignment = TextAlignmentOptions.Center;
            
            var rectTransform = textGO.GetComponent<RectTransform>();
            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            
            return textGO;
        }

        

        [ContextMenu("Setup Game")]
        public void ManualSetup()
        {
            SetupGame();
        }
    }
}
