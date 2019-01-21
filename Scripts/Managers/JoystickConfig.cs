using UnityEngine;
using System.Collections.Generic;

public class JoystickConfig
{
    private Dictionary<int, string> Axes = new Dictionary<int, string>();
    private int playerNumber;

    public JoystickConfig(int p)
    {
        string[] joysticks = Input.GetJoystickNames();
        playerNumber = p;
        string controllerNumber = joysticks[p-1];

        if (controllerNumber.Contains("XBOX"))
        {
            //Buttons
            Axes.Add(0, "Button_A_P" + playerNumber.ToString());
            Axes.Add(1, "Button_B_P" + playerNumber.ToString());
            Axes.Add(2, "Button_X_P" + playerNumber.ToString());
            Axes.Add(3, "Button_Y_P" + playerNumber.ToString());
            //Bumpers
            Axes.Add(4, "Left_Shoulder_P" + playerNumber.ToString());
            Axes.Add(5, "Right_Shoulder_P" + playerNumber.ToString());
            //Triggers
            Axes.Add(6, "Left_Trigger_P" + playerNumber.ToString());
            Axes.Add(7, "Right_Trigger_P" + playerNumber.ToString());
            //DPad
            Axes.Add(8, "Horizontal_DPad_P" + playerNumber.ToString());
            Axes.Add(9, "Vertical_DPad_P" + playerNumber.ToString());
            //Left Stick
            Axes.Add(10, "Horizontal_Left_Joystick_P" + playerNumber.ToString());
            Axes.Add(11, "Vertical_Left_Joystick_P" + playerNumber.ToString());
            //Right Stick
            Axes.Add(12, "Horizontal_Right_Joystick_P" + playerNumber.ToString());
            Axes.Add(13, "Vertical_Right_Joystick_P" + playerNumber.ToString());
            //Stick Buttons
            Axes.Add(14, "Left_Stick_Button_P" + playerNumber.ToString());
            Axes.Add(15, "Right_Stick_Button_P" + playerNumber.ToString());
            //Options
            Axes.Add(16, "Select_P" + playerNumber.ToString());
            Axes.Add(17, "Start_P" + playerNumber.ToString());
        }
        else if (controllerNumber.Contains("Wireless"))
        {
            //Buttons
            Axes.Add(0, "PS4_X_P" + playerNumber.ToString());
            Axes.Add(1, "PS4_Circle_P" + playerNumber.ToString());
            Axes.Add(2, "PS4_Square_P" + playerNumber.ToString());
            Axes.Add(3, "PS4_Triangle_P" + playerNumber.ToString());
            //Bumpers
            Axes.Add(4, "PS4_L1_P" + playerNumber.ToString());
            Axes.Add(5, "PS4_R1_P" + playerNumber.ToString());
            //Triggers
            Axes.Add(6, "PS4_L2_P" + playerNumber.ToString());
            Axes.Add(7, "PS4_R2_P" + playerNumber.ToString());
            //DPad
            Axes.Add(8, "PS4_Horizontal_DPad_P" + playerNumber.ToString());
            Axes.Add(9, "PS4_Vertical_DPad_P" + playerNumber.ToString());
            //Left Stick
            Axes.Add(10, "PS4_Horizontal_LeftStick_P" + playerNumber.ToString());
            Axes.Add(11, "PS4_Vertical_LeftStick_P" + playerNumber.ToString());
            //Right Stick
            Axes.Add(12, "PS4_Horizontal_RightStick_P" + playerNumber.ToString());
            Axes.Add(13, "PS4_Vertical_RightStick_P" + playerNumber.ToString());
            //Stick Buttons
            Axes.Add(14, "PS4_L3_P" + playerNumber.ToString());
            Axes.Add(15, "PS4_R3_P" + playerNumber.ToString());
            //Options
            Axes.Add(16, "PS4_Share_P" + playerNumber.ToString());
            Axes.Add(17, "PS4_Options_P" + playerNumber.ToString());
        }
    }

