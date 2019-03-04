using UnityEngine;

public class TransHolder
{
    public Vector3 Position = Vector3.zero;
    public Vector3 RotationEuler = Vector3.zero;

    public TransHolder(Vector3 position, Vector3 rotationEuler)
    {
        this.Position = position;
        this.RotationEuler = rotationEuler;
    }

    public void SetAsWorld(Transform trans)
    {
        trans.position = Position;
        trans.rotation = Quaternion.Euler(RotationEuler);
    }

    public void SetAsWorldPosition(Transform trans)
    {
        trans.position = Position;
    }

    public void SetAsLocal(Transform trans)
    {
        trans.localPosition = Position;
        trans.localRotation = Quaternion.Euler(RotationEuler);
    }
}