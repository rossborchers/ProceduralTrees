
/// <summary>
/// Hands destroy responsibility over to the class. Enabling custom resource handling ( eg. object pooling )
/// Instead of calling Destroy(myClass) You call myClass.Destroy()
/// </summary>
public interface Destroyable
{
    void Destroy();
}
