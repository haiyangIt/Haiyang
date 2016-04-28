namespace Arcserve.Office365.Exchange.Data.Event
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