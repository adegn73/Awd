#ifndef LZHENGINE_EXPORTS
#define LZHENGINE_EXPORTS


typedef enum
{
	LZHERR_OK = 0,
	LZHERR_UNKNOWN_METHOD,
	LZHERR_EXTHEADER_EXISTS,
} lzhErr;

#pragma pack(push, 1)

typedef struct
{
	uchar	headerSize;
	uchar	headerSum;

	uchar	method[5];
	ulong	compressedSize;
	ulong	originalSize;
	ushort	_unknown;
	ushort	fileType;
	uchar	_0x20;
	uchar	_0x01;
	uchar	filenameLen;
	uchar	filename[0];
} lzhHeader;
#pragma pack(pop)


#ifdef LZHENGINE_EXPORT_FUNCS
#define EXPORT __declspec(dllexport)
#else
#define EXPORT __declspec(dllimport)
#endif


EXPORT void	lzhInit(void);
EXPORT uchar	lzhCalcSum(uchar* ptr, ulong len);
EXPORT lzhErr	lzhCompress(void* fname, ulong fnamelen, void* inbuf, ulong inbufsize, void* outbuf, ulong outbufsize, ulong* usedsize, ushort fileType);
EXPORT lzhErr	lzhExpand(lzhHeader* lzhptr, void* outbuf, ulong outbufsize, ushort* crc);

#endif
