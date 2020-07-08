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

public class StoreSetup : MonoBehaviour {

    public static Button Login_Button, Arbitrary_Wishlist, Cat_Button, Dog_Button, Mango_Button;

    void Start() {
        Cat_Button = GameObject.FindGameObjectWithTag("Cat_Button").GetComponent<Button>();
        Dog_Button = GameObject.FindGameObjectWithTag("Dog_Button").GetComponent<Button>();
        Mango_Button = GameObject.FindGameObjectWithTag("Mango_Button").GetComponent<Button>();

    }

    public static void StoreStart() {
        Cat_Button = GameObject.FindGameObjectWithTag("Cat_Button").GetComponent<Button>();
        Dog_Button = GameObject.FindGameObjectWithTag("Dog_Button").GetComponent<Button>();
        Mango_Button = GameObject.FindGameObjectWithTag("Mango_Button").GetComponent<Button>();

        Cat_Button.GetComponentInChildren<Text>().text = "Cat (Add to Wishlist)";
        Cat_Button.GetComponent<Image>().color = Color.white; 

        Dog_Button.GetComponentInChildren<Text>().text = "Dog (Add to Wishlist)";
        Dog_Button.GetComponent<Image>().color = Color.white;

        Mango_Button.GetComponentInChildren<Text>().text = "Mango (Add to Wishlist)";
        Mango_Button.GetComponent<Image>().color = Color.white;       

    }

    public static void SetUpStore(string item_ids, bool remove_item) {

        // Ideally, this should go through and set text whether or not the button/item is on the list
        
        string[] items = item_ids.Split(',');

        for(int j = 0; j < items.Length; j++) {

            if( items[j].Equals("cat") ) {
                if( !remove_item ) {
                    Cat_Button.GetComponentInChildren<Text>().text = "Cat (Added!)";
                    Cat_Button.GetComponent<Image>().color = Color.green;
                } else {
                    Cat_Button.GetComponentInChildren<Text>().text = "Cat (Add to Wishlist)";
                    Cat_Button.GetComponent<Image>().color = Color.white;                   
                }
            } else if( items[j].Equals("dog") ) {
                if( !remove_item ) {
                    Dog_Button.GetComponentInChildren<Text>().text = "Dog (Added!)";
                    Dog_Button.GetComponent<Image>().color = Color.green;
                } else {
                    Dog_Button.GetComponentInChildren<Text>().text = "Dog (Add to Wishlist)";
                    Dog_Button.GetComponent<Image>().color = Color.white;
                }
            } else if( items[j].Equals("mango") ) {
                if( !remove_item ) {
                    Mango_Button.GetComponentInChildren<Text>().text = "Mango (Added!)";
                    Mango_Button.GetComponent<Image>().color = Color.green;
                } else {
                    Mango_Button.GetComponentInChildren<Text>().text = "Mango (Add to Wishlist)";
                    Mango_Button.GetComponent<Image>().color = Color.white;     
                }
            }
        }

    }

}
