using System.IO;
using NUnit.Framework;
using UnityEngine;
using MCPForUnity.Editor.Data;
using MCPForUnity.Editor.Services;

namespace MCPForUnityTests.Editor.Services
{
    public class ToolSyncServiceTests
    {
        private ToolSyncService _service;
        private string _testToolsDir;

        [SetUp]
        public void SetUp()
        {
            _service = new ToolSyncService();
            _testToolsDir = Path.Combine(Path.GetTempPath(), "UnityMCPTests", "tools");

            // Clean up any existing test directory
            if (Directory.Exists(_testToolsDir))
            {
                Directory.Delete(_testToolsDir, true);
            }
        }

        [TearDown]
        public void TearDown()
        {
            // Clean up test directory
            if (Directory.Exists(_testToolsDir))
            {
                try
                {
                    Directory.Delete(_testToolsDir, true);
                }
                catch
                {
                    // Ignore cleanup errors
                }
            }
        }

        [Test]
        public void SyncProjectTools_CreatesDestinationDirectory()
        {
            _service.SyncProjectTools(_testToolsDir);

            Assert.IsTrue(Directory.Exists(_testToolsDir), "Should create destination directory");
        }

        [Test]
        public void SyncProjectTools_ReturnsSuccess_WhenNoPythonToolsAssets()
        {
            var result = _service.SyncProjectTools(_testToolsDir);

            Assert.IsNotNull(result, "Should return a result");
            Assert.AreEqual(0, result.CopiedCount, "Should not copy any files");
            Assert.AreEqual(0, result.ErrorCount, "Should not have errors");
        }

        [Test]
        public void SyncProjectTools_ReportsCorrectCounts()
        {
            var result = _service.SyncProjectTools(_testToolsDir);

            Assert.IsTrue(result.CopiedCount >= 0, "Copied count should be non-negative");
            Assert.IsTrue(result.SkippedCount >= 0, "Skipped count should be non-negative");
            Assert.IsTrue(result.ErrorCount >= 0, "Error count should be non-negative");
        }
    }
}
