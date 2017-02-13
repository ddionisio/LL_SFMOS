using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Ignore Mission Manager, use this
/// </summary>
public struct Progress {
    public static void UpdateLessonProgress(int progress) {
        int lessonProgress = M8.SceneState.instance.global.GetValue(SceneStateVars.lessonsProgress);
        if(lessonProgress < progress)
            M8.SceneState.instance.global.SetValue(SceneStateVars.lessonsProgress, progress, false);

        SubmitCurrentProgress();
    }

    public static void SubmitCurrentProgress() {
        int curScore = M8.SceneState.instance.global.GetValue(SceneStateVars.curScore);
        int lessonProgress = M8.SceneState.instance.global.GetValue(SceneStateVars.lessonsProgress);
        //int questionsAnswered = LoLManager.instance.questionAnsweredCount;

        int progress = Mathf.Clamp(lessonProgress, 0, LoLManager.instance.progressMax);

        LoLManager.instance.ApplyProgress(progress, curScore);
    }
}
