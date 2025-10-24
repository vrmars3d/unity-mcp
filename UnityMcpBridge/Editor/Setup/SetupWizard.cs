using System;
using MCPForUnity.Editor.Dependencies;
using MCPForUnity.Editor.Dependencies.Models;
using MCPForUnity.Editor.Helpers;
using MCPForUnity.Editor.Windows;
using UnityEditor;
using UnityEngine;

namespace MCPForUnity.Editor.Setup
{
    /// <summary>
    /// Handles automatic triggering of the setup wizard
    /// </summary>
    [InitializeOnLoad]
    public static class SetupWizard
    {
        private const string SETUP_COMPLETED_KEY = "MCPForUnity.SetupCompleted";
        private const string SETUP_DISMISSED_KEY = "MCPForUnity.SetupDismissed";
        private static bool _hasCheckedThisSession = false;

        static SetupWizard()
        {
            // Skip in batch mode
            if (Application.isBatchMode)
                return;

            // Show setup wizard on package import
            EditorApplication.delayCall += CheckSetupNeeded;
        }

        /// <summary>
        /// Check if setup wizard should be shown
        /// </summary>
        private static void CheckSetupNeeded()
        {
            if (_hasCheckedThisSession)
                return;

            _hasCheckedThisSession = true;

            try
            {
                // Check if setup was already completed or dismissed in previous sessions
                bool setupCompleted = EditorPrefs.GetBool(SETUP_COMPLETED_KEY, false);
                bool setupDismissed = EditorPrefs.GetBool(SETUP_DISMISSED_KEY, false);

                // Only show setup wizard if it hasn't been completed or dismissed before
                if (!(setupCompleted || setupDismissed))
                {
                    McpLog.Info("Package imported - showing setup wizard", always: false);

                    var dependencyResult = DependencyManager.CheckAllDependencies();
                    EditorApplication.delayCall += () => ShowSetupWizard(dependencyResult);
                }
                else
                {
                    McpLog.Info("Setup wizard skipped - previously completed or dismissed", always: false);
                }
            }
            catch (Exception ex)
            {
                McpLog.Error($"Error checking setup status: {ex.Message}");
            }
        }

        /// <summary>
        /// Show the setup wizard window
        /// </summary>
        public static void ShowSetupWizard(DependencyCheckResult dependencyResult = null)
        {
            try
            {
                dependencyResult ??= DependencyManager.CheckAllDependencies();
                SetupWizardWindow.ShowWindow(dependencyResult);
            }
            catch (Exception ex)
            {
                McpLog.Error($"Error showing setup wizard: {ex.Message}");
            }
        }

        /// <summary>
        /// Mark setup as completed
        /// </summary>
        public static void MarkSetupCompleted()
        {
            EditorPrefs.SetBool(SETUP_COMPLETED_KEY, true);
            McpLog.Info("Setup marked as completed");
        }

        /// <summary>
        /// Mark setup as dismissed
        /// </summary>
        public static void MarkSetupDismissed()
        {
            EditorPrefs.SetBool(SETUP_DISMISSED_KEY, true);
            McpLog.Info("Setup marked as dismissed");
        }

        /// <summary>
        /// Force show setup wizard (for manual invocation)
        /// </summary>
        [MenuItem("Window/MCP For Unity/Setup Wizard", priority = 1)]
        public static void ShowSetupWizardManual()
        {
            ShowSetupWizard();
        }

        /// <summary>
        /// Check dependencies and show status
        /// </summary>
        [MenuItem("Window/MCP For Unity/Check Dependencies", priority = 3)]
        public static void CheckDependencies()
        {
            var result = DependencyManager.CheckAllDependencies();

            if (!result.IsSystemReady)
            {
                bool showWizard = EditorUtility.DisplayDialog(
                    "MCP for Unity - Dependencies",
                    $"System Status: {result.Summary}\n\nWould you like to open the Setup Wizard?",
                    "Open Setup Wizard",
                    "Close"
                );

                if (showWizard)
                {
                    ShowSetupWizard(result);
                }
            }
            else
            {
                EditorUtility.DisplayDialog(
                    "MCP for Unity - Dependencies",
                    "✓ All dependencies are available and ready!\n\nMCP for Unity is ready to use.",
                    "OK"
                );
            }
        }

        /// <summary>
        /// Open MCP Client Configuration window
        /// </summary>
        [MenuItem("Window/MCP For Unity/Open MCP Window", priority = 4)]
        public static void OpenClientConfiguration()
        {
            Windows.MCPForUnityEditorWindow.ShowWindow();
        }
    }
}
