using UnityEngine;
using UnityEditor;
using Pihkura.Camera.Core;

[CustomPropertyDrawer(typeof(CameraConfiguration))]
public class CameraConfigurationDrawer : PropertyDrawer
{
    private string GetFoldoutKey(string propertyName, string foldName) 
        => $"CameraConfig_{propertyName}_{foldName}_Foldout";

    private bool GetFoldout(string key, bool defaultValue = true)
        => EditorPrefs.GetBool(key, defaultValue);

    private void SetFoldout(string key, bool value)
        => EditorPrefs.SetBool(key, value);

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        property.serializedObject.Update();

        string propName = property.name;

        EditorGUILayout.Space();

        // --- Distance & Zoom ---
        string distanceKey = GetFoldoutKey(propName, "DistanceZoom");
        bool showDistanceZoom = GetFoldout(distanceKey);
        showDistanceZoom = EditorGUILayout.Foldout(showDistanceZoom, "Distance & Zoom", true);
        SetFoldout(distanceKey, showDistanceZoom);
        if (showDistanceZoom)
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.PropertyField(property.FindPropertyRelative("heightOffset"));
            EditorGUILayout.PropertyField(property.FindPropertyRelative("minDistance"));
            EditorGUILayout.PropertyField(property.FindPropertyRelative("maxDistance"));
            EditorGUILayout.PropertyField(property.FindPropertyRelative("zoomSpeed"));
            EditorGUILayout.PropertyField(property.FindPropertyRelative("invertZoom"));
            EditorGUILayout.EndVertical();
        }

        // --- Rotation ---
        string rotationKey = GetFoldoutKey(propName, "Rotation");
        bool showRotation = GetFoldout(rotationKey);
        showRotation = EditorGUILayout.Foldout(showRotation, "Rotation", true);
        SetFoldout(rotationKey, showRotation);
        if (showRotation)
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.PropertyField(property.FindPropertyRelative("rotationButton"));
            EditorGUILayout.PropertyField(property.FindPropertyRelative("yawSpeed"));
            EditorGUILayout.PropertyField(property.FindPropertyRelative("pitchSpeed"));
            EditorGUILayout.PropertyField(property.FindPropertyRelative("minPitch"));
            EditorGUILayout.PropertyField(property.FindPropertyRelative("maxPitch"));
            EditorGUILayout.PropertyField(property.FindPropertyRelative("keyboardRotationSpeed"));
            EditorGUILayout.EndVertical();
        }

        // --- Smoothing ---
        string smoothingKey = GetFoldoutKey(propName, "Smoothing");
        bool showSmoothing = GetFoldout(smoothingKey);
        showSmoothing = EditorGUILayout.Foldout(showSmoothing, "Smoothing", true);
        SetFoldout(smoothingKey, showSmoothing);
        if (showSmoothing)
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.PropertyField(property.FindPropertyRelative("moveSmoothTime"));
            EditorGUILayout.PropertyField(property.FindPropertyRelative("rotSmoothTime"));
            EditorGUILayout.EndVertical();
        }

        // --- Collision ---
        string collisionKey = GetFoldoutKey(propName, "Collision");
        bool showCollision = GetFoldout(collisionKey);
        showCollision = EditorGUILayout.Foldout(showCollision, "Collision", true);
        SetFoldout(collisionKey, showCollision);
        if (showCollision)
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.PropertyField(property.FindPropertyRelative("collisionMask"));
            EditorGUILayout.PropertyField(property.FindPropertyRelative("collisionRadius"));
            EditorGUILayout.PropertyField(property.FindPropertyRelative("collisionOffset"));
            EditorGUILayout.EndVertical();
        }

        // --- Auto LOS Correction ---
        string losKey = GetFoldoutKey(propName, "AutoLOS");
        bool showAutoLOS = GetFoldout(losKey);
        showAutoLOS = EditorGUILayout.Foldout(showAutoLOS, "Auto LOS Correction", true);
        SetFoldout(losKey, showAutoLOS);
        if (showAutoLOS)
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.PropertyField(property.FindPropertyRelative("autoPitchSpeed"));
            EditorGUILayout.PropertyField(property.FindPropertyRelative("maxAutoPitch"));
            EditorGUILayout.EndVertical();
        }

        // --- Movement ---
        string movementKey = GetFoldoutKey(propName, "Movement");
        bool showMovement = GetFoldout(movementKey);
        showMovement = EditorGUILayout.Foldout(showMovement, "Movement", true);
        SetFoldout(movementKey, showMovement);
        if (showMovement)
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.PropertyField(property.FindPropertyRelative("movementSpeed"));
            EditorGUILayout.EndVertical();
        }

        // --- Area / Map Settings ---
        string areaKey = GetFoldoutKey(propName, "AreaBounds");
        bool showArea = GetFoldout(areaKey);
        showArea = EditorGUILayout.Foldout(showArea, "Area / Map Settings", true);
        SetFoldout(areaKey, showArea);
        if (showArea)
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.PropertyField(property.FindPropertyRelative("areaBounds"), true);
            EditorGUILayout.EndVertical();
        }

        // --- Ray Settings ---
        string rayKey = GetFoldoutKey(propName, "RaySettings");
        bool showRaySettings = GetFoldout(rayKey);
        showRaySettings = EditorGUILayout.Foldout(showRaySettings, "Ray Settings", true);
        SetFoldout(rayKey, showRaySettings);
        if (showRaySettings)
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.PropertyField(property.FindPropertyRelative("forwardRay"), true);
            EditorGUILayout.PropertyField(property.FindPropertyRelative("downRay"), true);
            EditorGUILayout.PropertyField(property.FindPropertyRelative("groundRay"), true);
            EditorGUILayout.EndVertical();
        }

        // --- Input Settings ---
