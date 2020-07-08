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

    public static string group_entityKeyId;
    public static string group_entityKeyType;

    public static void FindOrCreateWishList(string player_entityKeyId, string player_entityKeyType) {

        // Need to render button color/text based on what appears in the wishlist

        PlayFab.GroupsModels.EntityKey entity = new PlayFab.GroupsModels.EntityKey {Id = player_entityKeyId, Type = player_entityKeyType };

        var request = new ListMembershipRequest { Entity = entity };

        PlayFabGroupsAPI.ListMembership( request, membershipResult => {

            Debug.Log(membershipResult.Groups[0].GroupName);
            bool found = false;

            for (int i = 0; i < membershipResult.Groups.Count; i++ ) {
                
                Debug.Log(membershipResult.Groups.Count);
                Debug.Log(i);
                string group_name = LoginClass.getPlayerEntityKeyId() + "wishlist";
                if( membershipResult.Groups[i].GroupName.Equals( group_name ) ) {

                    // Found our group!
                    found = true;

                    ButtonHandler.group_entityKeyId = membershipResult.Groups[i].Group.Id;
                    ButtonHandler.group_entityKeyType = membershipResult.Groups[i].Group.Type;

                    PlayFab.DataModels.EntityKey group_ek = new PlayFab.DataModels.EntityKey { Id = membershipResult.Groups[i].Group.Id, Type = membershipResult.Groups[i].Group.Type };

                    // Assuming that if the group exists, the wishlist exists

                    // Get wishlist object from group entity

                    GetObjectsRequest getObjectsRequest = new GetObjectsRequest { Entity = group_ek };

                    PlayFabDataAPI.GetObjects(getObjectsRequest, objectResult => {

                        Debug.Log(objectResult.Objects["wishlist"].DataObject); // This returns either the object or a CSV, can use for durables or consumables

                        string x = (string) objectResult.Objects["wishlist"].DataObject;

                        StoreSetup.SetUpStore(x, false); // Think if this setupstore flow is problematic

                    }, error => { Debug.LogError(error.GenerateErrorReport()); });

                }
            }

            if( !found ) {

                string group_name = LoginClass.getPlayerEntityKeyId() + "wishlist";
                CreateWishlist(group_name, "");

            }

        }, error => { Debug.LogError(error.GenerateErrorReport()); });

    }


    public void UpdateWishlist(string item_id) {

        PlayFab.DataModels.EntityKey group_ek = new PlayFab.DataModels.EntityKey { Id = ButtonHandler.group_entityKeyId, Type = ButtonHandler.group_entityKeyType };

        // Assuming that if the group exists, the wishlist exists

        // Get wishlist object from group entity

        GetObjectsRequest getObjectsRequest = new GetObjectsRequest { Entity = group_ek };

        PlayFabDataAPI.GetObjects(getObjectsRequest, objectResult => {

            Debug.Log(objectResult.Objects["wishlist"].DataObject); // This returns either the object or a CSV, can use for durables or consumables
            string x = (string) objectResult.Objects["wishlist"].DataObject;
            bool adding_item;

            if( !WishlistContainsItem(x, item_id) ) {

                x = AddItemToCSV(x, item_id);
                adding_item = true;

            } else {

                x = RemoveItemFromCSV(x, item_id);
                adding_item = false;

            }

            UpdateGroupObject(x, adding_item, item_id);
            
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

    private static string AddItemToCSV(string csv, string item_id) {
        return csv + "," + item_id;
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

    private void UpdateGroupObject(string dataobj, bool adding_item, string item_id) {

        PlayFabCloudScriptAPI.ExecuteEntityCloudScript(new PlayFab.CloudScriptModels.ExecuteEntityCloudScriptRequest() {

            Entity = new PlayFab.CloudScriptModels.EntityKey { Id = ButtonHandler.group_entityKeyId, Type = ButtonHandler.group_entityKeyType },
            FunctionName = "addItemtoWishlist",
            FunctionParameter = new { prop1 = dataobj },
            GeneratePlayStreamEvent = true

        }, result => {
            Debug.Log(result.FunctionResult);

            if( adding_item ) {

                StoreSetup.SetUpStore(item_id, false);

            } else {

                StoreSetup.SetUpStore(item_id, true);

            }

        }, error => { Debug.LogError(error.GenerateErrorReport()); });

    }

    private static void CreateWishlist(string group_name, string item_id) {

        PlayFabCloudScriptAPI.ExecuteEntityCloudScript(new PlayFab.CloudScriptModels.ExecuteEntityCloudScriptRequest() {

            Entity = new PlayFab.CloudScriptModels.EntityKey { Id = LoginClass.getPlayerEntityKeyId(), Type = LoginClass.getPlayerEntityKeyType() },
            FunctionName = "createUserWishList", // Arbitrary function name (must exist in your uploaded cloud.js file)
            FunctionParameter = new { groupName = group_name }, // The parameter provided to your function
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

            // UpdateWishlist(ek_id_string, ek_type_string, item_id, true, item_id);

        }, error => { Debug.LogError(error.GenerateErrorReport()); });

    }

}
