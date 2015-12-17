namespace DataProtectInterface.Event
{
    public enum CatalogItemProgressType : byte
    {
        SkipItem,
        Start,
        SaveItemStart,
        SaveItemEnd,
        SaveItemContentStart,
        SaveItemContentEnd,
        SaveItemContentEndForExist,
        EndWithError,
        EndWithNoError
    }
}