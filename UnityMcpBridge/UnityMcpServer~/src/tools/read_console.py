"""
Defines the read_console tool for accessing Unity Editor console messages.
"""
from typing import Annotated, Any, Literal

from mcp.server.fastmcp import Context
from registry import mcp_for_unity_tool
from unity_connection import send_command_with_retry


@mcp_for_unity_tool(
    description="Gets messages from or clears the Unity Editor console."
)
def read_console(
    ctx: Context,
    action: Annotated[Literal['get', 'clear'], "Get or clear the Unity Editor console."],
    types: Annotated[list[Literal['error', 'warning',
                                  'log', 'all']], "Message types to get"] | None = None,
    count: Annotated[int, "Max messages to return"] | None = None,
    filter_text: Annotated[str, "Text filter for messages"] | None = None,
    since_timestamp: Annotated[str,
                               "Get messages after this timestamp (ISO 8601)"] | None = None,
    format: Annotated[Literal['plain', 'detailed',
                              'json'], "Output format"] | None = None,
    include_stacktrace: Annotated[bool,
                                  "Include stack traces in output"] | None = None
) -> dict[str, Any]:
    ctx.info(f"Processing read_console: {action}")
    # Set defaults if values are None
    action = action if action is not None else 'get'
    types = types if types is not None else ['error', 'warning', 'log']
    format = format if format is not None else 'detailed'
    include_stacktrace = include_stacktrace if include_stacktrace is not None else True

    # Normalize action if it's a string
    if isinstance(action, str):
        action = action.lower()

    # Coerce count defensively (string/float -> int)
    def _coerce_int(value, default=None):
        if value is None:
            return default
        try:
            if isinstance(value, bool):
                return default
            if isinstance(value, int):
                return int(value)
            s = str(value).strip()
            if s.lower() in ("", "none", "null"):
                return default
            return int(float(s))
        except Exception:
            return default

    count = _coerce_int(count)

    # Prepare parameters for the C# handler
    params_dict = {
        "action": action,
        "types": types,
        "count": count,
        "filterText": filter_text,
        "sinceTimestamp": since_timestamp,
        "format": format.lower() if isinstance(format, str) else format,
        "includeStacktrace": include_stacktrace
    }

    # Remove None values unless it's 'count' (as None might mean 'all')
    params_dict = {k: v for k, v in params_dict.items()
                   if v is not None or k == 'count'}

    # Add count back if it was None, explicitly sending null might be important for C# logic
    if 'count' not in params_dict:
        params_dict['count'] = None

    # Use centralized retry helper
    resp = send_command_with_retry("read_console", params_dict)
    if isinstance(resp, dict) and resp.get("success") and not include_stacktrace:
        # Strip stacktrace fields from returned lines if present
        try:
            lines = resp.get("data", {}).get("lines", [])
            for line in lines:
                if isinstance(line, dict) and "stacktrace" in line:
                    line.pop("stacktrace", None)
        except Exception:
            pass
    return resp if isinstance(resp, dict) else {"success": False, "message": str(resp)}
