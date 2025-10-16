using System.Linq;
using UnityEditor;
using UnityEngine;

namespace TowerDefense
{
    public class ReloadWorkersManagerDebugWindow : EditorWindow
    {
        private Vector2 scrollPosition;
        private bool autoRefresh = true;
        private double lastRefreshTime;
        private const double REFRESH_INTERVAL = 0.5;

        [MenuItem("Tools/Tower Defense/Reload Workers Manager Debug")]
        public static void ShowWindow()
        {
            GetWindow<ReloadWorkersManagerDebugWindow>("Reload Workers Debug");
        }

        private void OnEnable()
        {
            lastRefreshTime = EditorApplication.timeSinceStartup;
        }

        private void Update()
        {
            if (autoRefresh && EditorApplication.timeSinceStartup - lastRefreshTime > REFRESH_INTERVAL)
            {
                Repaint();
                lastRefreshTime = EditorApplication.timeSinceStartup;
            }
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginVertical();

            DrawHeader();
            DrawControls();
            EditorGUILayout.Space();

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            
            DrawReservedStoragePoints();
            EditorGUILayout.Space();
            DrawTowersBeingServiced();
            EditorGUILayout.Space();
            DrawAllReloadWorkers();

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void DrawHeader()
        {
            EditorGUILayout.LabelField("Reload Workers Manager Debug", EditorStyles.boldLabel);
            EditorGUILayout.Space();
        }

        private void DrawControls()
        {
            EditorGUILayout.BeginHorizontal();
            
            autoRefresh = EditorGUILayout.Toggle("Auto Refresh", autoRefresh);
            
            if (GUILayout.Button("Manual Refresh", GUILayout.Width(120)))
            {
                Repaint();
            }
            
            if (GUILayout.Button("Clear All Reservations", GUILayout.Width(150)))
            {
                ReloadWorkersManager.ClearAllReservations();
            }
            
            EditorGUILayout.EndHorizontal();
        }

        private void DrawReservedStoragePoints()
        {
            var reservedPoints = ReloadWorkersManager.GetReservedStoragePoints();
            
            EditorGUILayout.LabelField($"Reserved Storage Points ({reservedPoints.Count})", EditorStyles.boldLabel);
            
            if (reservedPoints.Count == 0)
            {
                EditorGUILayout.LabelField("No storage points currently reserved", EditorStyles.miniLabel);
                return;
            }

            EditorGUI.indentLevel++;
            foreach (var kvp in reservedPoints)
            {
                EditorGUILayout.BeginHorizontal();
                
                string pointName = kvp.Key != null ? kvp.Key.name : "NULL";
                string workerName = kvp.Value != null ? kvp.Value.name : "NULL";
                
                EditorGUILayout.LabelField($"Point: {pointName}", GUILayout.Width(200));
                EditorGUILayout.LabelField($"Worker: {workerName}", GUILayout.Width(200));
                
                if (kvp.Key != null && GUILayout.Button("Select Point", GUILayout.Width(100)))
                {
                    Selection.activeGameObject = kvp.Key.gameObject;
                }
                
                if (kvp.Value != null && GUILayout.Button("Select Worker", GUILayout.Width(100)))
                {
                    Selection.activeGameObject = kvp.Value.gameObject;
                }
                
                EditorGUILayout.EndHorizontal();
            }
            EditorGUI.indentLevel--;
        }

        private void DrawTowersBeingServiced()
        {
            EditorGUILayout.LabelField("Towers Being Serviced", EditorStyles.boldLabel);
            
            var allWorkers = Object.FindObjectsByType<ReloadWorker>(FindObjectsSortMode.None);
            var servicedTowers = allWorkers
                .Where(w => w != null && w.GetCurrentTarget() != null)
                .Select(w => w.GetCurrentTarget())
                .Distinct()
                .ToList();
            
            if (servicedTowers.Count == 0)
            {
                EditorGUILayout.LabelField("No towers currently being serviced", EditorStyles.miniLabel);
                return;
            }

            EditorGUI.indentLevel++;
            foreach (var tower in servicedTowers)
            {
                EditorGUILayout.BeginHorizontal();
                
                string towerName = tower != null ? tower.name : "NULL";
                bool isReserved = ReloadWorkersManager.IsTowerBeingServiced(tower);
                
                EditorGUILayout.LabelField($"Tower: {towerName}", GUILayout.Width(200));
                EditorGUILayout.LabelField($"Reserved: {isReserved}", GUILayout.Width(100));
                
                if (tower != null)
                {
                    EditorGUILayout.LabelField($"Ammo: {tower.GetCurrentAmmo()}", GUILayout.Width(80));
                    EditorGUILayout.LabelField($"Reloading: {tower.IsReloading()}", GUILayout.Width(100));
                    
                    if (GUILayout.Button("Select", GUILayout.Width(60)))
                    {
                        Selection.activeGameObject = tower.gameObject;
                    }
                }
                
                EditorGUILayout.EndHorizontal();
            }
            EditorGUI.indentLevel--;
        }

        private void DrawAllReloadWorkers()
        {
            var allWorkers = Object.FindObjectsByType<ReloadWorker>(FindObjectsSortMode.None);
            
            EditorGUILayout.LabelField($"All Reload Workers ({allWorkers.Length})", EditorStyles.boldLabel);
            
            if (allWorkers.Length == 0)
            {
                EditorGUILayout.LabelField("No reload workers found in scene", EditorStyles.miniLabel);
                return;
            }

            EditorGUI.indentLevel++;
            foreach (var worker in allWorkers)
            {
                if (worker == null) continue;
                
                EditorGUILayout.BeginHorizontal();
                
                EditorGUILayout.LabelField($"Worker: {worker.name}", GUILayout.Width(150));
                
                var currentTarget = worker.GetCurrentTarget();
                string targetName = currentTarget != null ? currentTarget.name : "None";
                EditorGUILayout.LabelField($"Target: {targetName}", GUILayout.Width(150));
                
                EditorGUILayout.LabelField($"State: {worker.GetCurrentState()}", GUILayout.Width(120));
                
                if (GUILayout.Button("Select", GUILayout.Width(60)))
                {
                    Selection.activeGameObject = worker.gameObject;
                }
                
                EditorGUILayout.EndHorizontal();
            }
            EditorGUI.indentLevel--;
        }
    }
}
