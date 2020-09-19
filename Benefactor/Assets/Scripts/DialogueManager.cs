using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.UI;
using System;


/**
 * A one-off class that creates a singular object responsible for taking care of any dialogue events that come up
 * While this object is on, game functionality should be turned off (either turn things off in this class, or add a condition to the update of player / character / gameManager)
 */
public class DialogueManager : MonoBehaviour
{
    // Data Holders:
    IEnumerable<XElement> items;
    List<XMLData> data = new List<XMLData>();

    // Frame-to-Frame record keeping
    public string conversationType;
    private int currentIndex;
    private int pageNum;
    string currentCharacter;
    string currentDialogue;
    public bool typingInProgress;
    private bool fastForward;
    private bool transitioning;

    // Object References
    public GameObject talkingPlayer;
    public GameObject talkingNPC;
    public Text dialogueUI;
    public GameObject dialogueBackground;
    public GameObject portrait;
    public GameObject conversationOptions;

    // Speeds and whatnot
    public float typingSpeed = .05f;
    public float punctuationDelay = .5f;
    private float alphaIncrementRate = .02f;
    private float alphaIncrementAmount = .005f;
    private float postTransitionDelay = .5f;

    private void Awake()
    {
        conversationOptions.SetActive(false);
    }

    // Only to be used by players
    public void initiateDialogue(GameObject initiator, GameObject responder)
    {
        //currentIndex = 0;
        //pageNum = 0;
        conversationType = null;

        talkingPlayer = initiator;
        talkingNPC = responder;

        // Disable Player Functionality should be taken care of in other classes, but might not work fully yet
        GameManager.instance.dialogueInProgress = true;
        dialogueBackground.SetActive(true);
        portrait.GetComponent<PortraitManager>().changePortrait("RaskolnikovNeutral");
        conversationOptions.SetActive(true);
    }
    
    void Update()
    {
        if (GameManager.instance.dialogueInProgress && conversationType != null)
            determineInteraction();
    }

    private void determineInteraction()
    {

    }

    // To be used for non-player instigated dialogue, if need be
    public void triggerDialogueEvent()
    {

    }
}














//    void Start()
//    {
//        currentIndex = 0;
//        pageNum = 0;

//        LoadXML();
//        StartCoroutine("AssignData");
//        typingInProgress = false;
//        fastForward = false;

//        GameObject.Find("Cover").GetComponent<Image>().color += new Color(0, 0, 0, 1);
//        executeNext();
//    }

//    void LoadXML()
//    {
//        xmlDoc = XDocument.Load("Assets/Resources/XMLFiles/IntroSequence.xml");
//        items = xmlDoc.Descendants("page").Elements();
//    }

//    IEnumerator AssignData()
//    {
//        int assignmentIndex = 0;
//        bool firstCheck = true;

//        foreach (var item in items)
//        {
//            // Allows us to make large skips in assignment index values without having to process every number inbetween.
//            if (firstCheck && Int32.Parse(item.Parent.Attribute("number").Value.ToString()) > assignmentIndex)
//            {
//                assignmentIndex = Int32.Parse(item.Parent.Attribute("number").Value.ToString());
//                firstCheck = false;
//            }

//            // Handles creation of each individual XML "page"
//            if (item.Parent.Attribute("number").Value.ToString() == assignmentIndex.ToString())
//            {
//                string tempType = item.Parent.Element("type").Value.Trim();
//                int tempPageNum = int.Parse(item.Parent.Attribute("number").Value);
//                string tempBackdrop = item.Parent.Element("backdrop").Value.Trim();
//                string tempSFX = item.Parent.Element("SFX").Value.Trim();
//                string tempCharName = item.Parent.Element("character").Value.Trim();
//                string tempPortrait = item.Parent.Element("portrait").Value.Trim();
//                string tempDialogue = item.Parent.Element("dialogue").Value.Trim();

//                data.Add(new XMLData(tempType, tempPageNum, tempBackdrop, tempSFX, tempCharName, tempPortrait, tempDialogue));
//                //Debug.Log(data[assignmentIndex].dialogueText);
//                //Debug.Log(assignmentIndex);
//                assignmentIndex++;
//                firstCheck = true;
//            }
//        }
//        yield return null;
//    }

//    private void Update()
//    {

//        if (Input.GetMouseButtonDown(0) && !typingInProgress && !transitioning)
//        {
//            executeNext();
//        }
//        else if (Input.GetMouseButtonDown(0))
//        {
//            fastForward = true;
//        }
//    }

//    private void executeNext()
//    {
//        Debug.Log("executing page: " + currentIndex);
//        if (data[currentIndex].type == "Dialogue")
//        {
//            StartCoroutine("readDialogue");
//            currentIndex++;
//        }
//        else if (data[currentIndex].type == "SoundEffect")
//        {
//            // TO DO
//            // Also make this whole thing a switch statement please
//            currentIndex++;
//            executeNext();
//        }
//    }

//    IEnumerator readDialogue()
//    {
//        typingInProgress = true;

//        dialogueUI.text = "";
//        currentDialogue = data[currentIndex].dialogueText;

//        GameObject.Find("SpeakingCharacter").GetComponent<Text>().text = data[currentIndex].characterName;
//        GameObject.Find("Portrait").GetComponent<PortraitManager>().changePortrait(data[currentIndex].portrait);

//        foreach (char letter in currentDialogue)
//        {
//            dialogueUI.text += letter;
//            float specialCharacterDelay = 0f;

//            if (letter == '.' || letter == '?' || letter == '!')
//                specialCharacterDelay = punctuationDelay;

//            if (fastForward)
//            {
//                dialogueUI.text = currentDialogue;
//                fastForward = false;
//                typingInProgress = false;
//                yield break;
//            }

//            yield return new WaitForSeconds(typingSpeed + specialCharacterDelay);
//        }

//        typingInProgress = false;
//    }


//    private void clearFields()
//    {
//        GameObject.Find("SpeakingCharacter").GetComponent<Text>().text = data[currentIndex].characterName;
//        GameObject.Find("Portrait").GetComponent<PortraitManager>().changePortrait(data[currentIndex].portrait);
//        dialogueUI.text = "";
//    }

//}