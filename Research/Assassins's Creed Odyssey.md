# Assassin's Creed: Odyssey
Researched by theawesomecoder61

## Compression
Oodle

## Datafile Header
|  Type  |      Name      |     Size     |
|--------|----------------|--------------|
| int    | resource       | 4            |
| int    | fileSize       | 4            |
| int    | fileNameSize   | 4            |
| char[] | fileName       | fileNameSize |
|        | <skip 2 bytes> |              |
| int64  | fileID         | 8            |

## To be continued...
