using PlayFab;
using PlayFab.GroupsModels;
using PlayFab.ClientModels;
using PlayFab.Json;

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

public class ButtonHandler : MonoBehaviour
{

    public readonly HashSet<KeyValuePair<string, string>> EntityGroupPairs = new HashSet<KeyValuePair<string, string>>();
    public readonly Dictionary<string, string> GroupNameById = new Dictionary<string, string>();
    
    public void clicked()
    {
        if (string.IsNullOrEmpty(PlayFabSettings.staticSettings.TitleId)){
            /*
            Please change the titleId below to your own titleId from PlayFab Game Manager.
            If you have already set the value in the Editor Extensions, this can be skipped.
            */
            PlayFabSettings.staticSettings.TitleId = "1D8CF";
        }

        var request = new LoginWithCustomIDRequest { CustomId = "GettingStartedGuide", CreateAccount = true};
        PlayFabClientAPI.LoginWithCustomID(request, OnLoginSuccess, OnLoginFailure);

    }

    public static PlayFab.GroupsModels.EntityKey EntityKeyMaker(string entityId) {

        return new PlayFab.GroupsModels.EntityKey { Id = entityId };

    }

    private void OnSharedError(PlayFab.PlayFabError error) {
        Debug.LogError(error.GenerateErrorReport());
    }

    public void CreateGroup(string groupName, PlayFab.GroupsModels.EntityKey entityKey) {
        // A player-controlled entity creates a new group
        var request = new CreateGroupRequest { GroupName = groupName, Entity = entityKey };
    }

    private void OnCreateGroup(CreateGroupResponse response) {
        Debug.Log("Group Created: " + response.GroupName + " - " + response.Group.Id);

        var prevRequest = (CreateGroupRequest)response.Request;
        EntityGroupPairs.Add(new KeyValuePair<string, string>(prevRequest.Entity.Id, response.Group.Id));
        GroupNameById[response.Group.Id] = response.GroupName;
    }

    public void ListGroups(PlayFab.GroupsModels.EntityKey entityKey){
        var request = new ListMembershipRequest { Entity = entityKey };
        PlayFabGroupsAPI.ListMembership(request, OnListGroups, OnSharedError);
    }

    private void OnListGroups(ListMembershipResponse response) {
        var prevRequest = (ListMembershipRequest)response.Request;
        foreach (var pair in response.Groups)
        {
            GroupNameById[pair.Group.Id] = pair.GroupName;
            EntityGroupPairs.Add(new KeyValuePair<string, string>(prevRequest.Entity.Id, pair.Group.Id));
        }
    }

    private void OnLoginSuccess(LoginResult result)
    {
        Debug.Log("Congratulations, you made your first successful API call!");

        Debug.Log("Yeet");
        StartCloudHelloWorld(result.EntityToken.Entity.Id, result.EntityToken.Entity.Type);
        Debug.Log("Yote");
    }

    private void OnLoginFailure(PlayFabError error)
    {
        Debug.LogWarning("Something went wrong with your first API call.  :(");
        Debug.LogError("Here's some debug information:");
        Debug.LogError(error.GenerateErrorReport());
    }

    private static void StartCloudHelloWorld(string my_id, string my_type)
    {
        PlayFabCloudScriptAPI.ExecuteEntityCloudScript(new PlayFab.CloudScriptModels.ExecuteEntityCloudScriptRequest()
        {
            Entity = new PlayFab.CloudScriptModels.EntityKey {Id = my_id, Type = my_type},
            FunctionName = "createUserWishList", // Arbitrary function name (must exist in your uploaded cloud.js file)
            FunctionParameter = new { groupName = "myWishlist2" }, // The parameter provided to your function
            GeneratePlayStreamEvent = true // Optional - Shows this event in PlayStream
        }, result => {
            Debug.Log(result.FunctionResult);
        }, error => { Debug.LogError(error.GenerateErrorReport()); });
    }

    private static void OnCloudHelloWorld(ExecuteCloudScriptResult result) {
        // CloudScript returns arbitrary results, so you have to evaluate them one step and one parameter at a time
        Debug.Log(PlayFabSimpleJson.SerializeObject(result.FunctionResult));
        JsonObject jsonResult = (JsonObject)result.FunctionResult;
        object created;
        jsonResult.TryGetValue("created", out created); // note how "messageValue" directly corresponds to the JSON values set in CloudScript
        Debug.Log((string)created);
    }

    private static void OnErrorShared(PlayFabError error)
    {
        Debug.Log(error.GenerateErrorReport());
    }


}
