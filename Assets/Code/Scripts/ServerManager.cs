using UnityEngine;
using System.Collections;

public class ServerManager : MonoBehaviour {

	public const string typeName = "Hellomisterme.Suddjian.David.Needleneck";
	private const string gameName = "Server0";

	public Rect serverCreationButton;

	private void StartServer() {
		Network.InitializeServer(4, 25000, !Network.HavePublicAddress());
		MasterServer.RegisterHost(typeName, gameName);
	}

	void Start() {
		if (serverCreationButton == new Rect(0, 0, 0, 0)) {
			serverCreationButton = new Rect(Screen.width * 0.02f, Screen.width * 0.02f, 100f, 100f);
		}
		GUIManager.AddGUIElement(serverCreationButton);
	}

	void OnServerInitialized() {
		Debug.Log("Server Initializied");
	}

	void OnGUI() {
		if (!Network.isServer && GUI.Button(serverCreationButton, "Start Server")) {
			Debug.Log("connecting");
			GUIManager.RemoveGUIElement(serverCreationButton);
			StartServer();
		}
	}
}
