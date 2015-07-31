using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FTStreamUtil.Item
{
    public enum PropertyType : ushort
    {
        MV_FLAG = 0x1000,

        PT_UNSPECIFIED = 0,
        PT_NULL = 1,	/* NULL property value */
        PT_I2 = 2,	/* Signed 16-bit value */
        PT_LONG = 3,	/* Signed 32-bit value */
        PT_R4 = 4,	/* 4-byte floating point */
        PT_DOUBLE = 5,	/* Floating point double */
        PT_CURRENCY = 6,	/* Signed 64-bit int (decimal w/	4 digits right of decimal pt, */
        PT_APPTIME = 7,	/* Application time */
        PT_ERROR = 10,	/* 32-bit error value */
        PT_BOOLEAN = 11,	/* 16-bit boolean (non-zero true) */
        PT_OBJECT = 13,	/* Embedded object in a property */
        PT_I8 = 20,	/* 8-byte signed integer */
        PT_STRING8 = 30,	/* Null terminated 8-bit character string */
        PT_UNICODE = 31,	/* Null terminated Unicode string */
        PT_SYSTIME = 64,	/* FILETIME 64-bit int w/ number of 100ns periods since Jan 1,1601 */
        PT_CLSID = 72,	/* OLE GUID */
        PT_BINARY = 258,	/* Uninterpreted (counted byte array) */
        PT_SERVERID = 0x00FB,

        PT_MV_I2 = MV_FLAG | PT_I2,
        PT_MV_LONG = MV_FLAG | PT_LONG,
        PT_MV_R4 = MV_FLAG | PT_R4,
        PT_MV_DOUBLE = MV_FLAG | PT_DOUBLE,
        PT_MV_CURRENCY = MV_FLAG | PT_CURRENCY,
        PT_MV_APPTIME = MV_FLAG | PT_APPTIME,
        PT_MV_SYSTIME = MV_FLAG | PT_SYSTIME,
        PT_MV_STRING8 = MV_FLAG | PT_STRING8,
        PT_MV_BINARY = MV_FLAG | PT_BINARY,
        PT_MV_UNICODE = MV_FLAG | PT_UNICODE,
        PT_MV_CLSID = MV_FLAG | PT_CLSID,
        PT_MV_I8 = MV_FLAG | PT_I8
    }
}
