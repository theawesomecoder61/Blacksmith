# Assassin's Creed: Odyssey
Research by theawesomecoder61.

### How to interpret this documentation
- Forgive me if this documentation is confusing,. This is my first time doing this.
- If a struct has been spelt out, it will not be repeated. Thus, you must look for the struct elsewhere in the documentation.

## Notes
- In the raw data extracted from the forge, you should take each Data Chunk data, decompress it, and merge it with the other decompressed data. This is because the maximum decompressed data size for each Data Chunk is 262144 bytes (0x00 0x00 0x04 0x00).

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

## Texture Map
### Structure
- Header
- Texture Map Block
- Image Data

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
|  Type  |      Name       | Size (bytes) |
|--------|-----------------|--------------|
| int    | imageDataSize   | 4            |
| byte[] | imageData       | imageDataSize|

---

## TopMip
### Structure
- Header
- Image Data Block

### Image Data
|  Type  |      Name       | Size (bytes) | Notes |
|--------|-----------------|--------------|-------|
| byte[] | imageData       | fileSize | fileSize from the Header |
