using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace MCPForUnity.Editor.Helpers
{
    public static class ServerPathResolver
    {
        /// <summary>
        /// Attempts to locate the embedded UnityMcpServer/src directory inside the installed package
        /// or common development locations. Returns true if found and sets srcPath to the folder
        /// containing server.py.
        /// </summary>
        public static bool TryFindEmbeddedServerSource(out string srcPath)
        {
            // 1) Repo development layouts commonly used alongside this package
            try
            {
                string projectRoot = Path.GetDirectoryName(Application.dataPath);
                string[] devCandidates =
                {
                    Path.Combine(projectRoot ?? string.Empty, "unity-mcp", "UnityMcpServer", "src"),
                    Path.Combine(projectRoot ?? string.Empty, "..", "unity-mcp", "UnityMcpServer", "src"),
                };
                foreach (string candidate in devCandidates)
                {
                    string full = Path.GetFullPath(candidate);
                    if (Directory.Exists(full) && File.Exists(Path.Combine(full, "server.py")))
                    {
                        srcPath = full;
                        return true;
                    }
                }
            }
            catch { /* ignore */ }

            // 2) Resolve via local package info (no network). Fall back to Client.List on older editors.
            try
            {
#if UNITY_2021_2_OR_NEWER
                // Primary: the package that owns this assembly
                var owner = UnityEditor.PackageManager.PackageInfo.FindForAssembly(typeof(ServerPathResolver).Assembly);
                if (owner != null)
                {
                    if (TryResolveWithinPackage(owner, out srcPath))
                    {
                        return true;
                    }
                }

                // Secondary: scan all registered packages locally
                foreach (var p in UnityEditor.PackageManager.PackageInfo.GetAllRegisteredPackages())
                {
                    if (TryResolveWithinPackage(p, out srcPath))
                    {
                        return true;
                    }
                }
#else
                // Older Unity versions: use Package Manager Client.List as a fallback
                var list = UnityEditor.PackageManager.Client.List();
                while (!list.IsCompleted) { }
                if (list.Status == UnityEditor.PackageManager.StatusCode.Success)
                {
                    foreach (var pkg in list.Result)
                    {
                        if (TryResolveWithinPackage(pkg, out srcPath))
                        {
                            return true;
                        }
                    }
                }
#endif
            }
            catch { /* ignore */ }

            // 3) Fallback to previous common install locations
            try
            {
                string home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) ?? string.Empty;
                string[] candidates =
                {
                    Path.Combine(home, "unity-mcp", "UnityMcpServer", "src"),
                    Path.Combine(home, "Applications", "UnityMCP", "UnityMcpServer", "src"),
                };
                foreach (string candidate in candidates)
                {
                    if (Directory.Exists(candidate) && File.Exists(Path.Combine(candidate, "server.py")))
                    {
                        srcPath = candidate;
                        return true;
                    }
                }
            }
            catch { /* ignore */ }

            srcPath = null;
            return false;
        }

        private static bool TryResolveWithinPackage(UnityEditor.PackageManager.PackageInfo p, out string srcPath)
        {
            const string CurrentId = "com.coplaydev.unity-mcp";

            srcPath = null;
            if (p == null || p.name != CurrentId)
            {
                return false;
            }

            string packagePath = p.resolvedPath;

            // Preferred tilde folder (embedded but excluded from import)
            string embeddedTilde = Path.Combine(packagePath, "UnityMcpServer~", "src");
            if (Directory.Exists(embeddedTilde) && File.Exists(Path.Combine(embeddedTilde, "server.py")))
            {
                srcPath = embeddedTilde;
                return true;
            }

            // Legacy non-tilde folder
            string embedded = Path.Combine(packagePath, "UnityMcpServer", "src");
            if (Directory.Exists(embedded) && File.Exists(Path.Combine(embedded, "server.py")))
            {
                srcPath = embedded;
                return true;
            }

            // Dev-linked sibling of the package folder
            string sibling = Path.Combine(Path.GetDirectoryName(packagePath) ?? string.Empty, "UnityMcpServer", "src");
            if (Directory.Exists(sibling) && File.Exists(Path.Combine(sibling, "server.py")))
            {
                srcPath = sibling;
                return true;
            }

            return false;
        }
    }
}
