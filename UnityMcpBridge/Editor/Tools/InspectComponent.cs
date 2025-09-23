using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEditor;
using MCPForUnity.Editor.Helpers;

namespace MCPForUnity.Editor.Tools
{
    /// <summary>
    /// Enhanced component inspection tool with advanced filtering and control capabilities.
    /// </summary>
    public static class InspectComponent
    {
        private static readonly List<string> ValidActions = new List<string>
        {
            "inspect_single",
            "inspect_batch",
            "inspect_by_id",
            "inspect_filtered",
            "compare_components",
            "list_component_types"
        };

        /// <summary>
        /// Main handler for enhanced component inspection operations.
        /// </summary>
        public static object HandleCommand(JObject @params)
        {
            try
            {
                string action = @params["action"]?.ToString();
                if (string.IsNullOrEmpty(action))
                {
                    return Response.Error("Action parameter is required.");
                }

                if (!ValidActions.Contains(action))
                {
                    return Response.Error($"Invalid action '{action}'. Valid actions: {string.Join(", ", ValidActions)}");
                }

                switch (action)
                {
                    case "inspect_single":
                        return InspectSingleComponent(@params);
                    case "inspect_batch":
                        return InspectBatchComponents(@params);
                    case "inspect_by_id":
                        return InspectComponentById(@params);
                    case "inspect_filtered":
                        return InspectFilteredComponents(@params);
                    case "compare_components":
                        return CompareComponents(@params);
                    case "list_component_types":
                        return ListComponentTypes(@params);
                    default:
                        return Response.Error($"Action '{action}' not implemented.");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[InspectComponent] Error: {e.Message}\n{e.StackTrace}");
                return Response.Error($"Error in component inspection: {e.Message}");
            }
        }

        /// <summary>
        /// Inspect a specific component type on a single GameObject.
        /// </summary>
        private static object InspectSingleComponent(JObject @params)
        {
            string target = @params["target"]?.ToString();
            string searchMethod = @params["searchMethod"]?.ToString() ?? "by_name";
            string componentType = @params["componentType"]?.ToString();

            if (string.IsNullOrEmpty(target))
            {
                return Response.Error("Target parameter is required for inspect_single.");
            }

            if (string.IsNullOrEmpty(componentType))
            {
                return Response.Error("ComponentType parameter is required for inspect_single.");
            }

            GameObject targetGo = ManageGameObject.FindObjectInternal(target, searchMethod);
            if (targetGo == null)
            {
                return Response.Error($"Target GameObject '{target}' not found using method '{searchMethod}'.");
            }

            // Find the specific component type
            Type compType = GetComponentTypeByName(componentType);
            if (compType == null)
            {
                return Response.Error($"Component type '{componentType}' not found.");
            }

            Component component = targetGo.GetComponent(compType);
            if (component == null)
            {
                return Response.Error($"Component '{componentType}' not found on GameObject '{targetGo.name}'.");
            }

            // Get component data with filtering options
            var componentData = GetFilteredComponentData(component, @params);

            return Response.Success(
                $"Inspected component '{componentType}' on GameObject '{targetGo.name}'.",
                componentData
            );
        }

        /// <summary>
        /// Inspect components across multiple GameObjects.
        /// </summary>
        private static object InspectBatchComponents(JObject @params)
        {
            var targetsToken = @params["targets"];
            if (targetsToken == null || !targetsToken.HasValues)
            {
                return Response.Error("Targets parameter is required for inspect_batch.");
            }

            string searchMethod = @params["searchMethod"]?.ToString() ?? "by_name";
            string componentType = @params["componentType"]?.ToString();

            var targets = targetsToken.ToObject<List<string>>();
            var results = new List<object>();

