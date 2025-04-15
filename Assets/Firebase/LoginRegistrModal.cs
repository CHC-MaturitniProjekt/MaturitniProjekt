using System;
using TMPro;
using UnityEditor.PackageManager;
using UnityEngine;
using static UnityEngine.Rendering.STP;

public class LoginRegistrModal : MonoBehaviour
{
    [SerializeField] private TMP_InputField loginEmail;
    [SerializeField] private TMP_InputField loginPassword;
    [SerializeField] private GameObject loginError;
    [SerializeField] private GameObject loginParent;

    [SerializeField] private TMP_InputField registerEmail;
    [SerializeField] private TMP_InputField registerUsername;
    [SerializeField] private TMP_InputField registerPassword;
    [SerializeField] private TMP_InputField registerConfirmPassword;
    [SerializeField] private GameObject registerError;
    [SerializeField] private GameObject registerParent;


    FirebaseConfig config;
    private FirebaseClient client;
    void Start()
    {
        config = new FirebaseConfig("https://augumentum-default-rtdb.europe-west1.firebasedatabase.app/", "AIzaSyBkWcIDRsasLWlVmu2ZLHIsbu5LVT2-y3U");
        client = new FirebaseClient(config);

        string cachedId = client.LoadCachedLocalId();
        Debug.Log(cachedId);
        if (!string.IsNullOrEmpty(cachedId))
        {
            loginFinished();
            registerFinished();
        }
    }

    private void loginFinished()
    {
        loginParent.SetActive(false);
    }

    private void registerFinished()
    {
        registerParent.SetActive(false);
    }


    public async void onLoginClick()
    {
        try
        {
            string cachedId = client.LoadCachedLocalId();
            if (!string.IsNullOrEmpty(cachedId))
            {
                loginFinished();
            }
            else
            {
                var authResponse = await client.SignInWithEmailAndPasswordAsync(loginEmail.text, loginPassword.text);
                loginFinished();
            }
        }
        catch (Exception ex)
        {
            loginError.SetActive(true);
        }
    }

    public async void onRegisterClick()
    {
        try
        {
            var authResponse = await client.SignUpWithEmailAndPasswordAsync(registerEmail.text, registerPassword.text);
            var response = await client.AddUserAsync(registerUsername.text, authResponse.localId);
            client.SaveCachedLocalId(authResponse.localId);
            registerFinished();
            
        }
        catch (Exception ex)
        {
            registerError.SetActive(true);
        }
    }

    public void switchToRegister()
    {
        loginParent.SetActive(false);
        registerParent.SetActive(true);
    }

    public void switchToLogin()
    {
        registerParent.SetActive(false);
        loginParent.SetActive(true);
    }
}
