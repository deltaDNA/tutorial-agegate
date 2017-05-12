using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DeltaDNA;


public class ageGateDemo : MonoBehaviour
{

    public UnityEngine.UI.Text lblUser;  // used to display the DDNA userID on screen
    public UnityEngine.UI.Text lblAgeStatus;
    public GameObject ageGatePanel ;

    private string ageGateAnswer;
    private string storeURL; 

    public UnityEngine.UI.Text txtTitle;
    public UnityEngine.UI.Text txtText;
    public UnityEngine.UI.Text txtQuestion;
    public UnityEngine.UI.InputField frmAnswer;

    // Use this for initialization
    void Start()
    {
        // Start deltaDNA SDK
        StartDDNA();

        CloseAgeGate(); // Remove if you want to persist age gate setting across sessions. 

        // Update Age Gate Status from previously stored local settings if exist.
        if (PlayerPrefs.HasKey("ageGatePassed"))
        {
            UpdateAgeGateStatus(PlayerPrefs.GetInt("ageGatePassed") == 1 ? true : false);
        }
    }

    private void StartDDNA()
    {
        // Enter additional configuration here
        DDNA.Instance.ClientVersion = "0.0.1";
        DDNA.Instance.SetLoggingLevel(DeltaDNA.Logger.Level.DEBUG);

        // Launch the SDK
        DDNA.Instance.StartSDK(
            "23444477555359457070588074014944",
            "https://collect11373ttrlg.deltadna.net/collect/api",
            "https://engage11373ttrlg.deltadna.net"
        );


        lblUser.text = string.Format("userID : {0}", DDNA.Instance.UserID).ToString();

        // Age Gate Campaign details at 
        // https://www.deltadna.net/demo-account/tutorial-age-gate/dev/engagements 
    }

    private void CloseAgeGate()
    {
        GameEvent gameEvent = new GameEvent("ageGateUpdated")
            .AddParam("ageGatePassed", 0); 
     
        DDNA.Instance.RecordEvent(gameEvent);

        // Upload this event to DDNA immediately rather than wait for next scheduled upload to commence. 
        // This ensures player segmentaion and campaigns have latest info to act on.
        if (!DDNA.Instance.IsUploading)
        {
            DDNA.Instance.Upload();
        }

        UpdateAgeGateStatus(false);
        
    }

    public void Reset()
    { 
        // This method will reset the app, if it is active and isn't busy uploading.
        // A new userID will be generated and any previous age gate settings will be forgotten.
        // You wouldn't want to do this in a normal app
        if (DDNA.Instance.isActiveAndEnabled && DDNA.Instance.IsUploading == false)
        {         
            UpdateAgeGateStatus(false);
            DDNA.Instance.StopSDK();
            DDNA.Instance.ClearPersistentData();

            StartDDNA();
        }
    }

    public void VisitStore()
    {
        // This Mehod is called by the "Visit Store" button. It makes a request
        // to the deltaDNA campaign system which will either return an Age Gate question, 
        // or the URL to the store. Depending on the players current age gate status
        
        var engagement = new Engagement("ageGate");
        Dictionary<string, object> parameters = new Dictionary<string, object>();

        // Make Engage Request
        DDNA.Instance.RequestEngagement(engagement, (response) =>
        {
            // Handle Response
            if (response == null || response.JSON == null || !response.JSON.ContainsKey("parameters")) return ;
            parameters = response.JSON["parameters"] as Dictionary<string, object>;

            // Check response contains strore URL
            if (parameters.ContainsKey("ageGateSuccessURL"))
            {
                storeURL = parameters["ageGateSuccessURL"].ToString();

                // Either redirect to store or show Age Gate question
                if (PlayerPrefs.HasKey("ageGatePassed") && PlayerPrefs.GetInt("ageGatePassed") == 1)
                {
                    RedirectToStore(storeURL);
                }
                else
                {
                    CheckAge(parameters);
                }
            };                      

        }, (exception) =>
        {
            Debug.Log("Engage reported an error: " + exception.Message);
        });
    }


    private void CheckAge(Dictionary<string,object> ageGateParameters)
    {
        // THis method parses the DDNA Engage Age Gate campaign response before calling 
        // a method to display the Age Gate question popup.
        if (ageGateParameters.ContainsKey("ageGateQuestion") && ageGateParameters.ContainsKey("ageGateAnswer"))
        {
            // We have an age gate question to ask, parse out question details.
            string title    = ageGateParameters["ageGateTitle"].ToString();
            string text     = ageGateParameters["ageGateText"].ToString(); ;
            string question = ageGateParameters["ageGateQuestion"].ToString(); ;
            string answer   = ageGateParameters["ageGateAnswer"].ToString(); ;

            // Ask the user an Age Gate Question
             AskQuestion(title, text, question, answer);
        }        
    }

    private void AskQuestion(string title, string text, string question, string answer)
    {
        // This method populates the and displays the Age Gate question popup

        // Set pop-up text display
        txtTitle.text       = (title != null ? title :"")  ;
        txtText.text        = (text != null ? text : "") ;
        txtQuestion.text    = (question != null ? question : "");
        ageGateAnswer = answer;
        frmAnswer.text      = ""; 

        // Show PopUp
        ageGatePanel.SetActive(true); 
    }


    public void AnswerQuestion()
    {
        // This method is called when the player presses the OK button
        // to answer the an age gate question
        CloseQuestionPanel();

        // Check to see if awnser is correct
        bool result = (ageGateAnswer.ToLower() == frmAnswer.text.ToLower());

        // Update stored result for player
        UpdateAgeGateStatus(result);

        // Redirect to store if Age Gate PASSED
        if (result) RedirectToStore(storeURL); ;
    }

    public void CloseQuestionPanel()
    {
        // Hides Age Gate popup
        ageGatePanel.SetActive(false);
        return;
    }


    private void UpdateAgeGateStatus(bool status)
    {
        // This method Updates the locally stored Age Gate status for the player.
        // Sets the Status display 
        // And uploads an event to DDNA to update the DDNA player metrics

        GameEvent gameEvent = new GameEvent("ageGateUpdated")
            .AddParam("ageGatePassed", status == true ? 1 : 0);
    
        DDNA.Instance.RecordEvent(gameEvent);
        
        // Upload this event to DDNA immediately rather than wait for next scheduled upload to commence. 
        // This ensures player segmentaion and campaigns have latest info to act on.
        if (!DDNA.Instance.IsUploading)
        {
            DDNA.Instance.Upload();
        }

        // Update locally stored status
        PlayerPrefs.SetInt("ageGatePassed", status == true ? 1 : 0);

        // Update status display in App (Red / Green) depending on status.
        if (status)
        {
            lblAgeStatus.color = new Color(0.0f, 1.0f, 0.0f);
        }
        else
        {
            lblAgeStatus.color = new Color(1.0f, 0.0f, 0.0f);
        }
        Debug.Log(string.Format("Age Gate : {0}", status));
    }



    private void RedirectToStore(string storeURL)
    {
        // Redirect the player to the store page specified in Engage
        Debug.Log(string.Format("Redirecting to Store {0}",storeURL)); 
        Application.OpenURL(storeURL);
    }
}