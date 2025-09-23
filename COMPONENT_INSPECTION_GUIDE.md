# Enhanced Component Inspection Tool Guide

## Overview

The `inspect_component` tool provides advanced component inspection capabilities beyond the basic `get_components` action in `manage_gameobject`. It offers fine-grained control over component inspection with filtering, batch operations, and specialized lookup methods.

## Quick Reference

### Existing vs New Tool

**Existing `manage_gameobject` with `get_components`:**
```python
manage_gameobject(
    action="get_components",
    target="MyGameObject",
    search_method="by_name"
)
# Returns ALL components on the GameObject
```

**New `inspect_component` tool:**
```python
inspect_component(
    action="inspect_single",
    target="MyGameObject", 
    search_method="by_name",
    component_type="Transform"
)
# Returns only the Transform component with advanced filtering
```

## Actions

### 1. `inspect_single`
Inspect a specific component type on a single GameObject.

```python
inspect_component(
    action="inspect_single",
    target="Main Camera",
    search_method="by_name",
    component_type="Camera",
    include_properties=["fieldOfView", "nearClipPlane", "farClipPlane"]
)
```

### 2. `inspect_by_id`
Inspect a component directly by its instance ID.

```python
inspect_component(
    action="inspect_by_id",
    component_instance_id=12345
)
```

### 3. `inspect_batch`
Inspect components across multiple GameObjects.

```python
inspect_component(
    action="inspect_batch",
    targets=["Player", "Enemy1", "Enemy2"],
    search_method="by_name",
    component_type="Rigidbody"
)
```

### 4. `inspect_filtered`
Inspect components with type filtering and grouping.

```python
inspect_component(
    action="inspect_filtered",
    target="Player",
    search_method="by_name",
    component_types=["Transform", "Rigidbody", "Collider"],
    group_by_type=True
)
```

### 5. `compare_components`
Compare the same component type across multiple GameObjects.

```python
inspect_component(
    action="compare_components",
    targets=["Player", "Enemy1", "Enemy2"],
    search_method="by_name",
    component_type="Transform"
)
```

### 6. `list_component_types`
List all component types on target GameObject(s).

```python
inspect_component(
    action="list_component_types",
    target="Player",
    search_method="by_name"
)
```

## Parameters

### Core Parameters
- `action`: Operation to perform (required)
- `target`: Single GameObject identifier
- `targets`: Multiple GameObject identifiers for batch operations
- `search_method`: How to find GameObjects (`by_name`, `by_id`, `by_path`)

### Component Filtering
- `component_type`: Specific component type to inspect
- `component_types`: Multiple component types to filter
- `component_instance_id`: Direct component instance ID lookup

### Property Control
- `include_properties`: Specific properties to include
- `exclude_properties`: Specific properties to exclude
- `include_non_public_serialized`: Include private `[SerializeField]` fields (default: true)
- `include_inherited`: Include inherited properties (default: true)

### Output Control
- `format`: Output format (`detailed`, `summary`, `names_only`)
- `group_by_type`: Group results by component type

## Advanced Examples

### Property Filtering
```python
# Include only specific properties
inspect_component(
    action="inspect_single",
    target="Player",
    component_type="Transform",
    include_properties=["position", "rotation", "scale"]
)

# Exclude problematic properties
inspect_component(
    action="inspect_single", 
    target="Player",
    component_type="Rigidbody",
    exclude_properties=["velocity", "angularVelocity"]
)
```

### Batch Operations
```python
# Compare Transform components across multiple objects
inspect_component(
    action="compare_components",
    targets=["Player", "Enemy1", "Enemy2", "Boss"],
    component_type="Transform"
)
```

### Component Type Discovery
```python
# List all component types on multiple objects
inspect_component(
    action="list_component_types",
    targets=["Player", "UI Canvas", "Main Camera"]
)
```

## Key Advantages

1. **Targeted Inspection**: Inspect specific component types instead of all components
2. **Batch Operations**: Work with multiple GameObjects simultaneously
3. **Advanced Filtering**: Include/exclude specific properties
4. **Direct Lookup**: Find components by instance ID
5. **Comparison**: Compare same component types across objects
6. **Flexible Output**: Control output format and grouping

## Use Cases

- **Performance Analysis**: Compare Rigidbody settings across multiple physics objects
- **Debugging**: Inspect specific component properties without noise
- **Batch Validation**: Verify component configurations across similar objects
- **Component Discovery**: Find what components exist on objects
- **Property Auditing**: Check specific properties across multiple instances

## Error Handling

The tool provides detailed error messages for:
- Invalid actions
- Missing required parameters
- GameObject not found
- Component type not found
- Component not found on GameObject
- Invalid instance IDs

## Integration

The tool is automatically registered with the MCP server and available alongside existing tools. It complements rather than replaces the existing `get_components` functionality.