            foreach (string target in targets)
            {
                GameObject targetGo = ManageGameObject.FindObjectInternal(target, searchMethod);
                if (targetGo == null)
                {
                    results.Add(new
                    {
                        target = target,
                        success = false,
                        message = $"GameObject '{target}' not found",
                        data = (object)null
                    });
                    continue;
                }

                try
                {
                    object componentData;

                    if (!string.IsNullOrEmpty(componentType))
                    {
                        // Inspect specific component type
                        Type compType = GetComponentTypeByName(componentType);
                        if (compType == null)
                        {
                            results.Add(new
                            {
                                target = target,
                                gameObjectName = targetGo.name,
                                success = false,
                                message = $"Component type '{componentType}' not found",
                                data = (object)null
                            });
                            continue;
                        }

                        Component component = targetGo.GetComponent(compType);
                        if (component == null)
                        {
                            results.Add(new
                            {
                                target = target,
                                gameObjectName = targetGo.name,
                                success = false,
                                message = $"Component '{componentType}' not found on GameObject",
                                data = (object)null
                            });
                            continue;
                        }

                        componentData = GetFilteredComponentData(component, @params);
                    }
                    else
                    {
                        // Inspect all components
                        var components = targetGo.GetComponents<Component>();
                        var filteredComponents = FilterComponents(components, @params);
                        componentData = filteredComponents.Select(c => GetFilteredComponentData(c, @params)).ToList();
                    }

                    results.Add(new
                    {
                        target = target,
                        gameObjectName = targetGo.name,
                        success = true,
                        message = "Components inspected successfully",
                        data = componentData
                    });
                }
                catch (Exception e)
                {
                    results.Add(new
                    {
                        target = target,
                        gameObjectName = targetGo.name,
                        success = false,
                        message = $"Error inspecting components: {e.Message}",
                        data = (object)null
                    });
                }
            }

            return Response.Success($"Batch inspection completed for {targets.Count} targets.", results);
        }

        /// <summary>
        /// Inspect a component by its instance ID.
        /// </summary>
        private static object InspectComponentById(JObject @params)
        {
            var componentInstanceIdToken = @params["componentInstanceId"];
            if (componentInstanceIdToken == null)
            {
                return Response.Error("ComponentInstanceId parameter is required for inspect_by_id.");
            }

            if (!int.TryParse(componentInstanceIdToken.ToString(), out int instanceId))
            {
                return Response.Error("ComponentInstanceId must be a valid integer.");
            }

            // Find component by instance ID
            var allObjects = UnityEngine.Object.FindObjectsOfType<Component>();
            Component targetComponent = allObjects.FirstOrDefault(c => c.GetInstanceID() == instanceId);

            if (targetComponent == null)
            {
                return Response.Error($"Component with instance ID '{instanceId}' not found.");
            }

            var componentData = GetFilteredComponentData(targetComponent, @params);

            return Response.Success(
                $"Inspected component with instance ID '{instanceId}' on GameObject '{targetComponent.gameObject.name}'.",
                componentData
            );
        }

        /// <summary>
        /// Get component type by name, supporting both short names and full names.
        /// </summary>
        private static Type GetComponentTypeByName(string typeName)
        {
            // Try exact type name first
            Type type = Type.GetType(typeName);
            if (type != null) return type;

            // Try with UnityEngine namespace
            type = Type.GetType($"UnityEngine.{typeName}");
            if (type != null) return type;

            // Search through all loaded assemblies
            foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
            {
                type = assembly.GetTypes().FirstOrDefault(t =>
                    t.Name == typeName ||
                    t.FullName == typeName ||
                    t.Name.EndsWith($".{typeName}")
                );
                if (type != null && typeof(Component).IsAssignableFrom(type))
                {
                    return type;
                }
            }

            return null;
        }

        /// <summary>
        /// Filter components based on parameters.
        /// </summary>
        private static Component[] FilterComponents(Component[] components, JObject @params)
        {
            var componentTypesToken = @params["componentTypes"];
            if (componentTypesToken != null && componentTypesToken.HasValues)
            {
                var typeNames = componentTypesToken.ToObject<List<string>>();
                var filteredTypes = new List<Type>();

                foreach (string typeName in typeNames)
                {
                    Type type = GetComponentTypeByName(typeName);
                    if (type != null)
                    {
                        filteredTypes.Add(type);
                    }
                }

                if (filteredTypes.Count > 0)
                {
                    components = components.Where(c => filteredTypes.Any(t => t.IsAssignableFrom(c.GetType()))).ToArray();
                }
            }

            return components;
        }

