namespace MCPForUnity.Editor.Services
{
    /// <summary>
    /// Service for controlling the MCP for Unity Bridge connection
    /// </summary>
    public interface IBridgeControlService
    {
        /// <summary>
        /// Gets whether the bridge is currently running
        /// </summary>
        bool IsRunning { get; }
        
        /// <summary>
        /// Gets the current port the bridge is listening on
        /// </summary>
        int CurrentPort { get; }
        
        /// <summary>
        /// Gets whether the bridge is in auto-connect mode
        /// </summary>
        bool IsAutoConnectMode { get; }
        
        /// <summary>
        /// Starts the MCP for Unity Bridge
        /// </summary>
        void Start();
        
        /// <summary>
        /// Stops the MCP for Unity Bridge
        /// </summary>
        void Stop();
        
        /// <summary>
        /// Verifies the bridge connection by sending a ping and waiting for a pong response
        /// </summary>
        /// <param name="port">The port to verify</param>
        /// <returns>Verification result with detailed status</returns>
        BridgeVerificationResult Verify(int port);
    }
    
    /// <summary>
    /// Result of a bridge verification attempt
    /// </summary>
    public class BridgeVerificationResult
    {
        /// <summary>
        /// Whether the verification was successful
        /// </summary>
        public bool Success { get; set; }
        
        /// <summary>
        /// Human-readable message about the verification result
        /// </summary>
        public string Message { get; set; }
        
        /// <summary>
        /// Whether the handshake was valid (FRAMING=1 protocol)
        /// </summary>
        public bool HandshakeValid { get; set; }
        
        /// <summary>
        /// Whether the ping/pong exchange succeeded
        /// </summary>
        public bool PingSucceeded { get; set; }
    }
}
