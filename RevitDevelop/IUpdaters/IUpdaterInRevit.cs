namespace RevitDevelop.IUpdaters
{
    public abstract class IUpdaterInRevit : IUpdater
    {
        public void Execute(UpdaterData data)
        {
            throw new NotImplementedException();
        }

        public string GetAdditionalInformation()
        {
            throw new NotImplementedException();
        }

        public ChangePriority GetChangePriority()
        {
            throw new NotImplementedException();
        }

        public UpdaterId GetUpdaterId()
        {
            throw new NotImplementedException();
        }

        public string GetUpdaterName()
        {
            throw new NotImplementedException();
        }
    }
}
