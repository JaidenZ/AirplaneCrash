namespace AirplaneCrash.Server.Core.Hub
{
    /// <summary>
    /// 集线器标志
    /// </summary>
    public interface IHub<InT, OutT>
    {
        OutT Handle(InT obj);
    }
}
