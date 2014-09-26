using System.Collections;

public class GameInfo {
	public int no;
	public string scene;
	public bool open;
	public int score;
	public string grade;

	public GameInfo(int aNo, string aScene, int aOpen, int aScore, string aGrade) {
		no = aNo;
		scene = aScene;
		score = aScore;
		grade = aGrade;
		
		if (aOpen == 1)
			open = true;
		else 
			open = false;
	}
}
