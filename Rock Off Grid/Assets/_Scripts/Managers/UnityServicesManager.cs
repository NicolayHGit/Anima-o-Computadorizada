using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

public class UnityServicesManager : MonoBehaviour
{
    //private async void Awake()
    //{
        //await InitServices();
    //}

    public async Task InitServices()
    {
        await UnityServices.InitializeAsync();

        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            Debug.Log("Signed in anonymously with player ID: " + AuthenticationService.Instance.PlayerId);
        }
    }
}