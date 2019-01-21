using UnityEngine;

public abstract class Command {

    public virtual void Excecute(Character c) { }
    public virtual void Excecute(Character c, bool buttonDown) { }
    public virtual void Excecute(Character c, float time) { }
    public virtual void Excecute(Character c, Vector3 vector) { }
}