        /// <summary>
        /// Get component data with filtering applied.
        /// </summary>
        private static object GetFilteredComponentData(Component component, JObject @params)
        {
            bool includeNonPublicSerialized = @params["includeNonPublicSerialized"]?.ToObject<bool>() ?? true;

            // Get base component data
            var baseData = GameObjectSerializer.GetComponentData(component, includeNonPublicSerialized);

            // Apply property filtering if specified
            var includePropertiesToken = @params["includeProperties"];
            var excludePropertiesToken = @params["excludeProperties"];

            if (includePropertiesToken != null || excludePropertiesToken != null)
            {
                return ApplyPropertyFiltering(baseData, includePropertiesToken, excludePropertiesToken);
            }

            return baseData;
        }

        /// <summary>
        /// Apply property filtering to component data.
        /// </summary>
        private static object ApplyPropertyFiltering(object data, JToken includeProperties, JToken excludeProperties)
        {
            if (!(data is Dictionary<string, object> dict))
            {
                return data;
            }

            var filteredDict = new Dictionary<string, object>(dict);

            // Apply include filter
            if (includeProperties != null && includeProperties.HasValues)
            {
                var includeList = includeProperties.ToObject<List<string>>();
                var keysToKeep = new HashSet<string>(includeList) { "typeName", "instanceID" }; // Always keep these

                var keysToRemove = filteredDict.Keys.Where(k => !keysToKeep.Contains(k)).ToList();
                foreach (string key in keysToRemove)
                {
                    filteredDict.Remove(key);
                }
            }

            // Apply exclude filter
            if (excludeProperties != null && excludeProperties.HasValues)
            {
                var excludeList = excludeProperties.ToObject<List<string>>();
                foreach (string key in excludeList)
                {
                    if (key != "typeName" && key != "instanceID") // Never exclude these
                    {
                        filteredDict.Remove(key);
                    }
                }
            }

            return filteredDict;
        }

        /// <summary>
        /// Inspect components with type filtering.
        /// </summary>
        private static object InspectFilteredComponents(JObject @params)
        {
            string target = @params["target"]?.ToString();
            string searchMethod = @params["searchMethod"]?.ToString() ?? "by_name";

            if (string.IsNullOrEmpty(target))
            {
                return Response.Error("Target parameter is required for inspect_filtered.");
            }

            GameObject targetGo = ManageGameObject.FindObjectInternal(target, searchMethod);
            if (targetGo == null)
            {
                return Response.Error($"Target GameObject '{target}' not found using method '{searchMethod}'.");
            }

            var components = targetGo.GetComponents<Component>();
            var filteredComponents = FilterComponents(components, @params);

            bool groupByType = @params["groupByType"]?.ToObject<bool>() ?? false;

            if (groupByType)
            {
                var groupedData = filteredComponents
                    .GroupBy(c => c.GetType().Name)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(c => GetFilteredComponentData(c, @params)).ToList()
                    );

                return Response.Success(
                    $"Inspected {filteredComponents.Length} filtered components on GameObject '{targetGo.name}' (grouped by type).",
                    groupedData
                );
            }
            else
            {
                var componentData = filteredComponents.Select(c => GetFilteredComponentData(c, @params)).ToList();

                return Response.Success(
                    $"Inspected {filteredComponents.Length} filtered components on GameObject '{targetGo.name}'.",
                    componentData
                );
            }
        }

        /// <summary>
        /// Compare the same component type across multiple GameObjects.
        /// </summary>
        private static object CompareComponents(JObject @params)
        {
            var targetsToken = @params["targets"];
            if (targetsToken == null || !targetsToken.HasValues)
            {
                return Response.Error("Targets parameter is required for compare_components.");
            }

