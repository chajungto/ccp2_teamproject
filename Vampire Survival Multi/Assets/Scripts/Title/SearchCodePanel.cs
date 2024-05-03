using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SearchCodePanel : MonoBehaviour
{
    public GameObject roomCodePanel;
    [SerializeField]
    private TMP_InputField searchCodeField;

    public void openSearchCodePanel()
    {
        roomCodePanel.SetActive(true);
    }

    public void closeSearchCodePanel()
    {
        roomCodePanel.SetActive(false);
    }

    public void ClearSearchField()
    {
        searchCodeField.text = "";
    }
}