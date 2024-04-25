﻿using Photon.Pun;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(PlayerPanelUI))]
public class PlayerPanelManager : MonoBehaviourPun, IPunObservable
{
    [Header("참조 스크립트")]
    [SerializeField] private PlayerPanelUI ui;

    // 플레이어 정보
    private ClassData classData;

    // 패널 정보
    private bool _isClosed;
    public bool IsClosed
    {
        get { return _isClosed; }
        set
        {
            _isClosed = value;

            ui.SetActiveCloseMark(value);
        }
    }

    private bool _isExist;
    public bool IsExist
    {
        get { return _isExist; }
    }

    public void InitUI()
    {
        // Init UI
        ui.SetActiveCharacter(false);
        ui.SetActivePlayerMenu(false);
        ui.SetActivePlayerName(false);
        ui.SetClassName("");
    }

    [PunRPC]
    public void SetInfo(Photon.Realtime.Player player)
    {
        // 본인인 경우 클래스 초기 설정
        if (player.IsLocal)
        {
            // 마지막으로 했던 클래스로 설정
            SetInitClass();

            // 다른 사람들도 적용
            photonView.RPC(nameof(SetInfo), RpcTarget.Others, player);
        }

        // UI 설정
        if (PhotonNetwork.IsMasterClient)
        {
            // 방장은 캐릭터 이미지와 해당 플레이어의 메뉴 활성화
            ui.SetActiveCharacter(true);
            ui.SetActivePlayerMenu(true);
        }
        else
        {
            // 방장 외엔 캐릭터 이미지만 활성화
            ui.SetActiveCharacter(true);
        }
    }

    private void SetInitClass()
    {
        ClassData initClass = ClassResource.Instance.ClassList[0];

        SetClass(initClass);
    }

    public void SetClass(ClassData classData)
    {
        this.classData = classData;

        // 결정한 직업 이름 설정
        photonView.RPC(nameof(ui.SetClassName), RpcTarget.All, classData.Name);
    }

    /***************************************************************
    * [ 데이터 동기화 ]
    * 
    * 패널 상태 동기화
    ***************************************************************/

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        Debug.Log("serialize");
        if (stream.IsWriting)
        {
            stream.SendNext(classData.name);
            stream.SendNext(_isExist);
        }
        else
        {
            string className = (string)stream.ReceiveNext();
            ui.SetClassName(className);

            _isExist = (bool)stream.ReceiveNext();
        }
    }
}