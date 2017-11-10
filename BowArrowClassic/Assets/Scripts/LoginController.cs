using UnityEngine;
using UnityEngine.UI;
using Boomlagoon.JSON;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;

public class LoginController : MonoBehaviour
{
    private const string BaseURL = "http://runner-up.getsandbox.com/";

    User newUser;

    public List<User> listOfUsers = new List<User>();

    public GameObject panel_LoadingPanel;
    public GameObject panel_MessageBox;

    public Button button_LogIn;
    public Button button_Register;
    public Button button_Delete;
    public Button button_GeneralButton;

    public InputField inputfield_Username;
    public InputField inputfield_Password;

    public GameObject UI;
    public GameObject canvas;
    public Text text_GeneralText;

    [System.Serializable]
    public class User
    {
        public string username;
        public string password;
    }

    bool isUserLogged = false;


    /* This is a Get data Method to get data from the sandbox*/
    private void GetData()
    {
        WWW www = new WWW(BaseURL + "getusers");
        StartCoroutine(GetUsersFromSandbox(www));
    }


    /* This is a Get data Method to get data from the sandbox*/
    IEnumerator GetUsersFromSandbox(WWW www)
    {
        yield return www;
        if (www.error == "")
        {
            string data = www.text;
            JSONArray jsonArray = JSONArray.Parse(data);
            if (jsonArray == null)
            {
                Debug.LogError("Empty Json");
            }
            else
            {
                Debug.Log("length is" + jsonArray.Length);
                Debug.Log(jsonArray);

                for (int i = 0; i < jsonArray.Length; i++)
                {
                    newUser = new User();
                    if (jsonArray[i].Obj["username"] != null)
                        newUser.username = jsonArray[i].Obj["username"].Str;
                    if (jsonArray[i].Obj["password"] != null)
                        newUser.password = jsonArray[i].Obj["password"].Str;
                    listOfUsers.Add(newUser);
                }
            }
        }
        else
        {
            Debug.LogError(www.error);
        }
    }
    
    /*This is Post data to Sandbox*/
    private void PostData(string urlAddress, string newData)
    {
        StartCoroutine(PostRequest(urlAddress, newData));
    }

    IEnumerator PostRequest(string url, string bodyJsonString)
    {
        var request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = new System.Text.UTF8Encoding().GetBytes(bodyJsonString);
        request.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.Send();
        Debug.Log("Response: " + request.downloadHandler.text);
    }
   
    /*This is Delete a data to Sandbox*/
    private void DeleteData(string urlAddress, string newData)
    {
        StartCoroutine(DeleteRequest(urlAddress, newData));
    }

    IEnumerator DeleteRequest(string url, string bodyJsonString)
    {
        var request = new UnityWebRequest(url, "DELETE");
        byte[] bodyRaw = new System.Text.UTF8Encoding().GetBytes(bodyJsonString);
        request.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.Send();
        Debug.Log("Response: " + request.downloadHandler.text);
    }

    string deleteData = "null";


    #region LOGIN_CALLBACKS
    void Start()
    {
        //GET	
        GetData();

        //POST
        //PostData (BaseURL + "users",newData);

        //DELETE
        //DeleteData(BaseURL + "users/" + "/john", deleteData);



        #region REFERENCES
        panel_LoadingPanel = GameObject.Find("Panel-LogIn");
        panel_MessageBox = GameObject.Find("Panel-MessageBox");

        button_LogIn = GameObject.Find("Button-Login").GetComponent<Button>();
        button_Register = GameObject.Find("Button-Register").GetComponent<Button>();
        button_Delete = GameObject.Find("Button-Delete").GetComponent<Button>();
        button_GeneralButton = GameObject.Find("Button-GeneralButton").GetComponent<Button>();

        inputfield_Username = GameObject.Find("InputField-Username").GetComponent<InputField>();
        inputfield_Password = GameObject.Find("InputField-Password").GetComponent<InputField>();

        text_GeneralText = GameObject.Find("Text-GeneralText").GetComponent<Text>();
        #endregion REFERENCES

        #region CALLBACKS
        button_LogIn.onClick.AddListener(() => Callback_LogIn());
        button_Register.onClick.AddListener(() => Callback_Register());
        button_Delete.onClick.AddListener(() => Callback_Delete());
        button_GeneralButton.onClick.AddListener(() => Callback_MessageButton());
        #endregion CALLBACKS

        //panel_LoadingPanel.SetActive (true);
        LogInPanel_status(true);
        MessageBox_DisableStatus();
    }

