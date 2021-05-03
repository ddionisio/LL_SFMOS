using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissionQuizController : M8.SingletonBehaviour<MissionQuizController> {
    public M8.SceneAssetPath resultScene;

    private int mCorrectCount;

    public void ProcessComplete() {
        MissionManager.instance.CompleteQuiz(mCorrectCount);

        M8.SceneManager.instance.LoadScene(resultScene.name);
    }

    protected override void OnInstanceDeinit() {
        
    }

    protected override void OnInstanceInit() {
        mCorrectCount = 0;
    }
}
