using UnityEngine;
using System.Collections;

public class ClientManager : MonoBehaviour {

	private HostData[] hostList;

	private void RefreshHostList() {
		MasterServer.RequestHostList(ServerManager.typeName);
	}

	private void JoinServer(HostData hostData) {
		Network.Connect(hostData);
	}

	private void DestroyHostListButtons() {
		for (int i = 0; i < hostList.Length; i++) {
			GUIManager.RemoveGUIElement(new Rect(400, 100 + (110 * i), 300, 100));
		}
	}

	private void CreateHostListButtons() {
		for (int i = 0; i < hostList.Length; i++) {
			GUIManager.AddGUIElement(new Rect(400, 100 + (110 * i), 300, 100));
		}
	}

	void OnMasterServerEvent(MasterServerEvent msEvent) {
		if (msEvent == MasterServerEvent.HostListReceived) {
			if (hostList != null) {
				DestroyHostListButtons();
			}
			hostList = MasterServer.PollHostList();
			CreateHostListButtons();
		}
	}

	void OnConnectedToServer() {
		Debug.Log("Server Joined");
	}

	void Update() {
		RefreshHostList();
	}

	void OnGUI() {
		if (!Network.isClient && hostList != null) {
			for (int i = 0; i < hostList.Length; i++) {
				if (GUI.Button(new Rect(400, 100 + (110 * i), 300, 100), hostList[i].gameName)) {
					JoinServer(hostList[i]);
					DestroyHostListButtons();
				}
			}
		}
	}
}
