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

    [System.Serializable]
    public class QuestionPair {
        public QuestionData question1;
        public QuestionData question2;
    }

    [System.Serializable]
    public class dockInfo {
        public Transform anchor;
        public GameObject active;

        public Vector2 position { get { return anchor ? (Vector2)anchor.position : Vector2.zero; } }

        public void Show() {
            anchor.gameObject.SetActive(true);
        }

        public void Highlight(bool highlight) {
            active.SetActive(highlight);
        }

        public void Hide() {
            anchor.gameObject.SetActive(false);
            active.SetActive(false);
        }
    }

    [System.Serializable]
    public class dialogInfo {
        public enum Type {
            TextOnly,
            TextAndImage,
            TextAndComposite
        }
        
        public Type type;
        public string modal;
        public bool pause;

        [M8.Localize]
        public string[] stringRefs; //for TextOnly
        public ModalDialogImage.PairRef[] stringAndImageRefs; //for TextAndImage
        public ModalDialogComposite.PairRef[] stringAndCompositeRefs; //for TextAndComposite

        public bool isShowing {
            get {
                return M8.UIModal.Manager.instance.isBusy || M8.UIModal.Manager.instance.ModalIsInStack(modal);
            }
        }

        public void Show(M8.GenericParams parmCache) {
            parmCache.Clear();

            parmCache[ModalDialogBase.parmPauseOverride] = pause;

            switch(type) {
                case Type.TextOnly:
                    parmCache[ModalDialog.parmStringRefs] = stringRefs;
                    break;
                case Type.TextAndImage:
                    parmCache[ModalDialogImage.parmPairRefs] = stringAndImageRefs;
                    break;
                case Type.TextAndComposite:
                    parmCache[ModalDialogComposite.parmPairRefs] = stringAndCompositeRefs;
                    break;
            }

            M8.UIModal.Manager.instance.ModalOpen(modal, parmCache);
        }
    }

    [Header("Questions")]
    public QuestionPair[] questionPrimary;
    public QuestionData[] questionSecondary;
    public int questionPrimaryCount = 8;
    public int questionSecondaryCount = 2;

    [Header("Dialogs")]
    public dialogInfo[] dialogsBegin;
    public dialogInfo dialogMultiQuestionBegin;
    public dialogInfo[] dialogsEnd;

    public string modalQuestion; //during question
    public string modalPostQuestion;
    public string modalProceed;

    [M8.Localize]
    public string dialogStartStringRef;
    [M8.Localize]
    public string dialogEndStringRef;

    [Header("Animation")]
    public M8.Animator.AnimatorData animator;
    public string takeStart;
    public string takePrepare;
    public string takeQuestionResultEnter;
    public string takeQuestionResultExit;
    public string takeDeckEnter;
    public string takeDeckExit;
    public string takeEndPrepare;
    public string takeEnd;
    public int takeEndSkipFrame = 73;
    public bool takeEndWait = false;

    [Header("Music")]
    public string victoryMusicPath;

    [Header("Sound")]
    public string soundCorrectPath;
    public string soundWrongPath;

    [Header("Info")]
    public LessonInputField input;

    public LessonCard[] cards;

    public Transform[] deckAnchors;

    public dockInfo[] dockAnchors; //recommend: 0, 1 = pair answer; 2 = single answer
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
    private int mQuestionMultiStartIndex;

    private List<LessonCard> mDeckCards;
    private List<LessonCard> mDockedCards;
    private Queue<dockInfo> mAvailableAnchors;

    private int mCurScore;
    private int mIncorrectCounter;
    private int mMaxScore;

    private bool mIsAnswerPicked;
            
    void OnDestroy() {
        if(input) {
            input.pointerUpCallback -= OnInputPointerUp;
            input.pointerDragCallback -= OnInputPointerDrag;
        }
    }

    void Awake() {
        input.pointerUpCallback += OnInputPointerUp;
        input.pointerDragCallback += OnInputPointerDrag;

        mDeckCards = new List<LessonCard>();
        mDockedCards = new List<LessonCard>();
        mAvailableAnchors = new Queue<dockInfo>(dockAnchors.Length);

        for(int i = 0; i < cards.Length; i++)
            cards[i].gameObject.SetActive(false);

        for(int i = 0; i < dockAnchors.Length; i++)
            dockAnchors[i].Hide();

        gameObject.SetActive(false);
    }

    void GenerateQuestions() {
        mMaxScore = 0;

        var newQuestions = new List<QuestionData>(questionPrimaryCount + questionSecondaryCount);

        //alternate between the primary questions
        var mixPrimaryQuestionPairs = new List<QuestionPair>(questionPrimary.Length);
        mixPrimaryQuestionPairs.AddRange(questionPrimary);
        M8.Util.ShuffleList(mixPrimaryQuestionPairs);

        for(int i = 0; i < questionPrimaryCount; i++) {
            QuestionData q = i % 2 == 0 ? mixPrimaryQuestionPairs[i].question1 : mixPrimaryQuestionPairs[i].question2;

            newQuestions.Add(q);

            mMaxScore += q.score;
        }

        mQuestionMultiStartIndex = newQuestions.Count;

        //add the secondary questions
        var mixSecondaryQuestions = new List<QuestionData>(questionSecondary.Length);
        mixSecondaryQuestions.AddRange(questionSecondary);
        M8.Util.ShuffleList(mixSecondaryQuestions);

        for(int i = 0; i < questionSecondaryCount; i++) {
            newQuestions.Add(mixSecondaryQuestions[i]);

            mMaxScore += mixSecondaryQuestions[i].score*mixSecondaryQuestions[i].answers.Length;
        }
        
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

        for(int i = 0; i < dockAnchors.Length; i++)
            dockAnchors[i].Hide();
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
            mDeckCards[i].gameObject.SetActive(true);
            mDeckCards[i].Place(deckAnchors[i].position);
        }

        input.Populate(mDeckCards.ToArray());
    }

    void SetupDock() {
        var question = curQuestion;
        if(question == null)
            return;

        mAvailableAnchors.Clear();

        //show docks
                
        if(question.answers.Length > 1) {
            for(int i = 0; i < question.answers.Length; i++) {
                dockAnchors[i].Show();

                mAvailableAnchors.Enqueue(dockAnchors[i]);
            }
        }//special case single answer
        else {
            dockAnchors[dockAnchorSingleIndex].Show();

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
        for(int i = 0; i < dialogsBegin.Length; i++) {
            dialogsBegin[i].Show(parmDlg);

            while(dialogsBegin[i].isShowing)
                yield return null;
        }

        parmDlg.Clear();
        //

        //prepare animation
        animator.Play(takePrepare);
        while(animator.isPlaying)
            yield return null;

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
            while(mAvailableAnchors.Count > 0) {
                yield return null;

                if(mIsAnswerPicked) {
                    input.isLocked = true;
                    yield return null;
                    yield return null;
                    mIsAnswerPicked = false;
                    input.isLocked = false;
                }
            }

            input.isLocked = true;

            M8.UIModal.Manager.instance.ModalCloseUpTo(modalQuestion, true);

            //deck exit
            ClearDeck();

            animator.Play(takeDeckExit);
            while(animator.isPlaying)
                yield return null;

            bool isResultEnterPlayed = false;

            //results dialog
            if(!string.IsNullOrEmpty(curQuestion.resultStringRef)) {
                //move to an appropriate position
                animator.Play(takeQuestionResultEnter);
                isResultEnterPlayed = true;

                parmDlg[ModalDialog.parmStringRefs] = new string[] { curQuestion.resultStringRef };
                parmDlg[ModalDialog.parmPauseOverride] = false;

                M8.UIModal.Manager.instance.ModalOpen(modalPostQuestion, parmDlg);

                while(M8.UIModal.Manager.instance.isBusy || M8.UIModal.Manager.instance.ModalIsInStack(modalPostQuestion))
                    yield return null;

                parmDlg.Clear();
            }
            else
                yield return new WaitForSeconds(1f);
            //

            ClearDock();

            //next
            mCurQuestionIndex++;

            //show multi question dialog explanation
            if(mCurQuestionIndex == mQuestionMultiStartIndex) {
                dialogMultiQuestionBegin.Show(parmDlg);

                while(dialogMultiQuestionBegin.isShowing)
                    yield return null;

                parmDlg.Clear();
            }

            //move back
            if(isResultEnterPlayed) {
                //in case dialog was ended quickly
                while(animator.isPlaying)
                    yield return null;

                animator.Play(takeQuestionResultExit);
                while(animator.isPlaying)
                    yield return null;
            }

            yield return null;
        }

        ClearDock();
                
        //end
        input.isLocked = true;

        StartCoroutine(DoVictoryMusic());
        
        animator.Play(takeEndPrepare);
        while(animator.isPlaying)
            yield return null;

        //end dialog
        for(int i = 0; i < dialogsEnd.Length; i++) {
            dialogsEnd[i].Show(parmDlg);

            while(dialogsEnd[i].isShowing)
                yield return null;
        }

        parmDlg.Clear();
        //

        animator.Play(takeEnd);

        if(takeEndWait) {
            while(animator.isPlaying) {
                if(Input.anyKeyDown) {
                    animator.GotoFrame(takeEndSkipFrame);
                    break;
                }
                yield return null;
            }
        }

        parmDlg[ModalStartFinish.parmMaxScore] = mMaxScore;
        M8.UIModal.Manager.instance.ModalOpen(modalProceed, parmDlg);
    }

    void OnInputPointerDrag(LessonInputField inp) {
        bool active = inp.curCard != null && dockArea.Contains(inp.curPos);

        if(mAvailableAnchors.Count > 0) {
            var anchor = mAvailableAnchors.Peek();
            anchor.Highlight(active);
        }
    }

    void OnInputPointerUp(LessonInputField inp) {
        if(mIsAnswerPicked)
            return;

        var card = inp.curCard;
        if(card == null) {
            if(mAvailableAnchors.Count > 0) { //edge case
                var toAnchor = mAvailableAnchors.Peek();
                toAnchor.Highlight(false);
            }

            return;
        }
                
        //check if there's available
        if(mAvailableAnchors.Count > 0) {
            var toAnchor = mAvailableAnchors.Peek();
            toAnchor.Highlight(false);

            //check if we are in valid dock area
            if(!dockArea.Contains(inp.curPos))
                return;

            //check to see if correct
            var question = curQuestion;
            if(question != null && question.ContainsAnswer(card.type)) {
                //dock card
                mAvailableAnchors.Dequeue();
                                
                mDeckCards.Remove(card);
                mDockedCards.Add(card);

                card.Dock(toAnchor.position);

                //score
                int prevScore = mCurScore;
                int score = Mathf.RoundToInt(question.score*Mathf.Clamp((1.0f - (mIncorrectCounter/4.0f)), 0.1f, 1.0f));

                mCurScore += score;

                HUD.instance.UpdateScore(mCurScore, prevScore);

                //save score
                M8.SceneState.instance.global.SetValue(SceneStateVars.curScore, mCurScore, false);

                //progress
                Progress.UpdateLessonProgress(mCurQuestionIndex + 1);

                LoLManager.instance.PlaySound(soundCorrectPath, false, false);
            }
            else { //return as incorrect
                card.ReturnIncorrect();
                mIncorrectCounter++;

                LoLManager.instance.PlaySound(soundWrongPath, false, false);
            }

            mIsAnswerPicked = true;
            input.isLocked = true;
        }
        else
            card.Return();

        inp.ClearCurrent();
    }

    IEnumerator DoVictoryMusic() {
        LoLManager.instance.StopCurrentBackgroundSound();

        yield return new WaitForSeconds(1f);

        LoLManager.instance.PlaySound(victoryMusicPath, true, false);
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
