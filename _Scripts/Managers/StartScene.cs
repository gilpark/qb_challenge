using UnityEngine;
using System.Collections;
using M1.Utilities;
using UnityEngine.SceneManagement;

public class StartScene : MonoBehaviour
{
	IEnumerator Start ()
    {
        Application.runInBackground = true;
        yield return new WaitForSeconds(1.5f);
       
        var Open4thLane = bool.Parse(Config.Read(CONFIG_KEYS.openlane4));
        var CallibrationMode = bool.Parse(Config.Read(CONFIG_KEYS.callibrationmode));

        if (CallibrationMode)
        {
            SceneManager.LoadScene(Open4thLane ? "MainScene_4Lane_Cal" : "MainScene_3Lane_Cal");
        }
        else
        {
            SceneManager.LoadScene(Open4thLane ? "MainScene_4Lane" : "MainScene_3Lane");

        }
        
    }
}
