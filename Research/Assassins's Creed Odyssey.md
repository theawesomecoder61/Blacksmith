# Assassin's Creed: Odyssey
Research by theawesomecoder61.

### How to interpret this documentation
- Forgive me if this documentation is confusing. This is my first time doing this.
- If a struct has been spelt out, it will not be repeated. Therefore, you must look for the struct elsewhere in this documentation.

## Notes
- In the raw data extracted from the forge, you should take each Data Chunk data, decompress it, and merge it with the other decompressed data. This is because the maximum decompressed data size for each Data Chunk is 262144 bytes (0x00 0x00 0x04 0x00).
- **THIS DOCUMENTATION IS UNFINISHED.**

## Compression
Oodle

## Header
|  Type  |      Name      |     Size     | Notes |
|--------|----------------|--------------|-------|
| int    | resource       | 4            |       |
| int    | fileSize       | 4            |       |
| int    | fileNameSize   | 4            |       |
| char[] | fileName       | fileNameSize |       |
|        | <skip 2 bytes> |              |       |
| int64  | fileID         | 8            | fileID is the same from this file's entry in the .forge Index Table |

---

## DXT Type
Whatever you call it, DXT compression, fourCC, etc.

|  Value   | DXT Type |
|----------|----------|
|     0, 7 | DXT0     |
|  1, 2, 3 | DXT1     |
|        4 | DXT3     |
| 5, 6, 12 | DXT5     |
| 8, 9, 16 | DX10     |


## Texture Map
### Structure
- [Header](#header)
- Texture Map Block
- [Image Data](#image-data)

### TopMip Properties Block
| Type |      Name      | Size (bytes) |
|------|----------------|--------------|
| int  | width          |            4 |
| int  | height         |            4 |
|      | <skip 8 bytes> |              |
| int  | dxt            |            4 |
|      | <skip 4 bytes> |              |
| int  | mipmaps        |            4 |

### Texture Map Block
|  Type  |      Name       | Size (bytes) |
|--------|-----------------|--------------|
| int    | fileType        | 4            |
| TopMip | topMip0         | 28           |
|        | <skip 81 bytes> |              |
| TopMip | topMip1         | 28           |
|        | <skip 25 bytes> |              |

### Image Data
|  Type  |      Name       | Size (bytes)  |
|--------|-----------------|---------------|
| int    | imageDataSize   | 4             |
| byte[] | imageData       | imageDataSize |

---

## TopMip
### Structure
- [Header](#header)
- [Image Data Alt](#image-data-alt)

### Image Data Alt
This is different than the Image Data block above. This one lacks an int before the imageData.
|  Type  |      Name       | Size (bytes) | Notes |
|--------|-----------------|--------------|-------|
| byte[] | imageData       | fileSize | fileSize from the Header |