    void Callback_LogIn()
    {
        if (inputfield_Username.transform.Find("Text").GetComponent<Text>().text != "" &&
            inputfield_Password.transform.Find("Text").GetComponent<Text>().text != "")
        {

            for (int i = 0; i < listOfUsers.Count; i++)
            {
                if (listOfUsers[i].username == inputfield_Username.text && listOfUsers[i].password == inputfield_Password.text)
                {

                    LogInPanel_status(false);

                    MessageBox_SendMessage("Found the user");
                    MessageBox_EnableStatus();
                    isUserLogged = true;
                    break;
                }
                else
                {
                    MessageBox_SendMessage("User not found");
                    MessageBox_EnableStatus();
                }
            }
        }
        else
        {
            MessageBox_SendMessage("Empty credentials");
            MessageBox_EnableStatus();
        }
    }

    void Callback_Register()
    {
        if (inputfield_Username.transform.Find("Text").GetComponent<Text>().text != "" &&
            inputfield_Password.transform.Find("Text").GetComponent<Text>().text != "")
        {

            string enteredUsername = inputfield_Username.transform.Find("Text").GetComponent<Text>().text;
            string enteredPassword = inputfield_Password.transform.Find("Text").GetComponent<Text>().text;

            User newUser = new User();
            newUser.username = enteredUsername;
            newUser.password = enteredPassword;

            listOfUsers.Add(newUser);

            PostData(BaseURL + "postusers", JsonUtility.ToJson(newUser));

            MessageBox_SendMessage("Registration complete");
            MessageBox_EnableStatus();

        }
        else
        {
            MessageBox_SendMessage("Empty credentials");
            MessageBox_EnableStatus();
        }
    }

    void Callback_Delete()
    {
        if (inputfield_Username.transform.Find("Text").GetComponent<Text>().text != "" &&
            inputfield_Password.transform.Find("Text").GetComponent<Text>().text != "")
        {

            for (int i = 0; i < listOfUsers.Count; i++)
            {
                if (listOfUsers[i].username == inputfield_Username.text && listOfUsers[i].password == inputfield_Password.text)
                {

                    string temp = BaseURL + "deleteusers/" + "/" + listOfUsers[i].username;

                    User tempUser = new User();
                    tempUser.username = listOfUsers[i].username;
                    tempUser.password = listOfUsers[i].password;

                    if (tempUser.username == listOfUsers[i].username)
                    {
                        listOfUsers.Remove(tempUser);
                    }

                    DeleteData(temp, deleteData);
                    MessageBox_SendMessage("User successfully deleted");
                    MessageBox_EnableStatus();

                    listOfUsers.Clear();
                    GetData();

                    break;
                }
                else
                {
                    MessageBox_SendMessage("User not found");
                    MessageBox_EnableStatus();
                }
            }

        }
        else
        {
            MessageBox_SendMessage("Empty credentials");
            MessageBox_EnableStatus();
        }
    }

    void LogInPanel_status(bool status)
    {
        panel_LoadingPanel.SetActive(status);
    }

    void Callback_MessageButton()
    {
        

        MessageBox_DisableStatus();

        if(isUserLogged == true)
        {
            isUserLogged = false;
            UI.SetActive(true);
            canvas.SetActive(false);
        }

    }

    void MessageBox_EnableStatus()
    {
        panel_MessageBox.SetActive(true);
    }

    void MessageBox_DisableStatus()
    {
        panel_MessageBox.SetActive(false);
    }

    void MessageBox_SendMessage(string message)
    {
        text_GeneralText.text = message;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {

        }
    }

#endregion
}
