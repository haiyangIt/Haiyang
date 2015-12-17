namespace DataProtectInterface.Event
{
    public enum CatalogMailboxProgressType : byte
    {
        SkipMailbox,
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