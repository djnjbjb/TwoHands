#define TEST_RIGHT_LINE
#undef TEST_RIGHT_LINE
using UnityEngine;

public class HKey : MonoBehaviour
{
    public class DebugMod
    {
        public bool lGrabAlways = false;
        public bool rGrabAlways = false;
    }
    static public bool debugModOn = true;
    static public DebugMod debugMod = new DebugMod();

    //-------------------
    static private UnityEngine.InputSystem.Gamepad gamepad;
    static private UnityEngine.InputSystem.Keyboard keyboard; 

    //Keyboard
    static private bool lUp;
    static private bool lDown;
    static private bool lLeft;
    static private bool lRight;
    static private int lMvRight;
    static private int lMvUp;
           
    static private bool rUp;
    static private bool rDown;
    static private bool rLeft;
    static private bool rRight;
    static private int rMvRight;
    static private int rMvUp;

    static bool lAlt;
    static bool rAlt;

    static public bool lAnyDirKeyHold_Keyboard;
    static public bool rAnyDirKeyHold_Keyboard;

    //GamePad
    static Vector2 leftStick;
    static Vector2 rightStick;
    static bool leftTrigger;
    static bool rightTrigger;

    static public bool lAnyDirKeyHold_Gamepad;
    static public bool rAnyDirKeyHold_Gamepad;


    //Public
    static public bool lAnyDirKeyHold;
    static public bool rAnyDirKeyHold;

    static public Vector2 lMvDir;
    static public Vector2 rMvDir;
    
    static public bool lGrab;
    static public bool rGrab;

    public void Start()
    {
        keyboard = UnityEngine.InputSystem.Keyboard.current;
        gamepad = UnityEngine.InputSystem.Gamepad.current;

        if (gamepad == null && keyboard == null)
        {
            Ludo.LogFile.Log("✖ No Control device connected. No keyboard and no gamepad.");
        }
        if (keyboard != null)
        {
            Ludo.LogFile.Log("✔ Keyboard connected.");
        }
        if (gamepad != null)
        {
            Ludo.LogFile.Log("✔ Gamepad connected.");
        }
    }

    public void Update()
    {

        if (debugModOn)
        {
            DebugModUpdateRefresh();
            return;
        }

        Keyboard();
        GamePad();
        DeviceToOperation();
    }

    static private void Keyboard()
    {
        if (keyboard == null)
            return;

        lUp = keyboard[UnityEngine.InputSystem.Key.W].isPressed;
        lDown = keyboard[UnityEngine.InputSystem.Key.S].isPressed;
        lLeft = keyboard[UnityEngine.InputSystem.Key.A].isPressed;
        lRight = keyboard[UnityEngine.InputSystem.Key.D].isPressed;
        lMvUp = (lUp ? 1 : 0) - (lDown ? 1 : 0);
        lMvRight = (lRight ? 1 : 0) - (lLeft ? 1 : 0);
        lAlt = keyboard[UnityEngine.InputSystem.Key.LeftAlt].isPressed;

        rUp = keyboard[UnityEngine.InputSystem.Key.O].isPressed;
        rDown = keyboard[UnityEngine.InputSystem.Key.L].isPressed;
        rLeft = keyboard[UnityEngine.InputSystem.Key.K].isPressed;
        rRight = keyboard[UnityEngine.InputSystem.Key.Semicolon].isPressed;
        rAlt = keyboard[UnityEngine.InputSystem.Key.RightAlt].isPressed;

        rMvUp = (rUp ? 1 : 0) - (rDown ? 1 : 0);
        rMvRight = (rRight ? 1 : 0) - (rLeft ? 1 : 0);

        lAnyDirKeyHold_Keyboard = lDown || lUp || lRight || lLeft;
        rAnyDirKeyHold_Keyboard = rDown || rUp || rRight || rLeft;
    }

