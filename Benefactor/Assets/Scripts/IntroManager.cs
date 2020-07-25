using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.UI;

public class IntroManager : MonoBehaviour
{
    XDocument xmlDoc;
    IEnumerable<XElement> items;
    List<XMLData> data = new List<XMLData>();

    private int currentIndex;
    private int pageNum;

    string currentCharacter;
    string currentDialogue;

    public Text dialogueUI;
    public float typingSpeed = .05f;
    public float punctuationDelay = .5f;
    public bool typingInProgress;
    private bool fastForwardText;

    void Start()
    {
        currentIndex = 0;
        pageNum = 0;

        LoadXML();
        StartCoroutine("AssignData");
        typingInProgress = false;
        fastForwardText = false;
        StartCoroutine("readLine");
    }

    void LoadXML()
    {
        xmlDoc = XDocument.Load("Assets/Resources/XMLFiles/IntroSequence.xml");
        items = xmlDoc.Descendants("page").Elements();
    }

    IEnumerator AssignData()
    {
        int assignmentIndex = 0;
        foreach (var item in items)
        {
            if (item.Parent.Attribute("number").Value.ToString() == assignmentIndex.ToString())
            {
                int tempPageNum = int.Parse(item.Parent.Attribute("number").Value);
                string tempCharacter = item.Parent.Element("name").Value.Trim();
                string tempDialogue = item.Parent.Element("dialogue").Value.Trim();

                data.Add(new XMLData(tempPageNum, tempCharacter, tempDialogue));
                Debug.Log(data[assignmentIndex].dialogueText);
                assignmentIndex++;
                Debug.Log(assignmentIndex);
            }
        }
        yield return null;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && !typingInProgress) 
        {
            StartCoroutine("readLine");
        } else if (Input.GetMouseButtonDown(0))
        {
            fastForwardText = true;
        }
    }

    IEnumerator readLine()
    {
        typingInProgress = true;

        dialogueUI.text = "";
        currentDialogue = data[currentIndex].dialogueText;

        // Add code to change portrait

        foreach (char letter in currentDialogue)
        {
            dialogueUI.text += letter;
            float specialCharacterDelay = 0f;

            if (letter == '.' || letter == '?' || letter == '!')
                specialCharacterDelay = punctuationDelay;

            if (fastForwardText)
            {
                dialogueUI.text = currentDialogue;
                fastForwardText = false;
                typingInProgress = false;
                currentIndex++;
                yield break;
            }

            yield return new WaitForSeconds(typingSpeed + specialCharacterDelay);
        }

        typingInProgress = false;
        currentIndex++;
    }

}

public class XMLData
{
    public int pageNum;
    public string characterText;
    public string dialogueText;

    public XMLData(int page, string character, string dialogue)
    {
        pageNum = page;
        characterText = character;
        dialogueText = dialogue;
    }
}

