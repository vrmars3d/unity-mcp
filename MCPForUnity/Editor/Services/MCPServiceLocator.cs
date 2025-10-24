using System;

namespace MCPForUnity.Editor.Services
{
    /// <summary>
    /// Service locator for accessing MCP services without dependency injection
    /// </summary>
    public static class MCPServiceLocator
    {
        private static IBridgeControlService _bridgeService;
        private static IClientConfigurationService _clientService;
        private static IPathResolverService _pathService;
        private static IPythonToolRegistryService _pythonToolRegistryService;
        private static ITestRunnerService _testRunnerService;
        private static IToolSyncService _toolSyncService;
        private static IPackageUpdateService _packageUpdateService;
        private static IPlatformService _platformService;

        public static IBridgeControlService Bridge => _bridgeService ??= new BridgeControlService();
        public static IClientConfigurationService Client => _clientService ??= new ClientConfigurationService();
        public static IPathResolverService Paths => _pathService ??= new PathResolverService();
        public static IPythonToolRegistryService PythonToolRegistry => _pythonToolRegistryService ??= new PythonToolRegistryService();
        public static ITestRunnerService Tests => _testRunnerService ??= new TestRunnerService();
        public static IToolSyncService ToolSync => _toolSyncService ??= new ToolSyncService();
        public static IPackageUpdateService Updates => _packageUpdateService ??= new PackageUpdateService();
        public static IPlatformService Platform => _platformService ??= new PlatformService();

        /// <summary>
        /// Registers a custom implementation for a service (useful for testing)
        /// </summary>
        /// <typeparam name="T">The service interface type</typeparam>
        /// <param name="implementation">The implementation to register</param>
        public static void Register<T>(T implementation) where T : class
        {
            if (implementation is IBridgeControlService b)
                _bridgeService = b;
            else if (implementation is IClientConfigurationService c)
                _clientService = c;
            else if (implementation is IPathResolverService p)
                _pathService = p;
            else if (implementation is IPythonToolRegistryService ptr)
                _pythonToolRegistryService = ptr;
            else if (implementation is ITestRunnerService t)
                _testRunnerService = t;
            else if (implementation is IToolSyncService ts)
                _toolSyncService = ts;
            else if (implementation is IPackageUpdateService pu)
                _packageUpdateService = pu;
            else if (implementation is IPlatformService ps)
                _platformService = ps;
        }

        /// <summary>
        /// Resets all services to their default implementations (useful for testing)
        /// </summary>
        public static void Reset()
        {
            (_bridgeService as IDisposable)?.Dispose();
            (_clientService as IDisposable)?.Dispose();
            (_pathService as IDisposable)?.Dispose();
            (_pythonToolRegistryService as IDisposable)?.Dispose();
            (_testRunnerService as IDisposable)?.Dispose();
            (_toolSyncService as IDisposable)?.Dispose();
            (_packageUpdateService as IDisposable)?.Dispose();
            (_platformService as IDisposable)?.Dispose();

            _bridgeService = null;
            _clientService = null;
            _pathService = null;
            _pythonToolRegistryService = null;
            _testRunnerService = null;
            _toolSyncService = null;
            _packageUpdateService = null;
            _platformService = null;
        }
    }
}
