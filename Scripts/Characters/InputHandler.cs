using UnityEngine;

public class InputHandler {

    private Character character;
    private Locomotion locomotion;
    public JoystickConfig controller;
    #region Buttons Commands
    private Command LeftStick;
    private Command RightStick;

    private Command LeftStickButton;
    private Command RightStickButton;

    private Command ButtonDown;
    private Command ButtonLeft;
    private Command ButtonRight;
    private Command ButtonUp;

    private Command DPadDown;
    private Command DPadLeft;
    private Command DPadRight;
    private Command DPadUp;

    private Command LeftBumper;
    private Command RightBumper;

    private Command LeftTrigger;
    private Command RightTrigger;

    private Command ShareButton;
    private Command OptionsButton;
#endregion
    #region Button vars
    private float buttonDownPressTimer = 0;
    private float buttonRightPressTimer = 0;
    private float buttonLeftPressTimer = 0;
    private float buttonUpPressTimer = 0;

    private float DPadDownPressTimer = 0;
    private float DPadRightPressTimer = 0;
    private float DPadLeftPressTimer = 0;
    private float DPadUpPressTimer = 0;

    private float LeftTriggerTimer = 0;
    private float RightTriggerTimer = 0;

    private bool DPadDownPressed = false;
    private bool DPadRightPressed = false;
    private bool DPadLeftPressed = false;
    private bool DPadUpPressed = false;

    private bool leftTriggerInUse = false;
    private bool rightTriggerInUse = false;

    #endregion
    #region State Button Commands
    private Command CurrentButtonDownState;
    private Command CurrentButtonLeftState;
    private Command CurrentButtonRightState;
    private Command CurrentButtonUpState;

    private Command CurrentDPadDownState;
    private Command CurrentDPadLeftState;
    private Command CurrentDPadRightState;
    private Command CurrentDPadUpState;
    #endregion

    public InputHandler(Character c, Locomotion l, int n)
    {
        character = c;
        locomotion = l;
        locomotion.Init();

        controller = new JoystickConfig(n);
        SwitchStates(0);
    }

    public void GetInput()
    {
        ///STICKS
        LeftStick.Excecute(character, controller.MainLeftJoystick());
        RightStick.Excecute(character, controller.MainRightJoystick());
        if (controller.LeftStickClick()) LeftStickButton.Excecute(character);
        if (controller.RightStickClick()) RightStickButton.Excecute(character);

        ///BUTTON TAPS / HOLD
        if (controller.AButton())
        {
            buttonDownPressTimer = Time.time;
            ButtonDown.Excecute(character, -1);
        }
        if (controller.VerticalDPad() < 0 && !DPadDownPressed)
        {
            DPadDownPressTimer = Time.time;
            DPadDownPressed = true;
            DPadDown.Excecute(character);
        }

        if (controller.BButton())
        {
            buttonRightPressTimer = Time.time;
            ButtonRight.Excecute(character, -1);
        }
        if (controller.HorizontalDPad() > 0 && !DPadRightPressed)
        {
            DPadRightPressTimer = Time.time;
            DPadRightPressed = true;
            ButtonRight.Excecute(character, -1);
        }


        if (controller.XButton())
        {
            buttonLeftPressTimer = Time.time;
            ButtonLeft.Excecute(character, -1);
        }
        if (controller.HorizontalDPad() < 0 && !DPadLeftPressed)
        {
            DPadLeftPressTimer = Time.time;
            DPadLeftPressed = true;
            DPadLeft.Excecute(character, -1);
        }


        if (controller.YButton())
        {
            buttonUpPressTimer = Time.time;
            ButtonUp.Excecute(character, -1);
        }
        if (controller.VerticalDPad() > 0 && !DPadUpPressed)
        {
            DPadUpPressTimer = Time.time;
            DPadUpPressed = true;
            DPadUp.Excecute(character , -1);
        }

        ///BUTTON UPS
        if (controller.AButtonRelease())
        {
            float time = Time.time - buttonDownPressTimer;
            ButtonDown.Excecute(character, time);
        }
        if (controller.VerticalDPad() >= 0 && DPadDownPressed)
        {
            float time = Time.time - DPadDownPressTimer;
            DPadDownPressed = false;
            DPadDown.Excecute(character, time);
        }

        if (controller.BButtonRelease())
        {
            float time = Time.time - buttonRightPressTimer;
            ButtonRight.Excecute(character, time);
        }
        if (controller.HorizontalDPad() <= 0 && DPadRightPressed)
        {
            float time = Time.time - DPadRightPressTimer;
            DPadRightPressed = false;
            DPadRight.Excecute(character, time);
        }

        if (controller.XButtonRelease())
        {
            float time = Time.time - buttonLeftPressTimer;
            ButtonLeft.Excecute(character, time);
        }
        if (controller.HorizontalDPad() >= 0 && DPadLeftPressed)
        {
            float time = Time.time - DPadLeftPressTimer;
            DPadLeftPressed = false;
            DPadLeft.Excecute(character, time);
        }

        if (controller.YButtonRelease())
        {
            float time = Time.time - buttonUpPressTimer;
            ButtonUp.Excecute(character, time);
        }
        if (controller.VerticalDPad() <= 0 && DPadUpPressed)
        {
            float time = Time.time - DPadUpPressTimer;
            DPadUpPressed = false;
            DPadUp.Excecute(character, time);
        }

        ///BUMPERS
        if (controller.RightBumper()) RightBumper.Excecute(character);

        if (controller.LeftBumper()) LeftBumper.Excecute(character);

        if (controller.RightBumperRelease()) RightBumper.Excecute(character);

        if (controller.LeftBumperRelease()) LeftBumper.Excecute(character);

        ///TRIGGERS
        if (controller.RightTrigger() > 0)
        {
            rightTriggerInUse = true;
            RightTriggerTimer += Time.deltaTime;

            if (RightTriggerTimer > 0.5f)
                RightTrigger.Excecute(character, true);
        }
        if (controller.RightTrigger() <= 0 && rightTriggerInUse)
        {
            if (RightTriggerTimer > 0.5f)
                RightTrigger.Excecute(character, false);
            else
                RightTrigger.Excecute(character);

            RightTriggerTimer = 0;
            rightTriggerInUse = false;
        }

        if (controller.LeftTrigger() > 0) LeftTrigger.Excecute(character);
        if (controller.LeftTrigger() <= 0 && leftTriggerInUse) LeftTrigger.Excecute(character);

        ///OPTIONS
        if (controller.SelectButton()) ShareButton.Excecute(character);

        if (controller.StartButton()) OptionsButton.Excecute(character);
    }

