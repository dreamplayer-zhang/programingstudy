namespace RootTools.Control
{
    public interface IControl
    {
        Axis GetAxis(string id, Log log);

        AxisXY GetAxisXY(string id, Log log);
    }
}