            string componentType = @params["componentType"]?.ToString();
            if (string.IsNullOrEmpty(componentType))
            {
                return Response.Error("ComponentType parameter is required for compare_components.");
            }

            string searchMethod = @params["searchMethod"]?.ToString() ?? "by_name";
            var targets = targetsToken.ToObject<List<string>>();

            Type compType = GetComponentTypeByName(componentType);
            if (compType == null)
            {
                return Response.Error($"Component type '{componentType}' not found.");
            }

            var comparisonData = new List<object>();

            foreach (string target in targets)
            {
                GameObject targetGo = ManageGameObject.FindObjectInternal(target, searchMethod);
                if (targetGo == null)
                {
                    comparisonData.Add(new
                    {
                        target = target,
                        gameObjectName = (string)null,
                        hasComponent = false,
                        componentData = (object)null,
                        error = $"GameObject '{target}' not found"
                    });
                    continue;
                }

                Component component = targetGo.GetComponent(compType);
                if (component == null)
                {
                    comparisonData.Add(new
                    {
                        target = target,
                        gameObjectName = targetGo.name,
                        hasComponent = false,
                        componentData = (object)null,
                        error = (string)null
                    });
                }
                else
                {
                    comparisonData.Add(new
                    {
                        target = target,
                        gameObjectName = targetGo.name,
                        hasComponent = true,
                        componentData = GetFilteredComponentData(component, @params),
                        error = (string)null
                    });
                }
            }

            return Response.Success(
                $"Compared component '{componentType}' across {targets.Count} GameObjects.",
                comparisonData
            );
        }

        /// <summary>
        /// List all component types on target GameObject(s).
        /// </summary>
        private static object ListComponentTypes(JObject @params)
        {
            string target = @params["target"]?.ToString();
            var targetsToken = @params["targets"];

            if (string.IsNullOrEmpty(target) && (targetsToken == null || !targetsToken.HasValues))
            {
                return Response.Error("Either target or targets parameter is required for list_component_types.");
            }

            string searchMethod = @params["searchMethod"]?.ToString() ?? "by_name";
            var results = new List<object>();

            // Handle single target
            if (!string.IsNullOrEmpty(target))
            {
                GameObject targetGo = ManageGameObject.FindObjectInternal(target, searchMethod);
                if (targetGo == null)
                {
                    return Response.Error($"Target GameObject '{target}' not found using method '{searchMethod}'.");
                }

                var componentTypes = targetGo.GetComponents<Component>()
                    .Select(c => new
                    {
                        typeName = c.GetType().Name,
                        fullTypeName = c.GetType().FullName,
                        instanceID = c.GetInstanceID()
                    })
                    .ToList();

                return Response.Success(
                    $"Listed {componentTypes.Count} component types on GameObject '{targetGo.name}'.",
                    componentTypes
                );
            }

            // Handle multiple targets
            var targets = targetsToken.ToObject<List<string>>();
            foreach (string targetName in targets)
            {
                GameObject targetGo = ManageGameObject.FindObjectInternal(targetName, searchMethod);
                if (targetGo == null)
                {
                    results.Add(new
                    {
                        target = targetName,
                        gameObjectName = (string)null,
                        success = false,
                        componentTypes = (object)null,
                        error = $"GameObject '{targetName}' not found"
                    });
                    continue;
                }

                var componentTypes = targetGo.GetComponents<Component>()
                    .Select(c => new
                    {
                        typeName = c.GetType().Name,
                        fullTypeName = c.GetType().FullName,
                        instanceID = c.GetInstanceID()
                    })
                    .ToList();

                results.Add(new
                {
                    target = targetName,
                    gameObjectName = targetGo.name,
                    success = true,
                    componentTypes = componentTypes,
                    error = (string)null
                });
            }

            return Response.Success(
                $"Listed component types for {targets.Count} GameObjects.",
                results
            );
        }
    }
}
