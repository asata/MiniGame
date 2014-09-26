using UnityEngine;
using System.Collections;

public class Tutorial : MonoBehaviour {
	private string gameName;
	private int index = 0;

	public GUITexture printImage;

	public Texture[] tutorialAx;
	public Texture[] tutorialMoonRabbit;
	public Texture[] tutorialHeungbu;
	public Texture[] tutorialSunMoon;

	public void SetImage(string aName) {
		gameName = aName;

		if (gameName == "Ax") {
			printImage.texture = tutorialAx[index];
		} else if (gameName == "MoonRabbit") {
			printImage.texture = tutorialMoonRabbit[index];
		} else if (gameName == "Heungbu") {
			printImage.texture = tutorialHeungbu[index];
		} else if (gameName == "SunMoon") {
			printImage.texture = tutorialSunMoon[index];
		}

		index++;
	}

	public void NextImage() {
	/*	if (gameName == "Ax") {
			if (tutorialAx.Length > index) { 
				printImage.texture = tutorialAx[index];
			} else {
				GameManagerAx GM = GameObject.Find ("GameManager").GetComponent<GameManagerAx> ();
				//GM.TutoralDone("Ax");
			}
		} else if (gameName == "MoonRabbit") {
			if (tutorialMoonRabbit.Length > index) { 
				printImage.texture = tutorialMoonRabbit[index];
			} else {
				GameManagerRabbit GM = GameObject.Find ("GameManager").GetComponent<GameManagerRabbit> ();
				//GM.TutoralDone("MoonRabbit");
			}
		} else if (gameName == "Heungbu") {
			if (tutorialHeungbu.Length > index) { 
				printImage.texture = tutorialHeungbu[index];
			} else {
				GameManagerHeungbu GM = GameObject.Find ("GameManager").GetComponent<GameManagerHeungbu> ();
				//GM.TutoralDone("Heungbu");
			}
		} else if (gameName == "SunMoon") {
			if (tutorialSunMoon.Length > index) { 
				printImage.texture = tutorialSunMoon[index];
			} else {
				//GameManagerSunMoon GM = GameObject.Find ("GameManager").GetComponent<GameManagerSunMoon> ();
				//GM.TutoralDone("SunMoon");
			}
		}

		index++;*/
	}
}
