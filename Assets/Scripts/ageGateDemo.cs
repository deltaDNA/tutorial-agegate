using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DeltaDNA; 


public class ageGateDemo : MonoBehaviour {

    public UnityEngine.UI.Text lblUser;  // used to display the DDNA userID on screen


	// Use this for initialization
	void Start () {

        StartDDNA();
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

        


        // CloseAgeGate();
    }


    public void Reset()
    {
        lblUser.text = "Twat";
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

}
