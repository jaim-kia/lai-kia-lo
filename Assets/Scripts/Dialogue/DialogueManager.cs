using TMPro;
using Unity.Cinemachine;
using UnityEngine;
using Ink.Runtime;
using UnityEngine.InputSystem;
using System.Collections;
using Unity.VisualScripting;
using System;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class DialogueManager : MonoBehaviour
{
    [Header("Dialouge UI")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private InputActionReference continueRef;

    [Header("Choices UI")]
    [SerializeField] private GameObject[] choices;

    [SerializeField] private CanvasGroup dialogueCanvasGroup; // on the same object as dialoguePanel
    private CanvasGroup[] choicesCanvasGroups;
    private TextMeshProUGUI[] choicesText;

    private Story currentStory;
    public bool dialogueIsPlaying {get; private set;}

    private static DialogueManager instance;

    private GameObject currentSpeaker;
    private bool isPaused;
    public static event Action<GameObject, string> OnDialogueTag;

    private void SetDialogueVisible(bool visible)
    {
        dialogueCanvasGroup.alpha = visible ? 1f : 0f;
        dialogueCanvasGroup.interactable = visible;
        dialogueCanvasGroup.blocksRaycasts = visible;
    }

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("Found more than one Dialogue Manager instance");
        }

        instance = this;
    }

    public static DialogueManager GetInstance()
    {
        return instance;
    }

    private void Start()
    {
        dialogueIsPlaying = false;
        SetDialogueVisible(false);

        choicesText = new TextMeshProUGUI[choices.Length];
        choicesCanvasGroups = new CanvasGroup[choices.Length];
        
        int index = 0;
        foreach(GameObject choice in choices)
        {
            choicesText[index] = choice.GetComponentInChildren<TextMeshProUGUI>();

            choicesCanvasGroups[index] = choice.GetComponent<CanvasGroup>();
            if (choicesCanvasGroups[index] == null)
                choicesCanvasGroups[index] = choice.AddComponent<CanvasGroup>();

            index++;
        }
    }

    private void Update()
    {
        if (!dialogueIsPlaying || isPaused)
        {
            return;
        }

        if (ContinueDialogue())
        {
            ContinueStory();
        }
    }

    public void EnterDialogueMode(TextAsset inkJSON, GameObject speaker = null)
    {
        currentStory = new Story(inkJSON.text);
        currentSpeaker = speaker;
        isPaused = false;

        dialogueIsPlaying = true;
        SetDialogueVisible(true);

        ContinueStory();
    }


    // no delay
    // private void ExitDialogueMode()
    // {
    //     dialogueIsPlaying = false;
    //     dialoguePanel.SetActive(false);
    //     dialogueText.text = "";
    // }

    // IEnumerator version but I should change this into states

    private IEnumerator ExitDialogueMode()
    {
        yield return new WaitForSeconds(0.2f);

        EventSystem.current.SetSelectedGameObject(null);

        dialogueIsPlaying = false;
        SetDialogueVisible(false);
        dialogueText.text = "";
    }


    private bool ContinueDialogue()
    {
        if (continueRef == null) return false;


        return continueRef.action.WasPressedThisFrame();
    }

    private void ContinueStory()
    {
        if  (currentStory.canContinue)
        {
            dialogueText.text = currentStory.Continue();

            foreach  (string tag in currentStory.currentTags)
            {
                Debug.Log("Tag received: [" + tag + "]");
                OnDialogueTag?.Invoke(currentSpeaker, tag);
            }

            DisplayChoices();
        }
        else
        {
            StartCoroutine(ExitDialogueMode());
        }
    }

    public void PauseDialogue()
    {
        EventSystem.current.SetSelectedGameObject(null);
        isPaused = true;
        SetDialogueVisible(false);
    }

    public void ResumeDialogue()
    {
        isPaused = false;
        SetDialogueVisible(true);
        ContinueStory();
    }

    private void DisplayChoices()
    {
        List<Choice> currentChoices = currentStory.currentChoices;

        if (currentChoices.Count > choices.Length)
        {
            Debug.LogError("More choices were given than the UI can support. Number of choices given: " + currentChoices.Count);
        }

        int index = 0;

        foreach (Choice choice in currentChoices)
        {
            SetChoiceVisible(index, true);
            choicesText[index].text = choice.text;
            index++;
        }

        for (int i = index; i < choices.Length; i++)
        {
            SetChoiceVisible(i, false);
        }

        StartCoroutine(SelectFirstChoice());
    }

    private void SetChoiceVisible(int index, bool visible)
    {
        choicesCanvasGroups[index].alpha = visible ? 1f : 0f;
        choicesCanvasGroups[index].interactable = visible;
        choicesCanvasGroups[index].blocksRaycasts = visible;
    }


    private IEnumerator SelectFirstChoice()
    {
        if (currentStory.currentChoices.Count == 0) yield break;

        EventSystem.current.SetSelectedGameObject(null);
        yield return new WaitForEndOfFrame();
        EventSystem.current.SetSelectedGameObject(choices[0].gameObject);
    }

    public void MakeChoice(int choiceIndex)
    {
        currentStory.ChooseChoiceIndex(choiceIndex);
    }

    // getter setters for ink files
    public void SetStoryVariable(string name, object value)
    {
        if (currentStory != null)
            currentStory.variablesState[name] = value;
    }

    public object GetStoryVariable(string name)
    {
        return currentStory?.variablesState[name];
    }

}
