using UnityEngine;
using System.Collections;

public enum EffectSoundTiger {
	Correct = 0, 
	HitStone,
	HitTiger,
	BombGourd
}

public enum EffectSoundHeunbu {
	Correct = 0, 
	BombGourd
}


public class Sound : MonoBehaviour {
	public AudioClip[] clipList;// = new AudioClip[3];

	void Init (string gameName = "basic") {
		if (gameName == "basic" || gameName == "MoonRabbit" || gameName == "Gildong" || gameName == "Pig" || gameName == "RedShoe") {
			clipList = new AudioClip[1];
			clipList [0] = Resources.Load ("Sound/correct_sound") as AudioClip;
		} else if (gameName == "SunMoon") {
			clipList = new AudioClip[3];
			clipList [0] = Resources.Load ("Sound/correct_sound") as AudioClip;
			clipList [1] = Resources.Load ("Sound/hit_stone") as AudioClip;
			clipList [2] = Resources.Load ("Sound/hit_tiger") as AudioClip;
		} else if (gameName == "Heungbu") {
			clipList = new AudioClip[2];
			clipList [0] = Resources.Load ("Sound/correct_sound") as AudioClip;
			clipList [1] = Resources.Load ("Sound/bomb_gourd") as AudioClip;
		} 
	}

	public void SoundPlay() {
		audio.clip = clipList[0];
		audio.Play();
	}
	public void SoundPlayLoadFile(int index) {
		audio.clip = clipList[index];
		audio.Play();
	}
}
