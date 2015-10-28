namespace DataProtectInterface.Event
{
    public enum CatalogProgressType : byte
    {
        Start,
        GetAllMailboxStart,
        GetAllMailboxEnd,
        GRTForMailboxStart,
        GRTForMailboxEndWithNoError,
        GRTForMailboxEndWithError,
        EndWithNoError,
        EndWithError
    }
}