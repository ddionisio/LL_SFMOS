using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LessonController : M8.SingletonBehaviour<LessonController> {
    [System.Serializable]
    public class QuestionData {
        [M8.Localize]
        public string questionStringRef;
        public LessonCard.Type[] answers;
        public LessonCard.Type[] exclude;
        [M8.Localize]
        public string resultStringRef;

        public int score;

        public bool ContainsAnswer(LessonCard.Type type) {
            for(int i = 0; i < answers.Length; i++) {
                if(answers[i] == type)
                    return true;
            }
            return false;
        }
    }

    [Header("Questions")]
    public QuestionData[] questionPrimary;
    public QuestionData[] questionSecondary;

    [Header("Dialogs")]
    public string modalExclusive; //during intro, result, and end
    public string modalQuestion; //during question

    [M8.Localize]
    public string dialogStartStringRef;
    public string dialogEndStringRef;

    [Header("Animation")]
    public M8.Animator.AnimatorData animator;
    public string takeStart;
    public string takeDeckEnter;
    public string takeDeckExit;
    public string takeEnd;

    [Header("Info")]
    public LessonInputField input;

    public LessonCard[] cards;

    public Transform[] deckAnchors;

    public Transform[] dockAnchors; //recommend: 0, 1 = pair answer; 2 = single answer
    public int dockAnchorSingleIndex; //index within dock anchors for single answer

    public Bounds dockArea; //world pos.
        
    public QuestionData curQuestion {
        get {
            if(mQuestions != null && mCurQuestionIndex >= 0 && mCurQuestionIndex < mQuestions.Length)
                return mQuestions[mCurQuestionIndex];

            return null;
        }
    }

    private QuestionData[] mQuestions;
    private int mCurQuestionIndex;    

    private List<LessonCard> mDeckCards;
    private List<LessonCard> mDockedCards;
    private Queue<Transform> mAvailableAnchors;

    private int mCurScore;
    private int mIncorrectCounter;
        
    void OnDestroy() {
        if(input)
            input.pointerUpCallback -= OnInputPointerUp;
    }

    void Awake() {
        gameObject.SetActive(false);

        input.pointerUpCallback += OnInputPointerUp;

        mDeckCards = new List<LessonCard>();
        mDockedCards = new List<LessonCard>();
        mAvailableAnchors = new Queue<Transform>(dockAnchors.Length);

        for(int i = 0; i < cards.Length; i++)
            cards[i].gameObject.SetActive(false);

        for(int i = 0; i > dockAnchors.Length; i++)
            dockAnchors[i].gameObject.SetActive(false);
    }

    void GenerateQuestions() {
        //shuffle the two list
        var newQuestions1 = new List<QuestionData>(questionPrimary.Length);
        newQuestions1.AddRange(questionPrimary);
        M8.Util.ShuffleList(newQuestions1);

        var newQuestions2 = new List<QuestionData>(questionSecondary.Length);
        newQuestions2.AddRange(questionSecondary);
        M8.Util.ShuffleList(newQuestions2);

        //combine
        var newQuestions = new List<QuestionData>(questionPrimary.Length + questionSecondary.Length);
        newQuestions.AddRange(newQuestions1);
        newQuestions.AddRange(newQuestions2);

        mQuestions = newQuestions.ToArray();

        mCurQuestionIndex = 0;
    }

    void ClearDeck() {
        for(int i = 0; i < mDeckCards.Count; i++)
            mDeckCards[i].gameObject.SetActive(false);

        mDeckCards.Clear();
    }

    void ClearDock() {
        for(int i = 0; i < mDockedCards.Count; i++)
            mDockedCards[i].gameObject.SetActive(false);

        mDockedCards.Clear();

        for(int i = 0; i > dockAnchors.Length; i++)
            dockAnchors[i].gameObject.SetActive(false);
    }

    void PopulateDeck() {
        var question = curQuestion;
        if(question == null)
            return;

        List<LessonCard> cardPool = new List<LessonCard>(cards);
        mDeckCards = new List<LessonCard>(deckAnchors.Length);

        //remove exclude cards
        for(int i = 0; i < question.exclude.Length; i++) {
            var excludeType = question.exclude[i];

            for(int j = 0; j < cardPool.Count; j++) {
                if(cardPool[j].type == excludeType) {
                    cardPool.RemoveAt(j);
                    break;
                }
            }
        }

        //first, add answer cards
        for(int i = 0; i < question.answers.Length; i++) {
            var answerType = question.answers[i];

            for(int j = 0; j < cardPool.Count; j++) {
                if(cardPool[j].type == answerType) {
                    //move card to deck list
                    mDeckCards.Add(cardPool[j]);
                    cardPool.RemoveAt(j);
                    break;
                }
            }
        }
                
        //add the rest
        for(int i = mDeckCards.Count, j = 0; i < deckAnchors.Length; i++, j++) {
            mDeckCards.Add(cardPool[j]);
        }

        M8.Util.ShuffleList(mDeckCards);

        //activate cards
        for(int i = 0; i < mDeckCards.Count; i++) {
            mDeckCards[i].transform.position = deckAnchors[i].position;
            mDeckCards[i].gameObject.SetActive(true);
        }

        input.Populate(mDeckCards.ToArray());
    }

    void SetupDock() {
        var question = curQuestion;
        if(question == null)
            return;

        mAvailableAnchors.Clear();

        if(question.answers.Length > 1) {
            for(int i = 0; i < question.answers.Length; i++) {
                dockAnchors[i].gameObject.SetActive(true);

                mAvailableAnchors.Enqueue(dockAnchors[i]);
            }
        }
        else {
            dockAnchors[dockAnchorSingleIndex].gameObject.SetActive(true);

            mAvailableAnchors.Enqueue(dockAnchors[dockAnchorSingleIndex]);
        }
    }
    
    IEnumerator Start () {
        //intro
        input.isLocked = true;

        animator.Play(takeStart);
        while(animator.isPlaying)
            yield return null;

        var parmDlg = new M8.GenericParams();

        //start dialog
        parmDlg[ModalDialog.parmStringRefs] = new string[] { dialogStartStringRef };
        parmDlg[ModalDialog.parmPauseOverride] = true;

        M8.UIModal.Manager.instance.ModalOpen(modalExclusive, parmDlg);

        while(M8.UIModal.Manager.instance.ModalIsInStack(modalExclusive) || M8.UIModal.Manager.instance.isBusy)
            yield return null;

        parmDlg.Clear();
        //

        //init play        
        GenerateQuestions();

        mCurScore = 0;

        HUD.instance.SetMissionActive(true);
        HUD.instance.SetTimeActive(false);
        HUD.instance.UpdateScore(mCurScore, mCurScore);

        //play
        while(mCurQuestionIndex < mQuestions.Length) {
            mIncorrectCounter = 0;

            var question = curQuestion;

            ClearDeck();
            ClearDock();

            //deck entry
            animator.Play(takeDeckEnter);
            while(animator.isPlaying)
                yield return null;

            //populate deck
            PopulateDeck();

            //dock
            SetupDock();

            //question dialog
            parmDlg[ModalDialog.parmStringRefs] = new string[] { curQuestion.questionStringRef };
            parmDlg[ModalDialog.parmPauseOverride] = false;

            M8.UIModal.Manager.instance.ModalOpen(modalQuestion, parmDlg);

            parmDlg.Clear();
            //

            input.isLocked = false;

            //wait for anchors to be filled
            while(mAvailableAnchors.Count > 0)
                yield return null;

            input.isLocked = true;

            M8.UIModal.Manager.instance.ModalCloseUpTo(modalQuestion, true);

            //deck exit
            animator.Play(takeDeckExit);
            while(animator.isPlaying)
                yield return null;

            //results dialog
            parmDlg[ModalDialog.parmStringRefs] = new string[] { curQuestion.resultStringRef };
            parmDlg[ModalDialog.parmPauseOverride] = true;

            M8.UIModal.Manager.instance.ModalOpen(modalExclusive, parmDlg);

            while(M8.UIModal.Manager.instance.ModalIsInStack(modalExclusive) || M8.UIModal.Manager.instance.isBusy)
                yield return null;

            parmDlg.Clear();
            //

            //next
            mCurQuestionIndex++;

            yield return null;
        }

        //save score
        M8.SceneState.instance.global.SetValue(SceneStateVars.curScore, mCurScore, false);

        //end
        input.isLocked = true;

        animator.Play(takeEnd);

        //end dialog
        parmDlg[ModalDialog.parmStringRefs] = new string[] { dialogEndStringRef };
        parmDlg[ModalDialog.parmPauseOverride] = true;

        M8.UIModal.Manager.instance.ModalOpen(modalExclusive, parmDlg);

        while(M8.UIModal.Manager.instance.ModalIsInStack(modalExclusive) || M8.UIModal.Manager.instance.isBusy)
            yield return null;

        parmDlg.Clear();
        //
    }

    void OnInputPointerUp(LessonInputField inp) {
        var card = inp.curCard;

        //check if there's available
        if(mAvailableAnchors.Count > 0) {
            //check to see if correct
            var question = curQuestion;
            if(question != null && question.ContainsAnswer(card.type)) {
                //dock card
                var toAnchor = mAvailableAnchors.Dequeue();

                mDeckCards.Remove(card);
                mDockedCards.Add(card);

                card.Dock(toAnchor.position);

                //score
                int prevScore = mCurScore;
                int score;
                if(mIncorrectCounter == 0)
                    score = question.score;
                else if(mIncorrectCounter == 1)
                    score = Mathf.RoundToInt(question.score*0.75f);
                else if(mIncorrectCounter == 2)
                    score = Mathf.RoundToInt(question.score*0.5f);
                else if(mIncorrectCounter == 3)
                    score = Mathf.RoundToInt(question.score*0.25f);
                else
                    score = Mathf.RoundToInt(question.score*0.1f);

                mCurScore += score;

                HUD.instance.UpdateScore(mCurScore, prevScore);
            }
            else { //return as incorrect
                card.ReturnIncorrect();
                mIncorrectCounter++;
            }
        }
        else
            card.Return();

        inp.ClearCurrent();
    }

    void OnDrawGizmos() {
        Gizmos.color = Color.cyan;

        Gizmos.DrawWireCube(dockArea.center, dockArea.size);

        const float anchorR = 0.15f;

        if(dockAnchors != null) {
            Gizmos.color = Color.cyan * 0.5f;
                        
            for(int i = 0; i < dockAnchors.Length; i++)
                Gizmos.DrawSphere(dockAnchors[i].position, anchorR);

            if(dockAnchorSingleIndex >= 0 && dockAnchorSingleIndex < dockAnchors.Length)
                Gizmos.DrawWireSphere(dockAnchors[dockAnchorSingleIndex].position, anchorR + 0.1f);
        }

        if(deckAnchors != null) {
            Gizmos.color = Color.green * 0.5f;

            for(int i = 0; i < deckAnchors.Length; i++)
                Gizmos.DrawSphere(deckAnchors[i].position, anchorR);
        }
    }
}
