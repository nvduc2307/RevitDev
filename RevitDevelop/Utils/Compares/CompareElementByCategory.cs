namespace RevitDevelop.Utils.Compares
{
    public class CompareElementByCategory : IEqualityComparer<Category>
    {
        public bool Equals(Category x, Category y)
        {
            return x.Id.ToString() == y.Id.ToString();
        }

        public int GetHashCode(Category obj)
        {
            return 0;
        }
    }
}
