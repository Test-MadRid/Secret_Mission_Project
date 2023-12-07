using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerNameManager : MonoBehaviour
{
	[SerializeField] TMP_InputField usernameInput;

	void Start()
	{
		if(PlayerPrefs.HasKey("myUsername"))
		{
			usernameInput.text = PlayerPrefs.GetString("myUsername");
			PhotonNetwork.NickName = PlayerPrefs.GetString("myUsername");
		}
		else
		{
			usernameInput.text = "Player " + Random.Range(0, 10000).ToString("0000");
			OnUsernameInputValueChanged();
		}
	}

	public void OnUsernameInputValueChanged()
	{
		PhotonNetwork.NickName = usernameInput.text;
		PlayerPrefs.SetString("myUsername", usernameInput.text);
	}
}
