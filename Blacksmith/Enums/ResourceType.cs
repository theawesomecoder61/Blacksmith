using System;

namespace Blacksmith.Enums
{
    public enum ResourceType
    {
        _NONE = 0,
        COMPILED_MESH = 1,
        LOCALIZATION_DATA = 2,
        LOCALIZATION_MANAGER = 3,
        LOCALIZATION_PACKAGE = 4,
        MATERIAL = 5,
        MESH = 6,
        MIPMAP = 7,
        TEXTURE_MAP = 8,
        TEXTURE_SET = 9
    }

    public static class ResourceTypeExtensions
    {
        public static readonly string[] VALUES =
        {
            "", // none
            "FC9E1595", // compiled mesh
            "D28389B5", // localization data
            "4A70DE43", // localization manager
            "6E3C9C6F", // localization package
            "85C817C3", // material
            "415D9568", // mesh
            "1D4B87A3", // mipmap
            "A2B7E917", // texture map
            "D70E6670" // texture set
        };

        public static ResourceType GetResourceType(uint hexBytesAsInt)
        {
            string hex = string.Format("{0:X4}", hexBytesAsInt).PadLeft(8, '0');
            return Array.IndexOf(VALUES, hex) > -1 ?
                (ResourceType)Enum.GetValues(typeof(ResourceType)).GetValue(Array.IndexOf(VALUES, hex)) :
                ResourceType._NONE;
        }

        public static ResourceType GetResourceTypeWithString(string hexBytes)
        {
            return Array.IndexOf(VALUES, hexBytes) > -1 ?
                (ResourceType)Enum.GetValues(typeof(ResourceType)).GetValue(Array.IndexOf(VALUES, hexBytes)) :
                ResourceType._NONE;
        }
    }
}