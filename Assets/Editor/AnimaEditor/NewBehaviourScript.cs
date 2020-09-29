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


// Create a foldable menu that hides/shows the selected transform position.
// if no Transform is selected, the Foldout item will be folded until a transform is selected.
public class EditorGUIFoldout : EditorWindow
{
    public bool showPosition = true;
    public string status = "Select a GameObject";
    [MenuItem("Examples/Foldout Usage")]
    static void Init()
    {
        UnityEditor.EditorWindow window = GetWindow(typeof(EditorGUIFoldout));
        window.position = new Rect(0, 0, 750, 740);
        window.Show();
    }

    Vector2 scroll;

    void OnGUI()
    {
        EditorGUI.DrawRect(new Rect(120, 120, 300, 300), Color.blue);

        scroll = GUI.BeginScrollView(new Rect(100, 100, 100, 100), scroll, new Rect(100, 100, 150, 150));



        GUI.EndScrollView();
    }
    
}
