using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace Arcserve.Office365.Exchange.Com
{
    [Guid("115F7FE4-F37D-4165-83C8-11B81FEB731C")]
    [ComVisible(true)]
    public interface IQueryCatalog
    {
        IQueryResult Query(IQueryCondition query, int hParentId, int lParentId, uint startIndex, uint pageCount);
        int QueryCount(IQueryCondition query, int hParentId, int lParentId);
        void SetCatalogFile(string catalogFilePath);
    }

    [Guid("890BBE1B-C84F-40D9-ABB7-3C874C8BF40F")]
    [ComVisible(true)]
    public interface IQueryResult
    {
        uint QueryCount { get; set; }
        uint TotalCount { get; set; }
        IResult GetResult(uint index);
    }

    [Guid("53F503D3-FA80-4D8C-8025-20269CEFF165")]
    [ComVisible(true)]
    public interface IResult
    {
        int ObjType { get; set; }
        int LId { get; set; }
        int HId { get; set; }
        string DisplayName { get; set; }
        int LParentId { get; set; }
        int HParentId { get; set; }

        IMailboxResult GetMailboxResult();
        IFolderResult GetFolderResult();
        IMailItemResult GetMailItemResult();
    }

    [Guid("554883AA-6C68-47F0-83CF-EFDB02D3DCE7")]
    [ComVisible(true)]
    public interface IMailboxResult : IResult
    {

    }

    [Guid("FF8DCC3B-990E-4746-8856-59CD9AC79B45")]
    [ComVisible(true)]
    public interface IFolderResult : IResult
    {

    }

    [Guid("56A366AD-0DD9-4E1B-9F04-49BA70C98BCD")]
    [ComVisible(true)]
    public interface IMailItemResult : IResult
    {
        DateTime SentTime { get; set; }
        DateTime ReceiveTime { get; set; }
        string Sender { get; set; }
        string Receiver { get; set; }
        uint MailFlag { get; set; }
        uint MailSize { get; set; }
    }
}
