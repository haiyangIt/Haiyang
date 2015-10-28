namespace DataProtectInterface.Event
{
    public enum CatalogMailboxProgressType : byte
    {
        Start,
        ConnectMailboxStart,
        ConnectMailboxEnd,
        GetRootFolderStart,
        GetRootFolderEnd,
        SaveMailboxStart,
        SaveMailboxEnd,
        EndWithNoError,
        EndWithError
    }
}