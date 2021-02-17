using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class OtherControls : MonoBehaviour
{
    [Header("Soldier Controlling")]
    public GameObject[] Soldiers;
    public KeyCode NextSoldier;
    public KeyCode PreviousSoldier;
    public int SelectedSoldier = 0;

    [Header("Other")]
    public CinemachineVirtualCamera CinemachineCamera;

    // Start is called before the first frame update
    void Start()
    {
        GetSoldiers();
        CinemachineCamera.Follow = Soldiers[SelectedSoldier].GetComponent<Transform>();
        //Soldiers[SelectedSoldier].GetComponent<sPlayerController>().BeingControlled = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(NextSoldier)) {
            //Soldiers[SelectedSoldier].GetComponent<sPlayerController>().BeingControlled = false;

            SelectedSoldier += 1;
            if (SelectedSoldier > Soldiers.Length-1) {
                SelectedSoldier = 0;
            }

            CinemachineCamera.Follow = Soldiers[SelectedSoldier].GetComponent<Transform>();
            //Soldiers[SelectedSoldier].GetComponent<sPlayerController>().BeingControlled = true;
        } else if (Input.GetKeyDown(PreviousSoldier)) {
            //Soldiers[SelectedSoldier].GetComponent<sPlayerController>().BeingControlled = false;

            SelectedSoldier -= 1;
            if (SelectedSoldier < 0) {
                SelectedSoldier = Soldiers.Length-1;
            }

            CinemachineCamera.Follow = Soldiers[SelectedSoldier].GetComponent<Transform>();
            //Soldiers[SelectedSoldier].GetComponent<sPlayerController>().BeingControlled = true;
        }
    }

    public void GetSoldiers() {
        Soldiers = GameObject.FindGameObjectsWithTag("Player");
    }
}
