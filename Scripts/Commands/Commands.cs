using UnityEngine;

public class MoveCommand : Command
{
    public override void Excecute(Character c, Vector3 dir)
    {
        c.Move(dir);
    }
}
public class CameraCommand : Command
{
    public override void Excecute(Character c, Vector3 dir)
    {
        c.MoveCamera(dir);
    }
}
public class ResetCameraCommand : Command
{
    public override void Excecute(Character c)
    {
        c.ResetCamera();
    }
}
public class RunCommnad : Command
{
    public override void Excecute(Character c, float t)
    {
        c.Run();
    }
}
public class JumpCommand : Command
{
    public override void Excecute(Character c, float time)
    {
        if (time == -1)
            c.Jump();
    }
}
public class InteractCommand : Command
{
    public override void Excecute(Character c, float t)
    {
        c.Interact();
    }
}
public class ChangeStanceCommand : Command
{
    public override void Excecute(Character c)
    {
        c.ChangeStance();
    }
}
public class NormalAttackCommand : Command
{
    public override void Excecute(Character c, float t)
    {
        if (t == -1)
            c.NormalAttack();
    }
}
public class StunAttackCommand : Command
{
    public override void Excecute(Character c)
    {
        c.StunAttack();
    }
}
public class BreakDefenceCommand : Command
{
    public override void Excecute(Character c)
    {
        c.BreakDefence();
    }
}
public class Skill1Command : Command
{
    public override void Excecute(Character c)
    {
        c.Skill1();
    }
}
public class Skill2Command : Command
{
    public override void Excecute(Character c)
    {
        c.Skill2();
    }
}
public class Skill3Command : Command
{
    public override void Excecute(Character c)
    {
        c.Skill3();
    }
}
public class PauseCommand : Command
{
    public override void Excecute(Character c)
    {
        c.Pause();
    }
}
public class NullCommand : Command
{
}

public class NavigateOptionsCommand : Command
{
    public override void Excecute(Character c, Vector3 dir)
    {
        UIManager.INSTANCE.NavigateOptions(c);
    }
}
public class SubmitCommand : Command
{
    public override void Excecute(Character c, float t)
    {
        UIManager.INSTANCE.Submit(c);
    }
}
public class CancelCommand : Command
{
    public override void Excecute(Character c, float t)
    {
        UIManager.INSTANCE.Cancel(c);
    }
}

public class RagCommand : Command
{
    public override void Excecute(Character c)
    {
        c.Rag();
    }
}
