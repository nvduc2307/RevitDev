namespace RevitDevelop.Utils.RevTitleBlocks
{
    public class RevTitleBlock
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public RevTitleBlock(RevTitleBlockType revTitleBlockType)
        {
            Id = (int)revTitleBlockType;
            Name = revTitleBlockType.ToString();
        }

        public static List<RevTitleBlock> GetRevTitleBlocks()
        {
            return new List<RevTitleBlock>()
            {
                new RevTitleBlock(RevTitleBlockType.A3),
                new RevTitleBlock(RevTitleBlockType.A1),
            };
        }
    }
}