    static private void GamePad()
    {
        const float threshold = 0.05f;
        if (gamepad == null)
        {
            return;
        }

        leftStick = gamepad.leftStick.ReadValue();
        rightStick = gamepad.rightStick.ReadValue();
        if (Mathf.Abs(leftStick.x) < threshold)
        {
            leftStick.x = 0f;
        }
        if (Mathf.Abs(leftStick.y) < threshold)
        {
            leftStick.y = 0f;
        }
        if (Mathf.Abs(rightStick.x) < threshold)
        {
            rightStick.x = 0f;
        }
        if (Mathf.Abs(rightStick.y) < threshold)
        {
            rightStick.y = 0f;
        }
        
        leftTrigger = gamepad.leftTrigger.ReadValue() >= threshold;
        rightTrigger = gamepad.rightTrigger.ReadValue() >= threshold;

        lAnyDirKeyHold_Gamepad = leftStick != Vector2.zero;
        rAnyDirKeyHold_Gamepad = rightStick != Vector2.zero;

    }

    static private void DeviceToOperation()
    {
        if (keyboard == null && gamepad == null)
        {
            lMvDir = Vector2.zero;
            rMvDir = Vector2.zero;
            rGrab = false;
            lGrab = false;
            return;
        }
        Vector2 lMvDirKeyboard = new Vector2(lMvRight, lMvUp).normalized;
        Vector2 rMvDirKeyboard = new Vector2(rMvRight, rMvUp).normalized;

        Vector2 lMvDirGamepad = Vector2.zero;
        Vector2 rMvDirGamepad = Vector2.zero;
        if (leftStick != Vector2.zero)
        {
            if (Vector2.SignedAngle(leftStick, Vector2.up) >= -18
                && Vector2.SignedAngle(leftStick, Vector2.up) < 18)
            {
                lMvDirGamepad = Vector2.up;
            }
            else if (Vector2.SignedAngle(leftStick, Vector2.up) >= 18
                && Vector2.SignedAngle(leftStick, Vector2.up) < 18 + 54)
            {
                lMvDirGamepad = new Vector2(1, 1).normalized;
            }
            else if (Vector2.SignedAngle(leftStick, Vector2.up) >= 18+54
                && Vector2.SignedAngle(leftStick, Vector2.up) < 90 + 18)
            {
                lMvDirGamepad = Vector2.right;
            }

            else if (Vector2.SignedAngle(leftStick, Vector2.up) >= 90+18
                && Vector2.SignedAngle(leftStick, Vector2.up) < 180-18)
            {
                lMvDirGamepad = new Vector2(1, -1).normalized;
            }
            else if ( ( Vector2.SignedAngle(leftStick, Vector2.up) >= 180-18 && Vector2.SignedAngle(leftStick, Vector2.up) <= 180 )
                     || ( Vector2.SignedAngle(leftStick, Vector2.up) >= -180 && Vector2.SignedAngle(leftStick, Vector2.up) < -(180-18) )
                    ) 
            {
                lMvDirGamepad = Vector2.down;
            }
            else if (Vector2.SignedAngle(leftStick, Vector2.up) >= -(180 - 18)
                && Vector2.SignedAngle(leftStick, Vector2.up) < -(180 - 18 - 54))
            {
                lMvDirGamepad = new Vector2(-1, -1).normalized;
            }
            else if (Vector2.SignedAngle(leftStick, Vector2.up) >= -(90 + 18)
                && Vector2.SignedAngle(leftStick, Vector2.up) < -(90 - 18))
            {
                lMvDirGamepad = Vector2.left;
            }
            else if (Vector2.SignedAngle(leftStick, Vector2.up) >= -(90 - 18)
                && Vector2.SignedAngle(leftStick, Vector2.up) < -18)
            {
                lMvDirGamepad = new Vector2(-1, 1).normalized;
            }
        }
        
        if (rightStick != Vector2.zero)
        {
            if (Vector2.SignedAngle(rightStick, Vector2.up) >= -18
                && Vector2.SignedAngle(rightStick, Vector2.up) < 18)
            {
                rMvDirGamepad = Vector2.up;
            }
            else if (Vector2.SignedAngle(rightStick, Vector2.up) >= 18
                && Vector2.SignedAngle(rightStick, Vector2.up) < 18 + 54)
            {
                rMvDirGamepad = new Vector2(1, 1).normalized;
            }
            else if (Vector2.SignedAngle(rightStick, Vector2.up) >= 18 + 54
                && Vector2.SignedAngle(rightStick, Vector2.up) < 90 + 18)
            {
                rMvDirGamepad = Vector2.right;
            }

            else if (Vector2.SignedAngle(rightStick, Vector2.up) >= 90 + 18
                && Vector2.SignedAngle(rightStick, Vector2.up) < 180 - 18)
            {
                rMvDirGamepad = new Vector2(1, -1).normalized;
            }
            else if ((Vector2.SignedAngle(rightStick, Vector2.up) >= 180 - 18 && Vector2.SignedAngle(rightStick, Vector2.up) <= 180)
                     || (Vector2.SignedAngle(rightStick, Vector2.up) >= -180 && Vector2.SignedAngle(rightStick, Vector2.up) < -(180 - 18))
                    )
            {
                rMvDirGamepad = Vector2.down;
            }
            else if (Vector2.SignedAngle(rightStick, Vector2.up) >= -(180 - 18)
                && Vector2.SignedAngle(rightStick, Vector2.up) < -(180 - 18 - 54))
            {
                rMvDirGamepad = new Vector2(-1, -1).normalized;
            }
            else if (Vector2.SignedAngle(rightStick, Vector2.up) >= -(90 + 18)
                && Vector2.SignedAngle(rightStick, Vector2.up) < -(90 - 18))
            {
                rMvDirGamepad = Vector2.left;
            }
            else if (Vector2.SignedAngle(rightStick, Vector2.up) >= -(90 - 18)
                && Vector2.SignedAngle(rightStick, Vector2.up) < -18)
            {
                rMvDirGamepad = new Vector2(-1, 1).normalized;
            }
        }
        
        lMvDir = lMvDirGamepad != Vector2.zero ? lMvDirGamepad : lMvDirKeyboard;
        rMvDir = rMvDirGamepad != Vector2.zero ? rMvDirGamepad : rMvDirKeyboard;

        lGrab = lAlt || leftTrigger;
        rGrab = rAlt || rightTrigger;

        lAnyDirKeyHold = lAnyDirKeyHold_Keyboard || lAnyDirKeyHold_Gamepad;
        rAnyDirKeyHold = rAnyDirKeyHold_Keyboard || rAnyDirKeyHold_Gamepad;

        /*
        Debug.Log("stick:" + leftStick);
        Debug.Log("GamePad:" + lMvDirGamepad);
        Debug.Log("Move:" + lMvDir);
        */

        Y.DebugPanel.Log(_name: "lMvDirKeyboard", _message: lMvDirKeyboard, _category: "HKey");
        Y.DebugPanel.Log(_name: "leftStick", _message: leftStick, _category: "HKey");
        Y.DebugPanel.Log(_name: "angle", _message: Vector2.SignedAngle(leftStick, Vector2.up), _category: "HKey");
        Y.DebugPanel.Log(_name: "lMvDirGamePad", _message: lMvDirGamepad, _category: "HKey");
        Y.DebugPanel.Log(_name:"lMvDir", _message:lMvDir, _category:"HKey");
        Y.DebugPanel.Log(_name: "lAnyDirKeyHold", _message: lAnyDirKeyHold, _category: "HKey");
        Y.DebugPanel.Log(_name: "lAnyDirKeyHold_Keyboard", _message: lAnyDirKeyHold_Keyboard, _category: "HKey");
        Y.DebugPanel.Log(_name: "lAnyDirKeyHold_GamePad", _message: lAnyDirKeyHold_Gamepad, _category: "HKey");
    }

    static private void DebugModUpdateRefresh()
    {
        
        Keyboard();
        GamePad();
        DeviceToOperation();

        if (keyboard.digit1Key.wasPressedThisFrame)
        {
            debugMod.lGrabAlways = !debugMod.lGrabAlways;
        }
        if (keyboard.digit2Key.wasPressedThisFrame)
        {
            debugMod.rGrabAlways = !debugMod.rGrabAlways;
        }
        if (debugMod.lGrabAlways)
            lGrab = true;
        if (debugMod.rGrabAlways)
            rGrab = true;
    }

}