using UnityEngine;
using System.Collections;
using M1.Utilities;
using UnityEngine.SceneManagement;

public class InputManager : SingletonBehaviour<InputManager>
{

    // UI Button //
    public void ButtonDown(int _num)
    {
        StateMachine.SendButtonDown(_num);
    }
    // UI Button //
    public void ButtonUp(int _num)
    {
        StateMachine.SendButtonUp(_num);
    }

    void Awake()
    {
        DontDestroy();
        Application.runInBackground = true;
#if UNITY_EDITOR
        SetDebug(true);
#else
        if (Config.HasKey(CONFIG_KEYS.debug))
        {
            SetDebug(bool.Parse(Config.Read(CONFIG_KEYS.debug)));
        }
        else
        {
            SetDebug(false);
        }
#endif
    }

    private void SetDebug(bool _on)
    {
        Cursor.visible = _on;
        UIDebug.useUIDebug = _on;
    }

    public static void DisableDebug()
    {
        Instance.SetDebug(false);
    }

    // debug input
    void Update()
    {

        if (Input.GetKeyDown(KeyCode.Return))
        {
        }

        if (Input.GetKeyDown(KeyCode.Backspace))
        {
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            GameManager.ResetApp.Value = true;
        }
        if (Input.GetKeyDown(KeyCode.T))
        {
            GameManager.CalibrationMode.Value = true;
        }
        if (Input.GetKeyDown(KeyCode.G))
        {
            GameManager.GameMode.Value = true;
        }
        if (Input.GetKeyDown(KeyCode.M))
        {
            Cursor.visible = !Cursor.visible;
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            SetDebug(!UIDebug.useUIDebug);
        }

        if (Input.GetKeyDown(KeyCode.H))
        {
        }

    }
}
