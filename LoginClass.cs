using System;

using PlayFab;
using PlayFab.GroupsModels;
using PlayFab.ClientModels;
using PlayFab.DataModels;
using PlayFab.Json;

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

public class LoginClass : MonoBehaviour {

    private static string player_entityKeyId;
    private static string player_entityKeyType;

    public void loginButtonClicked() {

        if (string.IsNullOrEmpty(PlayFabSettings.staticSettings.TitleId)){
            /*
            Please change the titleId below to your own titleId from PlayFab Game Manager.
            If you have already set the value in the Editor Extensions, this can be skipped.
            */
            PlayFabSettings.staticSettings.TitleId = "1D8CF";
        }

        var request = new LoginWithPlayFabRequest {Username = "lit", TitleId = "1D8CF", Password = "yeetus"};
        PlayFabClientAPI.LoginWithPlayFab(request, OnLoginSuccess, OnLoginFailure);

    }

    // Start is called before the first frame update
    private void OnLoginSuccess(LoginResult result) {

        Debug.Log(result.EntityToken.Entity.Id);
        LoginClass.player_entityKeyId = result.EntityToken.Entity.Id;
        LoginClass.player_entityKeyType = result.EntityToken.Entity.Type;

        // Need to render button color/text based on what appears in the wishlist

        PlayFab.GroupsModels.EntityKey entity = new PlayFab.GroupsModels.EntityKey {Id = LoginClass.player_entityKeyId, Type = LoginClass.player_entityKeyType };

        var request = new ListMembershipRequest { Entity = entity };

        StoreSetup.StoreStart();

        ButtonHandler.FindOrCreateWishList(LoginClass.player_entityKeyId, LoginClass.player_entityKeyType);

    }

    private void OnLoginFailure(PlayFabError error) {
        Debug.LogError(error.GenerateErrorReport());
    }

    public static string getPlayerEntityKeyType() {
        return LoginClass.player_entityKeyType;
    }

    public static string getPlayerEntityKeyId() {
        return LoginClass.player_entityKeyId;
    }
}
