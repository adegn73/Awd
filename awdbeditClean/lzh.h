//
// Award BIOS Editor - lzh.h
// Copyright (C) 2002-2004, Michael Tedder
// All rights reserved
//
// $Id: lzh.h,v 1.3 2004/04/11 07:17:15 bpoint Exp $
//
//----------------------------------------------------------------------------
//
// This program is free software; you can redistribute it and/or modify it
// under the terms of the GNU General Public License as published by the Free
// Software Foundation; either version 2 of the License, or (at your option)
// any later version.
// 
// This program is distributed in the hope that it will be useful, but WITHOUT
// ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or
// FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for
// more details.
//
// You should have received a copy of the GNU General Public License along
// with this program; if not, write to the Free Software Foundation, Inc.,
// 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.
//
//---------------------------------------------------------------------------------------------------------------------

#ifndef LZH_H
#define LZH_H

#include "types.h"
#define LZHENGINE_EXPORT_FUNCS
#include "lzhEngine_exports.h"

#pragma warning(disable: 4200)			// zero-sized array warning

#pragma pack(push, 1)


 typedef struct
 {
	ushort	crc;
	uchar	_0x20;
	ushort	extendedHeaderSize;
	uchar	extendedHeader[0];
 } lzhHeaderAfterFilename;
#pragma pack(pop)



void	lzhInit(void);
//uchar	lzhCalcSum(uchar *ptr, ulong len);
lzhErr	lzhCompress(void *fname, ulong fnamelen, void *inbuf, ulong inbufsize, void *outbuf, ulong outbufsize, ulong *usedsize, ushort fileType);
lzhErr	lzhExpand(lzhHeader *lzhptr, void *outbuf, ulong outbufsize, ushort *crc);

#endif
