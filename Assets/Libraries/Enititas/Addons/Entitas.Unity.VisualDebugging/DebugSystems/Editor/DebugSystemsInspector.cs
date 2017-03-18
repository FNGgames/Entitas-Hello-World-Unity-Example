using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Entitas.Unity.VisualDebugging {

    [CustomEditor(typeof(DebugSystemsBehaviour))]
    public class DebugSystemsInspector : Editor {

        enum SortMethod {
            OrderOfOccurrence,

            Name,
            NameDescending,

            ExecutionTime,
            ExecutionTimeDescending
        }

        SystemsMonitor _systemsMonitor;
        Queue<float> _systemMonitorData;
        const int SYSTEM_MONITOR_DATA_LENGTH = 60;

        static bool _showDetails = false;
        static bool _showSystemsMonitor = true;
        static bool _showSystemsList = false;

        static bool _showInitializeSystems = true;
        static bool _showExecuteSystems = true;
        static bool _showCleanupSystems = true;
        static bool _showTearDownSystems = true;
        static bool _hideEmptySystems = true;
        static string _systemNameSearchString = string.Empty;
        
        float _threshold;
        SortMethod _systemSortMethod;

        int _lastRenderedFrameCount;

        GUIContent _stepButtonContent;
        GUIContent _pauseButtonContent;

        public override void OnInspectorGUI() {
            var debugSystemsBehaviour = (DebugSystemsBehaviour)target;
            var systems = debugSystemsBehaviour.systems;

            EditorGUILayout.Space();
            drawSystemsOverview(systems);

            EditorGUILayout.Space();
            drawSystemsMonitor(systems);

            EditorGUILayout.Space();
            drawSystemList(systems);

            EditorGUILayout.Space();

            EditorUtility.SetDirty(target);
        }

        static void drawSystemsOverview(DebugSystems systems) {
            _showDetails = EntitasEditorLayout.DrawSectionHeaderToggle("Details", _showDetails);
            if(_showDetails) {
                EntitasEditorLayout.BeginSectionContent();
                {
                    EditorGUILayout.LabelField(systems.name, EditorStyles.boldLabel);
                    EditorGUILayout.LabelField("Initialize Systems", systems.totalInitializeSystemsCount.ToString());
                    EditorGUILayout.LabelField("Execute Systems", systems.totalExecuteSystemsCount.ToString());
                    EditorGUILayout.LabelField("Cleanup Systems", systems.totalCleanupSystemsCount.ToString());
                    EditorGUILayout.LabelField("TearDown Systems", systems.totalTearDownSystemsCount.ToString());
                    EditorGUILayout.LabelField("Total Systems", systems.totalSystemsCount.ToString());
                }
                EntitasEditorLayout.EndSectionContent();
            }
        }

        void drawSystemsMonitor(DebugSystems systems) {
            if(_systemsMonitor == null) {
                _systemsMonitor = new SystemsMonitor(SYSTEM_MONITOR_DATA_LENGTH);
                _systemMonitorData = new Queue<float>(new float[SYSTEM_MONITOR_DATA_LENGTH]);
            }

            _showSystemsMonitor = EntitasEditorLayout.DrawSectionHeaderToggle("Performance", _showSystemsMonitor);
            if(_showSystemsMonitor) {
                EntitasEditorLayout.BeginSectionContent();
                {
                    EditorGUILayout.BeginHorizontal();
                    {
                        EditorGUILayout.BeginVertical();
                        {
                            EditorGUILayout.LabelField("Execution duration", systems.executeDuration.ToString());
                            EditorGUILayout.LabelField("Cleanup duration", systems.cleanupDuration.ToString());
                        }
                        EditorGUILayout.EndVertical();

                        if(_stepButtonContent == null) {
                            _stepButtonContent = EditorGUIUtility.IconContent("StepButton On");
                        }
                        if(_pauseButtonContent == null) {
                            _pauseButtonContent = EditorGUIUtility.IconContent("PauseButton On");
                        }

                        systems.paused = GUILayout.Toggle(systems.paused, _pauseButtonContent, "CommandLeft");

                        if(GUILayout.Button(_stepButtonContent, "CommandRight")) {
                            systems.paused = true;
                            systems.StepExecute();
                            systems.StepCleanup();
                            addDuration((float)systems.executeDuration + (float)systems.cleanupDuration);
                        }
                    }
                    EditorGUILayout.EndHorizontal();

                    if(!EditorApplication.isPaused && !systems.paused) {
                        addDuration((float)systems.executeDuration + (float)systems.cleanupDuration);
                    }
                    _systemsMonitor.Draw(_systemMonitorData.ToArray(), 80f);
                }
                EntitasEditorLayout.EndSectionContent();
            }
        }

        void drawSystemList(DebugSystems systems) {
            _showSystemsList = EntitasEditorLayout.DrawSectionHeaderToggle("Systems", _showSystemsList);
            if(_showSystemsList) {
                EntitasEditorLayout.BeginSectionContent();
                {
                    EditorGUILayout.BeginHorizontal();
                    {
                        DebugSystems.avgResetInterval = (AvgResetInterval)EditorGUILayout.EnumPopup("Reset average duration Ø", DebugSystems.avgResetInterval);
                        if(GUILayout.Button("Reset Ø now", EditorStyles.miniButton, GUILayout.Width(88))) {
                            systems.ResetDurations();
                        }
                    }
                    EditorGUILayout.EndHorizontal();

                    _threshold = EditorGUILayout.Slider("Threshold Ø ms", _threshold, 0f, 33f);

                    _hideEmptySystems = EditorGUILayout.Toggle("Hide empty systems", _hideEmptySystems);
                    EditorGUILayout.Space();

                    EditorGUILayout.BeginHorizontal();
                    {
                        _systemSortMethod = (SortMethod)EditorGUILayout.EnumPopup(_systemSortMethod, EditorStyles.popup, GUILayout.Width(150));
                        _systemNameSearchString = EntitasEditorLayout.SearchTextField(_systemNameSearchString);
                    }
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.Space();

                    _showInitializeSystems = EntitasEditorLayout.DrawSectionHeaderToggle("Initialize Systems", _showInitializeSystems);
                    if(_showInitializeSystems && shouldShowSystems(systems, SystemInterfaceFlags.IInitializeSystem)) {
                        EntitasEditorLayout.BeginSectionContent();
                        {
                            var systemsDrawn = drawSystemInfos(systems, SystemInterfaceFlags.IInitializeSystem);
                            if(systemsDrawn == 0) {
                                EditorGUILayout.LabelField(string.Empty);
                            }
                        }
                        EntitasEditorLayout.EndSectionContent();
                    }

                    _showExecuteSystems = EntitasEditorLayout.DrawSectionHeaderToggle("Execute Systems", _showExecuteSystems);
                    if(_showExecuteSystems && shouldShowSystems(systems, SystemInterfaceFlags.IExecuteSystem)) {
                        EntitasEditorLayout.BeginSectionContent();
                        {
                            var systemsDrawn = drawSystemInfos(systems, SystemInterfaceFlags.IExecuteSystem);
                            if(systemsDrawn == 0) {
                                EditorGUILayout.LabelField(string.Empty);
                            }
                        }
                        EntitasEditorLayout.EndSectionContent();
                    }

                    _showCleanupSystems = EntitasEditorLayout.DrawSectionHeaderToggle("Cleanup Systems", _showCleanupSystems);
                    if(_showCleanupSystems && shouldShowSystems(systems, SystemInterfaceFlags.ICleanupSystem)) {
                        EntitasEditorLayout.BeginSectionContent();
                        {
                            var systemsDrawn = drawSystemInfos(systems, SystemInterfaceFlags.ICleanupSystem);
                            if(systemsDrawn == 0) {
                                EditorGUILayout.LabelField(string.Empty);
                            }
                        }
                        EntitasEditorLayout.EndSectionContent();
                    }

                    _showTearDownSystems = EntitasEditorLayout.DrawSectionHeaderToggle("TearDown Systems", _showTearDownSystems);
                    if(_showTearDownSystems && shouldShowSystems(systems, SystemInterfaceFlags.ITearDownSystem)) {
                        EntitasEditorLayout.BeginSectionContent();
                        {
                            var systemsDrawn = drawSystemInfos(systems, SystemInterfaceFlags.ITearDownSystem);
                            if(systemsDrawn == 0) {
                                EditorGUILayout.LabelField(string.Empty);
                            }
                        }
                        EntitasEditorLayout.EndSectionContent();
                    }
                }
                EntitasEditorLayout.EndSectionContent();
            }
        }

        int drawSystemInfos(DebugSystems systems, SystemInterfaceFlags type) {
            SystemInfo[] systemInfos = null;

            switch(type) {
                case SystemInterfaceFlags.IInitializeSystem:
                    systemInfos = systems.initializeSystemInfos
                        .Where(systemInfo => systemInfo.initializationDuration >= _threshold)
                        .ToArray();
                    break;
                case SystemInterfaceFlags.IExecuteSystem:
                    systemInfos = systems.executeSystemInfos
                        .Where(systemInfo => systemInfo.averageExecutionDuration >= _threshold)
                        .ToArray();
                    break;
                case SystemInterfaceFlags.ICleanupSystem:
                    systemInfos = systems.cleanupSystemInfos
                        .Where(systemInfo => systemInfo.cleanupDuration >= _threshold)
                        .ToArray();
                    break;
                case SystemInterfaceFlags.ITearDownSystem:
                    systemInfos = systems.tearDownSystemInfos
                        .Where(systemInfo => systemInfo.teardownDuration >= _threshold)
                        .ToArray();
                    break;
            }

            systemInfos = getSortedSystemInfos(systemInfos, _systemSortMethod);

            var systemsDrawn = 0;
            foreach(var systemInfo in systemInfos) {
                var debugSystems = systemInfo.system as DebugSystems;
                if(debugSystems != null) {
                    if(!shouldShowSystems(debugSystems, type)) {
                        continue;
                    }
                }

                if(EntitasEditorLayout.MatchesSearchString(systemInfo.systemName.ToLower(), _systemNameSearchString.ToLower())) {
                    EditorGUILayout.BeginHorizontal();
                    {
                        var indent = EditorGUI.indentLevel;
                        EditorGUI.indentLevel = 0;

                        var wasActive = systemInfo.isActive;
                        if(systemInfo.areAllParentsActive) {
                            systemInfo.isActive = EditorGUILayout.Toggle(systemInfo.isActive, GUILayout.Width(20));
                        } else {
                            EditorGUI.BeginDisabledGroup(true);
                            {
                                EditorGUILayout.Toggle(false, GUILayout.Width(20));
                            }
                        }
                        EditorGUI.EndDisabledGroup();

                        EditorGUI.indentLevel = indent;

                        if(systemInfo.isActive != wasActive) {
                            var reactiveSystem = systemInfo.system as IReactiveSystem;
                            if(reactiveSystem != null) {
                                if(systemInfo.isActive) {
                                    reactiveSystem.Activate();
                                } else {
                                    reactiveSystem.Deactivate();
                                }
                            }
                        }

                        switch(type) {
                            case SystemInterfaceFlags.IInitializeSystem:
                                EditorGUILayout.LabelField(systemInfo.systemName, systemInfo.initializationDuration.ToString(), getSystemStyle(systemInfo));
                                break;
                            case SystemInterfaceFlags.IExecuteSystem:
                                var avgE = string.Format("Ø {0:00.000}", systemInfo.averageExecutionDuration).PadRight(12);
                                var minE = string.Format("▼ {0:00.000}", systemInfo.minExecutionDuration).PadRight(12);
                                var maxE = string.Format("▲ {0:00.000}", systemInfo.maxExecutionDuration);
                                EditorGUILayout.LabelField(systemInfo.systemName, avgE + minE + maxE, getSystemStyle(systemInfo));
                                break;
                            case SystemInterfaceFlags.ICleanupSystem:
                                var avgC = string.Format("Ø {0:00.000}", systemInfo.averageCleanupDuration).PadRight(12);
                                var minC = string.Format("▼ {0:00.000}", systemInfo.minCleanupDuration).PadRight(12);
                                var maxC = string.Format("▲ {0:00.000}", systemInfo.maxCleanupDuration);
                                EditorGUILayout.LabelField(systemInfo.systemName, avgC + minC + maxC, getSystemStyle(systemInfo));
                                break;
                            case SystemInterfaceFlags.ITearDownSystem:
                                EditorGUILayout.LabelField(systemInfo.systemName, systemInfo.teardownDuration.ToString(), getSystemStyle(systemInfo));
                                break;
                        }
                    }
                    EditorGUILayout.EndHorizontal();

                    systemsDrawn += 1;
                }

                var debugSystem = systemInfo.system as DebugSystems;
                if(debugSystem != null) {
                    var indent = EditorGUI.indentLevel;
                    EditorGUI.indentLevel += 1;
                    systemsDrawn += drawSystemInfos(debugSystem, type);
                    EditorGUI.indentLevel = indent;
                }
            }

            return systemsDrawn;
        }

        static SystemInfo[] getSortedSystemInfos(SystemInfo[] systemInfos, SortMethod sortMethod) {
            if(sortMethod == SortMethod.Name) {
                return systemInfos
                    .OrderBy(systemInfo => systemInfo.systemName)
                    .ToArray();
            }
            if(sortMethod == SortMethod.NameDescending) {
                return systemInfos
                    .OrderByDescending(systemInfo => systemInfo.systemName)
                    .ToArray();
            }

            if(sortMethod == SortMethod.ExecutionTime) {
                return systemInfos
                    .OrderBy(systemInfo => systemInfo.averageExecutionDuration)
                    .ToArray();
            }
            if(sortMethod == SortMethod.ExecutionTimeDescending) {
                return systemInfos
                    .OrderByDescending(systemInfo => systemInfo.averageExecutionDuration)
                    .ToArray();
            }

            return systemInfos;
        }

        static bool shouldShowSystems(DebugSystems systems, SystemInterfaceFlags type) {
            if(!_hideEmptySystems) {
                return true;
            }

            switch(type) {
                case SystemInterfaceFlags.IInitializeSystem:
                    return systems.totalInitializeSystemsCount > 0;
                case SystemInterfaceFlags.IExecuteSystem:
                    return systems.totalExecuteSystemsCount > 0;
                case SystemInterfaceFlags.ICleanupSystem:
                    return systems.totalCleanupSystemsCount > 0;
                case SystemInterfaceFlags.ITearDownSystem:
                    return systems.totalTearDownSystemsCount > 0;
                default:
                    return true;
            }
        }

        static GUIStyle getSystemStyle(SystemInfo systemInfo) {
            var style = new GUIStyle(GUI.skin.label);
            var color = systemInfo.isReactiveSystems && EditorGUIUtility.isProSkin
                            ? Color.white
                            : style.normal.textColor;

            style.normal.textColor = color;

            return style;
        }

        void addDuration(float duration) {
            // OnInspectorGUI is called twice per frame - only add duration once
            if(Time.renderedFrameCount != _lastRenderedFrameCount) {
                _lastRenderedFrameCount = Time.renderedFrameCount;

                if(_systemMonitorData.Count >= SYSTEM_MONITOR_DATA_LENGTH) {
                    _systemMonitorData.Dequeue();
                }

                _systemMonitorData.Enqueue(duration);
            }
        }
    }
}
