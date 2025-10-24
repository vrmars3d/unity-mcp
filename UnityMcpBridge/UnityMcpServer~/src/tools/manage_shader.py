import base64
from typing import Annotated, Any, Literal

from mcp.server.fastmcp import Context
from registry import mcp_for_unity_tool
from unity_connection import send_command_with_retry


@mcp_for_unity_tool(
    description="Manages shader scripts in Unity (create, read, update, delete)."
)
def manage_shader(
    ctx: Context,
    action: Annotated[Literal['create', 'read', 'update', 'delete'], "Perform CRUD operations on shader scripts."],
    name: Annotated[str, "Shader name (no .cs extension)"],
    path: Annotated[str, "Asset path (default: \"Assets/\")"],
    contents: Annotated[str,
                        "Shader code for 'create'/'update'"] | None = None,
) -> dict[str, Any]:
    ctx.info(f"Processing manage_shader: {action}")
    try:
        # Prepare parameters for Unity
        params = {
            "action": action,
            "name": name,
            "path": path,
        }

        # Base64 encode the contents if they exist to avoid JSON escaping issues
        if contents is not None:
            if action in ['create', 'update']:
                # Encode content for safer transmission
                params["encodedContents"] = base64.b64encode(
                    contents.encode('utf-8')).decode('utf-8')
                params["contentsEncoded"] = True
            else:
                params["contents"] = contents

        # Remove None values so they don't get sent as null
        params = {k: v for k, v in params.items() if v is not None}

        # Send command via centralized retry helper
        response = send_command_with_retry("manage_shader", params)

        # Process response from Unity
        if isinstance(response, dict) and response.get("success"):
            # If the response contains base64 encoded content, decode it
            if response.get("data", {}).get("contentsEncoded"):
                decoded_contents = base64.b64decode(
                    response["data"]["encodedContents"]).decode('utf-8')
                response["data"]["contents"] = decoded_contents
                del response["data"]["encodedContents"]
                del response["data"]["contentsEncoded"]

            return {"success": True, "message": response.get("message", "Operation successful."), "data": response.get("data")}
        return response if isinstance(response, dict) else {"success": False, "message": str(response)}

    except Exception as e:
        # Handle Python-side errors (e.g., connection issues)
        return {"success": False, "message": f"Python error managing shader: {str(e)}"}
