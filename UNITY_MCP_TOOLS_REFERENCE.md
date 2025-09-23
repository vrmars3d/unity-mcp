# Unity MCP Tools Reference

## Overview
Unity MCP provides comprehensive tools for managing Unity projects through code. All tools return structured responses with `success`, `message`, and `data` fields.

## Core Tools

### 1. `manage_editor`
**Purpose**: Control Unity editor state and settings  
**Actions**:
- `play` - Start play mode
- `pause` - Pause play mode  
- `stop` - Stop play mode
- `get_state` - Get editor state (playing, paused, etc.)
- `get_project_root` - Get project root path
- `get_windows` - List open editor windows
- `get_active_tool` - Get currently active tool
- `get_selection` - Get selected objects
- `set_active_tool` - Set active tool
- `add_tag` - Add new tag
- `remove_tag` - Remove tag
- `get_tags` - List all tags
- `add_layer` - Add new layer
- `remove_layer` - Remove layer
- `get_layers` - List all layers

### 2. `manage_scene`
**Purpose**: Scene management operations  
**Actions**:
- `create` - Create new scene
- `load` - Load scene by name/path or build index
- `save` - Save current scene
- `get_hierarchy` - Get scene hierarchy structure
- `get_active` - Get active scene info
- `get_build_settings` - Get build settings scenes

### 3. `manage_gameobject`
**Purpose**: GameObject operations in scenes  
**Actions**:
- `create` - Create GameObject (empty, primitive, or from prefab)
- `modify` - Modify GameObject properties (name, tag, parent, transform, etc.)
- `delete` - Delete GameObject
- `find` - Find GameObjects by name, tag, layer, component type
- `get_components` - Get all components on GameObject
- `add_component` - Add component to GameObject
- `remove_component` - Remove component from GameObject
- `set_component_property` - Set component property value

**Search Methods**: `by_name`, `by_id`, `by_path`, `by_tag`, `by_layer`, `by_component`

### 4. `manage_asset`
**Purpose**: Asset management in project  
**Actions**:
- `import` - Import assets
- `create` - Create new assets (materials, textures, etc.)
- `modify` - Modify asset properties
- `delete` - Delete assets
- `duplicate` - Duplicate assets
- `move` - Move assets
- `rename` - Rename assets
- `search` - Search for assets
- `get_info` - Get asset information
- `create_folder` - Create asset folders
- `get_components` - Get components on prefab assets

### 5. `manage_script`
**Purpose**: C# script file management  
**Actions**:
- `create` - Create new script
- `read` - Read script content
- `update` - Update script content
- `delete` - Delete script
- `validate` - Validate script syntax
- `get_capabilities` - Get script management capabilities

### 6. `script_apply_edits`
**Purpose**: Structured C# script editing  
**Operations**:
- `replace_method` - Replace entire method
- `delete_method` - Delete method
- `insert_method` - Insert new method
- `anchor_insert` - Insert at pattern location
- `anchor_delete` - Delete at pattern location
- `anchor_replace` - Replace at pattern location

### 7. `manage_shader`
**Purpose**: Shader script management  
**Actions**:
- `create` - Create new shader
- `read` - Read shader content
- `update` - Update shader content
- `delete` - Delete shader

### 8. `read_console`
**Purpose**: Unity console message management  
**Actions**:
- `get` - Get console messages with filtering
- `clear` - Clear console messages

**Filters**: message types (`error`, `warning`, `log`), count, text filter, timestamp

### 9. `manage_menu_item`
**Purpose**: Unity editor menu operations  
**Actions**:
- `execute` - Execute menu item
- `list` - List available menu items
- `exists` - Check if menu item exists

### 10. `inspect_component` ⭐ **NEW**
**Purpose**: Advanced component inspection with filtering  
**Actions**:
- `inspect_single` - Inspect specific component type on GameObject
- `inspect_batch` - Inspect components across multiple GameObjects
- `inspect_by_id` - Inspect component by instance ID
- `inspect_filtered` - Inspect with component type filtering
- `compare_components` - Compare same component across GameObjects
- `list_component_types` - List all component types on targets

**Advanced Features**:
- Property inclusion/exclusion filtering
- Batch operations across multiple GameObjects
- Component type filtering
- Direct instance ID lookup
- Result grouping by type

## Resource Tools

### 11. `list_resources`
**Purpose**: List project files and URIs  
**Parameters**: pattern (glob), folder scope, limit

### 12. `read_resource`
**Purpose**: Read file content with slicing  
**Parameters**: URI, line ranges, byte limits

### 13. `find_in_file`
**Purpose**: Search within files using regex  
**Parameters**: URI, pattern, case sensitivity

## Quick Usage Examples

```python
# Scene management
manage_scene(action="load", name="MainScene")
manage_scene(action="get_hierarchy")

# GameObject operations
manage_gameobject(action="create", name="Player", primitive_type="Cube")
manage_gameobject(action="find", search_term="Player", search_method="by_name")

# Component inspection (basic)
manage_gameobject(action="get_components", target="Player", search_method="by_name")

# Component inspection (advanced)
inspect_component(action="inspect_single", target="Player", component_type="Transform")
inspect_component(action="compare_components", targets=["Player", "Enemy"], component_type="Rigidbody")

# Asset management
manage_asset(action="create", path="Materials/NewMaterial.mat", asset_type="Material")

# Script editing
script_apply_edits(name="PlayerController", path="Assets/Scripts", 
                  edits=[{"op": "replace_method", "methodName": "Update", "replacement": "..."}])

# Editor control
manage_editor(action="play")
manage_editor(action="get_state")
```

## Key Features
- **Structured responses** with success/error handling
- **Flexible search methods** for finding objects
- **Batch operations** for efficiency
- **Advanced filtering** for precise control
- **Comprehensive coverage** of Unity functionality
- **Safe operations** with validation and error handling
