namespace RootTools.Comm
{
    public interface IComm
    {
        string p_id { get; set; }
        string Send(string sMsg); 
    }
}
