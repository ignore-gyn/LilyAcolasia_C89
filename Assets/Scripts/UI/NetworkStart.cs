﻿using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using UnityEngine.SceneManagement;


public class NetworkStart : MonoBehaviour {
	private const int DEFAULT_PORT = 8856;

	public static LilyAcolasia.NetworkUser user = null;
	public NetworkStartBase net;

	private int frameCount = 0;

	void Start() {
	}

	void Update() {
		frameCount++;
		if (frameCount % 300 == 0 && user != null) {
			if (user.IsServer) {
				if (user.Accept ()) {
					CameraFade.StartAlphaFade(Color.black, false, 0.5f, 0.2f, () =>
						{
							net.menuNet.SetActive (true);
							net.menuCreate.SetActive (false);
							GameObject.Find("NetworkMenu").SetActive (false);

							PlayerPrefs.SetInt("level", 0);
							PlayerPrefs.SetInt("network", 1);
							SceneManager.LoadScene("Game");
						});
				}
			} else {
				if (user.Connect ()) {
					CameraFade.StartAlphaFade(Color.black, false, 0.5f, 0.2f, () =>
						{
							net.menuNet.SetActive (true);
							net.menuJoin.SetActive (false);
							GameObject.Find("NetworkMenu").SetActive (false);

							PlayerPrefs.SetInt("network", 2);
							SceneManager.LoadScene("Game");
						});
				}
			}
		}

		/*
			if (nowId.Length < 16) {
				for (int i = 0; i < 10; i++) {
					string key = "" + i;
					if (Input.GetKeyDown (key)) {
						Debug.Log ("1");
						nowId += key;
					}
				}
			}
			if (Input.GetKeyDown(KeyCode.Backspace)) {
				if (nowId.Length > 0) {
					nowId = nowId.Substring (0, nowId.Length - 1);
				}
			}
			long id = 0;
			Int64.TryParse (nowId, out id);
			net.eFieldId.Number = id;
		}
	*/
	}

	void OnMouseDown()
	{
		if (this.name == "menu_item_network_create") {
			net.input.text = "127.0.0.1";
			net.menuNet.SetActive (false);
			net.networkUI.SetActive (true);
			net.menuCreate.SetActive (true);
		} else if (this.name == "menu_item_network_join") {
			net.input.text = "";
			net.menuNet.SetActive (false);
			net.menuJoin.SetActive (true);
			net.networkUI.SetActive (true);
		} else if (this.name == "menu_item_back_join") {
			close ();
			net.menuNet.SetActive (true);
			net.menuJoin.SetActive (false);
			net.networkUI.SetActive (false);
		} else if (this.name == "menu_item_back_create") {
			net.menuNet.SetActive (true);
			net.menuCreate.SetActive (false);
			net.networkUI.SetActive (false);
		} else if (this.name == "menu_item_back_create2") {
			close ();
			net.menuCreate.SetActive (true);
			net.networkUI.SetActive (true);
			net.menuCreate2.SetActive (false);
		} else if (this.name == "menu_item_enter_create") {
			enterServer ();
		} else if (this.name == "menu_item_enter") {
			enterClient ();
		} else if (this.name == "menu_item_back_create") {
			user.Close ();
			user = null;
			Debug.Log ("Server close.");
			net.menuNet.SetActive (true);
			net.menuCreate.SetActive (false);
		} else if (this.name == "menu_item_back") {
			GameObject.Find("NetworkMenu").SetActive (false);
			net.menuMain.SetActive(true);
		}
	}

	private void close() {
		if (user != null) {
			user.Close ();
			user = null;
			Debug.Log ("Client close.");
		}
	}

	public void enterClient() {
		string host;
		int port;

		long id = 0;
		Int64.TryParse (net.input.text, out id);

		if (!decode (id, out host, out port)) {
			net.statusText.text = "Invalid ID!";
		} else {
			net.statusText.text = "";
			user = new LilyAcolasia.NetworkUser (host, port, "ClientPlayer");
			Debug.Log ("Client connecting: " + host + ":" + port);
			Debug.Log ("E Filed ID: " + net.eFieldId.Number);
		}
	}

	public void enterServer () {
		Regex r = new Regex (@"^([\d\.]+)(:?:(\d+))?$");
		Match m = r.Match (net.input.text);

		if (m.Groups.Count < 2) {
			net.statusText.text = "ex. 101.102.103.104";
		} else {
			net.statusText.text = "";
			int port = 0;
			Int32.TryParse (m.Groups [2].Value, out port);
			if (port == 0) {
				port = DEFAULT_PORT;
			}
			string host = m.Groups [1].Value;

			net.eFieldId.Number = encode(host, port);
			int rand = new System.Random ().Next ();
			user = new LilyAcolasia.NetworkUser (port, "ServerPlayer", rand);
			Debug.Log ("Server waiting: " + host + ":" + port);
			Debug.Log ("E Filed ID: " + net.eFieldId.Number);

			net.menuCreate.SetActive (false);
			net.menuCreate2.SetActive (true);
			net.networkUI.SetActive (false);
		}
	}

	private static long encode(string host, int port) {
		long id = 0;
		foreach (string s in host.Split('.')) {
			id *= 256;
			id += Int32.Parse (s);
		}
		id *= 100000;
		id += port;
		return id;
	}

	private static bool decode (long id, out string host, out int port) {
		port = (int)(id % 100000);
		id /= 100000;

		string[] arr = new string[4];
		for (int i = 3; i >= 0; i --) {
			arr [i] = "" + (id % 256);
			id /= 256;
		}
		host = String.Join(".", arr);
		return arr[0] != "0" && id == 0;
	}
}
