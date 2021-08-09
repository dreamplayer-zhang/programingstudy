namespace RootTools.Control
{
    public interface IControl
    {
        Axis GetAxis(string id, Log log);

        AxisXY GetAxisXY(string id, Log log);

        AxisXZ GetAxisXZ(string id, Log log);

        Axis3D GetAxis3D(string id, Log log);
    }
}
