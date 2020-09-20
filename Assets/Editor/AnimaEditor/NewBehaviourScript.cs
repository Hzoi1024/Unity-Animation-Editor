using UnityEngine;
using UnityEditor;

public class ExampleClass : EditorWindow
{
    protected GameObject go;
    protected AnimationClip animationClip;
    protected float time = 0.0f;
    protected bool lockSelection = false;
    protected bool animationMode = false;

    [MenuItem("Examples/AnimationMode demo", false, 2000)]
    public static void DoWindow()
    {
        var window = GetWindowWithRect<ExampleClass>(new Rect(0, 0, 300, 80));
        window.Show();
    }

    // Has a GameObject been selection?
    public void OnSelectionChange()
    {
        if (!lockSelection)
        {
            go = Selection.activeGameObject;
            Repaint();
        }
    }

    // Main editor window
    public void OnGUI()
    {
        // Wait for user to select a GameObject
        if (go == null)
        {
            EditorGUILayout.HelpBox("Please select a GameObject", MessageType.Info);
            return;
        }

        // Animate and Lock buttons.  Check if Animate has changed
        GUILayout.BeginHorizontal();
        EditorGUI.BeginChangeCheck();
        GUILayout.Toggle(AnimationMode.InAnimationMode(), "Animate");
        if (EditorGUI.EndChangeCheck())
            ToggleAnimationMode();

        GUILayout.FlexibleSpace();
        lockSelection = GUILayout.Toggle(lockSelection, "Lock");
        GUILayout.EndHorizontal();

        // Slider to use when Animate has been ticked
        EditorGUILayout.BeginVertical();
        animationClip = EditorGUILayout.ObjectField(animationClip, typeof(AnimationClip), false) as AnimationClip;
        if (animationClip != null)
        {
            float startTime = 0.0f;
            float stopTime = animationClip.length;
            time = EditorGUILayout.Slider(time, startTime, stopTime);
        }
        else if (AnimationMode.InAnimationMode())
            AnimationMode.StopAnimationMode();
        EditorGUILayout.EndVertical();
    }

    void Update()
    {
        if (go == null)
            return;

        if (animationClip == null)
            return;

        // There is a bug in AnimationMode.SampleAnimationClip which crashes
        // Unity if there is no valid controller attached
        Animator animator = go.GetComponent<Animator>();
        if (animator != null && animator.runtimeAnimatorController == null)
            return;

        // Animate the GameObject
        if (!EditorApplication.isPlaying && AnimationMode.InAnimationMode())
        {
            AnimationMode.BeginSampling();
            AnimationMode.SampleAnimationClip(go, animationClip, time);
            AnimationMode.EndSampling();

            SceneView.RepaintAll();
        }
    }

    void ToggleAnimationMode()
    {
        if (AnimationMode.InAnimationMode())
            AnimationMode.StopAnimationMode();
        else
            AnimationMode.StartAnimationMode();
    }
}

public class EditorGUIGradientField : EditorWindow
{

    [MenuItem("Examples/Gradient Field demo")]
    static void Init()
    {
        EditorWindow window = GetWindow(typeof(EditorGUIGradientField));
        window.position = new Rect(0, 0, 400, 199);
        window.Show();
    }
    bool isplay;
    AminaEditorConfigSO scriptableObject;
    private void OnEnable()
    {
        isplay = false;
        scriptableObject = EditorGUIUtility.Load("AminaEditorConfig.asset") as AminaEditorConfigSO;

    }

    void OnGUI()
    {

        if(GUI.RepeatButton(new Rect(10, 10, 50, 50), "ca"))
            Debug.Log("Clicked the button with an image");

        if (GUI.Button(new Rect(10, 70, 50, 30), "Click"))Debug.Log("Clicked the button with text");

        Rect r = new Rect(10, 120, 50, 30);

        if(Event.current.type == EventType.MouseDown)
        {
            

            if (r.Contains(Event.current.mousePosition))
            {
                isplay = !isplay;
                Repaint();
            }
        }

        

        if (isplay)
        {
            GUI.Box(r, "playing");
        }
        else
        {
            GUIStyle k = new GUIStyle();
            k.alignment = TextAnchor.MiddleCenter;
            //k.normal.background=scriptableObject.Test;
            //k.hover.background=scriptableObject.Test2; 

            GUI.Box(r, "play",k);
        }
        
    }
}
