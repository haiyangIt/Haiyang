namespace DataProtectInterface.Event
{
    public enum CatalogItemProgressType : byte
    {
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