using ExitGames.Client.Photon;
using Mono.Cecil.Cil;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(MultiRoomUI))]
public class MultiRoomManager : MonoBehaviourPunCallbacks
{
    [Header("참조 스크립트")]
    [SerializeField] private MultiRoomUI ui;
    [SerializeField] private PasswordManager passwordManager;
    [SerializeField] private CreateRoomManager createManager;
    [SerializeField] private PhotonManager photonManager;
    [SerializeField] private ErrorManager errorManager;

    // 방 리스트
    private Dictionary<string, RoomInfo> cachedRoomList;
    //방 찾기 실패 메세지
    public TextMeshProUGUI messageText;
    [SerializeField]
    private TMP_InputField searchCodeField;

    public void OnClickMultiPlay()
    {
        if (PhotonNetwork.IsConnected == false)
        {
            // 서버 연결
            photonManager.ConnectServer();
            ui.SetActiveRoomList(true);
        }
        else
        {
            // 이미 서버에 접속되어 있으면 로비 바로 입장
            PhotonNetwork.JoinLobby();
            ui.SetActiveRoomList(true);
        }
    }

    public void OnClickRoomCode()
    {
        if (PhotonNetwork.IsConnected == false)
        {
            // 서버 연결
            photonManager.ConnectServer();
        }
        else
        {
            // 이미 서버에 접속되어 있으면 로비 바로 입장
            PhotonNetwork.JoinLobby();
            ui.SetActiveRoomList(false);
        }
    }
    /*
    public override void OnJoinedLobby()
    {
        // 로비에 있을 때에만 방 목록 보이기
        ui.SetActiveRoomList(true);
    }*/

    public void OnExit()
    {
        PhotonNetwork.LeaveLobby();
    }

    public override void OnLeftLobby()
    {
        // 방 목록 비활성화
        ui.SetActiveRoomList(false);

        // 방 정보를 표시하던 오브젝트 제거
        ui.RemoveAllRooms();
    }

    public override void OnJoinedRoom()
    {
        // 방 연결 시 로비창으로 이동
        SceneManager.LoadScene("LobbyScene");
    }

    /***************************************************************
    * [ 방 목록 ]
    * 
    * 현재 개설된 방 목록 표시
    ***************************************************************/

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        // 기본 오브젝트 삭제
        ui.RemoveAllRooms();

        // 캐시에 방 목록 업데이트
        UpdateCachedRoomList(roomList);

        // 방 목록 생성
        List<RoomInfo> createRoomList = new List<RoomInfo>(cachedRoomList.Values);
        CreateRoomList(createRoomList);
    }

    private void UpdateCachedRoomList(List<RoomInfo> roomList)
    {
        if (cachedRoomList == null)
            cachedRoomList = new Dictionary<string, RoomInfo>();

        foreach (RoomInfo room in roomList)
        {
            string title = room.Name;

            if (room.RemovedFromList)
            {
                // 목록에서 삭제되었다면 캐시에서도 삭제
                cachedRoomList.Remove(title);
            }
            else
            {
                cachedRoomList[title] = room;
            }
        }
    }

    private void CreateRoomList(List<RoomInfo> roomList)
    {
        foreach (RoomInfo room in roomList)
        {
            if (room.RemovedFromList == false)
            {
                ui.AddRoomObj(room, (id, roomPassword) => OnEnterRoom(id, roomPassword));
            }
        }
    }

    public void OnClickSearch()
    {
        // 검색창에 입력된 단어를 토대로 방 검색
        string keyword = ui.GetSearchKeyword();

        SearchRoom(keyword);
    }

    public void OnClickSearchByCode()
    {
        string keyword = searchCodeField.text;
        Debug.Log(keyword);
        SearchRoomByCode(keyword);
    }

    private void SearchRoom(string keyword)
    {
        // 기존 오브젝트 초기화
        ui.RemoveAllRooms();

        foreach (string title in cachedRoomList.Keys)
        {
            // 키워드가 포함된 방만 생성
            if (title.Contains(keyword))
            {
                RoomInfo room = cachedRoomList[title];

                ui.AddRoomObj(room, (id, roomPassword) => OnEnterRoom(id, roomPassword));
            }
        }
    }

    private void SearchRoomByCode(string keyword)       //코드 검색으로 인한 방 진입
    {
        bool found = false;  // 일치하는 코드를 발견했는지 여부를 추적

        foreach (string code in cachedRoomList.Keys)
        {
            Debug.Log(code);
            // 키워드가 코드와 같을 시
            if (code.Equals(keyword))
            {

                OnEnterRoom(code);
                found = true;  // 발견했음을 표시
                break;  // 더 이상 반복하지 않고 종료
            }
        }

        // 일치하는 코드를 찾지 못한 경우
        if (!found)
        {
            ShowText();
            Invoke("HideText", 3f);
        }
    }

    void ShowText()
    {
        messageText.gameObject.SetActive(true);
    }

    void HideText()
    {
        messageText.gameObject.SetActive(false);
    }

    /***************************************************************
    * [ 방 접속 ]
    * 
    * 현재 개설된 방 목록 중에 하나 접속
    ***************************************************************/

    public void OnEnterRoom(string id, string roomPassword)
    {
        if (roomPassword != "")
        {
            // 비밀번호가 걸린 방이면 비밀번호 입력시키기
            passwordManager.InputPassword(roomPassword, () =>
            {
                // 비밀번호 통과했을 경우 방 입장
                PhotonNetwork.JoinRoom(id);
            });
        }
        else
        {
            // 비밀번호가 걸리지 않은 방은 바로 입장
            PhotonNetwork.JoinRoom(id);
        }
    }

    public void OnEnterRoom(string code)
    {
        // 코드를 통한 방 입장
        PhotonNetwork.JoinRoom(code);
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        errorManager.AlertError(returnCode, message);
    }
}