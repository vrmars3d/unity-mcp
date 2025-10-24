import sys
import pathlib
import importlib.util


ROOT = pathlib.Path(__file__).resolve().parents[1]
SRC = ROOT / "MCPForUnity" / "UnityMcpServer~" / "src"
sys.path.insert(0, str(SRC))


def _load(path: pathlib.Path, name: str):
    spec = importlib.util.spec_from_file_location(name, path)
    mod = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(mod)
    return mod


manage_script = _load(SRC / "tools" / "manage_script.py", "manage_script_mod2")
manage_script_edits = _load(
    SRC / "tools" / "script_apply_edits.py", "script_apply_edits_mod2")


class DummyMCP:
    def __init__(self): self.tools = {}

    def tool(self, *args, **kwargs):
        def deco(fn): self.tools[fn.__name__] = fn; return fn
        return deco


from tests.test_helpers import DummyContext


def setup_tools():
    mcp = DummyMCP()
    # Import the tools module to trigger decorator registration
    import tools.manage_script
    # Get the registered tools from the registry
    from registry import get_registered_tools
    tools = get_registered_tools()
    # Add all script-related tools to our dummy MCP
    for tool_info in tools:
        tool_name = tool_info['name']
        if any(keyword in tool_name for keyword in ['script', 'apply_text', 'create_script', 'delete_script', 'validate_script', 'get_sha']):
            mcp.tools[tool_name] = tool_info['func']
    return mcp.tools


def test_normalizes_lsp_and_index_ranges(monkeypatch):
    tools = setup_tools()
    apply = tools["apply_text_edits"]
    calls = []

    def fake_send(cmd, params):
        calls.append(params)
        return {"success": True}

    # Patch the send_command_with_retry function at the module level where it's imported
    import unity_connection
    monkeypatch.setattr(unity_connection,
                        "send_command_with_retry", fake_send)
    # No need to patch tools.manage_script; it calls unity_connection.send_command_with_retry

    # LSP-style
    edits = [{
        "range": {"start": {"line": 10, "character": 2}, "end": {"line": 10, "character": 2}},
        "newText": "// lsp\n"
    }]
    apply(DummyContext(), uri="unity://path/Assets/Scripts/F.cs",
          edits=edits, precondition_sha256="x")
    p = calls[-1]
    e = p["edits"][0]
    assert e["startLine"] == 11 and e["startCol"] == 3

    # Index pair
    calls.clear()
    edits = [{"range": [0, 0], "text": "// idx\n"}]
    # fake read to provide contents length

    def fake_read(cmd, params):
        if params.get("action") == "read":
            return {"success": True, "data": {"contents": "hello\n"}}
        return {"success": True}
    
    # Override unity_connection for this read normalization case
    monkeypatch.setattr(unity_connection, "send_command_with_retry", fake_read)
    apply(DummyContext(), uri="unity://path/Assets/Scripts/F.cs",
          edits=edits, precondition_sha256="x")
    # last call is apply_text_edits


def test_noop_evidence_shape(monkeypatch):
    tools = setup_tools()
    apply = tools["apply_text_edits"]
    # Route response from Unity indicating no-op

    def fake_send(cmd, params):
        return {"success": True, "data": {"no_op": True, "evidence": {"reason": "identical_content"}}}
    # Patch the send_command_with_retry function at the module level where it's imported
    import unity_connection
    monkeypatch.setattr(unity_connection,
                        "send_command_with_retry", fake_send)
    # No need to patch tools.manage_script; it calls unity_connection.send_command_with_retry

    resp = apply(DummyContext(), uri="unity://path/Assets/Scripts/F.cs", edits=[
                 {"startLine": 1, "startCol": 1, "endLine": 1, "endCol": 1, "newText": ""}], precondition_sha256="x")
    assert resp["success"] is True
    assert resp.get("data", {}).get("no_op") is True


def test_atomic_multi_span_and_relaxed(monkeypatch):
    tools_text = setup_tools()
    apply_text = tools_text["apply_text_edits"]
    tools_struct = DummyMCP()
    # Import the tools module to trigger decorator registration
    import tools.script_apply_edits
    # Get the registered tools from the registry
    from registry import get_registered_tools
    tools = get_registered_tools()
    # Add all script-related tools to our dummy MCP
    for tool_info in tools:
        tool_name = tool_info['name']
        if any(keyword in tool_name for keyword in ['script_apply', 'apply_edits']):
            tools_struct.tools[tool_name] = tool_info['func']
    # Fake send for read and write; verify atomic applyMode and validate=relaxed passes through
    sent = {}

    def fake_send(cmd, params):
        if params.get("action") == "read":
            return {"success": True, "data": {"contents": "public class C{\nvoid M(){ int x=2; }\n}\n"}}
        sent.setdefault("calls", []).append(params)
        return {"success": True}
    
    # Patch the send_command_with_retry function at the module level where it's imported
    import unity_connection
    monkeypatch.setattr(unity_connection,
                        "send_command_with_retry", fake_send)

    edits = [
        {"startLine": 2, "startCol": 14, "endLine": 2, "endCol": 15, "newText": "3"},
        {"startLine": 3, "startCol": 2, "endLine": 3,
            "endCol": 2, "newText": "// tail\n"}
    ]
    resp = apply_text(DummyContext(), uri="unity://path/Assets/Scripts/C.cs", edits=edits,
                      precondition_sha256="sha", options={"validate": "relaxed", "applyMode": "atomic"})
    assert resp["success"] is True
    # Last manage_script call should include options with applyMode atomic and validate relaxed
    last = sent["calls"][-1]
    assert last.get("options", {}).get("applyMode") == "atomic"
    assert last.get("options", {}).get("validate") == "relaxed"
