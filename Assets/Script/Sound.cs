using UnityEngine;
using System.Collections;

public class Sound : MonoBehaviour {
	void SoundPlay() {
		audio.Play();
	}
	void SoundCorrectPlay() {
		//audio.clip = Resources.Load("Sound/correct_sound") as AudioClip;
		audio.Play();
	}
	void SoundIncorrectPlay() {
		//audio.clip = Resources.Load("Sound/correct_sound") as AudioClip;
		audio.Play();
	}
}
