namespace RevitDevelop.Tools.ConvertDuctToFlex.action
{
    public interface IConvertDuctToFlexAction
    {
        //elements of option A
        public void GetElementsByFittingToFitting();
        //elements of option B
        public void GetElementsByFittingToDistance();
        //elements of option C
        public void GetElementsByFittingToDistanceExactly();
    }
}
