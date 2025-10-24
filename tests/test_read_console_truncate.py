import sys
import pathlib
import importlib.util

ROOT = pathlib.Path(__file__).resolve().parents[1]
SRC = ROOT / "MCPForUnity" / "UnityMcpServer~" / "src"
sys.path.insert(0, str(SRC))


def _load_module(path: pathlib.Path, name: str):
    spec = importlib.util.spec_from_file_location(name, path)
    if spec is None or spec.loader is None:
        raise ImportError(f"Cannot load module {name} from {path}")
    mod = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(mod)
    return mod


read_console_mod = _load_module(
    SRC / "tools" / "read_console.py", "read_console_mod")


class DummyMCP:
    def __init__(self):
        self.tools = {}

    def tool(self, *args, **kwargs):
        def deco(fn):
            self.tools[fn.__name__] = fn
            return fn
        return deco


from tests.test_helpers import DummyContext


def setup_tools():
    mcp = DummyMCP()
    # Import the tools module to trigger decorator registration
    import tools.read_console
    # Get the registered tools from the registry
    from registry import get_registered_tools
    registered_tools = get_registered_tools()
    # Add all console-related tools to our dummy MCP
    for tool_info in registered_tools:
        tool_name = tool_info['name']
        if any(keyword in tool_name for keyword in ['read_console', 'console']):
            mcp.tools[tool_name] = tool_info['func']
    return mcp.tools


def test_read_console_full_default(monkeypatch):
    tools = setup_tools()
    read_console = tools["read_console"]

    captured = {}

    def fake_send(cmd, params):
        captured["params"] = params
        return {
            "success": True,
            "data": {"lines": [{"level": "error", "message": "oops", "stacktrace": "trace", "time": "t"}]},
        }

    # Patch the send_command_with_retry function in the tools module
    import tools.read_console
    monkeypatch.setattr(tools.read_console,
                        "send_command_with_retry", fake_send)

    resp = read_console(ctx=DummyContext(), action="get", count=10)
    assert resp == {
        "success": True,
        "data": {"lines": [{"level": "error", "message": "oops", "stacktrace": "trace", "time": "t"}]},
    }
    assert captured["params"]["count"] == 10
    assert captured["params"]["includeStacktrace"] is True


def test_read_console_truncated(monkeypatch):
    tools = setup_tools()
    read_console = tools["read_console"]

    captured = {}

    def fake_send(cmd, params):
        captured["params"] = params
        return {
            "success": True,
            "data": {"lines": [{"level": "error", "message": "oops", "stacktrace": "trace"}]},
        }

    # Patch the send_command_with_retry function in the tools module
    import tools.read_console
    monkeypatch.setattr(tools.read_console,
                        "send_command_with_retry", fake_send)

    resp = read_console(ctx=DummyContext(), action="get", count=10, include_stacktrace=False)
    assert resp == {"success": True, "data": {
        "lines": [{"level": "error", "message": "oops"}]}}
    assert captured["params"]["includeStacktrace"] is False
