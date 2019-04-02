using System;

namespace Blacksmith.Enums
{
    public enum ResourceType : uint
    {
        // source: http://wiki.tbotr.net/index.php?title=Assassins_Creed:File_Formats
        _NONE = 4291144048, // something I chose randomly
        ANIMATION = 262342271,
        BUILD_TABLE = 585940579,
        CELL_DATA_BLOCK = 2888548200, // unconfirmed
        COMPILED_MATERIAL_TEMPLATE = 1000043639,
        COMPILED_MESH = 4238218645,
        COMPILED_MESH_INSTANCE = 1130893339,
        COMPILED_TEXTURE_MAP = 321093609,
        COMRPESSED_LOCALIZATION_DATA = 3531835829,
        ENTITY = 159662430,
        HAIR_MESH = 2121000489,
        LOCALIZATION_MANAGER = 1248910915,
        LOCALIZATION_PACKAGE = 1849465967,
        LOD_SELECTOR = 1373399936,
        MASK_16 = 2461622132,
        MATERIAL = 2244483011,
        MATERIAL_INFO = 2560476850,
        MATERIAL_TEMPLATE = 3170581626,
        MESH = 1096652136,
        MESH_DATA = 105229237,
        MESH_INSTANCE_DATA = 1399756347,
        MESH_PRIMITIVE = 2775812079,
        MESH_SHAPE_TRIANGLE_MATERIAL_DATA = 2407545263,
        MESH_SOURCE = 3981995930,
        MIPMAP = 491489187,
        SKELETON = 615435132,
        TERRAIN = 130771501,
        TEXTURE_BASE = 535104481,
        TEXTURE_GRADIENT = 610229413,
        TEXTURE_MAP = 2729961751,
        TEXTURE_SET = 3608045168,
        TEXTURE_SOURCE = 2849914618
    }

    public static class ResourceTypeExtensions
    {
        public static ResourceType GetResourceType(uint hexBytesAsInt)
        {
            string hexBytes = string.Format("{0:X4}", hexBytesAsInt).PadLeft(8, '0');
            return GetResourceType(hexBytes);
        }

        public static ResourceType GetResourceType(string hexBytes)
        {
            ResourceType type = ResourceType._NONE;
            Enum.TryParse(hexBytes, out type);
            return type;
        }
    }
}