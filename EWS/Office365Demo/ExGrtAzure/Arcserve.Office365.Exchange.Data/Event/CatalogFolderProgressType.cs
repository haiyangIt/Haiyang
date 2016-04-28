namespace Arcserve.Office365.Exchange.Data.Event
{
    public enum CatalogFolderProgressType : byte
    {
        Start,

        ChildItemStart,
        GetChildItemStart,
        GetChildItemsEnd,
        ProcessingItemStart,
        ProcessingItemEndWithError,
        ProcessingItemEndNoError,
        NoChildItem,
        ChildItemEnd,

        ChildFolderStart,
        GetChildFoldersStart,
        GetChildFoldersEnd,
        ChildFolderSkip,
        SaveFolderStart,
        SaveFolderEnd,
        NoChildFolder,
        ChildFolderEnd,

        EndWithNoError,
        EndWithError,
        
    }
}