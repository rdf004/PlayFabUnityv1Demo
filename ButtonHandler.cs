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

public class ButtonHandler : MonoBehaviour
{

    public readonly HashSet<KeyValuePair<string, string>> EntityGroupPairs = new HashSet<KeyValuePair<string, string>>();
    public readonly Dictionary<string, string> GroupNameById = new Dictionary<string, string>();

    public static string player_entityKeyId;
    public static string player_entityKeyType;

    public static string group_entityKeyId;
    public static string group_entityKeyType;

    public Button Login_Button, Arbitrary_Wishlist, Cat_Button, Dog_Button, Mango_Button;

    void Start() {
        Cat_Button = GameObject.FindGameObjectWithTag("Cat_Button").GetComponent<Button>();
        Dog_Button = GameObject.FindGameObjectWithTag("Dog_Button").GetComponent<Button>();
        Mango_Button = GameObject.FindGameObjectWithTag("Mango_Button").GetComponent<Button>();



    }
    
    public void clicked() {
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

    private void OnLoginSuccess(LoginResult result) {

        Debug.Log(result.EntityToken.Entity.Id);
        ButtonHandler.player_entityKeyId = result.EntityToken.Entity.Id;
        ButtonHandler.player_entityKeyType = result.EntityToken.Entity.Type;

        // Need to render button color/text based on what appears in the wishlist

        CreateWishlist();

        PlayFab.GroupsModels.EntityKey entity = new PlayFab.GroupsModels.EntityKey {Id = ButtonHandler.player_entityKeyId, Type = ButtonHandler.player_entityKeyType };

        var request = new ListMembershipRequest { Entity = entity };

        PlayFabGroupsAPI.ListMembership( request, membershipResult => {

            Debug.Log(membershipResult.Groups[0].GroupName);
            bool found = false;

            for (int i = 0; i < membershipResult.Groups.Count; i++ ) {
                
                Debug.Log(membershipResult.Groups.Count);
                Debug.Log(i);
                string group_name = ButtonHandler.player_entityKeyId + "wishlist";
                if( membershipResult.Groups[i].GroupName.Equals( group_name ) ) {

                    // Found our group!
                    // found = true;

                    PlayFab.DataModels.EntityKey group_ek = new PlayFab.DataModels.EntityKey { Id = membershipResult.Groups[i].Group.Id, Type = membershipResult.Groups[i].Group.Type };

                    // Assuming that if the group exists, the wishlist exists

                    // Get wishlist object from group entity

                    GetObjectsRequest getObjectsRequest = new GetObjectsRequest { Entity = group_ek };

                    PlayFabDataAPI.GetObjects(getObjectsRequest, objectResult => {
                        
                        // JsonObject jsonResult = (JsonObject) objectResult.Objects["wishlist"].DataObject;

                        Debug.Log(objectResult.Objects["wishlist"].DataObject); // This returns either the object or a CSV, can use for durables or consumables

                        string x = (string) objectResult.Objects["wishlist"].DataObject;

                        string[] items = x.Split(',');

                        for(int j = 0; j < items.Length; j++) {

                            if( items[j].Equals("cat") ) {
                                Cat_Button.GetComponentInChildren<Text>().text = "Cat (Added!)";
                                Cat_Button.GetComponent<Image>().color = Color.green;
                            } else if( items[j].Equals("dog") ) {
                                Dog_Button.GetComponentInChildren<Text>().text = "Dog (Added!)";
                                Dog_Button.GetComponent<Image>().color = Color.green;
                            } else if( items[j].Equals("mango") ) {
                                Mango_Button.GetComponentInChildren<Text>().text = "Mango (Added!)";
                                Mango_Button.GetComponent<Image>().color = Color.green;
                            }
                        }

                    }, error => { Debug.LogError(error.GenerateErrorReport()); });

                }
            }

        }, error => { Debug.LogError(error.GenerateErrorReport()); });

    }

    public void checkIfWishListExists(string item_id) {

        // bool add_item = true;
        // string item_id = "albatross";

        PlayFab.GroupsModels.EntityKey entity = new PlayFab.GroupsModels.EntityKey {Id = ButtonHandler.player_entityKeyId, Type = ButtonHandler.player_entityKeyType };

        var request = new ListMembershipRequest { Entity = entity };

        PlayFabGroupsAPI.ListMembership(request, result => {

            Debug.Log(result.Groups[0].GroupName);
            bool found = false;

            for (int i = 0; i < result.Groups.Count; i++ ) {
                
                Debug.Log(result.Groups.Count);
                Debug.Log(i);
                string group_name = ButtonHandler.player_entityKeyId + "wishlist";
                if( result.Groups[i].GroupName.Equals( group_name ) ) {

                    // Found our group!
                    found = true;

                    PlayFab.DataModels.EntityKey group_ek = new PlayFab.DataModels.EntityKey { Id = result.Groups[i].Group.Id, Type = result.Groups[i].Group.Type };

                    // Assuming that if the group exists, the wishlist exists

                    // Get wishlist object from group entity

                    GetObjectsRequest getObjectsRequest = new GetObjectsRequest { Entity = group_ek };

                    PlayFabDataAPI.GetObjects(getObjectsRequest, objectResult => {
                        
                        // JsonObject jsonResult = (JsonObject) objectResult.Objects["wishlist"].DataObject;

                        Debug.Log(objectResult.Objects["wishlist"].DataObject); // This returns either the object or a CSV, can use for durables or consumables

                        string x = (string) objectResult.Objects["wishlist"].DataObject;

                        bool contains_item = WishlistContainsItem(x, item_id);

                        bool adding_item;

                        if( !contains_item ) {

                            // Add item to wishlist
                            x += ",";
                            x += item_id;

                            adding_item = true;

                        } else {

                            // Remove item from wishlist

                            x = RemoveItemFromCSV(x, item_id);

                            adding_item = false;

                        }

                        Debug.Log(x);

                        // x += ",";
                        // x += "albatross, dog, mango";

                        UpdateWishlist(group_ek.Id, group_ek.Type, x, adding_item, item_id);

                        // Debug.Log(RemoveItemFromCSV(x, "cat"));
                        
                    }, error => {
                        Debug.LogError(error.GenerateErrorReport());
                    });               

                    // Set object

                    // Case 1: Add item to wishlist
                    // Case 1a: Success
                    // Case 1b: Item already on wishlist

                    // SetObjectsRequest setObjReq = new SetObjectsRequest { Entity = group_ek }

                    // Case 2: Remove item from wishlist
                    // Case 2a: Success
                    // Case 2b: Item not on wishlist
                    
                } 
            }

            // Group doesn't exist, so we need to create the group and wishlist

            if( found == false ) {

                string group_name = ButtonHandler.player_entityKeyId + "wishlist";

                CreateGroup(group_name, entity);
            }

        }, error => {
            Debug.LogError(error.GenerateErrorReport());
        });

    }

    private static string RemoveItemFromCSV(string csv, string item_id) {
        string[] items = csv.Split(',');
        int ind = Array.IndexOf(items, item_id);
        List<string> items_list = new List<string>(items);

        if( ind != -1 ) {
            items_list.RemoveAt(ind);
        }

        string updated_csv = string.Join(",", items_list);

        return updated_csv;

    }

    private static bool WishlistContainsItem(string csv, string item_id) {
        string[] items = csv.Split(',');
        int ind = Array.IndexOf(items, item_id);
        List<string> items_list = new List<string>(items);

        if( ind != -1 ) {
            return true;
        } else {
            return false;
        }

    }

    // private static void addItemtoWishlist(string csv, string item_id) {

    // }

    // private static void removeItemfromWishlist(string csv, string item_id) {

    // }

    // Made method non-static

    private void UpdateWishlist(string entitykeyId, string entitykeyType, string dataobj, bool adding_item, string item_id) {

        PlayFabCloudScriptAPI.ExecuteEntityCloudScript(new PlayFab.CloudScriptModels.ExecuteEntityCloudScriptRequest() {

            Entity = new PlayFab.CloudScriptModels.EntityKey { Id = entitykeyId, Type = entitykeyType },
            FunctionName = "addItemtoWishlist",
            FunctionParameter = new { prop1 = dataobj },
            GeneratePlayStreamEvent = true

        }, result => {
            Debug.Log(result.FunctionResult);

            if( adding_item ) {
                
                if( item_id.Equals("cat") ) {
                    Cat_Button.GetComponentInChildren<Text>().text = "Cat (Added!)";
                    Cat_Button.GetComponent<Image>().color = Color.green;
                } else if( item_id.Equals("dog") ) {
                    Dog_Button.GetComponentInChildren<Text>().text = "Dog (Added!)";
                    Dog_Button.GetComponent<Image>().color = Color.green;
                } else if( item_id.Equals("mango") ) {
                    Mango_Button.GetComponentInChildren<Text>().text = "Mango (Added!)";
                    Mango_Button.GetComponent<Image>().color = Color.green;
                }

            } else {

                if( item_id.Equals("cat") ) {
                    Cat_Button.GetComponentInChildren<Text>().text = "Cat (Add to Wishlist)";
                    Cat_Button.GetComponent<Image>().color = Color.white;
                } else if( item_id.Equals("dog") ) {
                    Dog_Button.GetComponentInChildren<Text>().text = "Dog (Add to Wishlist)";
                    Dog_Button.GetComponent<Image>().color = Color.white;
                } else if( item_id.Equals("mango") ) {
                    Mango_Button.GetComponentInChildren<Text>().text = "Mango (Add to Wishlist)";
                    Mango_Button.GetComponent<Image>().color = Color.white;
                }

            }

        }, error => { Debug.LogError(error.GenerateErrorReport()); });

    }



    private static void CreateWishlist() {

        PlayFabCloudScriptAPI.ExecuteEntityCloudScript(new PlayFab.CloudScriptModels.ExecuteEntityCloudScriptRequest() {

            Entity = new PlayFab.CloudScriptModels.EntityKey { Id = ButtonHandler.player_entityKeyId, Type = ButtonHandler.player_entityKeyType },
            FunctionName = "createUserWishList", // Arbitrary function name (must exist in your uploaded cloud.js file)
            FunctionParameter = new { groupName = "wishlisttestyboi1da612" }, // The parameter provided to your function
            GeneratePlayStreamEvent = true // Optional - Shows this event in PlayStream

        }, result => {

            Debug.Log(PlayFabSimpleJson.SerializeObject(result.FunctionResult));
            JsonObject jsonResult = (JsonObject)result.FunctionResult;

            object ek_id;
            jsonResult.TryGetValue("ek_id", out ek_id); // note how "messageValue" directly corresponds to the JSON values set in CloudScript
            string ek_id_string = (string)ek_id;

            object ek_type;
            jsonResult.TryGetValue("ek_type", out ek_type); // note how "messageValue" directly corresponds to the JSON values set in CloudScript
            string ek_type_string = (string)ek_type;

            AddItemToWishlist(ButtonHandler.player_entityKeyId, ButtonHandler.player_entityKeyType, ek_id_string, ek_type_string, "cat");

        }, error => { Debug.LogError(error.GenerateErrorReport()); });

    }

    private static void AddItemToWishlist(string my_id, string my_type, string ek_id, string ek_type, string dataobj) {

        PlayFabCloudScriptAPI.ExecuteEntityCloudScript(new PlayFab.CloudScriptModels.ExecuteEntityCloudScriptRequest() {

            Entity = new PlayFab.CloudScriptModels.EntityKey {Id = ek_id, Type = ek_type},
            FunctionName = "addItemtoWishlist",
            FunctionParameter = new { prop1 = dataobj },
            GeneratePlayStreamEvent = true

        }, result => {
            Debug.Log(result.FunctionResult);

        }, error => { Debug.LogError(error.GenerateErrorReport()); });

    }



    // private static void OnCloudHelloWorld(ExecuteCloudScriptResult result) {

    //     // CloudScript returns arbitrary results, so you have to evaluate them one step and one parameter at a time
    //     Debug.Log(PlayFabSimpleJson.SerializeObject(result.FunctionResult));
    //     JsonObject jsonResult = (JsonObject)result.FunctionResult;
    //     object created;
    //     jsonResult.TryGetValue("created", out created); // note how "messageValue" directly corresponds to the JSON values set in CloudScript
    //     Debug.Log((string)created);

    // }

    private static void OnErrorShared(PlayFabError error) {

        Debug.Log(error.GenerateErrorReport());

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


    private void OnLoginFailure(PlayFabError error)
    {
        Debug.LogWarning("Something went wrong with your first API call.  :(");
        Debug.LogError("Here's some debug information:");
        Debug.LogError(error.GenerateErrorReport());
    }




}