#if (ENABLE_INPUT_SYSTEM)
        string inputKey = GetFoldoutKey(propName, "Input settings");
        bool showInputSettings = GetFoldout(inputKey);
        showInputSettings = EditorGUILayout.Foldout(showInputSettings, "Input Settings", true);
        SetFoldout(inputKey, showInputSettings);
        if (showInputSettings)
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.PropertyField(property.FindPropertyRelative("inputController"), true);
            EditorGUILayout.PropertyField(property.FindPropertyRelative("inputMap"), true);
            EditorGUILayout.EndVertical();
        }
#endif
        EditorGUILayout.Space();

        // --- Detect Defaults -nappi ---
        if (GUILayout.Button("Detect Defaults"))
        {
            DetectDefaults(property);
        }

        property.serializedObject.ApplyModifiedProperties();
        EditorGUI.EndProperty();
    }

    private void DetectDefaults(SerializedProperty property)
    {
        int defaultCollisionLayer = 1 << LayerMask.NameToLayer("Terrain");
        Vector3 size = new Vector3(4096, 500, 4096);
        Vector3 pos = Vector3.zero;

        SerializedProperty collisionMask = property.FindPropertyRelative("collisionMask");
        if (collisionMask != null)
            collisionMask.intValue = defaultCollisionLayer;

        SerializedProperty areaBounds = property.FindPropertyRelative("areaBounds");
        if (areaBounds != null)
        {
            SerializedProperty minBounds = areaBounds.FindPropertyRelative("minBounds");
            SerializedProperty maxBounds = areaBounds.FindPropertyRelative("maxBounds");

            if (minBounds != null) minBounds.vector3Value = pos;
            if (maxBounds != null) maxBounds.vector3Value = pos + size;
        }

        string[] rayKeys = new string[3] { "forwardRay", "downRay", "groundRay" };
        float[] maxDistances = new float[3] { size.y, size.y, 100f };
        Vector3[] offsets = new Vector3[3] { Vector3.zero, Vector3.zero, Vector3.up * 50f };

        for (int i = 0; i < rayKeys.Length; i++)
        {
            SerializedProperty ray = property.FindPropertyRelative(rayKeys[i]);
            if (ray != null)
            {
                SerializedProperty maxDistance = ray.FindPropertyRelative("maxDistance");
                if (maxDistance != null)
                    maxDistance.floatValue = maxDistances[i];

                SerializedProperty offset = ray.FindPropertyRelative("offset");
                if (offset != null)
                    offset.vector3Value = offsets[i];

                SerializedProperty mask = ray.FindPropertyRelative("mask");
                if (mask != null)
                    mask.intValue = defaultCollisionLayer;
            }
        }
    }
}
