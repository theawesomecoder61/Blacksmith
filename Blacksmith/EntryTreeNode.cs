using Blacksmith.Enums;
using Blacksmith.FileTypes;
using System.Windows.Forms;

namespace Blacksmith
{
    public class EntryTreeNode : TreeNode
    {
        public Forge Forge;
        public Game Game;
        //public int Levels { get { return Helpers.CountStringOccurrences(FullPath, "\\"); } }
        public long Offset;
        public string Path;
        public ResourceType ResourceType = ResourceType._NONE;
        public int Size = -1;
        public EntryTreeNodeType Type = EntryTreeNodeType.NONE;

        public EntryTreeNode() : base("")
        {
        }

        public EntryTreeNode(string text) : base(text)
        {
        }

        /// <summary>
        /// Returns the Forge instance assigned to the parent .forge node
        /// </summary>
        /// <returns></returns>
        public Forge GetForge()
        {
            // find the .forge node
            if (Type == EntryTreeNodeType.ENTRY)
                return ((EntryTreeNode)Parent).Forge;
            else if (Type == EntryTreeNodeType.SUBENTRY)
                return ((EntryTreeNode)Parent.Parent).Forge;
            else
                return null;
        }
    }
}