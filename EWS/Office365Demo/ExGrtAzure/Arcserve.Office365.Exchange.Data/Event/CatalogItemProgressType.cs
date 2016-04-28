namespace Arcserve.Office365.Exchange.Data.Event
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