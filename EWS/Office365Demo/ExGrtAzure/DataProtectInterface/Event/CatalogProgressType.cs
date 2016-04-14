namespace DataProtectInterface.Event
{
    public enum CatalogProgressType : byte
    {
        Init,
        Start,
        GetAllMailboxStart,
        GetAllMailboxEnd,
        GRTForMailboxRunning,
        GRTForFolderRunning,
        GRTForItemRunning,
        EndWithNoError,
        EndWithError
    }
}