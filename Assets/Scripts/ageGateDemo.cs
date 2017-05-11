using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DeltaDNA;


public class ageGateDemo : MonoBehaviour
{

    public UnityEngine.UI.Text lblUser;  // used to display the DDNA userID on screen
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
        StartDDNA();

        CloseAgeGate(); // Remove if you want to persist age gate setting across sessions. 
    }

    private void Update()
    {
        // Display userID on screen
        lblUser.text = string.Format("userID : {0}", DDNA.Instance.UserID).ToString();
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
    }

    private void CloseAgeGate()
    {
        GameEvent gameEvent = new GameEvent("ageGateUpdated")
            .AddParam("ageGatePassed", 0); 
     
        DDNA.Instance.RecordEvent(gameEvent);
    }

    public void Reset()
    { 
        // This method will reset the app, if it is active and isn't busy uploading.
        // A new userID will be generated and any previous age gate settings will be forgotten.
        // You wouldn't want to do this in a normal app
        if (DDNA.Instance.isActiveAndEnabled && DDNA.Instance.IsUploading == false)
        {
            DDNA.Instance.StopSDK();
            DDNA.Instance.ClearPersistentData();

            StartDDNA();
        }
    }

    public void VisitStore()
    {    
        var engagement = new Engagement("ageGate");
        Dictionary<string, object> parameters = new Dictionary<string, object>();

        DDNA.Instance.RequestEngagement(engagement, (response) =>
        {
            if (response == null || response.JSON == null || !response.JSON.ContainsKey("parameters")) return ;
            parameters = response.JSON["parameters"] as Dictionary<string, object>;

            if (parameters.ContainsKey("ageGateSuccessURL") && CheckAge(parameters))
            {
                storeURL = parameters["ageGateSuccessURL"].ToString();
                RedirectToStore(storeURL);
            };                      

        }, (exception) =>
        {
            Debug.Log("Engage reported an error: " + exception.Message);
        });
    }


    private bool CheckAge(Dictionary<string,object> ageGateParameters)
    {
        // Should always contain the store URL
        

        if (ageGateParameters.ContainsKey("ageGateQuestion") && ageGateParameters.ContainsKey("ageGateAnswer"))
        {
            // We have an age gate question to ask
            string title    = ageGateParameters["ageGateTitle"].ToString();
            string text     = ageGateParameters["ageGateText"].ToString(); ;
            string question = ageGateParameters["ageGateQuestion"].ToString(); ;
            string answer   = ageGateParameters["ageGateAnswer"].ToString(); ;

            Debug.Log(title);
            return AskQuestion(title, text, question, answer);

        }
        else
        {
            // No age gate question
            return true;
        }
        
    }

    private bool AskQuestion(string title, string text, string question, string answer)
    {
        bool result = false;

        txtTitle.text       = (title != null ? title :"")  ;
        txtText.text        = (text != null ? text : "") ;
        txtQuestion.text    = (question != null ? question : "");
        ageGateAnswer = answer;
        frmAnswer.text      = ""; 
        ageGatePanel.SetActive(true); 

        return result; 
    }


    public void AnswerQuestion()
    {

        CloseQuestionPanel();

        bool result = (ageGateAnswer.ToLower() == frmAnswer.text.ToLower()); 
        
            GameEvent gameEvent = new GameEvent("ageGateUpdated")
                .AddParam("ageGatePassed", result == true ? 1:0);

            DDNA.Instance.RecordEvent(gameEvent);

            Debug.Log(string.Format("Age Gate : {0}", result));
        if (result) RedirectToStore(storeURL); ; 

    }
    public void CloseQuestionPanel()
    {
        ageGatePanel.SetActive(false);

        return;
    }

    


    private void RedirectToStore(string storeURL)
    {
        // Redirect the player to the store page specified in Engage
        Application.OpenURL(storeURL);
    }
}