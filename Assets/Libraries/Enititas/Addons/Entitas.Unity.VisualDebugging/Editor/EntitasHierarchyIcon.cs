using UnityEditor;
using UnityEngine;

namespace Entitas.Unity.VisualDebugging {

    [InitializeOnLoad]
    public static class EntitasHierarchyIcon {

        static Texture2D contextHierarchyIcon {
            get {
                if(_contextHierarchyIcon == null) {
                    _contextHierarchyIcon = EntitasEditorLayout.LoadTexture("l:EntitasContextHierarchyIcon");
                }
                return _contextHierarchyIcon;
            }
        }

        static Texture2D contextErrorHierarchyIcon {
            get {
                if(_contextErrorHierarchyIcon == null) {
                    _contextErrorHierarchyIcon = EntitasEditorLayout.LoadTexture("l:EntitasContextErrorHierarchyIcon");
                }
                return _contextErrorHierarchyIcon;
            }
        }

        static Texture2D entityHierarchyIcon {
            get {
                if(_entityHierarchyIcon == null) {
                    _entityHierarchyIcon = EntitasEditorLayout.LoadTexture("l:EntitasEntityHierarchyIcon");
                }
                return _entityHierarchyIcon;
            }
        }

        static Texture2D entityErrorHierarchyIcon {
            get {
                if(_entityErrorHierarchyIcon == null) {
                    _entityErrorHierarchyIcon = EntitasEditorLayout.LoadTexture("l:EntitasEntityErrorHierarchyIcon");
                }
                return _entityErrorHierarchyIcon;
            }
        }

        static Texture2D systemsHierarchyIcon {
            get {
                if(_systemsHierarchyIcon == null) {
                    _systemsHierarchyIcon = EntitasEditorLayout.LoadTexture("l:EntitasSystemsHierarchyIcon");
                }
                return _systemsHierarchyIcon;
            }
        }

        static Texture2D _contextHierarchyIcon;
        static Texture2D _contextErrorHierarchyIcon;
        static Texture2D _entityHierarchyIcon;
        static Texture2D _entityErrorHierarchyIcon;
        static Texture2D _systemsHierarchyIcon;

        static EntitasHierarchyIcon() {
            EditorApplication.hierarchyWindowItemOnGUI += onHierarchyWindowItemOnGUI;
        }

        static void onHierarchyWindowItemOnGUI(int instanceID, Rect selectionRect) {
            var gameObject = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
            if(gameObject != null) {
                const float iconSize = 16f;
                const float iconOffset = iconSize + 2f;
                var rect = new Rect(selectionRect.x + selectionRect.width - iconOffset, selectionRect.y, iconSize, iconSize);

                var contextObserver = gameObject.GetComponent<ContextObserverBehaviour>();
                var entityBehaviour = gameObject.GetComponent<EntityBehaviour>();
                var debugSystemsBehaviour = gameObject.GetComponent<DebugSystemsBehaviour>();

                if(contextObserver != null) {
                    if(contextObserver.contextObserver.context.retainedEntitiesCount != 0) {
                        GUI.DrawTexture(rect, contextErrorHierarchyIcon);
                    } else {
                        GUI.DrawTexture(rect, contextHierarchyIcon);
                    }
                } else if(entityBehaviour != null) {
                    if(entityBehaviour.entity.isEnabled) {
                        GUI.DrawTexture(rect, entityHierarchyIcon);
                    } else {
                        GUI.DrawTexture(rect, entityErrorHierarchyIcon);
                    }
                } else if(debugSystemsBehaviour != null) {
                    GUI.DrawTexture(rect, systemsHierarchyIcon);
                }
            }
        }
    }
}