    #region MenuLeftJoystick
    private bool mainLeftInUse = false;
    public int MenuLeftJoystick()
    {
        if (LeftStickHorizontal() > 0 || HorizontalDPad() > 0 || LeftStickVertical() < 0 || VerticalDPad() < 0)
        {
            if (!mainLeftInUse)
            {
                mainLeftInUse = true;
                return 1;
            }
        }
        else if (LeftStickHorizontal() < 0 || HorizontalDPad() < 0 || LeftStickVertical() > 0 || VerticalDPad() > 0)
        {
            if (!mainLeftInUse)
            {
                mainLeftInUse = true;
                return -1;
            }
        }
        else
            mainLeftInUse = false;

        return 0;
    }
#endregion
    #region TargetingRightJoystick
    private bool mainRightInUse = false;
    public int TargetingRightJoystick()
    {
        if (RightStickHorizontal() > 0)
        {
            if (!mainRightInUse)
            {
                mainRightInUse = true;
                return 1;
            }
        }
        else if (RightStickHorizontal() < 0)
        {
            if (!mainRightInUse)
            {
                mainRightInUse = true;
                return -1;
            }
        }
        else
            mainRightInUse = false;

        return 0;
    }
    public Vector2 ParryRightJoystick()
    {
        if (MainRightJoystick() != Vector3.zero)
        {
            if (!mainRightInUse)
            {
                mainRightInUse = true;
                return MainRightJoystick();
            }
        }
        else
        {
            mainRightInUse = false;
        }

        return Vector2.zero;
    }
#endregion
    #region LeftJoystick
    public float LeftStickHorizontal()
    {
        float r = 0.0f;
        r += Input.GetAxis(Axes[10]);
        r += Input.GetAxis("Keyboard_Horizontal");
        return Mathf.Clamp(r, -1.0f, 1.0f);
    }
    public float LeftStickVertical()
    {
        float l = 0.0f;
        l += Input.GetAxis(Axes[11]);
        l += Input.GetAxis("Keyboard_Vertical");
        return Mathf.Clamp(l, -1.0f, 1.0f);
    }
    public Vector3 MainLeftJoystick()
    {
        return new Vector3(LeftStickHorizontal(), 0, LeftStickVertical());
    }
    public bool LeftStickClick()
    {
        return Input.GetButtonDown(Axes[14]);
    }
#endregion
    #region RightJoystick
    public float RightStickHorizontal()
    {
        float r = 0.0f;
        r += Input.GetAxis(Axes[12]);
        //r += Input.GetAxis("Mouse X");
        return Mathf.Clamp(r, -1.0f, 1.0f);
    }
    public float RightStickVertical()
    {
        float r = 0.0f;
        r += Input.GetAxis(Axes[13]);
        //r += Input.GetAxis("Mouse Y");
        return Mathf.Clamp(r, -1.0f, 1.0f);
    }
    public Vector3 MainRightJoystick()
    {
        return new Vector3(RightStickHorizontal(), RightStickVertical(), 0);
    }
    public bool RightStickClick()
    {
        return Input.GetButtonDown(Axes[15]);
    }
#endregion
    #region DPad
    public float HorizontalDPad()
    {
        float r = 0.0f;
        r += Input.GetAxisRaw(Axes[8]);
        return r;
    }
    public float VerticalDPad()
    {
        float r = 0.0f;
        r += Input.GetAxisRaw(Axes[9]);
        return r;
    }
    public Vector3 MainDPad()
    {
        return new Vector3(HorizontalDPad(), 0, VerticalDPad());
    }
#endregion
    #region Triggers
    public float LeftTrigger()
    {
        float r = 0.0f;
        r += Input.GetAxisRaw(Axes[6]);
        return r;
    }
    public float RightTrigger()
    {
        float r = 0.0f;
        r += Input.GetAxisRaw(Axes[7]);
        return r;
    }
    public bool RawLeftTrigger()
    {
        return Input.GetButtonDown(Axes[6]);
    }
    public bool RawRightTrigger()
    {
        return Input.GetButtonDown(Axes[7]);
    }
    public bool RawLeftTriggerRelease()
    {
        return Input.GetButtonUp(Axes[6]);
    }
    public bool RawRightTriggerRelease()
    {
        return Input.GetButtonUp(Axes[7]);
    }
#endregion
    #region Shoulders
    public bool LeftBumper()
    {
        return Input.GetButtonDown(Axes[4]);
    }
    public bool RightBumper()
    {
        return Input.GetButtonDown(Axes[5]);
    }
    public bool LeftBumperRelease()
    {
        return Input.GetButtonUp(Axes[4]);
    }
    public bool RightBumperRelease()
    {
        return Input.GetButtonUp(Axes[5]);
    }
    #endregion
    #region Buttons Taps
    public bool AButton()
    {
        return Input.GetButtonDown(Axes[0]);
    }
    public bool BButton()
    {
        return Input.GetButtonDown(Axes[1]);
    }
    public bool XButton()
    {
        return Input.GetButtonDown(Axes[2]);
    }
    public bool YButton()
    {
        return Input.GetButtonDown(Axes[3]);
    }
    #endregion
    #region Buttons Ups
    public bool AButtonRelease()
    {
        return Input.GetButtonUp(Axes[0]);
    }
    public bool BButtonRelease()
    {
        return Input.GetButtonUp(Axes[1]);
    }
    public bool XButtonRelease()
    {
        return Input.GetButtonUp(Axes[2]);
    }
    public bool YButtonRelease()
    {
        return Input.GetButtonUp(Axes[3]);
    }
    #endregion
    #region OptionButtons
    public bool SelectButton()
    {
        return Input.GetButtonDown(Axes[16]);
    }
    public bool StartButton()
    {
        return Input.GetButtonDown(Axes[17]);
    }
#endregion
}