    public void SwitchStates(int state)
    {
        switch (state)
        {
            case 0:
                PlayState();
                ButtonDown = CurrentButtonDownState = new JumpCommand();
                ButtonLeft = CurrentButtonLeftState = new NormalAttackCommand();
                ButtonRight = CurrentButtonRightState = new BreakDefenceCommand();
                ButtonUp = CurrentButtonUpState = new StunAttackCommand();

                DPadDown = CurrentDPadDownState = new JumpCommand();
                DPadLeft = CurrentDPadLeftState = new NormalAttackCommand();
                DPadRight = CurrentDPadRightState = new BreakDefenceCommand();
                DPadUp = CurrentDPadUpState = new StunAttackCommand();
                break;
            case 1:
                PauseState();
                break;
        }
    }

    private void PlayState()
    {
        LeftStick = new MoveCommand();
        RightStick = new CameraCommand();

        LeftStickButton = new ChangeStanceCommand();
        RightStickButton = new ResetCameraCommand();

        ButtonDown = CurrentButtonDownState;
        ButtonLeft = CurrentButtonLeftState;
        ButtonRight = CurrentButtonRightState;
        ButtonUp = CurrentButtonUpState;

        DPadDown = CurrentDPadDownState;
        DPadLeft = CurrentDPadLeftState;
        DPadRight = CurrentDPadRightState;
        DPadUp = CurrentDPadUpState;

        LeftBumper = new Skill1Command();
        RightBumper = new Skill2Command();

        LeftTrigger = new Skill3Command();
        RightTrigger = new RunCommnad();

        ShareButton = new NullCommand();
        OptionsButton = new PauseCommand();
}
    private void PauseState()
    {
        LeftStick = new NavigateOptionsCommand();
        RightStick = new NullCommand();

        LeftStickButton = new NullCommand();
        RightStickButton = new NullCommand();

        ButtonDown = new SubmitCommand();
        ButtonLeft = new NullCommand();
        ButtonRight = new CancelCommand();
        ButtonUp = new NullCommand();

        DPadDown = new NavigateOptionsCommand();
        DPadLeft = new NavigateOptionsCommand();
        DPadRight = new NavigateOptionsCommand();
        DPadUp = new NavigateOptionsCommand();

        LeftBumper = new NullCommand();
        RightBumper = new NullCommand();

        LeftTrigger = new NullCommand();
        RightTrigger = new NullCommand();

        ShareButton = new NullCommand();
        //OptionsButton;
    }
}
