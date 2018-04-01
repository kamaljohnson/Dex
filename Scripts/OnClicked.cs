using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnClicked : MonoBehaviour {

	playerController control;
	public GameObject OnClickUI;
	public GameObject Canvas;

	void Start()
	{
		control = GetComponent<playerController>();
	}
	void OnMouseDown()
	{
		OnClickUI.SetActive(true);
	}
	public void moveSelect()
	{
		control.selected = true;
		OnClickUI.SetActive(false);
	}
	public void actionSelect()
	{
		//this function should be writen for each truck type 
	}
}
