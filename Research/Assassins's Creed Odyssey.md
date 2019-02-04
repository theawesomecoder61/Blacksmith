# Assassin's Creed: Odyssey
Research by theawesomecoder61.

### How to interpret this documentation
- Forgive me if this documentation is confusing. This is my first time doing this.
- If a struct has been spelt out, it will not be repeated. Use the links to jump to that struct.

## Notes
- In the raw data extracted from the forge, you should take each Data Chunk data, decompress it, and merge it with the other decompressed data. This is because the maximum decompressed data size for each Data Chunk is 262144 bytes (0x00 0x00 0x04 0x00).
- **THIS DOCUMENTATION IS UNFINISHED.**

## Compression
Oodle

## Header
|  Type  |      Name      |     Size     | Notes |
|--------|----------------|--------------|-------|
| [Resource Type](#resource-type)    | resourceType   | 4            |       |
| int    | fileSize       | 4            |       |
| int    | fileNameSize   | 4            |       |
| char[] | fileName       | fileNameSize |       |
|        | <skip 2 bytes> |              |       |
| int64  | fileID         | 8            | fileID is the same from this file's entry in the .forge Index Table |

---

## Resource Type
This is always found at the first 4 bytes of decompressed data. Other Resource Types can appear in the same file, for example, in mesh files. Here is a selection of the most important Resource Types.

> These values are actually strings (names of the Resource Type without spaces) hashed with CRC32.

|   Value    | Resource Type |
|------------|---------------|
| 0xFC9E1595 | Compiled Mesh |
| 0x85C817C3 | Material      |
| 0x415D9568 | Mesh          |
| 0x1D4B87A3 | Mipmap        |
| 0x24AECB7C | Skeleton      |
| 0xA2B7E917 | Texture Map   |
| 0xD70E6670 | Texture Set   |

---

## DXT
Whatever you call it, DXT Type, DXT compression, fourCC, etc.

|  Value   |  DXT |
|----------|------|
|     0, 7 | DXT0 |
|  1, 2, 3 | DXT1 |
|        4 | DXT3 |
| 5, 6, 12 | DXT5 |
| 8, 9, 16 | DX10 |

---

## Texture Map file
### Structure
- [Header](#header)
- [Texture Map](#texture-map)
- [Image Data](#image-data)

### TopMip
| Type |      Name      | Size (bytes) |
|------|----------------|--------------|
| int  | width          |            4 |
| int  | height         |            4 |
|      | <skip 8 bytes> |              |
| [DXT](#dxt) | dxt |                4 |
|      | <skip 4 bytes> |              |
| int  | mipmaps        |            4 |

### Texture Map
|  Type  |      Name       | Size (bytes) |
|--------|-----------------|--------------|
| int    | fileType        | 4            |
| [TopMip](#topmip) | topMip0 | 28        |
|        | <skip 81 bytes> |              |
| [TopMip](#topmip) | topMip1 | 28        |
|        | <skip 25 bytes> |              |

### Image Data
|  Type  |      Name       | Size (bytes)  |
|--------|-----------------|---------------|
| int    | imageDataSize   | 4             |
| byte[] | imageData       | imageDataSize |

---

## TopMip file
### Structure
- [Header](#header)
- [Image Data Alt](#image-data-alt)

### Image Data Alt
This is different than the Image Data block above. This one lacks an int before the imageData.

|  Type  |      Name       | Size (bytes) | Notes |
|--------|-----------------|--------------|-------|
| byte[] | imageData       | fileSize | fileSize from the Header |
